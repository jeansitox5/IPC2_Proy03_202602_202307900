using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Respuesta de "Resumen de IVA por fecha y NIT".
/// </summary>
public class ResumenIvaRespuesta
{
    [JsonPropertyName("fecha")]
    public DateOnly Fecha { get; set; }

    [JsonPropertyName("nits")]
    public List<IvaPorNit> Nits { get; set; } = new List<IvaPorNit>();

    [JsonPropertyName("totalIvaEmitido")]
    public decimal TotalIvaEmitido { get; set; }

    [JsonPropertyName("totalIvaRecibido")]
    public decimal TotalIvaRecibido { get; set; }
}