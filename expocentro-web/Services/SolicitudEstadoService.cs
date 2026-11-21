using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Services;

/// <summary>
/// Centraliza los cambios de estado y el registro en el historial.
/// </summary>
public class SolicitudEstadoService
{
    private readonly ApplicationDbContext _context;

    public SolicitudEstadoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EstadoSolicitud> ObtenerEstadoAsync(string nombre)
    {
        return await _context.EstadosSolicitud
            .FirstAsync(x => x.Nombre == nombre);
    }

    /// <summary>
    /// Cambia el estado y agrega la entrada al historial.
    /// No llama SaveChanges.
    /// </summary>
    public async Task CambiarEstadoAsync(
        SolicitudEvento solicitud,
        string nuevoEstado,
        string actor,
        string? comentario)
    {
        var estado = await ObtenerEstadoAsync(nuevoEstado);
        var ahora = DateTimeOffset.UtcNow;

        solicitud.EstadoId = estado.Id;
        solicitud.Estado = estado;
        solicitud.FechaActualizacion = ahora;

        solicitud.HistorialEstados.Add(new HistorialEstadoSolicitud
        {
            EstadoSolicitudId = estado.Id,
            FechaCambio = ahora,
            Actor = actor,
            Comentario = comentario
        });
    }

    public static string GenerarCodigo(TipoEvento tipo)
    {
        var prefijo = tipo == TipoEvento.Externo ? "EX" : "EV";

        return $"{prefijo}-{DateTime.UtcNow:yyyy}-" +
               Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
    }
}
