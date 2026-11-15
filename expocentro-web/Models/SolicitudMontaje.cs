namespace Expocentro.Web.Models;

public class SolicitudMontaje
{
    public int Id { get; set; }

    public int SolicitudEventoId { get; set; }

    public SolicitudEvento SolicitudEvento { get; set; } = null!;

    public int TipoMontajeId { get; set; }

    public TipoMontaje TipoMontaje { get; set; } = null!;

    public int Cantidad { get; set; } = 1;

    public string? Observaciones { get; set; }
}