using Microsoft.AspNetCore.Mvc;
using Sat.Api.Servicios;
using Sat.Compartido.Contratos.Entrada;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Api.Controllers;

/// <summary>
/// Endpoint de autorizaciones:
///   POST /api/procesar → recibe un arreglo de solicitudes y devuelve el resultado
/// </summary>
[ApiController]
[Route("api/procesar")]
public class AutorizacionController : ControllerBase
{
    private readonly ServicioAutorizacion _servicio;

    public AutorizacionController(ServicioAutorizacion servicio)
    {
        _servicio = servicio;
    }

    [HttpPost]
    public ActionResult<RespuestaAutorizaciones> Procesar([FromBody] List<FacturaEntrada> solicitudes)
    {
        RespuestaAutorizaciones respuesta = _servicio.Procesar(solicitudes);
        return Ok(respuesta);
    }
}