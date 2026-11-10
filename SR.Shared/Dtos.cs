using System.ComponentModel.DataAnnotations;

namespace SR.Shared.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "Ingresá tu usuario")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "Ingresá tu contraseña")]
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public DateTime Expira { get; set; }
    public string Nombre { get; set; } = "";
    public string Rol { get; set; } = "";
    public int? IdCliente { get; set; }
}

public class ProvinciaDto
{
    public int IdProvincia { get; set; }
    public string Nombre { get; set; } = "";
    public List<LocalidadDto> Localidades { get; set; } = new();
}

public class LocalidadDto
{
    public int IdLocalidad { get; set; }
    public string Nombre { get; set; } = "";
    public int IdProvincia { get; set; }
    public string Provincia { get; set; } = "";
}

public class TipoMaquinaDto
{
    public int IdTipoMaquina { get; set; }
    public string Descripcion { get; set; } = "";
}

public class ClienteDto
{
    public int IdCliente { get; set; }
    public string RazonSocial { get; set; } = "";
    public string Responsable { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string? DatosFiscales { get; set; }
    public int IdLocalidad { get; set; }
    public string Localidad { get; set; } = "";
    public string Provincia { get; set; } = "";
    public bool Activo { get; set; }
    public int MaquinasInstaladas { get; set; }
    public string? UsuarioAcceso { get; set; }
}

public class ClienteGuardarDto
{
    [Required(ErrorMessage = "El nombre del comercio es obligatorio")]
    [StringLength(120)]
    public string RazonSocial { get; set; } = "";

    [Required(ErrorMessage = "El responsable es obligatorio")]
    [StringLength(100)]
    public string Responsable { get; set; } = "";

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(30)]
    public string Telefono { get; set; } = "";

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(150)]
    public string Direccion { get; set; } = "";

    [StringLength(60)]
    public string? DatosFiscales { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí una localidad")]
    public int IdLocalidad { get; set; }
}

public class UsuarioClienteDto
{
    [Required(ErrorMessage = "Ingresá un nombre de usuario")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Entre 4 y 50 caracteres")]
    public string NombreUsuario { get; set; } = "";

    [Required(ErrorMessage = "Ingresá una contraseña")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
    public string Password { get; set; } = "";

    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }
}

public class MaquinaDto
{
    public int IdMaquina { get; set; }
    public string NumeroIdentificacion { get; set; } = "";
    public string? Descripcion { get; set; }
    public int IdTipoMaquina { get; set; }
    public string Tipo { get; set; } = "";
    public EstadoMaquina Estado { get; set; }
    public DateTime FechaAlta { get; set; }
    public int? IdClienteActual { get; set; }
    public string? ClienteActual { get; set; }
    public string? LocalidadActual { get; set; }
    public DateTime? FechaRetiroProgramada { get; set; }
}

public class MaquinaGuardarDto
{
    [Required(ErrorMessage = "El número de identificación es obligatorio")]
    [StringLength(30)]
    public string NumeroIdentificacion { get; set; } = "";

    [StringLength(200)]
    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí el tipo de máquina")]
    public int IdTipoMaquina { get; set; }

    public DateTime FechaAlta { get; set; } = DateTime.Today;

    public EstadoMaquina Estado { get; set; } = EstadoMaquina.Disponible;
}

public class MaquinaDetalleDto
{
    public MaquinaDto Maquina { get; set; } = new();
    public List<InstalacionDto> Instalaciones { get; set; } = new();
    public List<CobranzaDto> Cobranzas { get; set; } = new();
    public List<MantenimientoDto> Mantenimientos { get; set; } = new();
    public decimal TotalRecaudado { get; set; }
    public decimal TotalMantenimiento { get; set; }
}

public class InstalacionDto
{
    public int IdInstalacion { get; set; }
    public int IdMaquina { get; set; }
    public string Maquina { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = "";
    public string Localidad { get; set; } = "";
    public DateTime FechaInstalacion { get; set; }
    public DateTime? FechaRetiroProgramada { get; set; }
    public DateTime? FechaRetiroReal { get; set; }
    public EstadoInstalacion Estado { get; set; }
}

public class InstalarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Elegí la máquina")]
    public int IdMaquina { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí el cliente")]
    public int IdCliente { get; set; }

    public DateTime FechaInstalacion { get; set; } = DateTime.Today;

    public DateTime? FechaRetiroProgramada { get; set; }
}

public class RetiroDto
{
    public DateTime FechaRetiro { get; set; } = DateTime.Today;
}

public class CobranzaDto
{
    public int IdCobranza { get; set; }
    public int IdMaquina { get; set; }
    public string Maquina { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = "";
    public string Localidad { get; set; } = "";
    public DateTime FechaCobro { get; set; }
    public decimal MontoRecaudado { get; set; }
    public decimal MontoComercio { get; set; }
    public decimal MontoEmpresa => MontoRecaudado - MontoComercio;
    public string? Observaciones { get; set; }
    public string Usuario { get; set; } = "";
}

public class CobranzaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Elegí la máquina")]
    public int IdMaquina { get; set; }

