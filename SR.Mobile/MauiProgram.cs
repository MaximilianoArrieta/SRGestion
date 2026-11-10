using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using SR.Mobile.Servicios;
using SR.Shared.Servicios;

namespace SR.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = new Uri(Configuracion.ApiUrl),
            Timeout = TimeSpan.FromSeconds(20)
        });
        builder.Services.AddSingleton<SrApi>();

        builder.Services.AddSingleton<ITokenStore, SecureTokenStore>();
        builder.Services.AddSingleton<SrAuthStateProvider>();
        builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<SrAuthStateProvider>());
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddSingleton<Archivos>();
        builder.Services.AddScoped<Tema>();

        var cultura = new CultureInfo("es-AR");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;

        return builder.Build();
    }
}
