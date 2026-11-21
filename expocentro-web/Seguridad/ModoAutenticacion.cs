namespace Expocentro.Web.Seguridad;

public enum TipoAutenticacion
{
    EntraId,
    Desarrollo
}

/// <summary>
/// Indica cómo se autentican los usuarios en esta instancia.
/// </summary>
public sealed record ModoAutenticacion(TipoAutenticacion Tipo)
{
    public bool EsEntraId => Tipo == TipoAutenticacion.EntraId;

    public bool EsDesarrollo => Tipo == TipoAutenticacion.Desarrollo;
}
