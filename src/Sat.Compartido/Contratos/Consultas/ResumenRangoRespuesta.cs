using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Respuesta de "Resumen por rango de fechas": valor autorizado por fecha (total o sin IVA).
/// </summary>
public class ResumenRangoRespuesta
{
    [JsonPropertyName("desde")]
    public DateOnly Desde { get; set; }

    [JsonPropertyName("hasta")]
    public DateOnly Hasta { get; set; }

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = "";

    [JsonPropertyName("fechas")]
    public List<TotalPorFecha> Fechas { get; set; } = new List<TotalPorFecha>();

    [JsonPropertyName("montoTotal")]
    public decimal MontoTotal { get; set; }
}