using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SR.Shared.Dtos;

namespace SR.Shared.Servicios;

public class ApiException(string mensaje, HttpStatusCode status) : Exception(mensaje)
{
    public HttpStatusCode Status { get; } = status;
}

public class SrApi(HttpClient http)
{
    public event Action? SesionVencida;

    public void SetToken(string? token) =>
        http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);

    public Task<LoginResponse> LoginAsync(LoginRequest req) => PostAsync<LoginResponse>("api/auth/login", req);

    public Task<List<ProvinciaDto>> ProvinciasAsync() => GetAsync<List<ProvinciaDto>>("api/catalogos/provincias");
    public Task<List<TipoMaquinaDto>> TiposMaquinaAsync() => GetAsync<List<TipoMaquinaDto>>("api/catalogos/tipos-maquina");

    public Task<ResumenDto> ResumenAsync() => GetAsync<ResumenDto>("api/dashboard");

    public Task<List<ClienteDto>> ClientesAsync(string? buscar = null, bool incluirBajas = false) =>
        GetAsync<List<ClienteDto>>($"api/clientes?buscar={Uri.EscapeDataString(buscar ?? "")}&incluirBajas={incluirBajas}");
    public Task<ClienteDto> ClienteAsync(int id) => GetAsync<ClienteDto>($"api/clientes/{id}");
    public Task<ClienteDto> CrearClienteAsync(ClienteGuardarDto dto) => PostAsync<ClienteDto>("api/clientes", dto);
    public Task EditarClienteAsync(int id, ClienteGuardarDto dto) => PutAsync($"api/clientes/{id}", dto);
    public Task BajaClienteAsync(int id) => DeleteAsync($"api/clientes/{id}");
    public Task ReactivarClienteAsync(int id) => PutAsync($"api/clientes/{id}/reactivar", new { });
    public Task CrearUsuarioClienteAsync(int id, UsuarioClienteDto dto) => PostAsync($"api/clientes/{id}/usuario", dto);

    public Task<List<MaquinaDto>> MaquinasAsync(string? buscar = null, EstadoMaquina? estado = null, int? idTipo = null) =>
        GetAsync<List<MaquinaDto>>($"api/maquinas?buscar={Uri.EscapeDataString(buscar ?? "")}&estado={estado}&idTipo={idTipo}");
    public Task<MaquinaDetalleDto> MaquinaAsync(int id) => GetAsync<MaquinaDetalleDto>($"api/maquinas/{id}");
    public Task<MaquinaDto> CrearMaquinaAsync(MaquinaGuardarDto dto) => PostAsync<MaquinaDto>("api/maquinas", dto);
    public Task EditarMaquinaAsync(int id, MaquinaGuardarDto dto) => PutAsync($"api/maquinas/{id}", dto);
    public Task BajaMaquinaAsync(int id) => DeleteAsync($"api/maquinas/{id}");
    public Task<List<MaquinaDto>> MisMaquinasAsync() => GetAsync<List<MaquinaDto>>("api/maquinas/mias");

    public Task<List<InstalacionDto>> InstalacionesAsync(bool soloActivas = false) =>
        GetAsync<List<InstalacionDto>>($"api/instalaciones?soloActivas={soloActivas}");
    public Task<InstalacionDto> InstalarAsync(InstalarDto dto) => PostAsync<InstalacionDto>("api/instalaciones", dto);
    public Task RetirarAsync(int idInstalacion, RetiroDto dto) => PostAsync($"api/instalaciones/{idInstalacion}/retiro", dto);

    public Task<List<CobranzaDto>> CobranzasAsync(DateTime? desde = null, DateTime? hasta = null, int? idCliente = null, int? idMaquina = null) =>
        GetAsync<List<CobranzaDto>>($"api/cobranzas?{Rango(desde, hasta)}&idCliente={idCliente}&idMaquina={idMaquina}");
    public Task<CobranzaDto> RegistrarCobranzaAsync(CobranzaCrearDto dto) => PostAsync<CobranzaDto>("api/cobranzas", dto);
    public Task<List<CobranzaDto>> MisCobranzasAsync(DateTime? desde = null, DateTime? hasta = null) =>
        GetAsync<List<CobranzaDto>>($"api/cobranzas/mias?{Rango(desde, hasta)}");

    public Task<List<MantenimientoDto>> MantenimientosAsync(EstadoMantenimiento? estado = null) =>
        GetAsync<List<MantenimientoDto>>($"api/mantenimientos?estado={estado}");
    public Task<MantenimientoDto> CrearMantenimientoAsync(MantenimientoGuardarDto dto) => PostAsync<MantenimientoDto>("api/mantenimientos", dto);
    public Task EditarMantenimientoAsync(int id, MantenimientoGuardarDto dto) => PutAsync($"api/mantenimientos/{id}", dto);

    public Task<List<SolicitudDto>> SolicitudesAsync(EstadoSolicitud? estado = null) =>
        GetAsync<List<SolicitudDto>>($"api/solicitudes?estado={estado}");
    public Task<List<SolicitudDto>> MisSolicitudesAsync() => GetAsync<List<SolicitudDto>>("api/solicitudes/mias");
    public Task<SolicitudDto> CrearSolicitudAsync(SolicitudCrearDto dto) => PostAsync<SolicitudDto>("api/solicitudes", dto);
    public Task ResolverSolicitudAsync(int id, SolicitudResolverDto dto) => PutAsync($"api/solicitudes/{id}", dto);

    public Task<List<RecaudacionPeriodoDto>> ReporteRecaudacionAsync(DateTime? desde, DateTime? hasta) =>
        GetAsync<List<RecaudacionPeriodoDto>>($"api/reportes/recaudacion?{Rango(desde, hasta)}");
    public Task<List<MaquinasLocalidadDto>> ReporteMaquinasLocalidadAsync() =>
        GetAsync<List<MaquinasLocalidadDto>>("api/reportes/maquinas-localidad");
    public Task<List<MaquinasEstadoDto>> ReporteMaquinasEstadoAsync() =>
        GetAsync<List<MaquinasEstadoDto>>("api/reportes/maquinas-estado");

    public Task<List<ReporteGeneradoDto>> ReportesGeneradosAsync() =>
        GetAsync<List<ReporteGeneradoDto>>("api/reportes/generados");

    public Task<byte[]> ReportePdfAsync(string reporte, DateTime? desde = null, DateTime? hasta = null) =>
        GetBytesAsync($"api/reportes/{reporte}/pdf?{Rango(desde, hasta)}");

    static string Rango(DateTime? desde, DateTime? hasta) =>
        $"desde={desde?.ToString("yyyy-MM-dd")}&hasta={hasta?.ToString("yyyy-MM-dd")}";

    async Task<T> GetAsync<T>(string url)
    {
        using var resp = await Enviar(() => http.GetAsync(url));
        return (await resp.Content.ReadFromJsonAsync<T>())!;
    }

    async Task<byte[]> GetBytesAsync(string url)
    {
        using var resp = await Enviar(() => http.GetAsync(url));
        return await resp.Content.ReadAsByteArrayAsync();
    }

    async Task<T> PostAsync<T>(string url, object body)
    {
        using var resp = await Enviar(() => http.PostAsJsonAsync(url, body));
        return (await resp.Content.ReadFromJsonAsync<T>())!;
    }

    async Task PostAsync(string url, object body)
    {
        using var resp = await Enviar(() => http.PostAsJsonAsync(url, body));
    }

    async Task PutAsync(string url, object body)
    {
        using var resp = await Enviar(() => http.PutAsJsonAsync(url, body));
    }

    async Task DeleteAsync(string url)
    {
        using var resp = await Enviar(() => http.DeleteAsync(url));
    }

    async Task<HttpResponseMessage> Enviar(Func<Task<HttpResponseMessage>> llamada)
    {
        HttpResponseMessage resp;
        try
        {
            resp = await llamada();
        }
        catch (HttpRequestException)
        {
            throw new ApiException("No se pudo conectar con el servidor. Revisá que la API esté funcionando.", HttpStatusCode.ServiceUnavailable);
        }

        if (resp.IsSuccessStatusCode) return resp;

        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            resp.Dispose();
            SesionVencida?.Invoke();
            throw new ApiException("Tu sesión venció. Volvé a iniciar sesión.", HttpStatusCode.Unauthorized);
        }

        var mensaje = await LeerMensajeError(resp);
        var status = resp.StatusCode;
        resp.Dispose();
        throw new ApiException(mensaje, status);
    }

    static async Task<string> LeerMensajeError(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.Forbidden) return "No tenés permiso para hacer esto.";
        try
        {
            var texto = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(texto);
            var raiz = doc.RootElement;
            if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
            {
                var lista = errores.EnumerateObject()
                    .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                return string.Join(" ", lista);
            }
            if (raiz.TryGetProperty("detail", out var detalle) && detalle.ValueKind == JsonValueKind.String)
                return detalle.GetString()!;
            if (raiz.TryGetProperty("title", out var titulo) && titulo.ValueKind == JsonValueKind.String)
                return titulo.GetString()!;
        }
        catch (JsonException) { }
        return $"Error del servidor ({(int)resp.StatusCode}).";
    }
}
