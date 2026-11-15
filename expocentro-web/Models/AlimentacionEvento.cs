namespace Expocentro.Web.Models;

public class AlimentacionEvento
{
    public int Id { get; set; }

    public int SolicitudEventoId { get; set; }

    public SolicitudEvento SolicitudEvento { get; set; } = null!;

    public bool RequiereAlimentacion { get; set; }

    public int? CantidadPersonas { get; set; }

    public TimeOnly? HoraServicio { get; set; }

    public string? Detalle { get; set; }

    public bool IncluyeAgua { get; set; }

    public bool IncluyeCafe { get; set; }

    public bool IncluyeRefrigerio { get; set; }

    public string? Observaciones { get; set; }
}