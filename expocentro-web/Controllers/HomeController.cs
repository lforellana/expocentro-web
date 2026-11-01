using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Expocentro.Web.Models;


namespace Expocentro.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
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


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
