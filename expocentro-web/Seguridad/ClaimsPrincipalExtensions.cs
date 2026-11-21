using System.Security.Claims;

namespace Expocentro.Web.Seguridad;

public static class ClaimsPrincipalExtensions
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirst(ExpocentroClaims.UsuarioId)?.Value;

        if (!int.TryParse(valor, out var id))
        {
            throw new InvalidOperationException(
                "El usuario autenticado no tiene un registro en Expocentro.");
        }

        return id;
    }

    public static string ObtenerNombre(this ClaimsPrincipal user)
    {
        return user.FindFirst("name")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.Identity?.Name
            ?? "Usuario";
    }

    public static bool EsCoordinador(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Coordinador)
            || user.IsInRole(Roles.Administrador);
    }

    public static bool EsAdministrador(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Administrador);
    }
}
