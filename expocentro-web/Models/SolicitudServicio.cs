namespace Expocentro.Web.Models;

public class SolicitudServicio
{
    public int Id { get; set; }

    public int SolicitudEventoId { get; set; }

    public SolicitudEvento SolicitudEvento { get; set; } = null!;

    public int ServicioId { get; set; }

    public Servicio Servicio { get; set; } = null!;

    public int Cantidad { get; set; } = 1;

    public string? Observaciones { get; set; }
}