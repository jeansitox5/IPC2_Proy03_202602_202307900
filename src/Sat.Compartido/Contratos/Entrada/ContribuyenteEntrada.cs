using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Entrada;

/// <summary>
/// Un elemento del mensaje de actualización del RTU:
/// {"NIT": "7378106", "nombre": "Empresa ABC, S.A."}
/// </summary>
public class ContribuyenteEntrada
{
    [JsonPropertyName("NIT")]
    public string? Nit { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }
}