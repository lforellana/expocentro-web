using Expocentro.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Expocentro.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Salon> Salones => Set<Salon>();
    public DbSet<EstadoSolicitud> EstadosSolicitud => Set<EstadoSolicitud>();
    public DbSet<SolicitudEvento> SolicitudesEvento => Set<SolicitudEvento>();

    public DbSet<TipoMontaje> TiposMontaje => Set<TipoMontaje>();
    public DbSet<SolicitudMontaje> SolicitudesMontaje => Set<SolicitudMontaje>();

public DbSet<Servicio> Servicios => Set<Servicio>();

public DbSet<SolicitudServicio> SolicitudesServicio
    => Set<SolicitudServicio>();

    public DbSet<AlimentacionEvento> AlimentacionesEvento
    => Set<AlimentacionEvento>();

    public DbSet<HistorialEstadoSolicitud> HistorialEstadosSolicitud
    => Set<HistorialEstadoSolicitud>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    // Fechas y horas locales del evento
    modelBuilder.Entity<SolicitudEvento>()
        .Property(x => x.FechaHoraInicio)
        .HasColumnType("timestamp without time zone");

    modelBuilder.Entity<SolicitudEvento>()
        .Property(x => x.FechaHoraFin)
        .HasColumnType("timestamp without time zone");


        // Eventos externos y referencia SAP
        modelBuilder.Entity<SolicitudEvento>(e =>
        {
            e.Property(x => x.TipoEvento)
                .HasDefaultValue(TipoEvento.Interno)
                .HasSentinel(0);

            e.Property(x => x.ClienteNombre).HasMaxLength(200);
            e.Property(x => x.ClienteContacto).HasMaxLength(150);
            e.Property(x => x.ClienteTelefono).HasMaxLength(50);
            e.Property(x => x.ClienteCorreo).HasMaxLength(150);
            e.Property(x => x.ReferenciaSap).HasMaxLength(50);

            e.HasIndex(x => x.ReferenciaSap)
                .IsUnique()
                .HasFilter("\"ReferenciaSap\" IS NOT NULL");

            // Búsqueda de choques de horario por salón
            e.HasIndex(x => new { x.SalonId, x.FechaHoraInicio, x.FechaHoraFin });
        });

        // Relaciones
        modelBuilder.Entity<SolicitudEvento>()
            .HasOne(x => x.SolicitanteUsuario)
            .WithMany()
            .HasForeignKey(x => x.SolicitanteUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudEvento>()
            .HasOne(x => x.ResponsableUsuario)
            .WithMany()
            .HasForeignKey(x => x.ResponsableUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);


// ============================
// MONTAJES
// ============================

modelBuilder.Entity<SolicitudMontaje>()
    .HasOne(x => x.SolicitudEvento)
    .WithMany(x => x.Montajes)
    .HasForeignKey(x => x.SolicitudEventoId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<SolicitudMontaje>()
    .HasOne(x => x.TipoMontaje)
    .WithMany(x => x.SolicitudesMontaje)
    .HasForeignKey(x => x.TipoMontajeId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<SolicitudMontaje>()
    .HasIndex(x => new
    {
        x.SolicitudEventoId,
        x.TipoMontajeId
    })
    .IsUnique();

// ============================
// SERVICIOS
// ============================

modelBuilder.Entity<SolicitudServicio>()
    .HasOne(x => x.SolicitudEvento)
    .WithMany(x => x.Servicios)
    .HasForeignKey(x => x.SolicitudEventoId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<SolicitudServicio>()
    .HasOne(x => x.Servicio)
    .WithMany(x => x.SolicitudesServicio)
    .HasForeignKey(x => x.ServicioId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<SolicitudServicio>()
    .HasIndex(x => new
    {
        x.SolicitudEventoId,
        x.ServicioId
    })
    .IsUnique();


        // Índices únicos
        modelBuilder.Entity<SolicitudEvento>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.Correo)
            .IsUnique();



// ============================
// ALIMENTACIÓN
// ============================

modelBuilder.Entity<AlimentacionEvento>()
    .HasOne(x => x.SolicitudEvento)
    .WithOne(x => x.Alimentacion)
    .HasForeignKey<AlimentacionEvento>(
        x => x.SolicitudEventoId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<AlimentacionEvento>()
    .HasIndex(x => x.SolicitudEventoId)
    .IsUnique();
    

// ============================
// HISTORIAL DE ESTADOS
// ============================

modelBuilder.Entity<HistorialEstadoSolicitud>()
    .HasOne(x => x.SolicitudEvento)
    .WithMany(x => x.HistorialEstados)
    .HasForeignKey(x => x.SolicitudEventoId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<HistorialEstadoSolicitud>()
    .HasOne(x => x.EstadoSolicitud)
    .WithMany()
    .HasForeignKey(x => x.EstadoSolicitudId)
    .OnDelete(DeleteBehavior.Restrict);

        // ============================
        // SALONES COMBINADOS
        // ============================

        modelBuilder.Entity<Salon>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(150);

            e.HasOne(x => x.SalonPadre)
                .WithMany(x => x.Subsalones)
                .HasForeignKey(x => x.SalonPadreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================
        // DATOS INICIALES
        // ============================

        modelBuilder.Entity<Departamento>().HasData(
            new Departamento
            {
                Id = 1,
                Nombre = "IT",
                Activo = true
            },
            new Departamento
            {
                Id = 2,
                Nombre = "Administración",
                Activo = true
            },
            new Departamento
            {
                Id = 3,
                Nombre = "Desarrollo Empresarial",
                Activo = true
            },
            new Departamento
            {
                Id = 4,
                Nombre = "Capacitaciones",
                Activo = true
            },
            new Departamento
            {
                // Usuarios nuevos de Entra ID hasta que se les asigne uno
                Id = 5,
                Nombre = "Sin asignar",
                Activo = true
            },
            new Departamento { Id = 6, Nombre = "Logística", Activo = true },
            new Departamento { Id = 7, Nombre = "Presidencia", Activo = true },
            new Departamento { Id = 8, Nombre = "Recursos Humanos y RSE", Activo = true },
            new Departamento { Id = 9, Nombre = "Servicios Generales", Activo = true },
            new Departamento { Id = 10, Nombre = "Bazar del Sábado", Activo = true },
            new Departamento { Id = 11, Nombre = "Eventos", Activo = true },
            new Departamento { Id = 12, Nombre = "Cobros", Activo = true },
            new Departamento { Id = 13, Nombre = "Comunicaciones y Mercadeo", Activo = true },
            new Departamento { Id = 14, Nombre = "Compras", Activo = true },
            new Departamento { Id = 15, Nombre = "Contabilidad", Activo = true },
            new Departamento { Id = 16, Nombre = "Legal", Activo = true }
        );

        // Capacidad null = por definir (se completa en Administración → Salones)
        modelBuilder.Entity<Salon>().HasData(
            new Salon
            {
                Id = 1,
                Nombre = "Centro de convenciones Felipe Arguello",
                Descripcion = "Salón para eventos y actividades institucionales.",
                CapacidadMaxima = 500,
                Activo = true
            },
            new Salon
            {
                Id = 2,
                Nombre = "Edificio Los Zorzales",
                Descripcion = "Espacio para reuniones y eventos.",
                CapacidadMaxima = 150,
                Activo = true
            },
            new Salon { Id = 3, Nombre = "Sala Rafael Pastor", Activo = true },
            new Salon { Id = 4, Nombre = "Sala Mario Belot", Activo = true },
            new Salon { Id = 5, Nombre = "Sala Alberto Smit", Activo = true },
            new Salon { Id = 6, Nombre = "Sala VIP #1 Carlos Katan", Activo = true },
            new Salon { Id = 7, Nombre = "Sala VIP #2 Roberto Reyes Silva", Activo = true },
            new Salon { Id = 8, Nombre = "Salon Forjadores 1", SalonPadreId = 18, Activo = true },
            new Salon { Id = 9, Nombre = "Salon Forjadores 2", SalonPadreId = 18, Activo = true },
            new Salon { Id = 10, Nombre = "Salon Forjadores 3", SalonPadreId = 18, Activo = true },
            new Salon { Id = 11, Nombre = "Sala de Juntas Cervecería Hondureña", Activo = true },
            new Salon { Id = 12, Nombre = "Edificio Los Laureles", Activo = true },
            new Salon { Id = 13, Nombre = "Emprendedores 2", SalonPadreId = 16, Activo = true },
            new Salon { Id = 14, Nombre = "SALA VIP #3", Activo = true },
            new Salon { Id = 15, Nombre = "SALA VIP #4", Activo = true },
            new Salon { Id = 16, Nombre = "Centro de Convenciones Emprendedores", Activo = true },
            new Salon { Id = 17, Nombre = "Emprendedores 1", SalonPadreId = 16, Activo = true },
            new Salon { Id = 18, Nombre = "Salon Forjadores Completo", Activo = true },
            new Salon { Id = 19, Nombre = "Emprendedores 3", SalonPadreId = 16, Activo = true }
        );

        modelBuilder.Entity<EstadoSolicitud>().HasData(
            new EstadoSolicitud
            {
                Id = 1,
                Nombre = "Borrador",
                Descripcion = "Solicitud todavía no enviada.",
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = 2,
                Nombre = "Enviada",
                Descripcion = "Solicitud enviada para revisión.",
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = 3,
                Nombre = "En revisión",
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = 4,
                Nombre = "Aprobada",
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = 5,
                Nombre = "Rechazada",
                Activo = true
            },
            new EstadoSolicitud
            {
                Id = 6,
                Nombre = "Cancelada",
                Descripcion = "Solicitud cancelada por el solicitante o Coordinación.",
                Activo = true
            }

            
        );

modelBuilder.Entity<TipoMontaje>().HasData(
    new TipoMontaje
    {
        Id = 1,
        Nombre = "Auditorio",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 2,
        Nombre = "Escuela",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 3,
        Nombre = "Banquete",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 4,
        Nombre = "Imperial",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 5,
        Nombre = "Mesa en U",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 6,
        Nombre = "Mesa principal",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 7,
        Nombre = "Mesa de inscripción",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 8,
        Nombre = "Mesa coctelera",
        Activo = true
    },
    new TipoMontaje
    {
        Id = 9,
        Nombre = "Buffet",
        Activo = true
    },
    new TipoMontaje
{
    Id = 10,
    Nombre = "Sin montaje",
    Activo = true
}
    
);
modelBuilder.Entity<Servicio>().HasData(
    new Servicio
    {
        Id = 1,
        Nombre = "Pantalla",
        Activo = true
    },
    new Servicio
    {
        Id = 2,
        Nombre = "Data Show",
        Activo = true
    },
    new Servicio
    {
        Id = 3,
        Nombre = "Audio",
        Activo = true
    },
    new Servicio
    {
        Id = 4,
        Nombre = "Micrófono",
        Activo = true
    },
    new Servicio
    {
        Id = 5,
        Nombre = "Computadora",
        Activo = true
    },
    new Servicio
    {
        Id = 6,
        Nombre = "Pizarra",
        Activo = true
    },
    new Servicio
    {
        Id = 7,
        Nombre = "Videoconferencia",
        Activo = true
    },
    new Servicio
    {
        Id = 8,
        Nombre = "Internet",
        Activo = true
    },
    new Servicio
    {
        Id = 9,
        Nombre = "Sin servicios adicionales",
        Activo = true
    }
);

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.EntraObjectId)
            .IsUnique()
            .HasFilter("\"EntraObjectId\" IS NOT NULL");

        // Usuarios de prueba para el acceso de desarrollo (sin Entra ID)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Usuario Demo",
                Correo = "usuario.demo@ccichonduras.org",
                DepartamentoId = 1,
                Rol = "Solicitante",
                Activo = true
            },
            new Usuario
            {
                Id = 2,
                Nombre = "Coordinador Demo",
                Correo = "coordinador.demo@ccichonduras.org",
                DepartamentoId = 2,
                Rol = "Coordinador",
                Activo = true
            },
            new Usuario
            {
                Id = 3,
                Nombre = "Administrador Demo",
                Correo = "admin.demo@ccichonduras.org",
                DepartamentoId = 1,
                Rol = "Administrador",
                Activo = true
            }
        );
    }
}