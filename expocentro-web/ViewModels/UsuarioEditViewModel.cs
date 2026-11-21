using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Expocentro.Web.ViewModels;

public class UsuarioEditViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "Solicitante";

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un departamento.")]
    [Display(Name = "Departamento")]
    public int DepartamentoId { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    public IEnumerable<SelectListItem> Departamentos { get; set; }
        = Enumerable.Empty<SelectListItem>();
}
