using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/maquinas")]
[Authorize]
public class MaquinasController(SrDbContext db) : BaseController
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<MaquinaDto>> Listar(string? buscar, EstadoMaquina? estado, int? idTipo)
    {
        var q = db.Maquinas.AsQueryable();
        if (estado is not null) q = q.Where(m => m.Estado == estado);
        if (idTipo is not null) q = q.Where(m => m.IdTipoMaquina == idTipo);
        if (!string.IsNullOrWhiteSpace(buscar))
            q = q.Where(m => m.NumeroIdentificacion.Contains(buscar)
                          || (m.Descripcion != null && m.Descripcion.Contains(buscar))
                          || m.Instalaciones.Any(i => i.Estado == EstadoInstalacion.Activa && i.Cliente.RazonSocial.Contains(buscar)));
        return await q.OrderBy(m => m.NumeroIdentificacion).Select(Mapeos.AMaquina).ToListAsync();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MaquinaDetalleDto>> Obtener(int id)
    {
        var m = await db.Maquinas.Where(x => x.IdMaquina == id).Select(Mapeos.AMaquina).FirstOrDefaultAsync();
        if (m is null) return NoEncontrado("La máquina");

        var detalle = new MaquinaDetalleDto
        {
            Maquina = m,
            Instalaciones = await db.Instalaciones.Where(i => i.IdMaquina == id)
                .OrderByDescending(i => i.FechaInstalacion).Select(Mapeos.AInstalacion).ToListAsync(),
            Cobranzas = await db.Cobranzas.Where(c => c.IdMaquina == id)
                .OrderByDescending(c => c.FechaCobro).Select(Mapeos.ACobranza).ToListAsync(),
            Mantenimientos = await db.Mantenimientos.Where(x => x.IdMaquina == id)
                .OrderByDescending(x => x.Fecha).Select(Mapeos.AMantenimiento).ToListAsync()
        };
        detalle.TotalRecaudado = detalle.Cobranzas.Sum(c => c.MontoRecaudado);
        detalle.TotalMantenimiento = detalle.Mantenimientos.Sum(x => x.Costo);
        return detalle;
    }

    [HttpGet("mias")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<List<MaquinaDto>> MisMaquinas()
    {
        var idCliente = IdClienteUsuario;
        return await db.Maquinas
            .Where(m => m.Instalaciones.Any(i => i.IdCliente == idCliente && i.Estado == EstadoInstalacion.Activa))
            .OrderBy(m => m.NumeroIdentificacion)
            .Select(Mapeos.AMaquina)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MaquinaDto>> Crear(MaquinaGuardarDto dto)
    {
        var validacion = await Validar(dto, null);
        if (validacion is not null) return validacion;
        if (dto.Estado == EstadoMaquina.Instalada)
            return Error("Para instalar una máquina usá Operaciones → Instalaciones.");

        var m = new Maquina();
        Copiar(dto, m);
        db.Maquinas.Add(m);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = m.IdMaquina },
            await db.Maquinas.Where(x => x.IdMaquina == m.IdMaquina).Select(Mapeos.AMaquina).FirstAsync());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Editar(int id, MaquinaGuardarDto dto)
    {
        var m = await db.Maquinas.FindAsync(id);
        if (m is null) return NoEncontrado("La máquina");
        var validacion = await Validar(dto, id);
        if (validacion is not null) return validacion;

        var instalada = m.Estado == EstadoMaquina.Instalada;
        if (instalada && dto.Estado != EstadoMaquina.Instalada)
            return Error("La máquina está instalada. Registrá el retiro antes de cambiarle el estado.");
        if (!instalada && dto.Estado == EstadoMaquina.Instalada)
            return Error("Para instalar una máquina usá Operaciones → Instalaciones.");

        Copiar(dto, m);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Baja(int id)
    {
        var m = await db.Maquinas.FindAsync(id);
        if (m is null) return NoEncontrado("La máquina");
        if (m.Estado == EstadoMaquina.Instalada)
            return Error("La máquina está instalada. Registrá el retiro antes de darla de baja.");
        m.Estado = EstadoMaquina.Baja;
        await db.SaveChangesAsync();
        return NoContent();
    }

    async Task<ObjectResult?> Validar(MaquinaGuardarDto dto, int? id)
    {
        var nro = dto.NumeroIdentificacion.Trim().ToUpper();
        if (await db.Maquinas.AnyAsync(m => m.NumeroIdentificacion == nro && m.IdMaquina != id))
            return Error($"Ya existe una máquina con el número {nro}.");
        if (!await db.TiposMaquina.AnyAsync(t => t.IdTipoMaquina == dto.IdTipoMaquina))
            return Error("El tipo de máquina no existe.");
        return null;
    }

    static void Copiar(MaquinaGuardarDto dto, Maquina m)
    {
        m.NumeroIdentificacion = dto.NumeroIdentificacion.Trim().ToUpper();
        m.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        m.IdTipoMaquina = dto.IdTipoMaquina;
        m.FechaAlta = dto.FechaAlta.Date;
        m.Estado = dto.Estado;
    }
}
