using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/catalogos")]
[Authorize]
public class CatalogosController(SrDbContext db) : BaseController
{
    [HttpGet("provincias")]
    public async Task<List<ProvinciaDto>> Provincias() =>
        await db.Provincias
            .OrderBy(p => p.Nombre)
            .Select(p => new ProvinciaDto
            {
                IdProvincia = p.IdProvincia,
                Nombre = p.Nombre,
                Localidades = p.Localidades
                    .OrderBy(l => l.Nombre)
                    .Select(l => new LocalidadDto { IdLocalidad = l.IdLocalidad, Nombre = l.Nombre, IdProvincia = p.IdProvincia, Provincia = p.Nombre })
                    .ToList()
            })
            .ToListAsync();

    [HttpGet("tipos-maquina")]
    public async Task<List<TipoMaquinaDto>> TiposMaquina() =>
        await db.TiposMaquina
            .OrderBy(t => t.IdTipoMaquina)
            .Select(t => new TipoMaquinaDto { IdTipoMaquina = t.IdTipoMaquina, Descripcion = t.Descripcion })
            .ToListAsync();
}
