using System.ComponentModel.DataAnnotations;

namespace Expocentro.Web.ViewModels;

public class SolicitudServicioViewModel
{
    public int SolicitudEventoId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string NombreEvento { get; set; } = string.Empty;

    public List<ServicioItemViewModel> Servicios { get; set; } = new();
}

public class ServicioItemViewModel
{
    public int ServicioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Seleccionado { get; set; }

    [Range(1, 1000)]
    public int Cantidad { get; set; } = 1;

    public string? Observaciones { get; set; }
}