using Expocentro.Web.Data;
using Expocentro.Web.Models;
using Expocentro.Web.Seguridad;
using Expocentro.Web.Services;
using Expocentro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Controllers;

/// <summary>
/// Solicitudes de eventos internos del usuario autenticado.
/// Flujo: Datos generales → Montaje → Servicios → Alimentación → Resumen → Enviar.
/// </summary>
public class SolicitudesController : Controller
{
    private const string SinMontaje = "Sin montaje";
    private const string SinServicios = "Sin servicios adicionales";

    private readonly ApplicationDbContext _context;
    private readonly DisponibilidadService _disponibilidad;
    private readonly SolicitudEstadoService _estados;

    public SolicitudesController(
        ApplicationDbContext context,
        DisponibilidadService disponibilidad,
        SolicitudEstadoService estados)
    {
        _context = context;
        _disponibilidad = disponibilidad;
        _estados = estados;
    }

    private int UsuarioId => User.ObtenerUsuarioId();

    private string Actor => User.ObtenerNombre();

    private IQueryable<SolicitudEvento> MisSolicitudes() =>
        _context.SolicitudesEvento
            .Where(x => x.SolicitanteUsuarioId == UsuarioId);

    /// <summary>Solicitud propia en estado Borrador (o null).</summary>
    private async Task<SolicitudEvento?> ObtenerBorradorAsync(int id)
    {
        return await MisSolicitudes()
            .Include(x => x.Estado)
            .Include(x => x.Alimentacion)
            .FirstOrDefaultAsync(x =>
                x.Id == id && x.Estado.Nombre == Estados.Borrador);
    }

