using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/instalaciones")]
[Authorize(Roles = Roles.Admin)]
public class InstalacionesController(SrDbContext db) : BaseController
{
    [HttpGet]
    public async Task<List<InstalacionDto>> Listar(bool soloActivas = false)
    {
        var q = db.Instalaciones.AsQueryable();
        if (soloActivas) q = q.Where(i => i.Estado == EstadoInstalacion.Activa);
        return await q
            .OrderBy(i => i.Estado)
            .ThenByDescending(i => i.FechaInstalacion)
            .Select(Mapeos.AInstalacion)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<InstalacionDto>> Instalar(InstalarDto dto)
    {
        var maquina = await db.Maquinas.FindAsync(dto.IdMaquina);
        if (maquina is null) return NoEncontrado("La máquina");
        if (maquina.Estado != EstadoMaquina.Disponible)
            return Error($"La máquina {maquina.NumeroIdentificacion} no está disponible (estado: {Textos.De(maquina.Estado)}).");

        var cliente = await db.Clientes.FindAsync(dto.IdCliente);
        if (cliente is null) return NoEncontrado("El cliente");
        if (!cliente.Activo) return Error("El cliente está dado de baja.");

        if (dto.FechaRetiroProgramada is not null && dto.FechaRetiroProgramada.Value.Date < dto.FechaInstalacion.Date)
            return Error("La fecha de retiro no puede ser anterior a la de instalación.");

        var inst = new Instalacion
        {
            IdMaquina = dto.IdMaquina,
            IdCliente = dto.IdCliente,
            FechaInstalacion = dto.FechaInstalacion.Date,
            FechaRetiroProgramada = dto.FechaRetiroProgramada?.Date,
            Estado = EstadoInstalacion.Activa
        };
        maquina.Estado = EstadoMaquina.Instalada;
        db.Instalaciones.Add(inst);
        await db.SaveChangesAsync();

        return await db.Instalaciones.Where(i => i.IdInstalacion == inst.IdInstalacion).Select(Mapeos.AInstalacion).FirstAsync();
    }

    [HttpPost("{id:int}/retiro")]
    public async Task<IActionResult> Retirar(int id, RetiroDto dto)
    {
        var inst = await db.Instalaciones.Include(i => i.Maquina).FirstOrDefaultAsync(i => i.IdInstalacion == id);
        if (inst is null) return NoEncontrado("La instalación");
        if (inst.Estado == EstadoInstalacion.Finalizada) return Error("Esa máquina ya fue retirada.");
        if (dto.FechaRetiro.Date < inst.FechaInstalacion) return Error("La fecha de retiro no puede ser anterior a la instalación.");

        inst.FechaRetiroReal = dto.FechaRetiro.Date;
        inst.Estado = EstadoInstalacion.Finalizada;
        inst.Maquina.Estado = EstadoMaquina.Disponible;

        var pedidos = await db.Solicitudes
            .Where(s => s.IdMaquina == inst.IdMaquina && s.TipoSolicitud == TipoSolicitud.Retiro
                        && (s.Estado == EstadoSolicitud.Pendiente || s.Estado == EstadoSolicitud.Aprobada))
            .ToListAsync();
        foreach (var s in pedidos)
        {
            s.Estado = EstadoSolicitud.Resuelta;
            s.Respuesta ??= $"Máquina retirada el {dto.FechaRetiro:dd/MM/yyyy}.";
        }

        await db.SaveChangesAsync();
        return NoContent();
    }
}
