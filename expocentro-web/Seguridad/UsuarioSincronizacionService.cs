using System.Security.Claims;
using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Seguridad;

/// <summary>
/// Vincula la identidad de Entra ID con la tabla Usuarios:
/// crea el usuario la primera vez que inicia sesión, actualiza
/// su nombre/correo y construye los claims de la aplicación.
/// </summary>
public class UsuarioSincronizacionService
{
    public const int DepartamentoSinAsignarId = 5;

    private const string ClaimObjectId =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsuarioSincronizacionService> _logger;

    public UsuarioSincronizacionService(
        ApplicationDbContext context,
        ILogger<UsuarioSincronizacionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sincroniza el usuario de Entra ID. Devuelve null si el
    /// usuario está desactivado en la aplicación.
    /// </summary>
    public async Task<ClaimsIdentity?> SincronizarEntraAsync(
        ClaimsPrincipal principal)
    {
        var objectId =
            principal.FindFirst(ClaimObjectId)?.Value
            ?? principal.FindFirst("oid")?.Value;

        var correo = (
            principal.FindFirst("preferred_username")?.Value
            ?? principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value
            ?? principal.FindFirst(ClaimTypes.Upn)?.Value
            ?? string.Empty).Trim().ToLowerInvariant();

        var nombre =
            principal.FindFirst("name")?.Value
            ?? correo;

        if (string.IsNullOrEmpty(objectId) && string.IsNullOrEmpty(correo))
        {
            throw new InvalidOperationException(
                "El token de Entra ID no contiene 'oid' ni correo.");
        }

        var usuario = await _context.Usuarios
            .Include(x => x.Departamento)
            .FirstOrDefaultAsync(x =>
                (objectId != null && x.EntraObjectId == objectId)
                || (correo != "" && x.Correo.ToLower() == correo));

        if (usuario == null)
        {
            usuario = new Usuario
            {
                EntraObjectId = objectId,
                Nombre = nombre,
                Correo = correo,
                DepartamentoId = DepartamentoSinAsignarId,
                Rol = Roles.Solicitante,
                Activo = true
            };

            _context.Usuarios.Add(usuario);

            _logger.LogInformation(
                "Nuevo usuario registrado desde Entra ID: {Correo}", correo);
        }
        else
        {
            usuario.EntraObjectId ??= objectId;
            usuario.Nombre = nombre;

            if (correo != "")
            {
                usuario.Correo = correo;
            }
        }

        await _context.SaveChangesAsync();

        if (!usuario.Activo)
        {
            return null;
        }

        // Roles efectivos = rol guardado en la BD + App roles de Entra ID
        var rolesEntra = principal.Claims
            .Where(c => c.Type == "roles" || c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        var roles = rolesEntra
            .Append(usuario.Rol)
            .Where(r => Roles.Todos.Contains(r))
            .Distinct();

        return ConstruirIdentidad(usuario, roles, "Expocentro");
    }

    /// <summary>
    /// Construye la identidad para el acceso de desarrollo (sin Entra ID).
    /// </summary>
    public async Task<ClaimsIdentity?> IdentidadDesarrolloAsync(
        int usuarioId,
        string authenticationType)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Id == usuarioId && x.Activo);

        if (usuario == null)
        {
            return null;
        }

        var identidad = ConstruirIdentidad(
            usuario, new[] { usuario.Rol }, authenticationType);

        identidad.AddClaim(new Claim("name", usuario.Nombre));
        identidad.AddClaim(new Claim(ClaimTypes.Name, usuario.Nombre));
        identidad.AddClaim(new Claim(ClaimTypes.Email, usuario.Correo));

        return identidad;
    }

    private static ClaimsIdentity ConstruirIdentidad(
        Usuario usuario,
        IEnumerable<string> roles,
        string authenticationType)
    {
        var identidad = new ClaimsIdentity(
            authenticationType,
            ClaimTypes.Name,
            ClaimTypes.Role);

        identidad.AddClaim(new Claim(
            ExpocentroClaims.UsuarioId,
            usuario.Id.ToString()));

        identidad.AddClaim(new Claim(
            ExpocentroClaims.Departamento,
            usuario.DepartamentoId.ToString()));

        foreach (var rol in roles)
        {
            identidad.AddClaim(new Claim(ClaimTypes.Role, rol));
        }

        return identidad;
    }
}
