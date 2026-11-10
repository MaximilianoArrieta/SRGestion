using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/dashboard")]
[Authorize(Roles = Roles.Admin)]
public class DashboardController(SrDbContext db) : BaseController
{
    [HttpGet]
    public async Task<ResumenDto> Get()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);
        var limiteRetiros = hoy.AddDays(30);

        var porEstado = await db.Maquinas
            .GroupBy(m => m.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();
        int Cant(EstadoMaquina e) => porEstado.FirstOrDefault(x => x.Estado == e)?.Cantidad ?? 0;

        return new ResumenDto
        {
            MaquinasInstaladas = Cant(EstadoMaquina.Instalada),
            MaquinasDisponibles = Cant(EstadoMaquina.Disponible),
            MaquinasEnReparacion = Cant(EstadoMaquina.EnReparacion),
            ClientesActivos = await db.Clientes.CountAsync(c => c.Activo),
            RecaudacionMes = await db.Cobranzas.Where(c => c.FechaCobro >= inicioMes).SumAsync(c => (decimal?)c.MontoRecaudado) ?? 0,
            RecaudacionMesAnterior = await db.Cobranzas.Where(c => c.FechaCobro >= inicioMesAnterior && c.FechaCobro < inicioMes).SumAsync(c => (decimal?)c.MontoRecaudado) ?? 0,
            SolicitudesPendientes = await db.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.Pendiente),
            MantenimientosPendientes = await db.Mantenimientos.CountAsync(m => m.Estado != EstadoMantenimiento.Finalizado),
            ProximosRetiros = await db.Instalaciones
                .Where(i => i.Estado == EstadoInstalacion.Activa && i.FechaRetiroProgramada != null && i.FechaRetiroProgramada <= limiteRetiros)
                .OrderBy(i => i.FechaRetiroProgramada)
                .Select(Mapeos.AInstalacion)
                .ToListAsync(),
            MantenimientosAbiertos = await db.Mantenimientos
                .Where(m => m.Estado != EstadoMantenimiento.Finalizado)
                .OrderBy(m => m.Fecha)
                .Select(Mapeos.AMantenimiento)
                .ToListAsync()
        };
    }
}
