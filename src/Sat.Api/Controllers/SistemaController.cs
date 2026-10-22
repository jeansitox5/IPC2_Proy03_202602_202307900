using Microsoft.AspNetCore.Mvc;
using Sat.Api.Datos;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Api.Controllers;

/// <summary>
/// Operaciones sobre el sistema completo:
///   POST /api/inicializar → borra todos los datos y deja el sistema en su estado inicial
/// </summary>
[ApiController]
[Route("api/inicializar")]
public class SistemaController : ControllerBase
{
    private readonly IAlmacenDatos _almacen;

    public SistemaController(IAlmacenDatos almacen)
    {
        _almacen = almacen;
    }

    [HttpPost]
    public ActionResult<MensajeRespuesta> Inicializar()
    {
        lock (_almacen.Candado)
        {
            _almacen.Inicializar();
        }

        return Ok(new MensajeRespuesta { Mensaje = "Sistema inicializado: se eliminaron todos los datos." });
    }
}