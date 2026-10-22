using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Salida;

/// <summary>
/// Respuesta simple con un mensaje de texto (ej. confirmaciones o errores de parámetros).
/// </summary>
public class MensajeRespuesta
{
    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = "";
}