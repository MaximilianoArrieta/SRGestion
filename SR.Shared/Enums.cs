namespace SR.Shared;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Cliente = "Cliente";
}

public enum EstadoMaquina
{
    Disponible,
    Instalada,
    EnReparacion,
    Baja
}

public enum EstadoInstalacion
{
    Activa,
    Finalizada
}

public enum TipoMantenimiento
{
    Preventivo,
    Correctivo
}

public enum EstadoMantenimiento
{
    Pendiente,
    EnProceso,
    Finalizado
}

public enum TipoSolicitud
{
    Mantenimiento,
    Retiro,
    NuevaMaquina
}

public enum EstadoSolicitud
{
    Pendiente,
    Aprobada,
    Rechazada,
    Resuelta
}

public static class Textos
{
    public static string De(EstadoMaquina e) => e switch
    {
        EstadoMaquina.Disponible => "Disponible",
        EstadoMaquina.Instalada => "Instalada",
        EstadoMaquina.EnReparacion => "En reparación",
        EstadoMaquina.Baja => "De baja",
        _ => e.ToString()
    };

    public static string De(EstadoInstalacion e) => e == EstadoInstalacion.Activa ? "Activa" : "Finalizada";

    public static string De(TipoMantenimiento e) => e.ToString();

    public static string De(EstadoMantenimiento e) => e switch
    {
        EstadoMantenimiento.EnProceso => "En proceso",
        _ => e.ToString()
    };

    public static string De(TipoSolicitud e) => e switch
    {
        TipoSolicitud.NuevaMaquina => "Nueva máquina",
        _ => e.ToString()
    };

    public static string De(EstadoSolicitud e) => e.ToString();

    public static string Plata(decimal monto) =>
        monto.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("es-AR"));

    public static string Fecha(DateTime f) => f.ToString("dd/MM/yyyy");

    public static string Fecha(DateTime? f) => f.HasValue ? f.Value.ToString("dd/MM/yyyy") : "-";
}
