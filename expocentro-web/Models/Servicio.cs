namespace Expocentro.Web.Models;

public class Servicio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<SolicitudServicio> SolicitudesServicio { get; set; }
        = new List<SolicitudServicio>();
}