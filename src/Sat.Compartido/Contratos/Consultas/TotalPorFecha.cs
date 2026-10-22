using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Una fila "por fecha" de un resumen: cuántas facturas y qué monto hubo ese día.
/// </summary>
public class TotalPorFecha
{
    [JsonPropertyName("fecha")]
    public DateOnly Fecha { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("monto")]
    public decimal Monto { get; set; }
}