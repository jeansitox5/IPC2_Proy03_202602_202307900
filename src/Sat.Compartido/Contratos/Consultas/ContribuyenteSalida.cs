using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Un contribuyente tal como lo devuelve GET /api/rtu (formato de diseño propio).
/// </summary>
public class ContribuyenteSalida
{
    [JsonPropertyName("nit")]
    public string Nit { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";
}