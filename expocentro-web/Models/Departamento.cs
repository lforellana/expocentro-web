namespace Expocentro.Web.Models;

public class Departamento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; }
        = new List<Usuario>();
}