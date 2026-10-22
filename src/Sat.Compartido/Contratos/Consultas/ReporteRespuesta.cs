using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Respuesta del reporte de autorizaciones por rango de fechas.
/// </summary>
public class ReporteRespuesta
{
    [JsonPropertyName("desde")]
    public DateOnly Desde { get; set; }

    [JsonPropertyName("hasta")]
    public DateOnly Hasta { get; set; }

    [JsonPropertyName("fechas")]
    public List<ReporteFecha> Fechas { get; set; } = new List<ReporteFecha>();
}