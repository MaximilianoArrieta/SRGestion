using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/solicitudes")]
[Authorize]
public class SolicitudesController(SrDbContext db) : BaseController
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<SolicitudDto>> Listar(EstadoSolicitud? estado)
    {
        var q = db.Solicitudes.AsQueryable();
        if (estado is not null) q = q.Where(s => s.Estado == estado);
        return await q.OrderBy(s => s.Estado != EstadoSolicitud.Pendiente)
                      .ThenByDescending(s => s.FechaSolicitud)
                      .ThenByDescending(s => s.IdSolicitud)
                      .Select(Mapeos.ASolicitud)
                      .ToListAsync();
    }

    [HttpGet("mias")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<List<SolicitudDto>> MisSolicitudes()
    {
        var idCliente = IdClienteUsuario;
        return await db.Solicitudes
            .Where(s => s.IdCliente == idCliente)
            .OrderByDescending(s => s.FechaSolicitud).ThenByDescending(s => s.IdSolicitud)
            .Select(Mapeos.ASolicitud)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<ActionResult<SolicitudDto>> Crear(SolicitudCrearDto dto)
    {
        var idCliente = IdClienteUsuario;
        if (idCliente is null) return Error("Tu usuario no está asociado a ningún comercio.");

        if (dto.TipoSolicitud == TipoSolicitud.NuevaMaquina)
        {
            dto.IdMaquina = null;
        }
        else
        {
            if (dto.IdMaquina is null) return Error("Elegí la máquina.");
            var esSuya = await db.Instalaciones.AnyAsync(i =>
                i.IdMaquina == dto.IdMaquina && i.IdCliente == idCliente && i.Estado == EstadoInstalacion.Activa);
            if (!esSuya) return Error("Esa máquina no está instalada en tu comercio.");

            var repetida = await db.Solicitudes.AnyAsync(s =>
                s.IdMaquina == dto.IdMaquina && s.TipoSolicitud == dto.TipoSolicitud && s.Estado == EstadoSolicitud.Pendiente);
            if (repetida) return Error("Ya hay una solicitud igual pendiente para esa máquina.");
        }

        var s = new Solicitud
        {
            IdCliente = idCliente.Value,
            IdMaquina = dto.IdMaquina,
            TipoSolicitud = dto.TipoSolicitud,
            FechaSolicitud = DateTime.Today,
            Estado = EstadoSolicitud.Pendiente,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim()
        };
        db.Solicitudes.Add(s);
        await db.SaveChangesAsync();
        return await db.Solicitudes.Where(x => x.IdSolicitud == s.IdSolicitud).Select(Mapeos.ASolicitud).FirstAsync();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Resolver(int id, SolicitudResolverDto dto)
    {
        var s = await db.Solicitudes.FindAsync(id);
        if (s is null) return NoEncontrado("La solicitud");
        s.Estado = dto.Estado;
        s.Respuesta = string.IsNullOrWhiteSpace(dto.Respuesta) ? s.Respuesta : dto.Respuesta.Trim();
        await db.SaveChangesAsync();
        return NoContent();
    }
}
