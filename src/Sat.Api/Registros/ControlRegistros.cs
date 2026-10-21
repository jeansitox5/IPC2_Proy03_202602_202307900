namespace Sat.Api.Datos.Registros;

/// <summary>
/// Forma en que el estado del correlativo se guarda en control.json.
/// </summary>
public class ControlRegistro
{
    public DateOnly FechaActual { get; set; }
    public int UltimoNumero { get; set; }
}