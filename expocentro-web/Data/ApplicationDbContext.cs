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
}