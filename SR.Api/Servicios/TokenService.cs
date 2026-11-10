using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SR.Api.Datos;
using SR.Shared.Dtos;

namespace SR.Api.Servicios;

public class TokenService(IConfiguration config)
{
    public LoginResponse Crear(Usuario u)
    {
        var jwt = config.GetSection("Jwt");
        var expira = DateTime.UtcNow.AddHours(jwt.GetValue("HorasValidez", 12));

        var claims = new List<Claim>
        {
            new("sub", u.IdUsuario.ToString()),
            new("name", u.NombreUsuario),
            new("nombre", $"{u.Nombre} {u.Apellido}".Trim()),
            new("role", u.Rol.NombreRol)
        };
        if (u.IdCliente is not null) claims.Add(new("idCliente", u.IdCliente.Value.ToString()));

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expira = expira,
            Nombre = $"{u.Nombre} {u.Apellido}".Trim(),
            Rol = u.Rol.NombreRol,
            IdCliente = u.IdCliente
        };
    }
}
