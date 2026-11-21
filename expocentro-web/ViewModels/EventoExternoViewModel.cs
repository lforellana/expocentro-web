using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expocentro.Web.ViewModels;

/// <summary>
/// Registro manual de un evento externo (cliente). En una fase
/// posterior estos datos podrán provenir de la cotización/pedido SAP.
/// </summary>
public class EventoExternoViewModel
{
    public int? Id { get; set; }

    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre del evento")]
    public string NombreEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique el cliente o empresa.")]
    [StringLength(200)]
    [Display(Name = "Cliente / empresa")]
    public string ClienteNombre { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Persona de contacto")]
    public string? ClienteContacto { get; set; }

    [StringLength(50)]
    [Phone(ErrorMessage = "Teléfono no válido.")]
    [Display(Name = "Teléfono")]
    public string? ClienteTelefono { get; set; }

    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [Display(Name = "Correo")]
    public string? ClienteCorreo { get; set; }

    [StringLength(50)]
    [Display(Name = "N.º cotización / pedido SAP")]
    public string? ReferenciaSap { get; set; }

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
    public TimeOnly HoraFin { get; set; } = new(17, 0);

    [Range(1, 10000,
        ErrorMessage = "La cantidad de personas debe ser mayor que cero.")]
    [Display(Name = "Cantidad de personas")]
    public int CantidadPersonas { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un salón.")]
    [Display(Name = "Salón")]
    public int SalonId { get; set; }

    [Display(Name = "Requerimientos logísticos")]
    public string? Otros { get; set; }

    [Display(Name = "Comentarios internos")]
    public string? Comentarios { get; set; }

    public bool EsEdicion => Id.HasValue;

    public DateTime Inicio => FechaInicio.ToDateTime(HoraInicio);

    public DateTime Fin => FechaFin.ToDateTime(HoraFin);

    public IEnumerable<SelectListItem> Salones { get; set; }
        = Enumerable.Empty<SelectListItem>();
}
