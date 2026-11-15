using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SolicitudesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // MIS SOLICITUDES
    public async Task<IActionResult> Index()
    {
        const int usuarioDemoId = 1;

        var solicitudes = await _context.SolicitudesEvento
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .Where(x => x.SolicitanteUsuarioId == usuarioDemoId)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync();

        return View(solicitudes);
    }

    // NUEVA SOLICITUD
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SolicitudEventoCreateViewModel();

        await CargarSalones(model);

        return View(model);
    }

    // GUARDAR SOLICITUD
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SolicitudEventoCreateViewModel model)
    {
        var inicio =
            model.FechaInicio.ToDateTime(model.HoraInicio);

        var fin =
            model.FechaFin.ToDateTime(model.HoraFin);

        if (fin <= inicio)
        {
            ModelState.AddModelError(
                nameof(model.FechaFin),
                "La fecha y hora de finalización debe ser posterior al inicio.");
        }

        if (!ModelState.IsValid)
        {
            await CargarSalones(model);
            return View(model);
        }

        const int usuarioDemoId = 1;

        var usuario = await _context.Usuarios
            .FirstAsync(x => x.Id == usuarioDemoId);

        var estadoBorrador = await _context.EstadosSolicitud
            .FirstAsync(x => x.Nombre == "Borrador");

        var solicitud = new SolicitudEvento
        {
            Codigo =
                $"EV-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",

            NombreEvento = model.NombreEvento,

            FechaHoraInicio = inicio,
            FechaHoraFin = fin,

            CantidadPersonas = model.CantidadPersonas,

            SolicitanteUsuarioId = usuario.Id,

            ResponsableUsuarioId = usuario.Id,

            DepartamentoCargoId = usuario.DepartamentoId,

            SalonId = model.SalonId,

            EstadoId = estadoBorrador.Id,

            Otros = model.Otros,

            Comentarios = model.Comentarios,

            FechaCreacion = DateTimeOffset.UtcNow,

            FechaActualizacion = DateTimeOffset.UtcNow
        };

        _context.SolicitudesEvento.Add(solicitud);

await _context.SaveChangesAsync();

_context.HistorialEstadosSolicitud.Add(
    new HistorialEstadoSolicitud
    {
        SolicitudEventoId = solicitud.Id,
        EstadoSolicitudId = estadoBorrador.Id,
        FechaCambio = DateTimeOffset.UtcNow,
        Actor = "Solicitante",
        Comentario = "Solicitud creada como borrador."
    });

await _context.SaveChangesAsync();

_context.HistorialEstadosSolicitud.Add(
    new HistorialEstadoSolicitud
    {
        SolicitudEventoId = solicitud.Id,
        EstadoSolicitudId = estadoBorrador.Id,
        FechaCambio = DateTimeOffset.UtcNow,
        Actor = "Solicitante",
        Comentario = "Solicitud creada como borrador."
    });

await _context.SaveChangesAsync();

TempData["Mensaje"] =
    "Datos generales guardados correctamente.";

return RedirectToAction(
    nameof(Montaje),
    new { id = solicitud.Id });
    }

// DETALLE DE SOLICITUD
public async Task<IActionResult> Details(int id)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Salon)
        .Include(x => x.Estado)
        .Include(x => x.SolicitanteUsuario)
        .Include(x => x.ResponsableUsuario)
        .Include(x => x.DepartamentoCargo)

        // NUEVO: cargar montajes
        .Include(x => x.Montajes)
            .ThenInclude(x => x.TipoMontaje)
