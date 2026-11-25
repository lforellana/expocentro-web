using System.Text;
using System.Text.Encodings.Web;
using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Expocentro.Web.Services.Notificaciones;

/// <summary>
/// Avisos por correo de los cambios de estado de las solicitudes:
/// - Enviada → Coordinación.
/// - Aprobada / Rechazada / Cancelada por Coordinación → solicitante.
/// - Cancelada por el solicitante (ya enviada) → Coordinación.
/// Nunca lanza excepciones: un fallo de correo no debe afectar la operación.
/// </summary>
public class NotificacionService
{
    private readonly ApplicationDbContext _context;
    private readonly ColaCorreos _cola;
    private readonly NotificacionesOptions _opciones;
    private readonly LinkGenerator _links;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        ApplicationDbContext context,
        ColaCorreos cola,
        IOptions<NotificacionesOptions> opciones,
        LinkGenerator links,
        IHttpContextAccessor http,
        ILogger<NotificacionService> logger)
    {
        _context = context;
        _cola = cola;
        _opciones = opciones.Value;
        _links = links;
        _http = http;
        _logger = logger;
    }

    public Task SolicitudEnviadaAsync(int solicitudId) =>
        Ejecutar(solicitudId, async s =>
        {
            var para = await CorreosCoordinacionAsync();

            Encolar(para,
                $"Nueva solicitud: {s.NombreEvento} ({s.Codigo})",
                "Nueva solicitud de evento",
                $"{s.SolicitanteUsuario.Nombre} envió una solicitud que está pendiente de revisión.",
                s, null, Enlace("Details", "Coordinacion", s.Id), "Revisar solicitud");
        });

    public Task SolicitudResueltaAsync(int solicitudId, string estado, string? comentario) =>
        Ejecutar(solicitudId, s =>
        {
            // Los eventos externos los registra Coordinación: no hay solicitante a quien avisar
            if (s.TipoEvento == TipoEvento.Externo)
                return Task.CompletedTask;

            var (titulo, texto) = estado switch
            {
                Estados.Aprobada => ("Tu solicitud fue aprobada",
                    "Coordinación aprobó tu solicitud y el salón quedó reservado."),
                Estados.Rechazada => ("Tu solicitud fue rechazada",
                    "Coordinación rechazó tu solicitud. Revisa el motivo abajo."),
                Estados.Cancelada => ("Tu evento fue cancelado",
                    "Coordinación canceló tu evento y el salón quedó liberado."),
                _ => ("Tu solicitud cambió de estado", $"Nuevo estado: {estado}.")
            };

            Encolar(new[] { s.SolicitanteUsuario.Correo },
                $"{titulo}: {s.NombreEvento} ({s.Codigo})",
                titulo, texto, s, comentario,
                Enlace("Details", "Solicitudes", s.Id), "Ver solicitud");

            return Task.CompletedTask;
        });

    public Task SolicitudCanceladaPorSolicitanteAsync(int solicitudId, string estadoAnterior, string? motivo) =>
        Ejecutar(solicitudId, async s =>
        {
            // Un borrador nunca llegó a Coordinación
            if (estadoAnterior == Estados.Borrador)
                return;

            var para = await CorreosCoordinacionAsync();

            Encolar(para,
                $"Solicitud cancelada: {s.NombreEvento} ({s.Codigo})",
                "Solicitud cancelada por el solicitante",
                $"{s.SolicitanteUsuario.Nombre} canceló su solicitud (estaba {estadoAnterior.ToLowerInvariant()}). El salón quedó liberado.",
                s, motivo, Enlace("Details", "Coordinacion", s.Id), "Ver solicitud");
        });

    // ============================
    // AUXILIARES
    // ============================

    private async Task Ejecutar(int solicitudId, Func<SolicitudEvento, Task> accion)
    {
        try
        {
            var solicitud = await _context.SolicitudesEvento
                .AsNoTracking()
                .Include(x => x.Salon)
                .Include(x => x.SolicitanteUsuario)
                .Include(x => x.DepartamentoCargo)
                .FirstOrDefaultAsync(x => x.Id == solicitudId);

            if (solicitud != null)
                await accion(solicitud);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo preparar la notificación de la solicitud {Id}", solicitudId);
        }
    }

    private async Task<List<string>> CorreosCoordinacionAsync()
    {
        var coordinadores = await _context.Usuarios
            .AsNoTracking()
            .Where(x => x.Activo && x.Rol == Roles.Coordinador)
            .Select(x => x.Correo)
            .ToListAsync();

        return _opciones.CorreosCoordinacion
            .Concat(coordinadores)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string Enlace(string accion, string controlador, int id)
    {
        var contexto = _http.HttpContext;

        if (!string.IsNullOrWhiteSpace(_opciones.UrlBase))
        {
            var ruta = _links.GetPathByAction(accion, controlador, new { id });
            return _opciones.UrlBase.TrimEnd('/') + ruta;
        }

        return contexto != null
            ? _links.GetUriByAction(contexto, accion, controlador, new { id }) ?? "#"
            : "#";
    }

    private void Encolar(
        IReadOnlyList<string> para,
        string asunto,
        string titulo,
        string texto,
        SolicitudEvento s,
        string? comentario,
        string enlace,
        string textoBoton)
    {
        var destinatarios = para
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (destinatarios.Count == 0)
        {
            _logger.LogWarning(
                "Correo \"{Asunto}\" sin destinatarios. Configure Notificaciones:CorreosCoordinacion o asigne el rol Coordinador.",
                asunto);
            return;
        }

        _cola.Encolar(new CorreoMensaje(
            destinatarios,
            asunto,
            PlantillaCorreo.Generar(titulo, texto, s, comentario, enlace, textoBoton)));
    }
}

