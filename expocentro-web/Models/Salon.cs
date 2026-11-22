namespace Expocentro.Web.Models;

public class Salon
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    /// <summary>Null = capacidad por definir (no se valida el aforo).</summary>
    public int? CapacidadMaxima { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Salón combinado del que este forma parte (ej. "Forjadores 1" es parte de
    /// "Salon Forjadores Completo"). Reservar uno bloquea al otro.
    /// </summary>
    public int? SalonPadreId { get; set; }

    public Salon? SalonPadre { get; set; }

    public ICollection<Salon> Subsalones { get; set; } = new List<Salon>();
}
