namespace Expocentro.Web.Models;

public class TipoMontaje
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<SolicitudMontaje> SolicitudesMontaje { get; set; }
        = new List<SolicitudMontaje>();
}