# SR · Sistema de Gestión de Máquinas Recreativas

Solución de Visual Studio con **.NET 10** que tiene tres partes:

| Proyecto | Qué es |
|---|---|
| **SR.Api** | Web API ASP.NET Core con Entity Framework Core (SQL Server), login con JWT y reportes en PDF. |
| **SR.Web** | Aplicación web **Blazor WebAssembly Standalone**. |
| **SR.Mobile** | Aplicación móvil **.NET MAUI Blazor Hybrid** (Android y Windows). |
| **SR.Shared** | Biblioteca común: DTOs, enums y el cliente HTTP de la API (lo usan la web y la app). |

Las pantallas de la web y de la app móvil están **separadas** (cada una tiene las suyas): la web usa tablas y la app usa tarjetas, una barra inferior con íconos y "hojas" que suben desde abajo, siguiendo las maquetas del documento. Las dos llaman a la misma API con el mismo cliente (`SR.Shared/Servicios/SrApi.cs`).

Las tres siguen la identidad de SR: fondo negro, crema del logo, las tres franjas (celeste `#04B9F8`, naranja `#FE9E10` y rojo `#F80343`), letra Tahoma, tres tamaños de letra (grande, 66% y 75%) y el botón de modo claro/oscuro.

---

## Requisitos

- **Visual Studio 2026** con las cargas de trabajo:
  - *Desarrollo de ASP.NET y web*
  - *Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET* (MAUI)
- **SQL Server LocalDB** (se instala con Visual Studio) o SQL Server Express.
- Para Android: un emulador creado desde *Herramientas → Android → Administrador de dispositivos Android*.

## Cómo ponerlo en marcha

1. Abrí `SRGestion.slnx` en Visual Studio. La primera vez descarga los paquetes NuGet solo.
2. **Base de datos:** no hace falta crear nada. Al arrancar la API por primera vez crea la base `SRMaquinas` en LocalDB y carga datos de ejemplo.
   - Si usás SQL Server Express u otro servidor, cambiá la cadena `ConnectionStrings:SR` en `SR.Api/appsettings.json`, por ejemplo:
     `Server=.\\SQLEXPRESS;Database=SRMaquinas;Trusted_Connection=True;TrustServerCertificate=True`
3. **Arrancar API + web juntas:** clic derecho en la solución → *Configurar proyectos de inicio* → *Varios proyectos de inicio* → poné **SR.Api** e **SR.Web** en *Iniciar* (API primero). Usá el perfil **https** en los dos.
   - API: `https://localhost:7080` (se abre `/scalar` para probar los endpoints)
   - Web: `https://localhost:7100`
4. **App móvil:** con la API corriendo, elegí **SR.Mobile** y el destino:
   - *Windows Machine*: se conecta a `https://localhost:7080`.
   - *Emulador de Android*: se conecta a `http://10.0.2.2:5080`, que dentro del emulador es tu PC.
   - Si nunca usaste el certificado de desarrollo: en una terminal, `dotnet dev-certs https --trust`.

### Usuarios de prueba

| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | `admin` | `Admin123!` |
| Cliente (Bar El Taco) | `eltaco` | `Cliente123!` |

El admin puede crearle usuario a cualquier comercio desde *Clientes → Editar → Acceso al sistema*.

### Probar en un celular real

1. Averiguá la IP de tu PC en la red WiFi (`ipconfig`, por ejemplo `192.168.0.15`).
2. En `SR.Api/Properties/launchSettings.json`, en el perfil `http`, cambiá `applicationUrl` a `http://0.0.0.0:5080` y arrancá la API con ese perfil.
3. En `SR.Mobile/Configuracion.cs` poné `http://192.168.0.15:5080/`.
4. Si el Firewall de Windows pregunta, permití el acceso.

---

## Base de datos

Se parte del diagrama entidad-relación del documento, con estos ajustes:

| Cambio | Por qué |
|---|---|
| **Usuario** tiene `IdCliente` (opcional) | El documento dice que el cliente solo ve *sus* máquinas: hace falta saber a qué comercio pertenece cada usuario con rol Cliente. |
| **Usuario.Password** pasa a `PasswordHash` y se agrega `NombreUsuario` (único) | Las contraseñas no se guardan en texto plano. El login de la maqueta pide "Usuario". |
| **Cobranza** tiene `IdCliente` y `MontoComercio` | Una máquina cambia de comercio con el tiempo: el cobro queda asociado al comercio donde estaba. La maqueta del cliente muestra "monto recaudado" y "monto pagado". |
| **Solicitud** tiene `IdMaquina` (opcional) y `Respuesta` | Mantenimiento y retiro son sobre una máquina concreta; pedir una máquina nueva no. El admin le responde al comercio. |
| **Cliente** tiene `Activo` | "Dar de baja" es una baja lógica: no se pierde el historial. Lo mismo para máquinas (estado *Baja*). |
| **Reporte** registra cada PDF generado (`TipoReporte`, `Filtros`, `FechaGeneracion`, `IdUsuario`) | Queda un historial de quién sacó qué reporte y con qué filtros. Se ve en la pantalla de Reportes. |
| Estados como texto (`Disponible`, `Instalada`, `EnReparacion`, `Baja`, etc.) | Se leen directamente en la base. |
| Ninguna relación borra en cascada | Nunca se pierde el historial de cobranzas, instalaciones o mantenimientos. |

