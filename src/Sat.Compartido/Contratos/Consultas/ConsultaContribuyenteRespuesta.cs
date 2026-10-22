using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Respuesta de "Consultar contribuyente": facturas emitidas o recibidas por un NIT en un rango,
/// agrupadas por fecha, con el total del rango y el listado de facturas.
/// </summary>
public class ConsultaContribuyenteRespuesta
{
    [JsonPropertyName("nit")]
    public string Nit { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = "";

    [JsonPropertyName("desde")]
    public DateOnly Desde { get; set; }

    [JsonPropertyName("hasta")]
    public DateOnly Hasta { get; set; }

    [JsonPropertyName("fechas")]
    public List<TotalPorFecha> Fechas { get; set; } = new List<TotalPorFecha>();

    [JsonPropertyName("cantidadTotal")]
    public int CantidadTotal { get; set; }

    [JsonPropertyName("montoTotal")]
    public decimal MontoTotal { get; set; }

    [JsonPropertyName("facturas")]
    public List<FacturaResumen> Facturas { get; set; } = new List<FacturaResumen>();
}