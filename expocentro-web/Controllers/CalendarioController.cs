using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.Seguridad;
using Expocentro.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

/// <summary>
/// Calendario de ocupación de salones (visible para todos los
/// empleados) y verificación de disponibilidad en vivo.
/// </summary>
public class CalendarioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DisponibilidadService _disponibilidad;

    public CalendarioController(
        ApplicationDbContext context,
        DisponibilidadService disponibilidad)
    {
        _context = context;
        _disponibilidad = disponibilidad;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Salones"] = await _context.Salones
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return View();
    }

    /// <summary>Fuente de eventos para FullCalendar.</summary>
    [HttpGet]
    public async Task<IActionResult> Eventos(
        string start,
        string end,
        int? salonId)
    {
        // FullCalendar envía "2026-11-01T00:00:00-06:00"; las fechas de
        // los eventos se guardan como hora local sin zona, así que se
        // ignora el desfase.
        if (!TryFechaLocal(start, out var desde) || !TryFechaLocal(end, out var hasta))
        {
            return BadRequest();
        }

        var usuarioId = User.ObtenerUsuarioId();
        var esCoordinador = User.EsCoordinador();

        var query = _context.SolicitudesEvento
            .AsNoTracking()
            .Where(x =>
                Estados.OcupanSalon.Contains(x.Estado.Nombre)
                && x.FechaHoraInicio < hasta
                && x.FechaHoraFin > desde);

        if (salonId.HasValue)
        {
            query = query.Where(x => x.SalonId == salonId);
        }

        var eventos = await query
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.NombreEvento,
                x.FechaHoraInicio,
                x.FechaHoraFin,
                Salon = x.Salon.Nombre,
                Estado = x.Estado.Nombre,
                x.TipoEvento,
                x.SolicitanteUsuarioId,
                x.CantidadPersonas
            })
            .ToListAsync();

        var resultado = eventos.Select(x =>
        {
            var confirmado = x.Estado == Estados.Aprobada;
            var esPropio = x.SolicitanteUsuarioId == usuarioId;

            string? url = esCoordinador
                ? Url.Action("Details", "Coordinacion", new { id = x.Id })
                : esPropio
                    ? Url.Action("Details", "Solicitudes", new { id = x.Id })
                    : null;

            var clases = new List<string>
            {
                x.TipoEvento == TipoEvento.Externo ? "ev-externo" : "ev-interno"
            };

            if (!confirmado) clases.Add("ev-pendiente");

            return new
            {
                id = x.Id,
                title = $"{x.NombreEvento} · {x.Salon}",
                start = x.FechaHoraInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = x.FechaHoraFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                url,
                classNames = clases,
                extendedProps = new
                {
                    codigo = x.Codigo,
                    salon = x.Salon,
                    estado = x.Estado,
                    tipo = x.TipoEvento.ToString(),
                    personas = x.CantidadPersonas
                }
            };
        });

        return Json(resultado);
    }

    private static bool TryFechaLocal(string? valor, out DateTime fecha)
    {
        fecha = default;

        if (string.IsNullOrEmpty(valor) || valor.Length < 10)
            return false;

        var parte = valor.Length >= 19 ? valor[..19] : valor[..10];

        return DateTime.TryParse(
            parte,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out fecha);
    }

    /// <summary>
    /// Verificación de disponibilidad usada por los formularios.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Disponibilidad(
        int salonId,
        DateTime inicio,
        DateTime fin,
        int? excluirId,
        int? personas)
    {
        if (salonId <= 0 || fin <= inicio)
        {
            return Json(new { valido = false });
        }

        var r = await _disponibilidad.VerificarAsync(
            salonId, inicio, fin, excluirId);

        return Json(new
        {
            valido = true,
            salon = r.NombreSalon,
            capacidad = r.CapacidadMaxima,
            excedeCapacidad = personas.HasValue
                && r.CapacidadMaxima.HasValue
                && personas > r.CapacidadMaxima,
            bloqueado = r.HayConflictoConfirmado,
            conflictos = r.Conflictos.Select(c => new
            {
                c.Codigo,
                c.NombreEvento,
                inicio = c.Inicio.ToString("dd/MM/yyyy hh:mm tt"),
                fin = c.Fin.ToString("dd/MM/yyyy hh:mm tt"),
                c.Estado,
                confirmado = c.EsConfirmado
            })
        });
    }
}
