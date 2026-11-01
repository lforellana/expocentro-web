using Microsoft.AspNetCore.Mvc;

namespace Expocentro.Web.Controllers;

public class CotizacionController : Controller
{
    [HttpGet("/solicitar-cotizacion")]
    public IActionResult Index()
    {
        return View();
    }
}