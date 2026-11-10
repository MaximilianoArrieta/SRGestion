using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/clientes")]
[Authorize(Roles = Roles.Admin)]
public class ClientesController(SrDbContext db) : BaseController
{
    [HttpGet]
    public async Task<List<ClienteDto>> Listar(string? buscar, bool incluirBajas = false)
    {
        var q = db.Clientes.AsQueryable();
        if (!incluirBajas) q = q.Where(c => c.Activo);
        if (!string.IsNullOrWhiteSpace(buscar))
            q = q.Where(c => c.RazonSocial.Contains(buscar) || c.Responsable.Contains(buscar) || c.Localidad.Nombre.Contains(buscar));
        return await q.OrderBy(c => c.RazonSocial).Select(Mapeos.ACliente).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Obtener(int id)
    {
        var c = await db.Clientes.Where(x => x.IdCliente == id).Select(Mapeos.ACliente).FirstOrDefaultAsync();
        if (c is null) return NoEncontrado("El cliente");
        return c;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteGuardarDto dto)
    {
        if (!await db.Localidades.AnyAsync(l => l.IdLocalidad == dto.IdLocalidad))
            return Error("La localidad elegida no existe.");

        var c = new Cliente();
        Copiar(dto, c);
        db.Clientes.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = c.IdCliente },
            await db.Clientes.Where(x => x.IdCliente == c.IdCliente).Select(Mapeos.ACliente).FirstAsync());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, ClienteGuardarDto dto)
    {
        var c = await db.Clientes.FindAsync(id);
        if (c is null) return NoEncontrado("El cliente");
        if (!await db.Localidades.AnyAsync(l => l.IdLocalidad == dto.IdLocalidad))
            return Error("La localidad elegida no existe.");
        Copiar(dto, c);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Baja(int id)
    {
        var c = await db.Clientes.FindAsync(id);
        if (c is null) return NoEncontrado("El cliente");
        if (await db.Instalaciones.AnyAsync(i => i.IdCliente == id && i.Estado == EstadoInstalacion.Activa))
            return Error("El cliente tiene máquinas instaladas. Retiralas antes de darlo de baja.");
        c.Activo = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/reactivar")]
    public async Task<IActionResult> Reactivar(int id)
    {
        var c = await db.Clientes.FindAsync(id);
        if (c is null) return NoEncontrado("El cliente");
        c.Activo = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/usuario")]
    public async Task<IActionResult> CrearUsuario(int id, UsuarioClienteDto dto)
    {
        var c = await db.Clientes.Include(x => x.Usuarios).FirstOrDefaultAsync(x => x.IdCliente == id);
        if (c is null) return NoEncontrado("El cliente");

        var nombre = dto.NombreUsuario.Trim().ToLower();
        var hasher = new PasswordHasher<Usuario>();
        var existente = c.Usuarios.FirstOrDefault();

        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre && u.IdCliente != id))
            return Error("Ese nombre de usuario ya está en uso.");

        if (existente is null)
        {
            var rol = await db.Roles.FirstAsync(r => r.NombreRol == Roles.Cliente);
            existente = new Usuario { IdRol = rol.IdRol, IdCliente = id };
            db.Usuarios.Add(existente);
        }
        existente.NombreUsuario = nombre;
        existente.Nombre = c.Responsable;
        existente.Apellido = "";
        existente.Email = dto.Email;
        existente.Activo = true;
        existente.PasswordHash = hasher.HashPassword(existente, dto.Password);
        await db.SaveChangesAsync();
        return NoContent();
    }

    static void Copiar(ClienteGuardarDto dto, Cliente c)
    {
        c.RazonSocial = dto.RazonSocial.Trim();
        c.Responsable = dto.Responsable.Trim();
        c.Telefono = dto.Telefono.Trim();
        c.Direccion = dto.Direccion.Trim();
        c.DatosFiscales = string.IsNullOrWhiteSpace(dto.DatosFiscales) ? null : dto.DatosFiscales.Trim();
        c.IdLocalidad = dto.IdLocalidad;
    }
}