// NUEVO: servicios
            .Include(x => x.Servicios)
    .ThenInclude(x => x.Servicio)
    .Include(x => x.Alimentacion)
    .Include(x => x.HistorialEstados)
    .ThenInclude(x => x.EstadoSolicitud)

        .FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    return View(solicitud);
}
// CONFIGURAR MONTAJE
[HttpGet]
public async Task<IActionResult> Montaje(int id)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        TempData["Error"] =
            "Solo se puede modificar el montaje de una solicitud en borrador.";

        return RedirectToAction(nameof(Details), new { id });
    }

    var montajesGuardados = await _context.SolicitudesMontaje
        .Where(x => x.SolicitudEventoId == id)
        .ToListAsync();

    var tiposMontaje = await _context.TiposMontaje
        .Where(x => x.Activo)
        .OrderBy(x => x.Nombre)
        .ToListAsync();

    var model = new SolicitudMontajeViewModel
    {
        SolicitudEventoId = solicitud.Id,
        Codigo = solicitud.Codigo,
        NombreEvento = solicitud.NombreEvento,

        Montajes = tiposMontaje.Select(tipo =>
        {
            var existente = montajesGuardados
                .FirstOrDefault(x => x.TipoMontajeId == tipo.Id);

            return new MontajeItemViewModel
            {
                TipoMontajeId = tipo.Id,
                Nombre = tipo.Nombre,
                Seleccionado = existente != null,
                Cantidad = existente?.Cantidad ?? 1,
                Observaciones = existente?.Observaciones
            };
        }).ToList()
    };

    return View(model);
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Montaje(
    SolicitudMontajeViewModel model)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .FirstOrDefaultAsync(x =>
            x.Id == model.SolicitudEventoId &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        return RedirectToAction(
            nameof(Details),
            new { id = model.SolicitudEventoId });
    }

    var seleccionados = model.Montajes
    .Where(x => x.Seleccionado)
    .ToList();

if (!seleccionados.Any())
{
    ModelState.AddModelError(
        string.Empty,
        "Debe seleccionar un tipo de montaje o la opción 'Sin montaje'.");
}

var sinMontaje = seleccionados
    .Any(x => x.Nombre == "Sin montaje");

if (sinMontaje && seleccionados.Count > 1)
{
    ModelState.AddModelError(
        string.Empty,
        "Si selecciona 'Sin montaje', no puede seleccionar otros tipos de montaje.");
}

    if (!ModelState.IsValid)
    {
        var nombres = await _context.TiposMontaje
            .ToDictionaryAsync(x => x.Id, x => x.Nombre);

        model.Codigo = solicitud.Codigo;
        model.NombreEvento = solicitud.NombreEvento;

        foreach (var item in model.Montajes)
        {
            if (nombres.TryGetValue(item.TipoMontajeId, out var nombre))
            {
                item.Nombre = nombre;
            }
        }

        return View(model);
    }

    var existentes = await _context.SolicitudesMontaje
        .Where(x => x.SolicitudEventoId == solicitud.Id)
        .ToListAsync();

    _context.SolicitudesMontaje.RemoveRange(existentes);

    foreach (var item in seleccionados)
    {

        if (item.Nombre == "Sin montaje")
{
    item.Cantidad = 1;
    item.Observaciones = null;
}
        _context.SolicitudesMontaje.Add(
            new SolicitudMontaje
            {
                SolicitudEventoId = solicitud.Id,
                TipoMontajeId = item.TipoMontajeId,
                Cantidad = item.Cantidad,
                Observaciones = item.Observaciones
            });
    }

    solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();

    TempData["Mensaje"] =
        "El montaje fue guardado correctamente.";

    return RedirectToAction(
    nameof(Servicios),
    new { id = solicitud.Id });
}

