namespace Expocentro.Web.Seguridad;

/// <summary>
/// Roles de la aplicación. Los valores deben coincidir con los
/// "App roles" definidos en el registro de la aplicación en Entra ID.
/// </summary>
public static class Roles
{
    public const string Solicitante = "Solicitante";
    public const string Coordinador = "Coordinador";
    public const string Administrador = "Administrador";

    public static readonly string[] Todos =
        { Solicitante, Coordinador, Administrador };
}

public static class Politicas
{
    /// <summary>Coordinadores y administradores.</summary>
    public const string Coordinacion = "Coordinacion";

    public const string Administracion = "Administracion";
}

public static class ExpocentroClaims
{
    public const string UsuarioId = "expocentro:usuario_id";
    public const string Departamento = "expocentro:departamento";
}
