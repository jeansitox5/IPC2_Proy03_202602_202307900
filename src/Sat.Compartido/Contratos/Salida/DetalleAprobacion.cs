using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Salida;

/// <summary>
/// Una aprobación dentro de la respuesta: {"referencia": "A1990", "codigoAprobacion": 2026092100000001}
/// </summary>
public class DetalleAprobacion
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; set; }
}