using Expocentro.Web.Data;
using Expocentro.Web.Seguridad;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

/// <summary>
/// Administración de usuarios: asignar departamento y rol.
/// Los usuarios se crean automáticamente al iniciar sesión con Entra ID.
/// </summary>
[Authorize(Policy = Politicas.Administracion)]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? texto)
    {
        var query = _context.Usuarios
            .Include(x => x.Departamento)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var patron = $"%{texto.Trim()}%";

            query = query.Where(x =>
                EF.Functions.ILike(x.Nombre, patron)
                || EF.Functions.ILike(x.Correo, patron));
        }

        ViewData["Texto"] = texto;

        var usuarios = await query
            .OrderBy(x => x.Departamento.Nombre == "Sin asignar" ? 0 : 1)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        return View(usuarios);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound();

        var model = new UsuarioEditViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Rol = usuario.Rol,
            DepartamentoId = usuario.DepartamentoId,
            Activo = usuario.Activo
        };

        await CargarDepartamentos(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UsuarioEditViewModel model)
    {
        var usuario = await _context.Usuarios.FindAsync(model.Id);

        if (usuario == null)
            return NotFound();

        if (!Roles.Todos.Contains(model.Rol))
        {
            ModelState.AddModelError(nameof(model.Rol), "Rol no válido.");
        }

        if (usuario.Id == User.ObtenerUsuarioId()
            && (!model.Activo || model.Rol != Roles.Administrador))
        {
            ModelState.AddModelError(
                string.Empty,
                "No puede quitarse a sí mismo el rol de administrador ni desactivarse.");
        }

        if (!ModelState.IsValid)
        {
            model.Nombre = usuario.Nombre;
            model.Correo = usuario.Correo;
            await CargarDepartamentos(model);
            return View(model);
        }

        usuario.Rol = model.Rol;
        usuario.DepartamentoId = model.DepartamentoId;
        usuario.Activo = model.Activo;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            $"Usuario {usuario.Nombre} actualizado. Los cambios aplican en su próximo inicio de sesión.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarDepartamentos(UsuarioEditViewModel model)
    {
        model.Departamentos = await _context.Departamentos
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem(x.Nombre, x.Id.ToString()))
            .ToListAsync();
    }
}
