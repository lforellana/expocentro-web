using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Services;

public record ConflictoEvento(
    int Id,
    string Codigo,
    string NombreEvento,
    DateTime Inicio,
    DateTime Fin,
    string Estado,
    TipoEvento TipoEvento,
    string Salon)
{
    public bool EsConfirmado => Estado == Estados.Aprobada;
}

public record ResultadoDisponibilidad(
    IReadOnlyList<ConflictoEvento> Conflictos,
    int? CapacidadMaxima,
    string? NombreSalon)
{
    public bool HayConflictoConfirmado =>
        Conflictos.Any(x => x.EsConfirmado);

    public IEnumerable<ConflictoEvento> Confirmados =>
        Conflictos.Where(x => x.EsConfirmado);

    public IEnumerable<ConflictoEvento> Pendientes =>
        Conflictos.Where(x => !x.EsConfirmado);
}

/// <summary>
/// Verifica choques de horario y aforo de los salones.
/// Regla: un evento APROBADO bloquea el salón; solicitudes
/// enviadas o en revisión generan una advertencia.
/// Un salón combinado choca con sus partes y viceversa
/// (ej. "Forjadores Completo" con "Forjadores 1").
/// </summary>
public class DisponibilidadService
{
    private readonly ApplicationDbContext _context;

    public DisponibilidadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResultadoDisponibilidad> VerificarAsync(
        int salonId,
        DateTime inicio,
        DateTime fin,
        int? excluirSolicitudId = null)
    {
        var salon = await _context.Salones
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == salonId);

        var salonesRelacionados = await SalonesRelacionadosAsync(salonId, salon?.SalonPadreId);

        var conflictos = await _context.SolicitudesEvento
            .AsNoTracking()
            .Where(x =>
                salonesRelacionados.Contains(x.SalonId)
                && x.Id != (excluirSolicitudId ?? 0)
                && Estados.OcupanSalon.Contains(x.Estado.Nombre)
                && x.FechaHoraInicio < fin
                && x.FechaHoraFin > inicio)
            .OrderBy(x => x.FechaHoraInicio)
            .Select(x => new ConflictoEvento(
                x.Id,
                x.Codigo,
                x.NombreEvento,
                x.FechaHoraInicio,
                x.FechaHoraFin,
                x.Estado.Nombre,
                x.TipoEvento,
                x.Salon.Nombre))
            .ToListAsync();

        return new ResultadoDisponibilidad(
            conflictos,
            salon?.CapacidadMaxima,
            salon?.Nombre);
    }

    /// <summary>El salón, el salón combinado del que forma parte y sus partes.</summary>
    private async Task<List<int>> SalonesRelacionadosAsync(int salonId, int? salonPadreId)
    {
        var ids = await _context.Salones
            .Where(x => x.SalonPadreId == salonId)
            .Select(x => x.Id)
            .ToListAsync();

        ids.Add(salonId);

        if (salonPadreId is int padre)
        {
            ids.Add(padre);
        }

        return ids;
    }

    /// <summary>
    /// Agrega errores al diccionario si hay choque con un evento
    /// aprobado o si se excede la capacidad del salón.
    /// </summary>
    public async Task<ResultadoDisponibilidad> ValidarAsync(
        Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState,
        int salonId,
        DateTime inicio,
        DateTime fin,
        int cantidadPersonas,
        int? excluirSolicitudId = null)
    {
        var resultado = await VerificarAsync(
            salonId, inicio, fin, excluirSolicitudId);

        if (resultado.CapacidadMaxima is int capacidad
            && cantidadPersonas > capacidad)
        {
            modelState.AddModelError(
                "CantidadPersonas",
                $"El salón {resultado.NombreSalon} tiene capacidad máxima de {capacidad} personas.");
        }

        foreach (var conflicto in resultado.Confirmados)
        {
            var donde = conflicto.Salon == resultado.NombreSalon
                ? "El salón ya está reservado"
                : $"El salón no está disponible: \"{conflicto.Salon}\" está reservado";

            modelState.AddModelError(
                string.Empty,
                $"{donde} para \"{conflicto.NombreEvento}\" ({conflicto.Codigo}) " +
                $"del {conflicto.Inicio:dd/MM/yyyy hh:mm tt} al {conflicto.Fin:dd/MM/yyyy hh:mm tt}.");
        }

        return resultado;
    }
}
