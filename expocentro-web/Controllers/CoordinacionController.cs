using Expocentro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        solicitud.EstadoId = nuevoEstado.Id;
        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

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

        solicitud.EstadoId = nuevoEstado.Id;
        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La solicitud fue aprobada.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // RECHAZAR
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id)
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
            .FirstAsync(x => x.Nombre == "Rechazada");

        solicitud.EstadoId = nuevoEstado.Id;
        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La solicitud fue rechazada.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}