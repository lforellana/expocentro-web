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
/// Bandeja de Coordinación: revisión, aprobación, rechazo y
/// cancelación de eventos internos y externos.
/// </summary>
[Authorize(Policy = Politicas.Coordinacion)]
public class CoordinacionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DisponibilidadService _disponibilidad;
    private readonly SolicitudEstadoService _estados;

    public CoordinacionController(
        ApplicationDbContext context,
        DisponibilidadService disponibilidad,
        SolicitudEstadoService estados)
    {
        _context = context;
        _disponibilidad = disponibilidad;
        _estados = estados;
    }

    private string Actor => $"{User.ObtenerNombre()} (Coordinación)";

    // ============================
    // BANDEJA
    // ============================

    public async Task<IActionResult> Index(BandejaCoordinacionViewModel filtros)
    {
        var baseQuery = _context.SolicitudesEvento
            .Where(x => x.Estado.Nombre != Estados.Borrador);

        // Filtros comunes (no dependen de la pestaña)
        if (filtros.Tipo.HasValue)
            baseQuery = baseQuery.Where(x => x.TipoEvento == filtros.Tipo);

        if (filtros.SalonId.HasValue)
            baseQuery = baseQuery.Where(x => x.SalonId == filtros.SalonId);

        if (filtros.Desde.HasValue)
        {
            var desde = filtros.Desde.Value.ToDateTime(TimeOnly.MinValue);
            baseQuery = baseQuery.Where(x => x.FechaHoraFin >= desde);
        }

        if (filtros.Hasta.HasValue)
        {
            var hasta = filtros.Hasta.Value.ToDateTime(TimeOnly.MaxValue);
            baseQuery = baseQuery.Where(x => x.FechaHoraInicio <= hasta);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Texto))
        {
            var texto = $"%{filtros.Texto.Trim()}%";

            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.NombreEvento, texto)
                || EF.Functions.ILike(x.Codigo, texto)
                || EF.Functions.ILike(x.SolicitanteUsuario.Nombre, texto)
                || (x.ClienteNombre != null && EF.Functions.ILike(x.ClienteNombre, texto))
                || (x.ReferenciaSap != null && EF.Functions.ILike(x.ReferenciaSap, texto)));
        }

        // Conteos por pestaña
        var porEstado = await baseQuery
            .GroupBy(x => x.Estado.Nombre)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Total);

        int Contar(params string[] estados) =>
            estados.Sum(e => porEstado.GetValueOrDefault(e));

        filtros.Conteos = new Dictionary<string, int>
        {
            ["pendientes"] = Contar(Estados.Enviada, Estados.EnRevision),
            ["aprobadas"] = Contar(Estados.Aprobada),
            ["rechazadas"] = Contar(Estados.Rechazada),
            ["canceladas"] = Contar(Estados.Cancelada),
            ["todas"] = porEstado.Values.Sum()
        };

        var query = filtros.Vista switch
        {
            "aprobadas" => baseQuery.Where(x => x.Estado.Nombre == Estados.Aprobada),
            "rechazadas" => baseQuery.Where(x => x.Estado.Nombre == Estados.Rechazada),
            "canceladas" => baseQuery.Where(x => x.Estado.Nombre == Estados.Cancelada),
            "todas" => baseQuery,
            _ => baseQuery.Where(x =>
                x.Estado.Nombre == Estados.Enviada
                || x.Estado.Nombre == Estados.EnRevision)
        };

        // Pendientes: el evento más próximo primero
        query = filtros.Vista is "pendientes" or "aprobadas" or null or ""
            ? query.OrderBy(x => x.FechaHoraInicio)
            : query.OrderByDescending(x => x.FechaActualizacion);

        filtros.Solicitudes = await query
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .Include(x => x.SolicitanteUsuario)
            .Include(x => x.DepartamentoCargo)
            .Take(300)
            .ToListAsync();

        filtros.Salones = await ListaSalones();

        return View(filtros);
    }

    // ============================
    // DETALLE
    // ============================

    public async Task<IActionResult> Details(int id)
    {
        var solicitud = await _context.SolicitudesEvento
            .ConDetalle()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        ViewData["Disponibilidad"] = await _disponibilidad.VerificarAsync(
            solicitud.SalonId,
            solicitud.FechaHoraInicio,
            solicitud.FechaHoraFin,
            solicitud.Id);

        return View(solicitud);
    }

    // ============================
    // TRANSICIONES DE ESTADO
    // ============================

    private async Task<SolicitudEvento?> ObtenerAsync(int id)
    {
        return await _context.SolicitudesEvento
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revisar(int id)
    {
        var solicitud = await ObtenerAsync(id);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado.Nombre == Estados.Enviada)
        {
            await _estados.CambiarEstadoAsync(
                solicitud,
                Estados.EnRevision,
                Actor,
                "La solicitud fue tomada para revisión.");

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La solicitud está ahora en revisión.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id, string? comentario)
    {
        var solicitud = await ObtenerAsync(id);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado.Nombre is not (Estados.Enviada or Estados.EnRevision))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        // No se puede aprobar si el salón ya está reservado
        var disponibilidad = await _disponibilidad.VerificarAsync(
            solicitud.SalonId,
            solicitud.FechaHoraInicio,
            solicitud.FechaHoraFin,
            solicitud.Id);

        if (disponibilidad.HayConflictoConfirmado)
        {
            var conflicto = disponibilidad.Confirmados.First();

            TempData["Error"] =
                $"No se puede aprobar: el salón ya está reservado para " +
                $"\"{conflicto.NombreEvento}\" ({conflicto.Codigo}). " +
                "Rechace la solicitud o coordine otro horario con el solicitante.";

            return RedirectToAction(nameof(Details), new { id });
        }

        await _estados.CambiarEstadoAsync(
            solicitud,
            Estados.Aprobada,
            Actor,
            string.IsNullOrWhiteSpace(comentario)
                ? "Solicitud aprobada."
                : comentario.Trim());

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La solicitud fue aprobada y el salón quedó reservado.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Rechazar(int id)
    {
        var solicitud = await ObtenerAsync(id);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado.Nombre is not (Estados.Enviada or Estados.EnRevision))
            return RedirectToAction(nameof(Details), new { id });

        return View(new RechazarSolicitudViewModel
        {
            SolicitudEventoId = solicitud.Id,
            Codigo = solicitud.Codigo,
            NombreEvento = solicitud.NombreEvento
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(RechazarSolicitudViewModel model)
    {
        var solicitud = await ObtenerAsync(model.SolicitudEventoId);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado.Nombre is not (Estados.Enviada or Estados.EnRevision))
            return RedirectToAction(nameof(Details), new { id = solicitud.Id });

        if (!ModelState.IsValid)
        {
            model.Codigo = solicitud.Codigo;
            model.NombreEvento = solicitud.NombreEvento;
            return View(model);
        }

        await _estados.CambiarEstadoAsync(
            solicitud,
            Estados.Rechazada,
            Actor,
            model.Motivo.Trim());

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La solicitud fue rechazada.";

        return RedirectToAction(nameof(Details), new { id = solicitud.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string? motivo)
    {
        var solicitud = await ObtenerAsync(id);

        if (solicitud == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(motivo))
        {
            TempData["Error"] = "Indique el motivo de la cancelación.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!Estados.Cancelables.Contains(solicitud.Estado.Nombre))
        {
            TempData["Error"] = "Esta solicitud ya no puede cancelarse.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _estados.CambiarEstadoAsync(
            solicitud,
            Estados.Cancelada,
            Actor,
            motivo.Trim());

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "El evento fue cancelado y el salón quedó liberado.";

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<List<SelectListItem>> ListaSalones()
    {
        return await _context.Salones
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem(x.Nombre, x.Id.ToString()))
            .ToListAsync();
    }
}
