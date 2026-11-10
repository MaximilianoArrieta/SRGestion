using Microsoft.AspNetCore.Mvc;

namespace SR.Api.Controladores;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected int IdUsuario => int.Parse(User.FindFirst("sub")!.Value);

    protected int? IdClienteUsuario =>
        int.TryParse(User.FindFirst("idCliente")?.Value, out var id) ? id : null;

    protected ObjectResult Error(string mensaje) =>
        Problem(detail: mensaje, statusCode: StatusCodes.Status400BadRequest, title: "No se pudo completar la operación");

    protected ObjectResult NoEncontrado(string que) =>
        Problem(detail: $"{que} no existe.", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

    protected FileContentResult Pdf(byte[] contenido, string nombre) =>
        File(contenido, "application/pdf", $"{nombre}-{DateTime.Now:yyyyMMdd}.pdf");
}
