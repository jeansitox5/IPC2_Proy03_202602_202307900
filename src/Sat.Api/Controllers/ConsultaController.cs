using Microsoft.AspNetCore.Mvc;
using Sat.Api.Servicios;
using Sat.Compartido.Contratos.Consultas;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Api.Controllers;

/// <summary>
/// Endpoints de consulta. Todos son GET y reciben sus datos como parámetros en la URL.
/// Las fechas van en formato yyyy-MM-dd (ej. 2025-01-15).
///   GET /api/consultadatos  → parámetros: nit, tipo (emitidas | recibidas), desde, hasta
///   GET /api/resumeniva     → parámetro: fecha
///   GET /api/resumenrango   → parámetros: desde, hasta, tipo (total | siniva)
///   GET /api/reporte        → parámetros: desde, hasta
/// </summary>
[ApiController]
[Route("api")]
public class ConsultaController : ControllerBase
{
    private readonly ServicioConsultas _servicio;

    public ConsultaController(ServicioConsultas servicio)
    {
        _servicio = servicio;
    }

    [HttpGet("consultadatos")]
    public ActionResult<ConsultaContribuyenteRespuesta> ConsultaDatos(
        [FromQuery] string? nit, [FromQuery] string? tipo,
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        if (string.IsNullOrWhiteSpace(nit) || string.IsNullOrWhiteSpace(tipo) || desde == null || hasta == null)
        {
            return BadRequest(Mensaje("Parámetros requeridos: nit, tipo (emitidas | recibidas), desde y hasta (yyyy-MM-dd)."));
        }

        try
        {
            return Ok(_servicio.ConsultarContribuyente(nit, tipo, desde.Value, hasta.Value));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(Mensaje(ex.Message));
        }
    }

    [HttpGet("resumeniva")]
    public ActionResult<ResumenIvaRespuesta> ResumenIva([FromQuery] DateOnly? fecha)
    {
        if (fecha == null)
        {
            return BadRequest(Mensaje("Parámetro requerido: fecha (yyyy-MM-dd)."));
        }

        return Ok(_servicio.ResumenIva(fecha.Value));
    }

    [HttpGet("resumenrango")]
    public ActionResult<ResumenRangoRespuesta> ResumenRango(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] string? tipo)
    {
        if (desde == null || hasta == null || string.IsNullOrWhiteSpace(tipo))
        {
            return BadRequest(Mensaje("Parámetros requeridos: desde, hasta (yyyy-MM-dd) y tipo (total | siniva)."));
        }

        try
        {
            return Ok(_servicio.ResumenRango(desde.Value, hasta.Value, tipo));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(Mensaje(ex.Message));
        }
    }

    [HttpGet("reporte")]
    public ActionResult<ReporteRespuesta> Reporte([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        if (desde == null || hasta == null)
        {
            return BadRequest(Mensaje("Parámetros requeridos: desde y hasta (yyyy-MM-dd)."));
        }

        try
        {
            return Ok(_servicio.Reporte(desde.Value, hasta.Value));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(Mensaje(ex.Message));
        }
    }

    /// <summary>Crea el cuerpo JSON {"mensaje": "..."} para las respuestas de error.</summary>
    private static MensajeRespuesta Mensaje(string texto)
    {
        return new MensajeRespuesta { Mensaje = texto };
    }
}