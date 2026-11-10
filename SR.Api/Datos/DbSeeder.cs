using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SR.Shared;

namespace SR.Api.Datos;

public static class DbSeeder
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SrDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        if (await db.Roles.AnyAsync()) return;

        var hasher = new PasswordHasher<Usuario>();

        var rolAdmin = new Rol { NombreRol = Roles.Admin };
        var rolCliente = new Rol { NombreRol = Roles.Cliente };
        db.Roles.AddRange(rolAdmin, rolCliente);

        Provincia Prov(string nombre, params string[] localidades) => new()
        {
            Nombre = nombre,
            Localidades = localidades.Select(l => new Localidad { Nombre = l }).ToList()
        };
        var laPampa = Prov("La Pampa", "Santa Rosa", "General Pico", "Toay", "Realicó", "General Acha");
        var bsAs = Prov("Buenos Aires", "Bahía Blanca", "Trenque Lauquen", "Pehuajó", "La Plata", "Mar del Plata");
        var cordoba = Prov("Córdoba", "Córdoba Capital", "Río Cuarto", "Villa María", "Laboulaye");
        db.Provincias.AddRange(laPampa, bsAs, cordoba);

        var pool = new TipoMaquina { Descripcion = "Pool" };
        var metegol = new TipoMaquina { Descripcion = "Metegol" };
        var arcade = new TipoMaquina { Descripcion = "Arcade" };
        var otro = new TipoMaquina { Descripcion = "Otro" };
        db.TiposMaquina.AddRange(pool, metegol, arcade, otro);

        var admin = new Usuario
        {
            NombreUsuario = config["Seed:AdminUsuario"] ?? "admin",
            Nombre = "Administrador",
            Apellido = "SR",
            Email = "admin@sr-recreativas.com",
            Rol = rolAdmin
        };
        admin.PasswordHash = hasher.HashPassword(admin, config["Seed:AdminPassword"] ?? "Admin123!");
        db.Usuarios.Add(admin);

        if (!config.GetValue("Seed:DatosDeEjemplo", true))
        {
            await db.SaveChangesAsync();
            return;
        }

        Localidad Loc(Provincia p, string n) => p.Localidades.First(l => l.Nombre == n);

        var c1 = new Cliente { RazonSocial = "Bar El Taco", Responsable = "Jorge Pérez", Telefono = "2954 41-2233", Direccion = "Av. San Martín 450", DatosFiscales = "20-28444555-3", Localidad = Loc(laPampa, "Santa Rosa") };
        var c2 = new Cliente { RazonSocial = "Club Atlético Pico", Responsable = "Laura Gómez", Telefono = "2302 43-1188", Direccion = "Calle 15 N° 1020", Localidad = Loc(laPampa, "General Pico") };
        var c3 = new Cliente { RazonSocial = "Salón Recreativo Sur", Responsable = "Martín Díaz", Telefono = "291 455-9012", Direccion = "Alsina 230", Localidad = Loc(bsAs, "Bahía Blanca") };
        var c4 = new Cliente { RazonSocial = "Cervecería La Esquina", Responsable = "Sofía Ruiz", Telefono = "358 462-7744", Direccion = "Constitución 880", Localidad = Loc(cordoba, "Río Cuarto") };
        db.Clientes.AddRange(c1, c2, c3, c4);

        var hoy = DateTime.Today;
        Maquina Maq(string nro, TipoMaquina t, string desc, int mesesAlta) =>
            new() { NumeroIdentificacion = nro, TipoMaquina = t, Descripcion = desc, FechaAlta = hoy.AddMonths(-mesesAlta) };

        var m1 = Maq("POOL-001", pool, "Mesa de pool profesional, paño verde", 30);
        var m2 = Maq("POOL-002", pool, "Mesa de pool de bar", 24);
        var m3 = Maq("MET-001", metegol, "Metegol profesional", 20);
        var m4 = Maq("ARC-001", arcade, "Arcade multijuego", 18);
        var m5 = Maq("ARC-002", arcade, "Arcade de carreras", 12);
        var m6 = Maq("MET-002", metegol, "Metegol de bar", 10);
        var m7 = Maq("OTR-001", otro, "Hockey de mesa", 6);
        var m8 = Maq("POOL-003", pool, "Mesa de pool de bar", 3);
        db.Maquinas.AddRange(m1, m2, m3, m4, m5, m6, m7, m8);

        void Instalar(Maquina m, Cliente c, int mesesAtras, int? retiroEnDias)
        {
            m.Estado = EstadoMaquina.Instalada;
            db.Instalaciones.Add(new Instalacion
            {
                Maquina = m, Cliente = c,
                FechaInstalacion = hoy.AddMonths(-mesesAtras),
                FechaRetiroProgramada = retiroEnDias is null ? null : hoy.AddDays(retiroEnDias.Value)
            });
        }
        Instalar(m1, c1, 8, null);
        Instalar(m3, c1, 6, 20);
        Instalar(m2, c2, 5, null);
        Instalar(m4, c3, 7, 45);
        Instalar(m5, c3, 4, null);
        Instalar(m6, c4, 3, 10);
        m7.Estado = EstadoMaquina.EnReparacion;

        db.Instalaciones.Add(new Instalacion
        {
            Maquina = m8, Cliente = c2,
            FechaInstalacion = hoy.AddMonths(-3), FechaRetiroProgramada = hoy.AddMonths(-1),
            FechaRetiroReal = hoy.AddMonths(-1), Estado = EstadoInstalacion.Finalizada
        });

        var rnd = new Random(7);
        var instaladas = new (Maquina m, Cliente c)[] { (m1, c1), (m3, c1), (m2, c2), (m4, c3), (m5, c3), (m6, c4) };
        for (int mes = 4; mes >= 0; mes--)
        {
            foreach (var (m, c) in instaladas)
            {
                var fecha = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-mes).AddDays(rnd.Next(2, 25));
                if (fecha > hoy) continue;
                var monto = rnd.Next(40, 160) * 1000m;
                db.Cobranzas.Add(new Cobranza
                {
                    Maquina = m, Cliente = c, Usuario = admin, FechaCobro = fecha,
                    MontoRecaudado = monto, MontoComercio = Math.Round(monto * 0.4m, 0)
                });
            }
        }

        db.Mantenimientos.AddRange(
            new Mantenimiento { Maquina = m7, Fecha = hoy.AddDays(-4), TipoMantenimiento = TipoMantenimiento.Correctivo, Descripcion = "Cambio de motor del disco de aire", Costo = 85000, Estado = EstadoMantenimiento.EnProceso },
            new Mantenimiento { Maquina = m1, Fecha = hoy.AddMonths(-2), TipoMantenimiento = TipoMantenimiento.Preventivo, Descripcion = "Cambio de paño y nivelación", Costo = 120000, Estado = EstadoMantenimiento.Finalizado },
            new Mantenimiento { Maquina = m4, Fecha = hoy.AddDays(-1), TipoMantenimiento = TipoMantenimiento.Correctivo, Descripcion = "Revisar monedero, no acepta fichas", Costo = 0, Estado = EstadoMantenimiento.Pendiente });

        db.Solicitudes.AddRange(
            new Solicitud { Cliente = c1, Maquina = m3, TipoSolicitud = TipoSolicitud.Mantenimiento, FechaSolicitud = hoy.AddDays(-2), Observaciones = "Una de las barras está floja." },
            new Solicitud { Cliente = c3, TipoSolicitud = TipoSolicitud.NuevaMaquina, FechaSolicitud = hoy.AddDays(-5), Observaciones = "Queremos sumar un metegol para el verano." });

        var usuarioCliente = new Usuario
        {
            NombreUsuario = "eltaco", Nombre = "Jorge", Apellido = "Pérez",
            Email = "eltaco@ejemplo.com", Rol = rolCliente, Cliente = c1
        };
        usuarioCliente.PasswordHash = hasher.HashPassword(usuarioCliente, "Cliente123!");
        db.Usuarios.Add(usuarioCliente);

        await db.SaveChangesAsync();
    }
}
