using Microsoft.JSInterop;
using SR.Shared.Servicios;

namespace SR.Web.Servicios;

public class LocalStorageTokenStore(IJSRuntime js) : ITokenStore
{
    const string Clave = "sr-token";
    public async Task<string?> LeerAsync() => await js.InvokeAsync<string?>("sr.leer", Clave);
    public async Task GuardarAsync(string token) => await js.InvokeVoidAsync("sr.guardar", Clave, token);
    public async Task BorrarAsync() => await js.InvokeVoidAsync("sr.borrar", Clave);
}

public class Navegador(IJSRuntime js)
{
    public async Task DescargarAsync(string nombre, byte[] contenido, string tipo = "application/pdf") =>
        await js.InvokeVoidAsync("sr.descargar", nombre, Convert.ToBase64String(contenido), tipo);

    public async Task<bool> TemaClaroAsync() => await js.InvokeAsync<bool>("sr.temaClaro");

    public async Task<bool> CambiarTemaAsync() => await js.InvokeAsync<bool>("sr.cambiarTema");
}
