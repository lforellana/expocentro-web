using Expocentro.Web.Models;

namespace Expocentro.Web.ViewModels;

public class HomeDashboardViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;

    public int TotalSolicitudes { get; set; }

    public int TotalBorradores { get; set; }

    public int TotalPendientes { get; set; }

    public int TotalAprobadas { get; set; }

    public List<SolicitudEvento> SolicitudesRecientes { get; set; }
        = new();

    // Solo para Coordinación
    public bool EsCoordinador { get; set; }

    public int PorRevisar { get; set; }

    public int EnRevision { get; set; }

    public List<SolicitudEvento> ProximosEventos { get; set; }
        = new();
}
