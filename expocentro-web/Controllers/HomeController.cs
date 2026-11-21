using System.Diagnostics;
using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.Seguridad;
using Expocentro.Web.Services;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

    // PANEL PRINCIPAL DEL EMPLEADO
    public async Task<IActionResult> Index()
    {
        var usuarioId = User.ObtenerUsuarioId();

        var mias = _context.SolicitudesEvento
            .Where(x => x.SolicitanteUsuarioId == usuarioId
                && x.TipoEvento == TipoEvento.Interno);

        var conteos = await mias
            .GroupBy(x => x.Estado.Nombre)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Total);

        var model = new HomeDashboardViewModel
        {
            NombreUsuario = User.ObtenerNombre(),
            TotalSolicitudes = conteos.Values.Sum(),
            TotalBorradores = conteos.GetValueOrDefault(Estados.Borrador),
            TotalPendientes =
                conteos.GetValueOrDefault(Estados.Enviada)
                + conteos.GetValueOrDefault(Estados.EnRevision),
            TotalAprobadas = conteos.GetValueOrDefault(Estados.Aprobada),
            SolicitudesRecientes = await mias
                .Include(x => x.Salon)
                .Include(x => x.Estado)
                .OrderByDescending(x => x.FechaActualizacion)
                .Take(5)
                .ToListAsync(),
            EsCoordinador = User.EsCoordinador()
        };

        if (model.EsCoordinador)
        {
            model.PorRevisar = await _context.SolicitudesEvento
                .CountAsync(x => x.Estado.Nombre == Estados.Enviada);

            model.EnRevision = await _context.SolicitudesEvento
                .CountAsync(x => x.Estado.Nombre == Estados.EnRevision);

            var ahora = Fechas.Ahora();
            var limite = ahora.Date.AddDays(8);

            model.ProximosEventos = await _context.SolicitudesEvento
                .Include(x => x.Salon)
                .Include(x => x.Estado)
                .Where(x => x.Estado.Nombre == Estados.Aprobada
                    && x.FechaHoraFin >= ahora
                    && x.FechaHoraInicio < limite)
                .OrderBy(x => x.FechaHoraInicio)
                .Take(8)
                .ToListAsync();
        }

        return View(model);
    }

    // PÁGINAS PÚBLICAS

    [AllowAnonymous]
    [HttpGet("/espacios")]
    public IActionResult Espacios()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet("/servicios")]
    public IActionResult Servicios()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet("/como-llegar")]
    public IActionResult ComoLlegar()
    {
        return View();
    }

    [AllowAnonymous]
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
