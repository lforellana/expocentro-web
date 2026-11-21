using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expocentro.Web.Controllers;

[AllowAnonymous]
public class CotizacionController : Controller
{
    [HttpGet("/solicitar-cotizacion")]
    public IActionResult Index()
    {
        return View();
    }
}