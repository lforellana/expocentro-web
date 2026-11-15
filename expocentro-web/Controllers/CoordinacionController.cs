using Expocentro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Expocentro.Web.Models;
using Expocentro.Web.ViewModels;

namespace Expocentro.Web.Controllers;

public class CoordinacionController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoordinacionController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // PANEL DEL COORDINADOR
    public async Task<IActionResult> Index()
    {
        var solicitudes = await _context.SolicitudesEvento
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .Include(x => x.SolicitanteUsuario)
            .Include(x => x.DepartamentoCargo)
            .Where(x =>
                x.Estado.Nombre != "Borrador")
            .OrderByDescending(x => x.FechaActualizacion)
            .ToListAsync();

        return View(solicitudes);
    }

    // DETALLE
    public async Task<IActionResult> Details(int id)
    {
        var solicitud = await _context.SolicitudesEvento
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .Include(x => x.SolicitanteUsuario)
            .Include(x => x.ResponsableUsuario)
            .Include(x => x.DepartamentoCargo)
            .Include(x => x.Montajes)
                .ThenInclude(x => x.TipoMontaje)
            .Include(x => x.Servicios)
                .ThenInclude(x => x.Servicio)
            .Include(x => x.Alimentacion)
            .Include(x => x.HistorialEstados)
                .ThenInclude(x => x.EstadoSolicitud)
            .FirstOrDefaultAsync(x => x.Id == id);
            



        if (solicitud == null)
        {
            return NotFound();
        }

        return View(solicitud);
    }

    // PASAR A REVISIÓN
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revisar(int id)
    {
        var solicitud = await _context.SolicitudesEvento
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        if (solicitud.Estado.Nombre != "Enviada")
        {
            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var nuevoEstado = await _context.EstadosSolicitud
            .FirstAsync(x => x.Nombre == "En revisión");

        var ahora = DateTimeOffset.UtcNow;

solicitud.EstadoId = nuevoEstado.Id;
solicitud.FechaActualizacion = ahora;

_context.HistorialEstadosSolicitud.Add(
    new HistorialEstadoSolicitud
    {
        SolicitudEventoId = solicitud.Id,
        EstadoSolicitudId = nuevoEstado.Id,
        FechaCambio = ahora,
        Actor = "Coordinación",
        Comentario = "La solicitud fue tomada para revisión."
    });

await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La solicitud está ahora en revisión.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // APROBAR
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await _context.SolicitudesEvento
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        if (solicitud.Estado.Nombre != "En revisión")
        {
            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var nuevoEstado = await _context.EstadosSolicitud
            .FirstAsync(x => x.Nombre == "Aprobada");

        var ahora = DateTimeOffset.UtcNow;

solicitud.EstadoId = nuevoEstado.Id;
solicitud.FechaActualizacion = ahora;

_context.HistorialEstadosSolicitud.Add(
    new HistorialEstadoSolicitud
    {
        SolicitudEventoId = solicitud.Id,
        EstadoSolicitudId = nuevoEstado.Id,
        FechaCambio = ahora,
        Actor = "Coordinación",
        Comentario = "Solicitud aprobada."
    });

await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La solicitud fue aprobada.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // RECHAZAR
    // FORMULARIO DE RECHAZO
[HttpGet]
public async Task<IActionResult> Rechazar(int id)
{
    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (solicitud == null)
        return NotFound();

    if (solicitud.Estado.Nombre != "En revisión")
        return RedirectToAction(nameof(Details), new { id });

    var model = new RechazarSolicitudViewModel
    {
        SolicitudEventoId = solicitud.Id,
        Codigo = solicitud.Codigo,
        NombreEvento = solicitud.NombreEvento
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Rechazar(
    RechazarSolicitudViewModel model)
{
    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .FirstOrDefaultAsync(
            x => x.Id == model.SolicitudEventoId);

    if (solicitud == null)
        return NotFound();

    if (solicitud.Estado.Nombre != "En revisión")
        return RedirectToAction(
            nameof(Details),
            new { id = solicitud.Id });

    if (!ModelState.IsValid)
    {
        model.Codigo = solicitud.Codigo;
        model.NombreEvento = solicitud.NombreEvento;

        return View(model);
    }

    var nuevoEstado = await _context.EstadosSolicitud
        .FirstAsync(x => x.Nombre == "Rechazada");

    var ahora = DateTimeOffset.UtcNow;

    solicitud.EstadoId = nuevoEstado.Id;
    solicitud.FechaActualizacion = ahora;

    _context.HistorialEstadosSolicitud.Add(
        new HistorialEstadoSolicitud
        {
            SolicitudEventoId = solicitud.Id,
            EstadoSolicitudId = nuevoEstado.Id,
            FechaCambio = ahora,
            Actor = "Coordinación",
            Comentario = model.Motivo.Trim()
        });

    await _context.SaveChangesAsync();

    TempData["Mensaje"] =
        "La solicitud fue rechazada.";

    return RedirectToAction(
        nameof(Details),
        new { id = solicitud.Id });
}



}