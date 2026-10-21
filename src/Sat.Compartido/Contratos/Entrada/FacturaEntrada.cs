using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Entrada;

/// <summary>
/// Una solicitud de autorización del mensaje de entrada, con los nombres EXACTOS del enunciado:
/// {"TIEMPO": "...", "REFERENCIA": "...", "NIT_EMISOR": "...", "NIT_RECEPTOR": "...",
///  "VALOR": 100.00, "IVA": 12.00, "TOTAL": 112.00}
/// </summary>
public class FacturaEntrada
{
    [JsonPropertyName("TIEMPO")]
    public string? Tiempo { get; set; }

    [JsonPropertyName("REFERENCIA")]
    public string? Referencia { get; set; }

    [JsonPropertyName("NIT_EMISOR")]
    public string? NitEmisor { get; set; }

    [JsonPropertyName("NIT_RECEPTOR")]
    public string? NitReceptor { get; set; }

    // AllowReadingFromString: acepta 100.00 y también "100.00" (por si algún archivo trae el número entre comillas)
    [JsonPropertyName("VALOR")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Valor { get; set; }

    [JsonPropertyName("IVA")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Iva { get; set; }

    [JsonPropertyName("TOTAL")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Total { get; set; }
}