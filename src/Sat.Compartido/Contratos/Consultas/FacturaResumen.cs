using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Datos de una factura autorizada para mostrarla en un listado.
/// </summary>
public class FacturaResumen
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("fechaEmision")]
    public DateTime FechaEmision { get; set; }

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; set; }

    [JsonPropertyName("nitEmisor")]
    public string NitEmisor { get; set; } = "";

    [JsonPropertyName("nitReceptor")]
    public string NitReceptor { get; set; } = "";

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("iva")]
    public decimal Iva { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }
}