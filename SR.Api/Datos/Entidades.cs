using SR.Shared;

namespace SR.Api.Datos;

public class Provincia
{
    public int IdProvincia { get; set; }
    public string Nombre { get; set; } = "";
    public List<Localidad> Localidades { get; set; } = new();
}

public class Localidad
{
    public int IdLocalidad { get; set; }
    public string Nombre { get; set; } = "";
    public int IdProvincia { get; set; }
    public Provincia Provincia { get; set; } = null!;
}

public class Cliente
{
    public int IdCliente { get; set; }
    public string RazonSocial { get; set; } = "";
    public string Responsable { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string? DatosFiscales { get; set; }
    public int IdLocalidad { get; set; }
    public Localidad Localidad { get; set; } = null!;
    public bool Activo { get; set; } = true;

    public List<Instalacion> Instalaciones { get; set; } = new();
    public List<Solicitud> Solicitudes { get; set; } = new();
    public List<Usuario> Usuarios { get; set; } = new();
}

public class TipoMaquina
{
    public int IdTipoMaquina { get; set; }
    public string Descripcion { get; set; } = "";
}

public class Maquina
{
    public int IdMaquina { get; set; }
    public string NumeroIdentificacion { get; set; } = "";
    public string? Descripcion { get; set; }
    public EstadoMaquina Estado { get; set; } = EstadoMaquina.Disponible;
    public DateTime FechaAlta { get; set; }
    public int IdTipoMaquina { get; set; }
    public TipoMaquina TipoMaquina { get; set; } = null!;

    public List<Instalacion> Instalaciones { get; set; } = new();
    public List<Cobranza> Cobranzas { get; set; } = new();
    public List<Mantenimiento> Mantenimientos { get; set; } = new();
}

public class Instalacion
{
    public int IdInstalacion { get; set; }
    public int IdMaquina { get; set; }
    public Maquina Maquina { get; set; } = null!;
    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaInstalacion { get; set; }
    public DateTime? FechaRetiroProgramada { get; set; }
    public DateTime? FechaRetiroReal { get; set; }
    public EstadoInstalacion Estado { get; set; } = EstadoInstalacion.Activa;
}

public class Cobranza
{
    public int IdCobranza { get; set; }
    public int IdMaquina { get; set; }
    public Maquina Maquina { get; set; } = null!;
    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTime FechaCobro { get; set; }
    public decimal MontoRecaudado { get; set; }
    public decimal MontoComercio { get; set; }
    public string? Observaciones { get; set; }
}

public class Mantenimiento
{
    public int IdMantenimiento { get; set; }
    public int IdMaquina { get; set; }
    public Maquina Maquina { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public TipoMantenimiento TipoMantenimiento { get; set; }
    public string Descripcion { get; set; } = "";
    public decimal Costo { get; set; }
    public EstadoMantenimiento Estado { get; set; }
}

public class Solicitud
{
    public int IdSolicitud { get; set; }
    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int? IdMaquina { get; set; }
    public Maquina? Maquina { get; set; }
    public TipoSolicitud TipoSolicitud { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
    public string? Observaciones { get; set; }
    public string? Respuesta { get; set; }
}

public class Rol
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = "";
}

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = "";
    public bool Activo { get; set; } = true;
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;
    public int? IdCliente { get; set; }
    public Cliente? Cliente { get; set; }
}

public class Reporte
{
    public int IdReporte { get; set; }
    public string TipoReporte { get; set; } = "";
    public string? Filtros { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