// CONFIGURAR SERVICIOS
[HttpGet]
public async Task<IActionResult> Servicios(int id)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        TempData["Error"] =
            "Solo se pueden modificar los servicios de una solicitud en borrador.";

        return RedirectToAction(nameof(Details), new { id });
    }

    var serviciosGuardados = await _context.SolicitudesServicio
        .Where(x => x.SolicitudEventoId == id)
        .ToListAsync();

    var serviciosDisponibles = await _context.Servicios
        .Where(x => x.Activo)
        .OrderBy(x => x.Nombre)
        .ToListAsync();

    var model = new SolicitudServicioViewModel
    {
        SolicitudEventoId = solicitud.Id,
        Codigo = solicitud.Codigo,
        NombreEvento = solicitud.NombreEvento,

        Servicios = serviciosDisponibles.Select(servicio =>
        {
            var existente = serviciosGuardados
                .FirstOrDefault(x => x.ServicioId == servicio.Id);

            return new ServicioItemViewModel
            {
                ServicioId = servicio.Id,
                Nombre = servicio.Nombre,
                Seleccionado = existente != null,
                Cantidad = existente?.Cantidad ?? 1,
                Observaciones = existente?.Observaciones
            };
        }).ToList()
    };

    return View(model);
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Servicios(
    SolicitudServicioViewModel model)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .FirstOrDefaultAsync(x =>
            x.Id == model.SolicitudEventoId &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        return RedirectToAction(
            nameof(Details),
            new { id = model.SolicitudEventoId });
    }

    var serviciosDisponibles = await _context.Servicios
        .Where(x => x.Activo)
        .ToDictionaryAsync(x => x.Id, x => x.Nombre);

    var seleccionados = model.Servicios
        .Where(x => x.Seleccionado)
        .ToList();

    if (!seleccionados.Any())
    {
        ModelState.AddModelError(
            string.Empty,
            "Debe seleccionar al menos un servicio o la opción 'Sin servicios adicionales'.");
    }

    var sinServiciosId = serviciosDisponibles
        .FirstOrDefault(x => x.Value == "Sin servicios adicionales")
        .Key;

    if (seleccionados.Any(x => x.ServicioId == sinServiciosId)
        && seleccionados.Count > 1)
    {
        ModelState.AddModelError(
            string.Empty,
            "Si selecciona 'Sin servicios adicionales', no puede seleccionar otros servicios.");
    }

    if (!ModelState.IsValid)
    {
        model.Codigo = solicitud.Codigo;
        model.NombreEvento = solicitud.NombreEvento;

        foreach (var item in model.Servicios)
        {
            if (serviciosDisponibles.TryGetValue(
                item.ServicioId,
                out var nombre))
            {
                item.Nombre = nombre;
            }
        }

        return View(model);
    }

    var existentes = await _context.SolicitudesServicio
        .Where(x => x.SolicitudEventoId == solicitud.Id)
        .ToListAsync();

    _context.SolicitudesServicio.RemoveRange(existentes);

    foreach (var item in seleccionados)
    {
        if (!serviciosDisponibles.ContainsKey(item.ServicioId))
        {
            continue;
        }

        if (item.ServicioId == sinServiciosId)
        {
            item.Cantidad = 1;
            item.Observaciones = null;
        }

        _context.SolicitudesServicio.Add(
            new SolicitudServicio
            {
                SolicitudEventoId = solicitud.Id,
                ServicioId = item.ServicioId,
                Cantidad = item.Cantidad,
                Observaciones = item.Observaciones
            });
    }

    solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();

    TempData["Mensaje"] =
    "Los servicios fueron guardados correctamente.";

return RedirectToAction(
    nameof(Alimentacion),
    new { id = solicitud.Id });
}

// CONFIGURAR ALIMENTACIÓN
[HttpGet]
public async Task<IActionResult> Alimentacion(int id)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .Include(x => x.Alimentacion)
        .FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    var model = new SolicitudAlimentacionViewModel
    {
        SolicitudEventoId = solicitud.Id,
        Codigo = solicitud.Codigo,
        NombreEvento = solicitud.NombreEvento,

        RequiereAlimentacion =
            solicitud.Alimentacion?.RequiereAlimentacion,

        CantidadPersonas =
            solicitud.Alimentacion?.CantidadPersonas
            ?? solicitud.CantidadPersonas,

        HoraServicio =
            solicitud.Alimentacion?.HoraServicio,

        Detalle =
            solicitud.Alimentacion?.Detalle,

        IncluyeAgua =
            solicitud.Alimentacion?.IncluyeAgua ?? false,

        IncluyeCafe =
            solicitud.Alimentacion?.IncluyeCafe ?? false,

        IncluyeRefrigerio =
            solicitud.Alimentacion?.IncluyeRefrigerio ?? false,

        Observaciones =
            solicitud.Alimentacion?.Observaciones
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Alimentacion(
    SolicitudAlimentacionViewModel model)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .Include(x => x.Alimentacion)
        .FirstOrDefaultAsync(x =>
            x.Id == model.SolicitudEventoId &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        return RedirectToAction(
            nameof(Details),
            new { id = solicitud.Id });
    }

    if (model.RequiereAlimentacion == true)
    {
        if (model.CantidadPersonas is null
            || model.CantidadPersonas <= 0)
        {
            ModelState.AddModelError(
                nameof(model.CantidadPersonas),
                "Debe indicar la cantidad de personas.");
        }

        if (string.IsNullOrWhiteSpace(model.Detalle))
        {
            ModelState.AddModelError(
                nameof(model.Detalle),
                "Debe indicar el detalle de la alimentación.");
        }
    }

    if (!ModelState.IsValid)
    {
        model.Codigo = solicitud.Codigo;
        model.NombreEvento = solicitud.NombreEvento;

        return View(model);
    }

    var alimentacion =
        solicitud.Alimentacion
        ?? new AlimentacionEvento
        {
            SolicitudEventoId = solicitud.Id
        };

    alimentacion.RequiereAlimentacion =
        model.RequiereAlimentacion!.Value;

    if (model.RequiereAlimentacion == true)
    {
        alimentacion.CantidadPersonas =
            model.CantidadPersonas;

        alimentacion.HoraServicio =
            model.HoraServicio;

        alimentacion.Detalle =
            model.Detalle;

        alimentacion.IncluyeAgua =
            model.IncluyeAgua;

        alimentacion.IncluyeCafe =
            model.IncluyeCafe;

        alimentacion.IncluyeRefrigerio =
            model.IncluyeRefrigerio;

        alimentacion.Observaciones =
            model.Observaciones;
    }
    else
    {
        alimentacion.CantidadPersonas = null;
        alimentacion.HoraServicio = null;
        alimentacion.Detalle = null;
        alimentacion.IncluyeAgua = false;
        alimentacion.IncluyeCafe = false;
        alimentacion.IncluyeRefrigerio = false;
        alimentacion.Observaciones = null;
    }

    if (solicitud.Alimentacion == null)
    {
        _context.AlimentacionesEvento.Add(alimentacion);
    }

    solicitud.FechaActualizacion =
        DateTimeOffset.UtcNow;

    await _context.SaveChangesAsync();

    return RedirectToAction(
        nameof(Resumen),
        new { id = solicitud.Id });
}


