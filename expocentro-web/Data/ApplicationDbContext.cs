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




        // Índices únicos
        modelBuilder.Entity<SolicitudEvento>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.Correo)
            .IsUnique();

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
            }
        );

        modelBuilder.Entity<Salon>().HasData(
            new Salon
            {
                Id = 1,
                Nombre = "Centro de Convenciones Felipe Arguello",
                Descripcion = "Salón para eventos y actividades institucionales.",
                CapacidadMaxima = 500,
                Activo = true
            },
            new Salon
            {
                Id = 2,
                Nombre = "Los Zorzales",
                Descripcion = "Espacio para reuniones y eventos.",
                CapacidadMaxima = 150,
                Activo = true
            }
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
        // Usuario temporal hasta implementar Entra ID
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Usuario Demo",
                Correo = "usuario.demo@ccichonduras.org",
                DepartamentoId = 1,
                Rol = "Solicitante",
                Activo = true
            }
        );
    }
}