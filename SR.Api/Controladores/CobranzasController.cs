using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/cobranzas")]
[Authorize]
public class CobranzasController(SrDbContext db) : BaseController
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<CobranzaDto>> Listar(DateTime? desde, DateTime? hasta, int? idCliente, int? idMaquina) =>
        await Filtrar(db.Cobranzas, desde, hasta, idCliente, idMaquina)
            .OrderByDescending(c => c.FechaCobro).ThenByDescending(c => c.IdCobranza)
            .Select(Mapeos.ACobranza)
            .ToListAsync();

    [HttpGet("mias")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<List<CobranzaDto>> MisCobranzas(DateTime? desde, DateTime? hasta) =>
        await Filtrar(db.Cobranzas, desde, hasta, IdClienteUsuario ?? -1, null)
            .OrderByDescending(c => c.FechaCobro)
            .Select(Mapeos.ACobranza)
            .ToListAsync();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CobranzaDto>> Registrar(CobranzaCrearDto dto)
    {
        var inst = await db.Instalaciones
            .Include(i => i.Maquina)
            .FirstOrDefaultAsync(i => i.IdMaquina == dto.IdMaquina && i.Estado == EstadoInstalacion.Activa);
        if (inst is null) return Error("La máquina no está instalada en ningún comercio.");
        if (dto.MontoComercio > dto.MontoRecaudado)
            return Error("Lo que se le paga al comercio no puede superar lo recaudado.");
        if (dto.FechaCobro.Date > DateTime.Today)
            return Error("La fecha de cobro no puede ser futura.");

        var c = new Cobranza
        {
            IdMaquina = dto.IdMaquina,
            IdCliente = inst.IdCliente,
            IdUsuario = IdUsuario,
            FechaCobro = dto.FechaCobro.Date,
            MontoRecaudado = dto.MontoRecaudado,
            MontoComercio = dto.MontoComercio,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim()
        };
        db.Cobranzas.Add(c);
        await db.SaveChangesAsync();
        return await db.Cobranzas.Where(x => x.IdCobranza == c.IdCobranza).Select(Mapeos.ACobranza).FirstAsync();
    }

    internal static IQueryable<Cobranza> Filtrar(IQueryable<Cobranza> q, DateTime? desde, DateTime? hasta, int? idCliente, int? idMaquina)
    {
        if (desde is not null) q = q.Where(c => c.FechaCobro >= desde.Value.Date);
        if (hasta is not null) q = q.Where(c => c.FechaCobro <= hasta.Value.Date);
        if (idCliente is not null) q = q.Where(c => c.IdCliente == idCliente);
        if (idMaquina is not null) q = q.Where(c => c.IdMaquina == idMaquina);
        return q;
    }
}
