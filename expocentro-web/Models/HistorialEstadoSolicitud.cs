namespace Expocentro.Web.Models;

public class HistorialEstadoSolicitud
{
    public int Id { get; set; }

    public int SolicitudEventoId { get; set; }

    public SolicitudEvento SolicitudEvento { get; set; } = null!;

    public int EstadoSolicitudId { get; set; }

    public EstadoSolicitud EstadoSolicitud { get; set; } = null!;

    public DateTimeOffset FechaCambio { get; set; }

    public string? Comentario { get; set; }

    public string Actor { get; set; } = string.Empty;
}