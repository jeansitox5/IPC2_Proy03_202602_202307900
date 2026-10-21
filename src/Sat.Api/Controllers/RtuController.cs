using Microsoft.AspNetCore.Mvc;
using Sat.Api.Servicios;
using Sat.Compartido.Contratos.Consultas;
using Sat.Compartido.Contratos.Entrada;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Api.Controllers;

/// <summary>
/// Endpoints del RTU:
///   POST /api/rtu  → actualiza el RTU con un arreglo de contribuyentes
///   GET  /api/rtu  → lista los contribuyentes registrados
/// </summary>
[ApiController]
[Route("api/rtu")]
public class RtuController : ControllerBase
{
    private readonly ServicioRtu _servicio;

    public RtuController(ServicioRtu servicio)
    {
        _servicio = servicio;
    }

    [HttpPost]
    public ActionResult<RespuestaRtu> Actualizar([FromBody] List<ContribuyenteEntrada> contribuyentes)
    {
        RespuestaRtu respuesta = _servicio.Procesar(contribuyentes);
        return Ok(respuesta);
    }

    [HttpGet]
    public ActionResult<List<ContribuyenteSalida>> Listar()
    {
        return Ok(_servicio.Listar());
    }
}