namespace Expocentro.Web.Models;

public class Salon
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int CapacidadMaxima { get; set; }

    public bool Activo { get; set; } = true;
}