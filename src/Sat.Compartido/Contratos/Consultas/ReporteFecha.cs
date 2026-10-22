using System.Text.Json.Serialization;

namespace Sat.Compartido.Contratos.Consultas;

/// <summary>
/// Una fecha del reporte de autorizaciones, con todos los conteos que pide el enunciado.
/// </summary>
public class ReporteFecha
{
    [JsonPropertyName("fecha")]
    public DateOnly Fecha { get; set; }

    [JsonPropertyName("totalRecibidas")]
    public int TotalRecibidas { get; set; }

    [JsonPropertyName("nitEmisorInvalido")]
    public int NitEmisorInvalido { get; set; }

    [JsonPropertyName("nitReceptorInvalido")]
    public int NitReceptorInvalido { get; set; }

    [JsonPropertyName("nitEmisorInexistente")]
    public int NitEmisorInexistente { get; set; }

    [JsonPropertyName("nitReceptorInexistente")]
    public int NitReceptorInexistente { get; set; }

    [JsonPropertyName("ivaMalCalculado")]
    public int IvaMalCalculado { get; set; }

    [JsonPropertyName("totalMalCalculado")]
    public int TotalMalCalculado { get; set; }

    [JsonPropertyName("referenciaDuplicada")]
    public int ReferenciaDuplicada { get; set; }

    [JsonPropertyName("formatoInvalido")]
    public int FormatoInvalido { get; set; }

    [JsonPropertyName("sinErrores")]
    public int SinErrores { get; set; }

    [JsonPropertyName("cantidadEmisores")]
    public int CantidadEmisores { get; set; }

    [JsonPropertyName("cantidadReceptores")]
    public int CantidadReceptores { get; set; }
}