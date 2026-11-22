using Expocentro.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Services;

public static class SalonListas
{
    /// <summary>Salones activos para los formularios de reserva, con su capacidad.</summary>
    public static async Task<List<SelectListItem>> SalonesParaReservaAsync(
        this ApplicationDbContext context)
    {
        var salones = await context.Salones
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new { x.Id, x.Nombre, x.CapacidadMaxima })
            .ToListAsync();

        return salones
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.CapacidadMaxima is int c
                    ? $"{x.Nombre} (hasta {c} personas)"
                    : x.Nombre
            })
            .ToList();
    }
}
