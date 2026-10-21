using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Salida;

/// <summary>
/// Respuesta a una actualización del RTU, con la estructura EXACTA del enunciado:
/// {"CONTRIBUYENTES_NUEVOS": 2, "CONTRIBUYENTES_ACTUALIZADOS": 0, "NITS_INVALIDOS": 0}
/// </summary>
public class RespuestaRtu
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")]
    public int ContribuyentesNuevos { get; set; }

    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")]
    public int ContribuyentesActualizados { get; set; }

    [JsonPropertyName("NITS_INVALIDOS")]
    public int NitsInvalidos { get; set; }
}