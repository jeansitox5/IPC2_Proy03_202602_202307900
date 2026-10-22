using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// IVA emitido y recibido por un NIT en una fecha.
/// </summary>
public class IvaPorNit
{
    [JsonPropertyName("nit")]
    public string Nit { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("ivaEmitido")]
    public decimal IvaEmitido { get; set; }

    [JsonPropertyName("ivaRecibido")]
    public decimal IvaRecibido { get; set; }
}