using System.Linq.Expressions;
using SR.Shared;
using SR.Shared.Dtos;

namespace SR.Api.Datos;

public static class Mapeos
{
    public static readonly Expression<Func<Cliente, ClienteDto>> ACliente = c => new ClienteDto
    {
        IdCliente = c.IdCliente,
        RazonSocial = c.RazonSocial,
        Responsable = c.Responsable,
        Telefono = c.Telefono,
        Direccion = c.Direccion,
        DatosFiscales = c.DatosFiscales,
        IdLocalidad = c.IdLocalidad,
        Localidad = c.Localidad.Nombre,
        Provincia = c.Localidad.Provincia.Nombre,
        Activo = c.Activo,
        MaquinasInstaladas = c.Instalaciones.Count(i => i.Estado == EstadoInstalacion.Activa),
        UsuarioAcceso = c.Usuarios.Select(u => u.NombreUsuario).FirstOrDefault()
    };

    public static readonly Expression<Func<Maquina, MaquinaDto>> AMaquina = m => new MaquinaDto
    {
        IdMaquina = m.IdMaquina,
        NumeroIdentificacion = m.NumeroIdentificacion,
        Descripcion = m.Descripcion,
        IdTipoMaquina = m.IdTipoMaquina,
        Tipo = m.TipoMaquina.Descripcion,
        Estado = m.Estado,
        FechaAlta = m.FechaAlta,
        IdClienteActual = m.Instalaciones.Where(i => i.Estado == EstadoInstalacion.Activa).Select(i => (int?)i.IdCliente).FirstOrDefault(),
        ClienteActual = m.Instalaciones.Where(i => i.Estado == EstadoInstalacion.Activa).Select(i => i.Cliente.RazonSocial).FirstOrDefault(),
        LocalidadActual = m.Instalaciones.Where(i => i.Estado == EstadoInstalacion.Activa).Select(i => i.Cliente.Localidad.Nombre).FirstOrDefault(),
        FechaRetiroProgramada = m.Instalaciones.Where(i => i.Estado == EstadoInstalacion.Activa).Select(i => i.FechaRetiroProgramada).FirstOrDefault()
    };

    public static readonly Expression<Func<Instalacion, InstalacionDto>> AInstalacion = i => new InstalacionDto
    {
        IdInstalacion = i.IdInstalacion,
        IdMaquina = i.IdMaquina,
        Maquina = i.Maquina.NumeroIdentificacion,
        Tipo = i.Maquina.TipoMaquina.Descripcion,
        IdCliente = i.IdCliente,
        Cliente = i.Cliente.RazonSocial,
        Localidad = i.Cliente.Localidad.Nombre,
        FechaInstalacion = i.FechaInstalacion,
        FechaRetiroProgramada = i.FechaRetiroProgramada,
        FechaRetiroReal = i.FechaRetiroReal,
        Estado = i.Estado
    };

    public static readonly Expression<Func<Cobranza, CobranzaDto>> ACobranza = c => new CobranzaDto
    {
        IdCobranza = c.IdCobranza,
        IdMaquina = c.IdMaquina,
        Maquina = c.Maquina.NumeroIdentificacion,
        Tipo = c.Maquina.TipoMaquina.Descripcion,
        IdCliente = c.IdCliente,
        Cliente = c.Cliente.RazonSocial,
        Localidad = c.Cliente.Localidad.Nombre,
        FechaCobro = c.FechaCobro,
        MontoRecaudado = c.MontoRecaudado,
        MontoComercio = c.MontoComercio,
        Observaciones = c.Observaciones,
        Usuario = c.Usuario.Nombre + " " + c.Usuario.Apellido
    };

    public static readonly Expression<Func<Mantenimiento, MantenimientoDto>> AMantenimiento = m => new MantenimientoDto
    {
        IdMantenimiento = m.IdMantenimiento,
        IdMaquina = m.IdMaquina,
        Maquina = m.Maquina.NumeroIdentificacion,
        Tipo = m.Maquina.TipoMaquina.Descripcion,
        Fecha = m.Fecha,
        TipoMantenimiento = m.TipoMantenimiento,
        Descripcion = m.Descripcion,
        Costo = m.Costo,
        Estado = m.Estado
    };

    public static readonly Expression<Func<Solicitud, SolicitudDto>> ASolicitud = s => new SolicitudDto
    {
        IdSolicitud = s.IdSolicitud,
        IdCliente = s.IdCliente,
        Cliente = s.Cliente.RazonSocial,
        Localidad = s.Cliente.Localidad.Nombre,
        IdMaquina = s.IdMaquina,
        Maquina = s.Maquina != null ? s.Maquina.NumeroIdentificacion : null,
        TipoSolicitud = s.TipoSolicitud,
        FechaSolicitud = s.FechaSolicitud,
        Estado = s.Estado,
        Observaciones = s.Observaciones,
        Respuesta = s.Respuesta
    };
}
