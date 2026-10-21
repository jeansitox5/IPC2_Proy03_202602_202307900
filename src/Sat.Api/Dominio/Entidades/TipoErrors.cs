namespace Sat.Api.Dominio.Entidades;

/// <summary>
/// Categorías de error que puede tener una factura.
/// Las 7 primeras son las del reporte del enunciado; FormatoInvalido es nuestra (decisión D7).
/// </summary>
public enum TipoError
{
    NitEmisorInvalido,
    NitReceptorInvalido,
    NitEmisorInexistente,
    NitReceptorInexistente,
    IvaMalCalculado,
    TotalMalCalculado,
    ReferenciaDuplicada,
    FormatoInvalido
}