using Expocentro.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Services;

public static class SolicitudQueryExtensions
{
    /// <summary>Incluye toda la información para mostrar el detalle.</summary>
    public static IQueryable<SolicitudEvento> ConDetalle(
        this IQueryable<SolicitudEvento> query)
    {
        return query
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .Include(x => x.SolicitanteUsuario)
            .Include(x => x.ResponsableUsuario)
            .Include(x => x.DepartamentoCargo)
            .Include(x => x.Montajes)
                .ThenInclude(x => x.TipoMontaje)
            .Include(x => x.Servicios)
                .ThenInclude(x => x.Servicio)
            .Include(x => x.Alimentacion)
            .Include(x => x.HistorialEstados)
                .ThenInclude(x => x.EstadoSolicitud)
            .AsSplitQuery();
    }
}
