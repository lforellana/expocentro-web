using System.ComponentModel.DataAnnotations;

namespace Expocentro.Web.ViewModels;

public class RechazarSolicitudViewModel
{
    public int SolicitudEventoId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string NombreEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe indicar el motivo del rechazo.")]
    [StringLength(500)]
    [Display(Name = "Motivo del rechazo")]
    public string Motivo { get; set; } = string.Empty;
}