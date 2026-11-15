using System.ComponentModel.DataAnnotations;

namespace Expocentro.Web.ViewModels;

public class SolicitudAlimentacionViewModel
{
    public int SolicitudEventoId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string NombreEvento { get; set; } = string.Empty;

    [Required(ErrorMessage =
        "Debe indicar si requiere alimentación.")]
    public bool? RequiereAlimentacion { get; set; }

    public int? CantidadPersonas { get; set; }

    public TimeOnly? HoraServicio { get; set; }

    public string? Detalle { get; set; }

    public bool IncluyeAgua { get; set; }

    public bool IncluyeCafe { get; set; }

    public bool IncluyeRefrigerio { get; set; }

    public string? Observaciones { get; set; }
}