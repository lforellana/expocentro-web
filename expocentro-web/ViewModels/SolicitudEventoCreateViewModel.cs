using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expocentro.Web.ViewModels;

public class SolicitudEventoCreateViewModel
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [Display(Name = "Nombre del evento")]
    public string NombreEvento { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateOnly FechaInicio { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de inicio")]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de finalización")]
    public DateOnly FechaFin { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de finalización")]
    public TimeOnly HoraFin { get; set; }

    [Range(1, 10000,
        ErrorMessage = "La cantidad de personas debe ser mayor que cero.")]
    [Display(Name = "Cantidad de personas")]
    public int CantidadPersonas { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un salón.")]
    [Display(Name = "Salón")]
    public int SalonId { get; set; }

    [Display(Name = "Otros requerimientos")]
    public string? Otros { get; set; }

    [Display(Name = "Comentarios")]
    public string? Comentarios { get; set; }

    public IEnumerable<SelectListItem> Salones { get; set; }
        = Enumerable.Empty<SelectListItem>();
}