    public DateTime FechaCobro { get; set; } = DateTime.Today;

    [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Monto inválido")]
    public decimal MontoRecaudado { get; set; }

    [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Monto inválido")]
    public decimal MontoComercio { get; set; }

    [StringLength(300)]
    public string? Observaciones { get; set; }
}

public class MantenimientoDto
{
    public int IdMantenimiento { get; set; }
    public int IdMaquina { get; set; }
    public string Maquina { get; set; } = "";
    public string Tipo { get; set; } = "";
    public DateTime Fecha { get; set; }
    public TipoMantenimiento TipoMantenimiento { get; set; }
    public string Descripcion { get; set; } = "";
    public decimal Costo { get; set; }
    public EstadoMantenimiento Estado { get; set; }
}

public class MantenimientoGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Elegí la máquina")]
    public int IdMaquina { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    public TipoMantenimiento TipoMantenimiento { get; set; } = TipoMantenimiento.Correctivo;

    [Required(ErrorMessage = "Describí la tarea")]
    [StringLength(400)]
    public string Descripcion { get; set; } = "";

    [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Costo inválido")]
    public decimal Costo { get; set; }

    public EstadoMantenimiento Estado { get; set; } = EstadoMantenimiento.Pendiente;
}

public class SolicitudDto
{
    public int IdSolicitud { get; set; }
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = "";
    public string Localidad { get; set; } = "";
    public int? IdMaquina { get; set; }
    public string? Maquina { get; set; }
    public TipoSolicitud TipoSolicitud { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public EstadoSolicitud Estado { get; set; }
    public string? Observaciones { get; set; }
    public string? Respuesta { get; set; }
}

public class SolicitudCrearDto
{
    public TipoSolicitud TipoSolicitud { get; set; }

    public int? IdMaquina { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }
}

public class SolicitudResolverDto
{
    public EstadoSolicitud Estado { get; set; }

    [StringLength(400)]
    public string? Respuesta { get; set; }
}

public class ResumenDto
{
    public int MaquinasInstaladas { get; set; }
    public int MaquinasDisponibles { get; set; }
    public int MaquinasEnReparacion { get; set; }
    public int ClientesActivos { get; set; }
    public decimal RecaudacionMes { get; set; }
    public decimal RecaudacionMesAnterior { get; set; }
    public int SolicitudesPendientes { get; set; }
    public int MantenimientosPendientes { get; set; }
    public List<InstalacionDto> ProximosRetiros { get; set; } = new();
    public List<MantenimientoDto> MantenimientosAbiertos { get; set; } = new();
}

public class RecaudacionPeriodoDto
{
    public string Periodo { get; set; } = "";
    public int Cobranzas { get; set; }
    public decimal Recaudado { get; set; }
    public decimal Comercios { get; set; }
    public decimal Empresa { get; set; }
}

public class MaquinasLocalidadDto
{
    public string Provincia { get; set; } = "";
    public string Localidad { get; set; } = "";
    public int Pool { get; set; }
    public int Metegol { get; set; }
    public int Arcade { get; set; }
    public int Otros { get; set; }
    public int Total { get; set; }
}

public class MaquinasEstadoDto
{
    public string Estado { get; set; } = "";
    public int Cantidad { get; set; }
}

public class ReporteGeneradoDto
{
    public int IdReporte { get; set; }
    public string TipoReporte { get; set; } = "";
    public string? Filtros { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string Usuario { get; set; } = "";
}