**12 tablas:** `Provincia`, `Localidad`, `Cliente`, `TipoMaquina`, `Maquina`, `Instalacion`, `Cobranza`, `Mantenimiento`, `Solicitud`, `Rol`, `Usuario` y `Reporte`. Están definidas en `SR.Api/Datos/Entidades.cs` y `SR.Api/Datos/SrDbContext.cs`.

**Con migraciones (opcional, si te las piden):** en *Herramientas → Administrador de paquetes NuGet → Consola*, con **SR.Api** como proyecto predeterminado:

```
Add-Migration Inicial
Update-Database
```

Si ya se había creado la base sin migraciones, borrala antes (desde el *Explorador de objetos de SQL Server*). Desde que existe una migración, la API usa migraciones en vez de crear la base directamente. Para obtener el script SQL completo: `Script-Migration`.

---

## Funcionalidades

**Administrador**
- **Inicio:** máquinas activas, recaudación del mes contra el mes anterior, solicitudes y mantenimientos pendientes, próximos retiros (30 días).
- **Clientes:** alta, edición, baja lógica con confirmación, reactivación y usuario de acceso del comercio.
- **Máquinas:** alta, edición, baja, filtros por estado y tipo, y ficha con todo el historial (ubicaciones, cobranzas y mantenimientos).
- **Operaciones:**
  - *Instalaciones:* instalar una máquina disponible en un comercio y registrar el retiro.
  - *Cobranzas:* registrar el cobro y la parte que se paga al comercio (calcula el % automáticamente).
  - *Mantenimientos:* registrar preventivos y correctivos con costo y estado.
  - *Solicitudes:* aprobar, rechazar o resolver los pedidos con una respuesta.
- **Reportes** (en PDF): recaudación por período, historial de cobranzas, máquinas por localidad, máquinas activas e inactivas e historial de mantenimientos. Cada PDF queda registrado en la tabla Reporte.

**Cliente (comercio)**
- **Máquinas asignadas:** solicitar retiro o mantenimiento de cada una, o pedir una máquina nueva.
- **Historial de cobranzas:** con exportación a PDF.
- **Mis solicitudes:** estado y respuesta de SR.

**Reglas automáticas**
- Solo se puede instalar una máquina *Disponible*. Al instalarla pasa a *Instalada* y al retirarla vuelve a *Disponible*.
- No se puede dar de baja un cliente con máquinas instaladas ni una máquina instalada.
- Al registrar un retiro, la solicitud de retiro de esa máquina queda *Resuelta*.
- Al crear un mantenimiento, la solicitud de mantenimiento pasa a *Aprobada*. Al finalizarlo, queda *Resuelta*.
- El cliente solo puede pedir algo sobre máquinas instaladas en su comercio y no puede repetir una solicitud pendiente.

---

## Estructura

```
SRGestion.slnx
SR.Shared/            DTOs, enums, SrApi (cliente HTTP) y SrAuthStateProvider (sesión)
SR.Api/
  Datos/              Entidades, DbContext, datos iniciales y mapeos a DTOs
  Controladores/      Auth, Catalogos, Dashboard, Clientes, Maquinas, Instalaciones,
                      Cobranzas, Mantenimientos, Solicitudes, Reportes
  Servicios/          TokenService (JWT) y PdfService (reportes con QuestPDF)
SR.Web/
  Layout/ Pages/Admin/ Pages/Cliente/ Componentes/
  wwwroot/css/app.css estilos con la identidad de SR
SR.Mobile/
  Components/Layout/ Components/Pages/Admin/ Components/Pages/Cliente/ Components/Shared/
  Platforms/          Android y Windows
  wwwroot/css/app.css estilos móviles
```

## Endpoints de la API

| Método | Ruta | Rol |
|---|---|---|
| POST | `api/auth/login` | público |
| GET | `api/catalogos/provincias`, `api/catalogos/tipos-maquina` | logueado |
| GET | `api/dashboard` | Admin |
| GET/POST/PUT/DELETE | `api/clientes`, `api/clientes/{id}`, `PUT api/clientes/{id}/reactivar`, `POST api/clientes/{id}/usuario` | Admin |
| GET/POST/PUT/DELETE | `api/maquinas`, `api/maquinas/{id}` | Admin |
| GET | `api/maquinas/mias` | Cliente |
| GET/POST | `api/instalaciones`, `POST api/instalaciones/{id}/retiro` | Admin |
| GET/POST | `api/cobranzas` | Admin |
| GET | `api/cobranzas/mias` | Cliente |
| GET/POST/PUT | `api/mantenimientos`, `api/mantenimientos/{id}` | Admin |
| GET/PUT | `api/solicitudes`, `api/solicitudes/{id}` | Admin |
| GET/POST | `api/solicitudes/mias`, `POST api/solicitudes` | Cliente |
| GET | `api/reportes/recaudacion`, `maquinas-localidad`, `maquinas-estado` | Admin |
| GET | `api/reportes/{recaudacion,cobranzas,maquinas-localidad,maquinas-estado,mantenimientos}/pdf` | Admin |
| GET | `api/reportes/mis-cobranzas/pdf` | Cliente |
| GET | `api/reportes/generados` (tabla Reporte) | Admin |

## Notas

- **QuestPDF** se usa con la licencia *Community*, que es gratuita para proyectos educativos y para empresas que facturan menos de 1 millón de dólares al año.
- La clave JWT de `appsettings.json` es solo para desarrollo. En producción hay que cambiarla.
- Los datos de ejemplo (comercios, máquinas y cobranzas) se cargan solo la primera vez. Para no cargarlos, poné `"DatosDeEjemplo": false` antes de crear la base.
