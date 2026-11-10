using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SR.Api.Datos;
using SR.Api.Servicios;
using SR.Shared.Dtos;

namespace SR.Api.Controladores;

[Route("api/auth")]
public class AuthController(SrDbContext db, TokenService tokens) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Cliente)
            .FirstOrDefaultAsync(u => u.NombreUsuario == req.Usuario.Trim());

        var hasher = new PasswordHasher<Usuario>();
        if (usuario is null || !usuario.Activo ||
            hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            return Error("Usuario o contraseña incorrectos.");

        if (usuario.Cliente is { Activo: false })
            return Error("El comercio está dado de baja. Comunicate con SR.");

        return tokens.Crear(usuario);
    }
}
