using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/mantenimientos")]
[Authorize(Roles = Roles.Admin)]
public class MantenimientosController(SrDbContext db) : BaseController
{
    [HttpGet]
    public async Task<List<MantenimientoDto>> Listar(EstadoMantenimiento? estado)
    {
        var q = db.Mantenimientos.AsQueryable();
        if (estado is not null) q = q.Where(m => m.Estado == estado);
        return await q.OrderBy(m => m.Estado == EstadoMantenimiento.Finalizado)
                      .ThenByDescending(m => m.Fecha)
                      .Select(Mapeos.AMantenimiento)
                      .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<MantenimientoDto>> Crear(MantenimientoGuardarDto dto)
    {
        if (!await db.Maquinas.AnyAsync(m => m.IdMaquina == dto.IdMaquina)) return NoEncontrado("La máquina");

        var m = new Mantenimiento();
        Copiar(dto, m);
        db.Mantenimientos.Add(m);

        var pedidos = await db.Solicitudes
            .Where(s => s.IdMaquina == dto.IdMaquina && s.TipoSolicitud == TipoSolicitud.Mantenimiento && s.Estado == EstadoSolicitud.Pendiente)
            .ToListAsync();
        foreach (var s in pedidos)
        {
            s.Estado = EstadoSolicitud.Aprobada;
            s.Respuesta ??= $"Mantenimiento programado para el {dto.Fecha:dd/MM/yyyy}.";
        }

        await db.SaveChangesAsync();
        return await db.Mantenimientos.Where(x => x.IdMantenimiento == m.IdMantenimiento).Select(Mapeos.AMantenimiento).FirstAsync();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, MantenimientoGuardarDto dto)
    {
        var m = await db.Mantenimientos.FindAsync(id);
        if (m is null) return NoEncontrado("El mantenimiento");
        if (m.IdMaquina != dto.IdMaquina) return Error("No se puede cambiar la máquina de un mantenimiento.");
        Copiar(dto, m);

        if (dto.Estado == EstadoMantenimiento.Finalizado)
        {
            var pedidos = await db.Solicitudes
                .Where(s => s.IdMaquina == m.IdMaquina && s.TipoSolicitud == TipoSolicitud.Mantenimiento
                            && (s.Estado == EstadoSolicitud.Pendiente || s.Estado == EstadoSolicitud.Aprobada))
                .ToListAsync();
            foreach (var s in pedidos) s.Estado = EstadoSolicitud.Resuelta;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    static void Copiar(MantenimientoGuardarDto dto, Mantenimiento m)
    {
        m.IdMaquina = dto.IdMaquina;
        m.Fecha = dto.Fecha.Date;
        m.TipoMantenimiento = dto.TipoMantenimiento;
        m.Descripcion = dto.Descripcion.Trim();
        m.Costo = dto.Costo;
        m.Estado = dto.Estado;
    }
}
