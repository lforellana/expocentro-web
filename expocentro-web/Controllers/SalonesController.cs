using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.Seguridad;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

/// <summary>
/// Catálogo de salones: nombre, capacidad, salones combinados y activo.
/// </summary>
[Authorize(Policy = Politicas.Administracion)]
public class SalonesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SalonesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var salones = await _context.Salones
            .AsNoTracking()
            .Include(x => x.SalonPadre)
            .Include(x => x.Subsalones)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return View(salones);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SalonEditViewModel();
        await CargarListas(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SalonEditViewModel model)
    {
        model.Id = null;
        await Validar(model);

        if (!ModelState.IsValid)
        {
            await CargarListas(model);
            return View("Form", model);
        }

        var salon = new Salon();
        Aplicar(model, salon);
        _context.Salones.Add(salon);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = $"Salón {salon.Nombre} creado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var salon = await _context.Salones
            .Include(x => x.Subsalones)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (salon == null)
            return NotFound();

        var model = new SalonEditViewModel
        {
            Id = salon.Id,
            Nombre = salon.Nombre,
            Descripcion = salon.Descripcion,
            CapacidadMaxima = salon.CapacidadMaxima,
            SalonPadreId = salon.SalonPadreId,
            Activo = salon.Activo
        };

        await CargarListas(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SalonEditViewModel model)
    {
        var salon = await _context.Salones.FindAsync(id);

        if (salon == null)
            return NotFound();

        model.Id = id;
        await Validar(model);

        if (!ModelState.IsValid)
        {
            await CargarListas(model);
            return View("Form", model);
        }

        Aplicar(model, salon);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = $"Salón {salon.Nombre} actualizado.";
        return RedirectToAction(nameof(Index));
    }

    private static void Aplicar(SalonEditViewModel model, Salon salon)
    {
        salon.Nombre = model.Nombre.Trim();
        salon.Descripcion = string.IsNullOrWhiteSpace(model.Descripcion)
            ? null
            : model.Descripcion.Trim();
        salon.CapacidadMaxima = model.CapacidadMaxima;
        salon.SalonPadreId = model.SalonPadreId;
        salon.Activo = model.Activo;
    }

    private async Task Validar(SalonEditViewModel model)
    {
        var nombre = model.Nombre?.Trim() ?? string.Empty;

        if (await _context.Salones.AnyAsync(x =>
                x.Id != (model.Id ?? 0)
                && x.Nombre.ToLower() == nombre.ToLower()))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un salón con ese nombre.");
        }

        if (model.SalonPadreId is not int padreId)
            return;

        // Solo un nivel: el combinado no puede ser parte de otro, y un salón
        // que ya tiene partes no puede ser parte de otro.
        var padre = await _context.Salones.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == padreId);

        var tienePartes = model.Id is int id
            && await _context.Salones.AnyAsync(x => x.SalonPadreId == id);

        if (padre == null || padre.Id == model.Id || padre.SalonPadreId != null || tienePartes)
        {
            // Clave vacía: el selector no se muestra en salones con partes
            ModelState.AddModelError(
                string.Empty,
                "Seleccione un salón combinado válido (un salón con partes no puede ser parte de otro).");
        }
    }

    private async Task CargarListas(SalonEditViewModel model)
    {
        model.SalonesPadre = await _context.Salones
            .AsNoTracking()
            .Where(x => x.SalonPadreId == null && x.Id != (model.Id ?? 0))
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem(x.Nombre, x.Id.ToString()))
            .ToListAsync();

        if (model.Id is int id)
        {
            model.Subsalones = await _context.Salones
                .Where(x => x.SalonPadreId == id)
                .OrderBy(x => x.Nombre)
                .Select(x => x.Nombre)
                .ToListAsync();
        }
    }
}
