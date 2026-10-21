using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Salida;

/// <summary>
/// Respuesta a un archivo de autorizaciones. El enunciado la envuelve en un objeto:
/// { "resultadoAutorizaciones": { ... } }
/// </summary>
public class RespuestaAutorizaciones
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public ResultadoAutorizaciones Resultado { get; set; } = new ResultadoAutorizaciones();
}