// RESUMEN DE SOLICITUD
[HttpGet]
public async Task<IActionResult> Resumen(int id)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Salon)
        .Include(x => x.Estado)
        .Include(x => x.SolicitanteUsuario)
        .Include(x => x.DepartamentoCargo)
        .Include(x => x.Montajes)
            .ThenInclude(x => x.TipoMontaje)
        .Include(x => x.Servicios)
            .ThenInclude(x => x.Servicio)
            .Include(x => x.Alimentacion)
        .FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    return View(solicitud);
}


// ENVIAR SOLICITUD
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Enviar(int id)
{
    const int usuarioDemoId = 1;

    var solicitud = await _context.SolicitudesEvento
        .Include(x => x.Estado)
        .Include(x => x.Montajes)
        .Include(x => x.Servicios)
        .Include(x => x.Alimentacion)
        .FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.SolicitanteUsuarioId == usuarioDemoId);

    if (solicitud == null)
    {
        return NotFound();
    }

    if (solicitud.Estado.Nombre != "Borrador")
    {
        TempData["Error"] =
            "Solo las solicitudes en borrador pueden enviarse.";

        return RedirectToAction(nameof(Details), new { id });
    }

    if (!solicitud.Montajes.Any())
{
    TempData["Error"] =
        "Debe configurar el montaje antes de enviar la solicitud.";

    return RedirectToAction(nameof(Details), new { id });
}

if (!solicitud.Servicios.Any())
{
    TempData["Error"] =
        "Debe configurar los servicios antes de enviar la solicitud.";

    return RedirectToAction(nameof(Details), new { id });
}

if (solicitud.Alimentacion == null)
{
    TempData["Error"] =
        "Debe completar la información de alimentación antes de enviar la solicitud.";

    return RedirectToAction(nameof(Details), new { id });
}

    var estadoEnviada = await _context.EstadosSolicitud
        .FirstAsync(x => x.Nombre == "Enviada");

    var ahora = DateTimeOffset.UtcNow;

solicitud.EstadoId = estadoEnviada.Id;
solicitud.FechaActualizacion = ahora;

_context.HistorialEstadosSolicitud.Add(
    new HistorialEstadoSolicitud
    {
        SolicitudEventoId = solicitud.Id,
        EstadoSolicitudId = estadoEnviada.Id,
        FechaCambio = ahora,
        Actor = "Solicitante",
        Comentario = "Solicitud enviada a Coordinación."
    });

await _context.SaveChangesAsync();

    TempData["Mensaje"] =
        "La solicitud fue enviada correctamente.";

    return RedirectToAction(nameof(Details), new { id });
}



// MÉTODO AUXILIAR
private async Task CargarSalones(
    SolicitudEventoCreateViewModel model)
{
    model.Salones = await _context.Salones
        .Where(x => x.Activo)
        .OrderBy(x => x.Nombre)
        .Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Nombre
        })
        .ToListAsync();
}
}