    private IActionResult NoEditable(int id)
    {
        TempData["Error"] =
            "Solo se pueden modificar solicitudes en borrador.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ============================
    // MIS SOLICITUDES
    // ============================

    public async Task<IActionResult> Index(string? estado)
    {
        var query = MisSolicitudes()
            .Include(x => x.Salon)
            .Include(x => x.Estado)
            .AsQueryable();

        query = estado switch
        {
            null or "" => query,
            "pendientes" => query.Where(x =>
                x.Estado.Nombre == Estados.Enviada
                || x.Estado.Nombre == Estados.EnRevision),
            "cerradas" => query.Where(x =>
                x.Estado.Nombre == Estados.Rechazada
                || x.Estado.Nombre == Estados.Cancelada),
            _ => query.Where(x => x.Estado.Nombre == estado)
        };

        ViewData["Estado"] = estado;

        var solicitudes = await query
            .OrderByDescending(x => x.FechaActualizacion)
            .ToListAsync();

        return View(solicitudes);
    }

    // ============================
    // PASO 1: DATOS GENERALES
    // ============================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SolicitudEventoCreateViewModel();

        var departamento = await _context.Usuarios
            .Where(x => x.Id == UsuarioId)
            .Select(x => x.DepartamentoId)
            .FirstOrDefaultAsync();

        if (departamento != UsuarioSincronizacionService.DepartamentoSinAsignarId)
        {
            model.DepartamentoCargoId = departamento;
        }

        await CargarListas(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SolicitudEventoCreateViewModel model)
    {
        await ValidarDatosGenerales(model);

        if (!ModelState.IsValid)
        {
            await CargarListas(model);
            return View(model);
        }

        var solicitud = new SolicitudEvento
        {
            Codigo = SolicitudEstadoService.GenerarCodigo(TipoEvento.Interno),
            TipoEvento = TipoEvento.Interno,
            SolicitanteUsuarioId = UsuarioId,
            ResponsableUsuarioId = UsuarioId,
            FechaCreacion = DateTimeOffset.UtcNow
        };

        AplicarDatosGenerales(solicitud, model);

        await _estados.CambiarEstadoAsync(
            solicitud,
            Estados.Borrador,
            Actor,
            "Solicitud creada como borrador.");

        _context.SolicitudesEvento.Add(solicitud);

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Datos generales guardados. Continúe con el montaje.";

        return RedirectToAction(nameof(Montaje), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(id);

        if (solicitud == null)
        {
            return NoEditable(id);
        }

        var model = new SolicitudEventoCreateViewModel
        {
            Id = solicitud.Id,
            Codigo = solicitud.Codigo,
            NombreEvento = solicitud.NombreEvento,
            FechaInicio = DateOnly.FromDateTime(solicitud.FechaHoraInicio),
            HoraInicio = TimeOnly.FromDateTime(solicitud.FechaHoraInicio),
            FechaFin = DateOnly.FromDateTime(solicitud.FechaHoraFin),
            HoraFin = TimeOnly.FromDateTime(solicitud.FechaHoraFin),
            CantidadPersonas = solicitud.CantidadPersonas,
            SalonId = solicitud.SalonId,
            DepartamentoCargoId = solicitud.DepartamentoCargoId,
            Otros = solicitud.Otros,
            Comentarios = solicitud.Comentarios
        };

        ViewData["DesdeResumen"] = desdeResumen;

        await CargarListas(model);

        return View(nameof(Create), model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        SolicitudEventoCreateViewModel model,
        bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(model.Id ?? 0);

        if (solicitud == null)
        {
            return NoEditable(model.Id ?? 0);
        }

        await ValidarDatosGenerales(model);

        if (!ModelState.IsValid)
        {
            model.Codigo = solicitud.Codigo;
            ViewData["DesdeResumen"] = desdeResumen;
            await CargarListas(model);
            return View(nameof(Create), model);
        }

        AplicarDatosGenerales(solicitud, model);

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Datos generales actualizados.";

        return RedirectToAction(
            desdeResumen ? nameof(Resumen) : nameof(Montaje),
            new { id = solicitud.Id });
    }

    private async Task ValidarDatosGenerales(SolicitudEventoCreateViewModel model)
    {
        if (model.Fin <= model.Inicio)
        {
            ModelState.AddModelError(
                nameof(model.FechaFin),
                "La fecha y hora de finalización debe ser posterior al inicio.");
        }

        if (model.Inicio < Fechas.Ahora())
        {
            ModelState.AddModelError(
                nameof(model.FechaInicio),
                "La fecha de inicio no puede estar en el pasado.");
        }

        if (ModelState.IsValid)
        {
            await _disponibilidad.ValidarAsync(
                ModelState,
                model.SalonId,
                model.Inicio,
                model.Fin,
                model.CantidadPersonas,
                model.Id);
        }
    }

    private static void AplicarDatosGenerales(
        SolicitudEvento solicitud,
        SolicitudEventoCreateViewModel model)
    {
        solicitud.NombreEvento = model.NombreEvento.Trim();
        solicitud.FechaHoraInicio = model.Inicio;
        solicitud.FechaHoraFin = model.Fin;
        solicitud.CantidadPersonas = model.CantidadPersonas;
        solicitud.SalonId = model.SalonId;
        solicitud.DepartamentoCargoId = model.DepartamentoCargoId;
        solicitud.Otros = model.Otros;
        solicitud.Comentarios = model.Comentarios;
        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;
    }

    // ============================
    // DETALLE
    // ============================

    public async Task<IActionResult> Details(int id)
    {
        var solicitud = await MisSolicitudes()
            .ConDetalle()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        return View(solicitud);
    }

    // ============================
    // PASO 2: MONTAJE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Montaje(int id, bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(id);

        if (solicitud == null)
        {
            return NoEditable(id);
        }

        var guardados = await _context.SolicitudesMontaje
            .Where(x => x.SolicitudEventoId == id)
            .ToListAsync();

        var tipos = await _context.TiposMontaje
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre == SinMontaje)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var model = new SolicitudMontajeViewModel
        {
            SolicitudEventoId = solicitud.Id,
            Codigo = solicitud.Codigo,
            NombreEvento = solicitud.NombreEvento,
            Montajes = tipos.Select(tipo =>
            {
                var existente = guardados
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

        ViewData["DesdeResumen"] = desdeResumen;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Montaje(
        SolicitudMontajeViewModel model,
        bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(model.SolicitudEventoId);

        if (solicitud == null)
        {
            return NoEditable(model.SolicitudEventoId);
        }

        var nombres = await _context.TiposMontaje
            .Where(x => x.Activo)
            .ToDictionaryAsync(x => x.Id, x => x.Nombre);

        // Los nombres se toman de la BD, no del formulario
        foreach (var item in model.Montajes)
        {
            item.Nombre = nombres.GetValueOrDefault(item.TipoMontajeId, "");
        }

        var seleccionados = model.Montajes
            .Where(x => x.Seleccionado && x.Nombre != "")
            .ToList();

        if (!seleccionados.Any())
        {
            ModelState.AddModelError(
                string.Empty,
                "Seleccione al menos un tipo de montaje o la opción 'Sin montaje'.");
        }

        if (seleccionados.Any(x => x.Nombre == SinMontaje)
            && seleccionados.Count > 1)
        {
            ModelState.AddModelError(
                string.Empty,
                "Si selecciona 'Sin montaje', no puede seleccionar otros tipos de montaje.");
        }

        if (!ModelState.IsValid)
        {
            model.Codigo = solicitud.Codigo;
            model.NombreEvento = solicitud.NombreEvento;
            ViewData["DesdeResumen"] = desdeResumen;
            return View(model);
        }

        var existentes = await _context.SolicitudesMontaje
            .Where(x => x.SolicitudEventoId == solicitud.Id)
            .ToListAsync();

        _context.SolicitudesMontaje.RemoveRange(existentes);

        foreach (var item in seleccionados)
        {
            var sinMontaje = item.Nombre == SinMontaje;

            _context.SolicitudesMontaje.Add(new SolicitudMontaje
            {
                SolicitudEventoId = solicitud.Id,
                TipoMontajeId = item.TipoMontajeId,
                Cantidad = sinMontaje ? 1 : item.Cantidad,
                Observaciones = sinMontaje ? null : item.Observaciones
            });
        }

        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Montaje guardado.";

        return RedirectToAction(
            desdeResumen ? nameof(Resumen) : nameof(Servicios),
            new { id = solicitud.Id });
    }

    // ============================
    // PASO 3: SERVICIOS
    // ============================

    [HttpGet]
    public async Task<IActionResult> Servicios(int id, bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(id);

        if (solicitud == null)
        {
            return NoEditable(id);
        }

        var guardados = await _context.SolicitudesServicio
            .Where(x => x.SolicitudEventoId == id)
            .ToListAsync();

        var disponibles = await _context.Servicios
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre == SinServicios)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var model = new SolicitudServicioViewModel
        {
            SolicitudEventoId = solicitud.Id,
            Codigo = solicitud.Codigo,
            NombreEvento = solicitud.NombreEvento,
            Servicios = disponibles.Select(servicio =>
            {
                var existente = guardados
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

        ViewData["DesdeResumen"] = desdeResumen;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Servicios(
        SolicitudServicioViewModel model,
        bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(model.SolicitudEventoId);

        if (solicitud == null)
        {
            return NoEditable(model.SolicitudEventoId);
        }

        var nombres = await _context.Servicios
            .Where(x => x.Activo)
            .ToDictionaryAsync(x => x.Id, x => x.Nombre);

        foreach (var item in model.Servicios)
        {
            item.Nombre = nombres.GetValueOrDefault(item.ServicioId, "");
        }

        var seleccionados = model.Servicios
            .Where(x => x.Seleccionado && x.Nombre != "")
            .ToList();

        if (!seleccionados.Any())
        {
            ModelState.AddModelError(
                string.Empty,
                "Seleccione al menos un servicio o la opción 'Sin servicios adicionales'.");
        }

        if (seleccionados.Any(x => x.Nombre == SinServicios)
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
            ViewData["DesdeResumen"] = desdeResumen;
            return View(model);
        }

        var existentes = await _context.SolicitudesServicio
            .Where(x => x.SolicitudEventoId == solicitud.Id)
            .ToListAsync();

        _context.SolicitudesServicio.RemoveRange(existentes);

        foreach (var item in seleccionados)
        {
            var sinServicios = item.Nombre == SinServicios;

            _context.SolicitudesServicio.Add(new SolicitudServicio
            {
                SolicitudEventoId = solicitud.Id,
                ServicioId = item.ServicioId,
                Cantidad = sinServicios ? 1 : item.Cantidad,
                Observaciones = sinServicios ? null : item.Observaciones
            });
        }

        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Servicios guardados.";

        return RedirectToAction(
            desdeResumen ? nameof(Resumen) : nameof(Alimentacion),
            new { id = solicitud.Id });
    }

    // ============================
    // PASO 4: ALIMENTACIÓN
    // ============================

    [HttpGet]
    public async Task<IActionResult> Alimentacion(int id, bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(id);

        if (solicitud == null)
        {
            return NoEditable(id);
        }

        var a = solicitud.Alimentacion;

        var model = new SolicitudAlimentacionViewModel
        {
            SolicitudEventoId = solicitud.Id,
            Codigo = solicitud.Codigo,
            NombreEvento = solicitud.NombreEvento,
            RequiereAlimentacion = a?.RequiereAlimentacion,
            CantidadPersonas = a?.CantidadPersonas ?? solicitud.CantidadPersonas,
            HoraServicio = a?.HoraServicio,
            Detalle = a?.Detalle,
            IncluyeAgua = a?.IncluyeAgua ?? false,
            IncluyeCafe = a?.IncluyeCafe ?? false,
            IncluyeRefrigerio = a?.IncluyeRefrigerio ?? false,
            Observaciones = a?.Observaciones
        };

        ViewData["DesdeResumen"] = desdeResumen;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alimentacion(
        SolicitudAlimentacionViewModel model,
        bool desdeResumen = false)
    {
        var solicitud = await ObtenerBorradorAsync(model.SolicitudEventoId);

        if (solicitud == null)
        {
            return NoEditable(model.SolicitudEventoId);
        }

        if (model.RequiereAlimentacion == true)
        {
            if (model.CantidadPersonas is null or <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.CantidadPersonas),
                    "Indique la cantidad de personas.");
            }

            if (string.IsNullOrWhiteSpace(model.Detalle))
            {
                ModelState.AddModelError(
                    nameof(model.Detalle),
                    "Indique el detalle de la alimentación.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.Codigo = solicitud.Codigo;
            model.NombreEvento = solicitud.NombreEvento;
            ViewData["DesdeResumen"] = desdeResumen;
            return View(model);
        }

        var alimentacion = solicitud.Alimentacion
            ?? new AlimentacionEvento { SolicitudEventoId = solicitud.Id };

        var requiere = model.RequiereAlimentacion!.Value;

        alimentacion.RequiereAlimentacion = requiere;
        alimentacion.CantidadPersonas = requiere ? model.CantidadPersonas : null;
        alimentacion.HoraServicio = requiere ? model.HoraServicio : null;
        alimentacion.Detalle = requiere ? model.Detalle : null;
        alimentacion.IncluyeAgua = requiere && model.IncluyeAgua;
        alimentacion.IncluyeCafe = requiere && model.IncluyeCafe;
        alimentacion.IncluyeRefrigerio = requiere && model.IncluyeRefrigerio;
        alimentacion.Observaciones = requiere ? model.Observaciones : null;

        if (solicitud.Alimentacion == null)
        {
            _context.AlimentacionesEvento.Add(alimentacion);
        }

        solicitud.FechaActualizacion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Resumen), new { id = solicitud.Id });
    }

    // ============================
    // PASO 5: RESUMEN Y ENVÍO
    // ============================

    [HttpGet]
    public async Task<IActionResult> Resumen(int id)
    {
        var solicitud = await MisSolicitudes()
            .ConDetalle()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        if (solicitud.Estado.Nombre != Estados.Borrador)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewData["Disponibilidad"] = await _disponibilidad.VerificarAsync(
            solicitud.SalonId,
            solicitud.FechaHoraInicio,
            solicitud.FechaHoraFin,
            solicitud.Id);

        return View(solicitud);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(int id)
    {
        var solicitud = await MisSolicitudes()
            .Include(x => x.Estado)
            .Include(x => x.Montajes)
            .Include(x => x.Servicios)
            .Include(x => x.Alimentacion)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        if (solicitud.Estado.Nombre != Estados.Borrador)
        {
            return NoEditable(id);
        }

        string? error = null;

        if (!solicitud.Montajes.Any())
            error = "Configure el montaje antes de enviar la solicitud.";
        else if (!solicitud.Servicios.Any())
            error = "Configure los servicios antes de enviar la solicitud.";
        else if (solicitud.Alimentacion == null)
            error = "Complete la información de alimentación antes de enviar la solicitud.";
        else if (solicitud.FechaHoraInicio < Fechas.Ahora())
            error = "La fecha del evento ya pasó. Edite los datos generales.";

        if (error == null)
        {
            // Revalidar: otro evento pudo aprobarse mientras era borrador
            await _disponibilidad.ValidarAsync(
                ModelState,
                solicitud.SalonId,
                solicitud.FechaHoraInicio,
                solicitud.FechaHoraFin,
                solicitud.CantidadPersonas,
                solicitud.Id);

            if (!ModelState.IsValid)
            {
                error = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }
        }

        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Resumen), new { id });
        }

        await _estados.CambiarEstadoAsync(
            solicitud,
            Estados.Enviada,
            Actor,
            "Solicitud enviada a Coordinación.");

        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "¡Solicitud enviada! Coordinación la revisará y verás aquí cada cambio de estado.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ============================
    // CANCELAR
    // ============================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string? motivo)
    {
        var solicitud = await MisSolicitudes()
            .Include(x => x.Estado)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud == null)
        {
            return NotFound();
        }

        if (!Estados.Cancelables.Contains(solicitud.Estado.Nombre)
            || solicitud.FechaHoraFin < Fechas.Ahora())
        {
            TempData["Error"] = "Esta solicitud ya no puede cancelarse.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _estados.CambiarEstadoAsync(
            solicitud,
            Estados.Cancelada,
            Actor,
            string.IsNullOrWhiteSpace(motivo)
                ? "Cancelada por el solicitante."
                : motivo.Trim());

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La solicitud fue cancelada y el salón quedó liberado.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ============================
    // AUXILIARES
    // ============================

    private async Task CargarListas(SolicitudEventoCreateViewModel model)
    {
        model.Salones = await _context.SalonesParaReservaAsync();

        model.Departamentos = await _context.Departamentos
            .Where(x => x.Activo
                && x.Id != UsuarioSincronizacionService.DepartamentoSinAsignarId)
            .OrderBy(x => x.Nombre)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Nombre
            })
            .ToListAsync();
    }
}
