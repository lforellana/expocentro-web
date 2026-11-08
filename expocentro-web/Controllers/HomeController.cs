using System.Diagnostics;
using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Temporal hasta implementar Microsoft Entra ID
        const int usuarioDemoId = 1;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioDemoId);

        var solicitudesUsuario = _context.SolicitudesEvento
            .Where(x => x.SolicitanteUsuarioId == usuarioDemoId);

        var model = new HomeDashboardViewModel
        {
            NombreUsuario = usuario?.Nombre ?? "Usuario",

            TotalSolicitudes =
                await solicitudesUsuario.CountAsync(),

            TotalBorradores =
                await solicitudesUsuario
                    .CountAsync(x => x.Estado.Nombre == "Borrador"),

            SolicitudesRecientes =
                await solicitudesUsuario
                    .Include(x => x.Salon)
                    .Include(x => x.Estado)
                    .OrderByDescending(x => x.FechaCreacion)
                    .Take(5)
                    .ToListAsync()
        };

        return View(model);
    }

    [HttpGet("/espacios")]
    public IActionResult Espacios()
    {
        return View();
    }

    [HttpGet("/servicios")]
    public IActionResult Servicios()
    {
        return View();
    }

    [HttpGet("/como-llegar")]
    public IActionResult ComoLlegar()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId =
                Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}