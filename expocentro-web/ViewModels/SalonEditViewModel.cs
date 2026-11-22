using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expocentro.Web.ViewModels;

public class SalonEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Ingrese el nombre del salón.")]
    [StringLength(150)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(1, 10000, ErrorMessage = "La capacidad debe estar entre 1 y 10,000 personas.")]
    [Display(Name = "Capacidad máxima (personas)")]
    public int? CapacidadMaxima { get; set; }

    [Display(Name = "Forma parte de")]
    public int? SalonPadreId { get; set; }

    public bool Activo { get; set; } = true;

    public bool EsEdicion => Id.HasValue;

    public List<string> Subsalones { get; set; } = new();

    public IEnumerable<SelectListItem> SalonesPadre { get; set; }
        = new List<SelectListItem>();
}
