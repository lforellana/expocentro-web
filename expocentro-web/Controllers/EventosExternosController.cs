using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.Seguridad;
using Expocentro.Web.Services;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

/// <summary>
/// Registro manual de eventos de clientes externos. Quedan como
/// "Aprobada" (reserva confirmada) y bloquean el salón.
/// Fase futura: crear/actualizar automáticamente desde SAP usando
/// ReferenciaSap como llave.
/// </summary>
[Authorize(Policy = Politicas.Coordinacion)]
public class EventosExternosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DisponibilidadService _disponibilidad;
    private readonly SolicitudEstadoService _estados;

    public EventosExternosController(
        ApplicationDbContext context,
        DisponibilidadService disponibilidad,
        SolicitudEstadoService estados)
    {
        _context = context;
        _disponibilidad = disponibilidad;
        _estados = estados;
    }

    public IActionResult Index()
    {
        return RedirectToAction(
            nameof(CoordinacionController.Index),
            "Coordinacion",
            new { vista = "todas", tipo = TipoEvento.Externo });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new EventoExternoViewModel();
        await CargarSalones(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventoExternoViewModel model)
    {
        await Validar(model);

        if (!ModelState.IsValid)
        {
            await CargarSalones(model);
            return View("Form", model);
        }

        var usuarioId = User.ObtenerUsuarioId();

        var departamentoId = await _context.Usuarios
            .Where(x => x.Id == usuarioId)
            .Select(x => x.DepartamentoId)
            .FirstAsync();

        var evento = new SolicitudEvento
        {
            Codigo = SolicitudEstadoService.GenerarCodigo(TipoEvento.Externo),
            TipoEvento = TipoEvento.Externo,
            SolicitanteUsuarioId = usuarioId,
            ResponsableUsuarioId = usuarioId,
            DepartamentoCargoId = departamentoId,
            FechaCreacion = DateTimeOffset.UtcNow
        };

        Aplicar(evento, model);

        await _estados.CambiarEstadoAsync(
            evento,
            Estados.Aprobada,
            $"{User.ObtenerNombre()} (Coordinación)",
            "Evento externo registrado como reserva confirmada.");

        _context.SolicitudesEvento.Add(evento);

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = $"Evento externo {evento.Codigo} registrado.";

        return RedirectToAction(
            nameof(CoordinacionController.Details),
            "Coordinacion",
            new { id = evento.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var evento = await ObtenerEditableAsync(id);

        if (evento == null)
            return NotFound();

        var model = new EventoExternoViewModel
        {
            Id = evento.Id,
            Codigo = evento.Codigo,
            NombreEvento = evento.NombreEvento,
            ClienteNombre = evento.ClienteNombre ?? "",
            ClienteContacto = evento.ClienteContacto,
            ClienteTelefono = evento.ClienteTelefono,
            ClienteCorreo = evento.ClienteCorreo,
            ReferenciaSap = evento.ReferenciaSap,
            FechaInicio = DateOnly.FromDateTime(evento.FechaHoraInicio),
            HoraInicio = TimeOnly.FromDateTime(evento.FechaHoraInicio),
            FechaFin = DateOnly.FromDateTime(evento.FechaHoraFin),
            HoraFin = TimeOnly.FromDateTime(evento.FechaHoraFin),
            CantidadPersonas = evento.CantidadPersonas,
            SalonId = evento.SalonId,
            Otros = evento.Otros,
            Comentarios = evento.Comentarios
        };

        await CargarSalones(model);

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventoExternoViewModel model)
    {
        var evento = await ObtenerEditableAsync(model.Id ?? 0);

        if (evento == null)
            return NotFound();

        await Validar(model);

        if (!ModelState.IsValid)
        {
            model.Codigo = evento.Codigo;
            await CargarSalones(model);
            return View("Form", model);
        }

        Aplicar(evento, model);

        evento.HistorialEstados.Add(new HistorialEstadoSolicitud
        {
            EstadoSolicitudId = evento.EstadoId,
            FechaCambio = DateTimeOffset.UtcNow,
            Actor = $"{User.ObtenerNombre()} (Coordinación)",
            Comentario = "Datos del evento externo actualizados."
        });

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Evento externo actualizado.";

        return RedirectToAction(
            nameof(CoordinacionController.Details),
            "Coordinacion",
            new { id = evento.Id });
    }

    private async Task<SolicitudEvento?> ObtenerEditableAsync(int id)
    {
        return await _context.SolicitudesEvento
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x =>
                x.Id == id
                && x.TipoEvento == TipoEvento.Externo
                && x.Estado.Nombre == Estados.Aprobada);
    }

    private async Task Validar(EventoExternoViewModel model)
    {
        if (model.Fin <= model.Inicio)
        {
            ModelState.AddModelError(
                nameof(model.FechaFin),
                "La fecha y hora de finalización debe ser posterior al inicio.");
        }

        var referencia = model.ReferenciaSap?.Trim();

        if (!string.IsNullOrEmpty(referencia))
        {
            var duplicado = await _context.SolicitudesEvento
                .AnyAsync(x =>
                    x.ReferenciaSap == referencia
                    && x.Id != (model.Id ?? 0));

            if (duplicado)
            {
                ModelState.AddModelError(
                    nameof(model.ReferenciaSap),
                    "Ya existe un evento con esta referencia SAP.");
            }
        }

        if (ModelState.IsValid)
        {
            await _disponibilidad.ValidarAsync(
                ModelState,
                model.SalonId,
                model.Inicio,
                model.Fin,
                model.CantidadPersonas,
                model.Id);
        }
    }

    private static void Aplicar(SolicitudEvento evento, EventoExternoViewModel model)
    {
        evento.NombreEvento = model.NombreEvento.Trim();
        evento.ClienteNombre = model.ClienteNombre.Trim();
        evento.ClienteContacto = model.ClienteContacto?.Trim();
        evento.ClienteTelefono = model.ClienteTelefono?.Trim();
        evento.ClienteCorreo = model.ClienteCorreo?.Trim();
        evento.ReferenciaSap = string.IsNullOrWhiteSpace(model.ReferenciaSap)
            ? null
            : model.ReferenciaSap.Trim();
        evento.FechaHoraInicio = model.Inicio;
        evento.FechaHoraFin = model.Fin;
        evento.CantidadPersonas = model.CantidadPersonas;
        evento.SalonId = model.SalonId;
        evento.Otros = model.Otros;
        evento.Comentarios = model.Comentarios;
        evento.FechaActualizacion = DateTimeOffset.UtcNow;
    }

    private async Task CargarSalones(EventoExternoViewModel model)
    {
        model.Salones = await _context.Salones
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Nombre} (hasta {x.CapacidadMaxima} personas)"
            })
            .ToListAsync();
    }
}
