using Microsoft.JSInterop;
using SR.Shared.Servicios;

namespace SR.Mobile.Servicios;

public class SecureTokenStore : ITokenStore
{
    const string Clave = "sr-token";

    public async Task<string?> LeerAsync()
    {
        try { return await SecureStorage.Default.GetAsync(Clave); }
        catch { return Preferences.Default.Get<string?>(Clave, null); }
    }

    public async Task GuardarAsync(string token)
    {
        try { await SecureStorage.Default.SetAsync(Clave, token); }
        catch { Preferences.Default.Set(Clave, token); }
    }

    public Task BorrarAsync()
    {
        try { SecureStorage.Default.Remove(Clave); } catch { }
        Preferences.Default.Remove(Clave);
        return Task.CompletedTask;
    }
}

public class Archivos
{
    public async Task AbrirPdfAsync(string nombre, byte[] contenido)
    {
        var ruta = Path.Combine(FileSystem.CacheDirectory, nombre);
        await File.WriteAllBytesAsync(ruta, contenido);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest(nombre, new ReadOnlyFile(ruta, "application/pdf")));
            }
            catch
            {
                await Share.Default.RequestAsync(new ShareFileRequest(nombre, new ShareFile(ruta, "application/pdf")));
            }
        });
    }
}

public class Tema(IJSRuntime js)
{
    public bool Claro => Preferences.Default.Get("sr-tema", "dark") == "light";

    public async Task AplicarAsync() =>
        await js.InvokeVoidAsync("sr.aplicarTema", Claro ? "light" : "dark");

    public async Task<bool> CambiarAsync()
    {
        Preferences.Default.Set("sr-tema", Claro ? "dark" : "light");
        await AplicarAsync();
        return Claro;
    }
}
