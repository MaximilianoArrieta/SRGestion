using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Api.Servicios;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/reportes")]
[Authorize]
public class ReportesController(SrDbContext db, PdfService pdf) : BaseController
{
    [HttpGet("recaudacion")]
    [Authorize(Roles = Roles.Admin)]
    public Task<List<RecaudacionPeriodoDto>> Recaudacion(DateTime? desde, DateTime? hasta) => CalcularRecaudacion(desde, hasta);

    [HttpGet("recaudacion/pdf")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<FileContentResult> RecaudacionPdf(DateTime? desde, DateTime? hasta)
    {
        var datos = await CalcularRecaudacion(desde, hasta);
        var bytes = pdf.Tabla("Recaudación por período", Rango(desde, hasta),
            ["Mes", "Cobranzas", "Recaudado", "Comercios", "SR"],
            datos.Select(d => new[] { d.Periodo, d.Cobranzas.ToString(), Textos.Plata(d.Recaudado), Textos.Plata(d.Comercios), Textos.Plata(d.Empresa) }),
            ["Total", datos.Sum(d => d.Cobranzas).ToString(), Textos.Plata(datos.Sum(d => d.Recaudado)), Textos.Plata(datos.Sum(d => d.Comercios)), Textos.Plata(datos.Sum(d => d.Empresa))],
            [1, 2, 3, 4]);
        return await Entregar(bytes, "recaudacion", "Recaudación por período", Rango(desde, hasta));
    }

    async Task<List<RecaudacionPeriodoDto>> CalcularRecaudacion(DateTime? desde, DateTime? hasta)
    {
        var grupos = await CobranzasController.Filtrar(db.Cobranzas, desde, hasta, null, null)
            .GroupBy(c => new { c.FechaCobro.Year, c.FechaCobro.Month })
            .Select(g => new
            {
                g.Key.Year, g.Key.Month,
                Cantidad = g.Count(),
                Recaudado = g.Sum(c => c.MontoRecaudado),
                Comercios = g.Sum(c => c.MontoComercio)
            })
            .ToListAsync();

        return grupos
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .Select(g => new RecaudacionPeriodoDto
            {
                Periodo = $"{g.Month:00}/{g.Year}",
                Cobranzas = g.Cantidad,
                Recaudado = g.Recaudado,
                Comercios = g.Comercios,
                Empresa = g.Recaudado - g.Comercios
            })
            .ToList();
    }

    [HttpGet("cobranzas/pdf")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<FileContentResult> CobranzasPdf(DateTime? desde, DateTime? hasta)
    {
        var datos = await CobranzasController.Filtrar(db.Cobranzas, desde, hasta, null, null)
            .OrderByDescending(c => c.FechaCobro).Select(Mapeos.ACobranza).ToListAsync();
        var bytes = pdf.Tabla("Historial de cobranzas", Rango(desde, hasta),
            ["Fecha", "Máquina", "Comercio", "Localidad", "Recaudado", "Comercio", "SR"],
            datos.Select(c => new[] { Textos.Fecha(c.FechaCobro), c.Maquina, c.Cliente, c.Localidad, Textos.Plata(c.MontoRecaudado), Textos.Plata(c.MontoComercio), Textos.Plata(c.MontoEmpresa) }),
            ["Total", $"{datos.Count} cobros", "", "", Textos.Plata(datos.Sum(c => c.MontoRecaudado)), Textos.Plata(datos.Sum(c => c.MontoComercio)), Textos.Plata(datos.Sum(c => c.MontoEmpresa))],
            [4, 5, 6]);
        return await Entregar(bytes, "cobranzas", "Historial de cobranzas", Rango(desde, hasta));
    }

    [HttpGet("mis-cobranzas/pdf")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<FileContentResult> MisCobranzasPdf(DateTime? desde, DateTime? hasta)
    {
        var idCliente = IdClienteUsuario ?? -1;
        var comercio = await db.Clientes.Where(c => c.IdCliente == idCliente).Select(c => c.RazonSocial).FirstOrDefaultAsync() ?? "";
        var datos = await CobranzasController.Filtrar(db.Cobranzas, desde, hasta, idCliente, null)
            .OrderByDescending(c => c.FechaCobro).Select(Mapeos.ACobranza).ToListAsync();
        var bytes = pdf.Tabla("Historial de cobranzas", $"{comercio} · {Rango(desde, hasta)}",
            ["Fecha", "Máquina", "Tipo", "Recaudado", "Pagado al comercio"],
            datos.Select(c => new[] { Textos.Fecha(c.FechaCobro), c.Maquina, c.Tipo, Textos.Plata(c.MontoRecaudado), Textos.Plata(c.MontoComercio) }),
            ["Total", "", "", Textos.Plata(datos.Sum(c => c.MontoRecaudado)), Textos.Plata(datos.Sum(c => c.MontoComercio))],
            [3, 4]);
        return await Entregar(bytes, "mis-cobranzas", "Historial de cobranzas (cliente)", $"{comercio} · {Rango(desde, hasta)}");
    }

    [HttpGet("maquinas-localidad")]
    [Authorize(Roles = Roles.Admin)]
    public Task<List<MaquinasLocalidadDto>> MaquinasLocalidad() => CalcularPorLocalidad();

    [HttpGet("maquinas-localidad/pdf")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<FileContentResult> MaquinasLocalidadPdf()
    {
        var datos = await CalcularPorLocalidad();
        var bytes = pdf.Tabla("Máquinas instaladas por localidad", $"Situación al {DateTime.Today:dd/MM/yyyy}",
            ["Provincia", "Localidad", "Pool", "Metegol", "Arcade", "Otros", "Total"],
            datos.Select(d => new[] { d.Provincia, d.Localidad, d.Pool.ToString(), d.Metegol.ToString(), d.Arcade.ToString(), d.Otros.ToString(), d.Total.ToString() }),
            ["Total", "", datos.Sum(d => d.Pool).ToString(), datos.Sum(d => d.Metegol).ToString(), datos.Sum(d => d.Arcade).ToString(), datos.Sum(d => d.Otros).ToString(), datos.Sum(d => d.Total).ToString()],
            [2, 3, 4, 5, 6]);
        return await Entregar(bytes, "maquinas-por-localidad", "Máquinas por localidad", $"Al {DateTime.Today:dd/MM/yyyy}");
    }

    async Task<List<MaquinasLocalidadDto>> CalcularPorLocalidad()
    {
        var instaladas = await db.Instalaciones
            .Where(i => i.Estado == EstadoInstalacion.Activa)
            .Select(i => new
            {
                Provincia = i.Cliente.Localidad.Provincia.Nombre,
                Localidad = i.Cliente.Localidad.Nombre,
                Tipo = i.Maquina.TipoMaquina.Descripcion
            })
            .ToListAsync();

        return instaladas
            .GroupBy(x => new { x.Provincia, x.Localidad })
            .OrderBy(g => g.Key.Provincia).ThenBy(g => g.Key.Localidad)
            .Select(g => new MaquinasLocalidadDto
            {
                Provincia = g.Key.Provincia,
                Localidad = g.Key.Localidad,
                Pool = g.Count(x => x.Tipo == "Pool"),
                Metegol = g.Count(x => x.Tipo == "Metegol"),
                Arcade = g.Count(x => x.Tipo == "Arcade"),
                Otros = g.Count(x => x.Tipo != "Pool" && x.Tipo != "Metegol" && x.Tipo != "Arcade"),
                Total = g.Count()
            })
            .ToList();
    }

    [HttpGet("maquinas-estado")]
    [Authorize(Roles = Roles.Admin)]
    public Task<List<MaquinasEstadoDto>> MaquinasEstado() => CalcularPorEstado();

    [HttpGet("maquinas-estado/pdf")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<FileContentResult> MaquinasEstadoPdf()
    {
        var maquinas = await db.Maquinas.Select(Mapeos.AMaquina).ToListAsync();
        var filas = maquinas
            .OrderBy(m => m.Estado).ThenBy(m => m.NumeroIdentificacion)
            .Select(m => new[]
            {
                m.Estado is EstadoMaquina.Instalada or EstadoMaquina.Disponible ? "Activa" : "Inactiva",
                Textos.De(m.Estado), m.NumeroIdentificacion, m.Tipo,
                m.ClienteActual ?? "-", m.LocalidadActual ?? "-"
            });
        var activas = maquinas.Count(m => m.Estado is EstadoMaquina.Instalada or EstadoMaquina.Disponible);
        var bytes = pdf.Tabla("Máquinas activas e inactivas", $"Situación al {DateTime.Today:dd/MM/yyyy} · {activas} activas, {maquinas.Count - activas} inactivas",
            ["Situación", "Estado", "Máquina", "Tipo", "Comercio", "Localidad"],
            filas,
            ["Total", $"{maquinas.Count} máquinas", "", "", "", ""]);
        return await Entregar(bytes, "maquinas-estado", "Máquinas activas e inactivas", $"Al {DateTime.Today:dd/MM/yyyy}");
    }

    async Task<List<MaquinasEstadoDto>> CalcularPorEstado()
    {
        var grupos = await db.Maquinas.GroupBy(m => m.Estado)
            .Select(g => new { g.Key, Cantidad = g.Count() }).ToListAsync();
        return Enum.GetValues<EstadoMaquina>()
            .Select(e => new MaquinasEstadoDto { Estado = Textos.De(e), Cantidad = grupos.FirstOrDefault(g => g.Key == e)?.Cantidad ?? 0 })
            .ToList();
    }

    [HttpGet("mantenimientos/pdf")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<FileContentResult> MantenimientosPdf(DateTime? desde, DateTime? hasta)
    {
        var q = db.Mantenimientos.AsQueryable();
        if (desde is not null) q = q.Where(m => m.Fecha >= desde.Value.Date);
        if (hasta is not null) q = q.Where(m => m.Fecha <= hasta.Value.Date);
        var datos = await q.OrderByDescending(m => m.Fecha).Select(Mapeos.AMantenimiento).ToListAsync();

        var bytes = pdf.Tabla("Historial de mantenimientos", Rango(desde, hasta),
            ["Fecha", "Máquina", "Tipo", "Descripción", "Estado", "Costo"],
            datos.Select(m => new[] { Textos.Fecha(m.Fecha), m.Maquina, Textos.De(m.TipoMantenimiento), m.Descripcion, Textos.De(m.Estado), Textos.Plata(m.Costo) }),
            ["Total", $"{datos.Count} trabajos", "", "", "", Textos.Plata(datos.Sum(m => m.Costo))],
            [5]);
        return await Entregar(bytes, "mantenimientos", "Historial de mantenimientos", Rango(desde, hasta));
    }

    [HttpGet("generados")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<ReporteGeneradoDto>> Generados() =>
        await db.Reportes
            .OrderByDescending(r => r.FechaGeneracion)
            .Take(30)
            .Select(r => new ReporteGeneradoDto
            {
                IdReporte = r.IdReporte,
                TipoReporte = r.TipoReporte,
                Filtros = r.Filtros,
                FechaGeneracion = r.FechaGeneracion,
                Usuario = r.Usuario.Nombre + " " + r.Usuario.Apellido
            })
            .ToListAsync();

    async Task<FileContentResult> Entregar(byte[] bytes, string archivo, string tipo, string filtros)
    {
        db.Reportes.Add(new Reporte
        {
            TipoReporte = tipo,
            Filtros = filtros.Length > 200 ? filtros[..200] : filtros,
            FechaGeneracion = DateTime.Now,
            IdUsuario = IdUsuario
        });
        await db.SaveChangesAsync();
        return Pdf(bytes, archivo);
    }

    static string Rango(DateTime? desde, DateTime? hasta) => (desde, hasta) switch
    {
        (null, null) => "Todo el historial",
        (not null, null) => $"Desde el {desde:dd/MM/yyyy}",
        (null, not null) => $"Hasta el {hasta:dd/MM/yyyy}",
        _ => $"Del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}"
    };
}
