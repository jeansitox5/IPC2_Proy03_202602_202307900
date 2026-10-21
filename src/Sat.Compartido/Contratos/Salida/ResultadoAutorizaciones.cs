using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Salida;

/// <summary>
/// Contenido de "resultadoAutorizaciones" en la respuesta, con los nombres EXACTOS del enunciado.
/// </summary>
public class ResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]
    public int Aprobadas { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]
    public int RechazadasPorNitEmisorInexistente { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]
    public int RechazadasPorNitReceptorInexistente { get; set; }

    [JsonPropertyName("detalleAprobaciones")]
    public List<DetalleAprobacion> Detalle { get; set; } = new List<DetalleAprobacion>();
}