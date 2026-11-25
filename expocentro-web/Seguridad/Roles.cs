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

    /// <summary>Ordenados de menor a mayor nivel.</summary>
    public static readonly string[] Todos =
        { Solicitante, Coordinador, Administrador };

    /// <summary>Nivel del rol (mayor = más permisos); -1 si no es un rol conocido.</summary>
    public static int Nivel(string? rol) => Array.IndexOf(Todos, rol);
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
