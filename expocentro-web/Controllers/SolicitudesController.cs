using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SolicitudesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // MIS SOLICITUDES
    public async Task<IActionResult> Index()
    {
        const int usuarioDemoId = 1;

        var solicitudes = await _context.SolicitudesEvento
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .Where(x => x.SolicitanteUsuarioId == usuarioDemoId)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync();

        return View(solicitudes);
    }

    // NUEVA SOLICITUD
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SolicitudEventoCreateViewModel();

        await CargarSalones(model);

        return View(model);
    }

    // GUARDAR SOLICITUD
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SolicitudEventoCreateViewModel model)
    {
        var inicio =
            model.FechaInicio.ToDateTime(model.HoraInicio);

        var fin =
            model.FechaFin.ToDateTime(model.HoraFin);

        if (fin <= inicio)
        {
            ModelState.AddModelError(
                nameof(model.FechaFin),
                "La fecha y hora de finalización debe ser posterior al inicio.");
        }

        if (!ModelState.IsValid)
        {
            await CargarSalones(model);
            return View(model);
        }

        const int usuarioDemoId = 1;

        var usuario = await _context.Usuarios
            .FirstAsync(x => x.Id == usuarioDemoId);

        var estadoBorrador = await _context.EstadosSolicitud
            .FirstAsync(x => x.Nombre == "Borrador");

        var solicitud = new SolicitudEvento
        {
            Codigo =
                $"EV-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",

            NombreEvento = model.NombreEvento,

            FechaHoraInicio = inicio,
            FechaHoraFin = fin,

            CantidadPersonas = model.CantidadPersonas,

            SolicitanteUsuarioId = usuario.Id,

            ResponsableUsuarioId = usuario.Id,

            DepartamentoCargoId = usuario.DepartamentoId,

            SalonId = model.SalonId,

            EstadoId = estadoBorrador.Id,

            Otros = model.Otros,

            Comentarios = model.Comentarios,

            FechaCreacion = DateTimeOffset.UtcNow,

            FechaActualizacion = DateTimeOffset.UtcNow
        };

        _context.SolicitudesEvento.Add(solicitud);

        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La solicitud fue registrada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarSalones(
        SolicitudEventoCreateViewModel model)
    {
        model.Salones = await _context.Salones
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Nombre
            })
            .ToListAsync();
    }
}