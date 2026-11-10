using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SR.Shared.Dtos;

namespace SR.Shared.Servicios;

public interface ITokenStore
{
    Task<string?> LeerAsync();
    Task GuardarAsync(string token);
    Task BorrarAsync();
}

public class SrAuthStateProvider : AuthenticationStateProvider
{
    readonly ITokenStore _store;
    readonly SrApi _api;
    static readonly AuthenticationState Anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public SrAuthStateProvider(ITokenStore store, SrApi api)
    {
        _store = store;
        _api = api;
        _api.SesionVencida += () => _ = CerrarSesionAsync();
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _store.LeerAsync();
        var usuario = LeerToken(token);
        if (usuario is null)
        {
            _api.SetToken(null);
            return Anonimo;
        }
        _api.SetToken(token);
        return new AuthenticationState(usuario);
    }

    public async Task<LoginResponse> IniciarSesionAsync(LoginRequest req)
    {
        var resp = await _api.LoginAsync(req);
        await _store.GuardarAsync(resp.Token);
        _api.SetToken(resp.Token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(LeerToken(resp.Token)!)));
        return resp;
    }

    public async Task CerrarSesionAsync()
    {
        await _store.BorrarAsync();
        _api.SetToken(null);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
    }

    static ClaimsPrincipal? LeerToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 3) return null;
            var json = Encoding.UTF8.GetString(Base64Url(partes[1]));
            using var doc = JsonDocument.Parse(json);

            var claims = new List<Claim>();
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                if (p.Value.ValueKind == JsonValueKind.Array)
                    claims.AddRange(p.Value.EnumerateArray().Select(v => new Claim(Mapear(p.Name), v.ToString())));
                else
                    claims.Add(new Claim(Mapear(p.Name), p.Value.ToString()));
            }

            var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp is not null && DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)) < DateTimeOffset.UtcNow)
                return null;

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role));
        }
        catch
        {
            return null;
        }
    }

    static string Mapear(string nombre) => nombre switch
    {
        "role" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
        "unique_name" or "name" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
        _ => nombre
    };

    static byte[] Base64Url(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        s += (s.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
        return Convert.FromBase64String(s);
    }
}

public static class UsuarioExtensions
{
    public static bool EsAdmin(this ClaimsPrincipal u) => u.IsInRole(Roles.Admin);
    public static string NombreCompleto(this ClaimsPrincipal u) => u.FindFirst("nombre")?.Value ?? u.Identity?.Name ?? "";
    public static int? IdCliente(this ClaimsPrincipal u) =>
        int.TryParse(u.FindFirst("idCliente")?.Value, out var id) ? id : null;
}
