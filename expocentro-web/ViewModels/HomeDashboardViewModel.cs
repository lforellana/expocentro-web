using Expocentro.Web.Models;

namespace Expocentro.Web.ViewModels;

public class HomeDashboardViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;

    public int TotalSolicitudes { get; set; }

    public int TotalBorradores { get; set; }

    public List<SolicitudEvento> SolicitudesRecientes { get; set; }
        = new();
}