/// <summary>HTML del correo, con estilos en línea para que se vea bien en Outlook.</summary>
public static class PlantillaCorreo
{
    public static string Generar(
        string titulo,
        string texto,
        SolicitudEvento s,
        string? comentario,
        string enlace,
        string textoBoton)
    {
        var e = HtmlEncoder.Default;
        var html = new StringBuilder();

        void Fila(string etiqueta, string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return;
            html.Append($"<tr><td style=\"padding:6px 12px 6px 0;color:#667085;white-space:nowrap;vertical-align:top\">{e.Encode(etiqueta)}</td>")
                .Append($"<td style=\"padding:6px 0;color:#101828;font-weight:600\">{e.Encode(valor)}</td></tr>");
        }

        html.Append("<div style=\"background:#f4f6fa;padding:24px 12px;font-family:Segoe UI,Arial,sans-serif\">")
            .Append("<div style=\"max-width:560px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e4e7ec\">")
            .Append("<div style=\"background:#0f2340;color:#ffffff;padding:16px 24px;font-size:15px;font-weight:700;letter-spacing:.04em\">EXPOCENTRO <span style=\"font-weight:400;color:#8ea3c2\">· Gestión de eventos</span></div>")
            .Append("<div style=\"padding:24px\">")
            .Append($"<h1 style=\"font-size:20px;margin:0 0 8px;color:#101828\">{e.Encode(titulo)}</h1>")
            .Append($"<p style=\"margin:0 0 16px;color:#475467;font-size:14px\">{e.Encode(texto)}</p>")
            .Append("<table style=\"border-collapse:collapse;font-size:14px;width:100%\">");

        Fila("Evento", s.NombreEvento);
        Fila("Código", s.Codigo);
        Fila("Salón", s.Salon?.Nombre);
        Fila("Fecha", Fechas.Rango(s.FechaHoraInicio, s.FechaHoraFin));
        Fila("Personas", s.CantidadPersonas.ToString());
        Fila("Departamento", s.DepartamentoCargo?.Nombre);
        Fila("Solicitante", s.SolicitanteUsuario?.Nombre);

        html.Append("</table>");

        if (!string.IsNullOrWhiteSpace(comentario))
        {
            html.Append("<div style=\"margin-top:16px;padding:12px 14px;background:#f9fafb;border-left:3px solid #1d5fc4;border-radius:4px;font-size:14px;color:#344054\">")
                .Append($"<div style=\"font-size:12px;color:#667085;margin-bottom:4px\">Comentario</div>{e.Encode(comentario)}</div>");
        }

        html.Append($"<div style=\"margin-top:24px\"><a href=\"{e.Encode(enlace)}\" style=\"background:#1d5fc4;color:#ffffff;text-decoration:none;padding:10px 18px;border-radius:8px;font-size:14px;font-weight:600;display:inline-block\">{e.Encode(textoBoton)}</a></div>")
            .Append("</div>")
            .Append("<div style=\"padding:12px 24px;border-top:1px solid #e4e7ec;font-size:12px;color:#98a2b3\">Mensaje automático del sistema de gestión de eventos de la CCIC. No responda a este correo.</div>")
            .Append("</div></div>");

        return html.ToString();
    }
}
