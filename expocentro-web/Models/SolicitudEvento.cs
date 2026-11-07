namespace Expocentro.Web.Models;

public class SolicitudEvento
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string NombreEvento { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public int CantidadPersonas { get; set; }

    public int SolicitanteUsuarioId { get; set; }

    public Usuario SolicitanteUsuario { get; set; } = null!;

    public int? ResponsableUsuarioId { get; set; }

    public Usuario? ResponsableUsuario { get; set; }

    public int SalonId { get; set; }

    public Salon Salon { get; set; } = null!;

    public int DepartamentoCargoId { get; set; }

    public Departamento DepartamentoCargo { get; set; } = null!;

    public int EstadoId { get; set; }

    public EstadoSolicitud Estado { get; set; } = null!;

    public string? Otros { get; set; }

    public string? Comentarios { get; set; }

    public DateTimeOffset FechaCreacion { get; set; }

    public DateTimeOffset FechaActualizacion { get; set; }
}