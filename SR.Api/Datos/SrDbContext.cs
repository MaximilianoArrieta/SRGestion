using Microsoft.EntityFrameworkCore;

namespace SR.Api.Datos;

public class SrDbContext(DbContextOptions<SrDbContext> options) : DbContext(options)
{
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Localidad> Localidades => Set<Localidad>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<TipoMaquina> TiposMaquina => Set<TipoMaquina>();
    public DbSet<Maquina> Maquinas => Set<Maquina>();
    public DbSet<Instalacion> Instalaciones => Set<Instalacion>();
    public DbSet<Cobranza> Cobranzas => Set<Cobranza>();
    public DbSet<Mantenimiento> Mantenimientos => Set<Mantenimiento>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Reporte> Reportes => Set<Reporte>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Provincia>(e =>
        {
            e.ToTable("Provincia");
            e.HasKey(x => x.IdProvincia);
            e.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        });

        mb.Entity<Localidad>(e =>
        {
            e.ToTable("Localidad");
            e.HasKey(x => x.IdLocalidad);
            e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            e.HasOne(x => x.Provincia).WithMany(p => p.Localidades).HasForeignKey(x => x.IdProvincia);
        });

        mb.Entity<Cliente>(e =>
        {
            e.ToTable("Cliente");
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.RazonSocial).HasMaxLength(120).IsRequired();
            e.Property(x => x.Responsable).HasMaxLength(100).IsRequired();
            e.Property(x => x.Telefono).HasMaxLength(30).IsRequired();
            e.Property(x => x.Direccion).HasMaxLength(150).IsRequired();
            e.Property(x => x.DatosFiscales).HasMaxLength(60);
            e.HasOne(x => x.Localidad).WithMany().HasForeignKey(x => x.IdLocalidad);
        });

        mb.Entity<TipoMaquina>(e =>
        {
            e.ToTable("TipoMaquina");
            e.HasKey(x => x.IdTipoMaquina);
            e.Property(x => x.Descripcion).HasMaxLength(60).IsRequired();
        });

        mb.Entity<Maquina>(e =>
        {
            e.ToTable("Maquina");
            e.HasKey(x => x.IdMaquina);
            e.Property(x => x.NumeroIdentificacion).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.NumeroIdentificacion).IsUnique();
            e.Property(x => x.Descripcion).HasMaxLength(200);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.FechaAlta).HasColumnType("date");
            e.HasOne(x => x.TipoMaquina).WithMany().HasForeignKey(x => x.IdTipoMaquina);
        });

        mb.Entity<Instalacion>(e =>
        {
            e.ToTable("Instalacion");
            e.HasKey(x => x.IdInstalacion);
            e.Property(x => x.FechaInstalacion).HasColumnType("date");
            e.Property(x => x.FechaRetiroProgramada).HasColumnType("date");
            e.Property(x => x.FechaRetiroReal).HasColumnType("date");
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Maquina).WithMany(m => m.Instalaciones).HasForeignKey(x => x.IdMaquina);
            e.HasOne(x => x.Cliente).WithMany(c => c.Instalaciones).HasForeignKey(x => x.IdCliente);
        });

        mb.Entity<Cobranza>(e =>
        {
            e.ToTable("Cobranza");
            e.HasKey(x => x.IdCobranza);
            e.Property(x => x.FechaCobro).HasColumnType("date");
            e.Property(x => x.MontoRecaudado).HasPrecision(14, 2);
            e.Property(x => x.MontoComercio).HasPrecision(14, 2);
            e.Property(x => x.Observaciones).HasMaxLength(300);
            e.HasOne(x => x.Maquina).WithMany(m => m.Cobranzas).HasForeignKey(x => x.IdMaquina);
            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        });

        mb.Entity<Mantenimiento>(e =>
        {
            e.ToTable("Mantenimiento");
            e.HasKey(x => x.IdMantenimiento);
            e.Property(x => x.Fecha).HasColumnType("date");
            e.Property(x => x.TipoMantenimiento).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Descripcion).HasMaxLength(400).IsRequired();
            e.Property(x => x.Costo).HasPrecision(14, 2);
            e.HasOne(x => x.Maquina).WithMany(m => m.Mantenimientos).HasForeignKey(x => x.IdMaquina);
        });

        mb.Entity<Solicitud>(e =>
        {
            e.ToTable("Solicitud");
            e.HasKey(x => x.IdSolicitud);
            e.Property(x => x.TipoSolicitud).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Observaciones).HasMaxLength(400);
            e.Property(x => x.Respuesta).HasMaxLength(400);
            e.HasOne(x => x.Cliente).WithMany(c => c.Solicitudes).HasForeignKey(x => x.IdCliente);
            e.HasOne(x => x.Maquina).WithMany().HasForeignKey(x => x.IdMaquina);
        });

        mb.Entity<Rol>(e =>
        {
            e.ToTable("Rol");
            e.HasKey(x => x.IdRol);
            e.Property(x => x.NombreRol).HasMaxLength(30).IsRequired();
        });

        mb.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.Property(x => x.Nombre).HasMaxLength(60);
            e.Property(x => x.Apellido).HasMaxLength(60);
            e.Property(x => x.Email).HasMaxLength(120);
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol);
            e.HasOne(x => x.Cliente).WithMany(c => c.Usuarios).HasForeignKey(x => x.IdCliente);
        });

        mb.Entity<Reporte>(e =>
        {
            e.ToTable("Reporte");
            e.HasKey(x => x.IdReporte);
            e.Property(x => x.TipoReporte).HasMaxLength(60).IsRequired();
            e.Property(x => x.Filtros).HasMaxLength(200);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);
        });

        foreach (var fk in mb.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
