namespace Expocentro.Web.Models;

public class Usuario
{
    public int Id { get; set; }

    public string? EntraObjectId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public int DepartamentoId { get; set; }

    public Departamento Departamento { get; set; } = null!;

    public string Rol { get; set; } = "Solicitante";

    public bool Activo { get; set; } = true;
}