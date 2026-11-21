using Expocentro.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expocentro.Web.ViewModels;

public class BandejaCoordinacionViewModel
{
    // Filtros
    public string Vista { get; set; } = "pendientes";

    public TipoEvento? Tipo { get; set; }

    public int? SalonId { get; set; }

    public string? Texto { get; set; }

    public DateOnly? Desde { get; set; }

    public DateOnly? Hasta { get; set; }

    // Contadores por vista
    public Dictionary<string, int> Conteos { get; set; } = new();

    public List<SolicitudEvento> Solicitudes { get; set; } = new();

    public IEnumerable<SelectListItem> Salones { get; set; }
        = Enumerable.Empty<SelectListItem>();

    public static readonly (string Clave, string Texto)[] Vistas =
    {
        ("pendientes", "Por atender"),
        ("aprobadas", "Aprobadas"),
        ("rechazadas", "Rechazadas"),
        ("canceladas", "Canceladas"),
        ("todas", "Todas")
    };
}
