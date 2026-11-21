using System.Security.Claims;
using Expocentro.Web.Data;
using Expocentro.Web.Seguridad;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

[AllowAnonymous]
public class CuentaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ModoAutenticacion _modo;
    private readonly UsuarioSincronizacionService _usuarios;

    public CuentaController(
        ApplicationDbContext context,
        ModoAutenticacion modo,
        UsuarioSincronizacionService usuarios)
    {
        _context = context;
        _modo = modo;
        _usuarios = usuarios;
    }

    // INICIAR SESIÓN (redirige a Entra ID o al acceso de desarrollo)
    [HttpGet]
    public IActionResult Entrar(string? returnUrl = null)
    {
        var destino = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

        if (_modo.EsDesarrollo)
        {
            return RedirectToAction(
                nameof(AccesoDesarrollo),
                new { returnUrl = destino });
        }

        return Challenge(
            new AuthenticationProperties { RedirectUri = destino },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    // CERRAR SESIÓN
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Salir()
    {
        var propiedades = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(SesionCerrada))
        };

        if (_modo.EsEntraId)
        {
            return SignOut(
                propiedades,
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        return SignOut(
            propiedades,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult SesionCerrada()
    {
        return View();
    }

    // Error al volver del inicio de sesión de Microsoft
    [HttpGet]
    public IActionResult ErrorInicio(string? motivo = null)
    {
        ViewData["Desactivado"] = motivo == "desactivado";
        return View();
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    // ACCESO DE DESARROLLO (solo cuando Entra ID no está configurado)
    [HttpGet]
    public async Task<IActionResult> AccesoDesarrollo(string? returnUrl = null)
    {
        if (!_modo.EsDesarrollo)
        {
            return NotFound();
        }

        ViewData["ReturnUrl"] = returnUrl;

        var usuarios = await _context.Usuarios
            .Include(x => x.Departamento)
            .Where(x => x.Activo)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return View(usuarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AccesoDesarrollo(
        int usuarioId,
        string? returnUrl = null)
    {
        if (!_modo.EsDesarrollo)
        {
            return NotFound();
        }

        var identidad = await _usuarios.IdentidadDesarrolloAsync(
            usuarioId,
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (identidad == null)
        {
            return RedirectToAction(nameof(AccesoDesarrollo));
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}
