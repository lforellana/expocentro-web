using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expocentro.Web.ViewModels;

/// <summary>
/// Datos generales de una solicitud interna (crear y editar).
/// </summary>
public class SolicitudEventoCreateViewModel
{
    /// <summary>Null al crear; Id de la solicitud al editar.</summary>
    public int? Id { get; set; }

    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre del evento")]
    public string NombreEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique la fecha de inicio.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateOnly FechaInicio { get; set; } =
        DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Required(ErrorMessage = "Indique la hora de inicio.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de inicio")]
    public TimeOnly HoraInicio { get; set; } = new(8, 0);

    [Required(ErrorMessage = "Indique la fecha de finalización.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de finalización")]
    public DateOnly FechaFin { get; set; } =
        DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Required(ErrorMessage = "Indique la hora de finalización.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de finalización")]
    public TimeOnly HoraFin { get; set; } = new(12, 0);

    [Range(1, 10000,
        ErrorMessage = "La cantidad de personas debe ser mayor que cero.")]
    [Display(Name = "Cantidad de personas")]
    public int CantidadPersonas { get; set; } = 1;

    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar un salón.")]
    [Display(Name = "Salón")]
    public int SalonId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Seleccione el departamento a cargo.")]
    [Display(Name = "Departamento a cargo")]
    public int DepartamentoCargoId { get; set; }

    [Display(Name = "Otros requerimientos")]
    public string? Otros { get; set; }

    [Display(Name = "Comentarios")]
    public string? Comentarios { get; set; }

    public bool EsEdicion => Id.HasValue;

    public DateTime Inicio => FechaInicio.ToDateTime(HoraInicio);

    public DateTime Fin => FechaFin.ToDateTime(HoraFin);

    public IEnumerable<SelectListItem> Salones { get; set; }
        = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Departamentos { get; set; }
        = Enumerable.Empty<SelectListItem>();
}
