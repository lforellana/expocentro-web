namespace Expocentro.Web.Services;

/// <summary>
/// Nombres de los estados (deben coincidir con la tabla EstadosSolicitud).
/// </summary>
public static class Estados
{
    public const string Borrador = "Borrador";
    public const string Enviada = "Enviada";
    public const string EnRevision = "En revisión";
    public const string Aprobada = "Aprobada";
    public const string Rechazada = "Rechazada";
    public const string Cancelada = "Cancelada";

    /// <summary>Estados que ocupan el salón (confirmados o en trámite).</summary>
    public static readonly string[] OcupanSalon =
        { Enviada, EnRevision, Aprobada };

    /// <summary>Estados en los que la solicitud aún puede cancelarse.</summary>
    public static readonly string[] Cancelables =
        { Borrador, Enviada, EnRevision, Aprobada };

    public static string ClaseBadge(string estado) => estado switch
    {
        Borrador => "estado-borrador",
        Enviada => "estado-enviada",
        EnRevision => "estado-revision",
        Aprobada => "estado-aprobada",
        Rechazada => "estado-rechazada",
        Cancelada => "estado-cancelada",
        _ => "estado-borrador"
    };

    public static string Icono(string estado) => estado switch
    {
        Borrador => "bi-pencil",
        Enviada => "bi-send",
        EnRevision => "bi-hourglass-split",
        Aprobada => "bi-check-circle",
        Rechazada => "bi-x-circle",
        Cancelada => "bi-slash-circle",
        _ => "bi-circle"
    };
}
