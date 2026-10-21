namespace Sat.Api.Dominio.Estructuras;

/// <summary>
/// Genera los códigos de autorización yyyymmdd######## con un correlativo que
/// reinicia en 1 cada día. Recuerda la fecha y el último número usado.
/// </summary>
public class ControlCorrelativo
{
    private const long Multiplicador = 100_000_000L; // 10^8 → deja espacio para 8 dígitos
    private const int MaximoDiario = 99_999_999;      // el mayor número de 8 dígitos

    public DateOnly FechaActual { get; private set; }
    public int UltimoNumero { get; private set; }

    /// <summary>Constructor para un sistema nuevo (sin códigos emitidos).</summary>
    public ControlCorrelativo() : this(DateOnly.MinValue, 0)
    {
    }

    /// <summary>Constructor para restaurar el estado guardado en archivo.</summary>
    public ControlCorrelativo(DateOnly fechaActual, int ultimoNumero)
    {
        FechaActual = fechaActual;
        UltimoNumero = ultimoNumero;
    }

    /// <summary>
    /// Devuelve el siguiente código para el instante "ahora".
    /// Recibe la fecha como parámetro (en vez de leer el reloj) para poder probarlo.
    /// </summary>
    public long GenerarCodigo(DateTime ahora)
    {
        DateOnly hoy = DateOnly.FromDateTime(ahora);

        if (hoy != FechaActual)   // cambió el día → reinicia el correlativo
        {
            FechaActual = hoy;
            UltimoNumero = 0;
        }

        if (UltimoNumero >= MaximoDiario)
        {
            throw new InvalidOperationException("Se agotó el correlativo del día.");
        }

        UltimoNumero++;

        long fechaNumerica = hoy.Year * 10_000L + hoy.Month * 100 + hoy.Day; // ej. 20261020
        return fechaNumerica * Multiplicador + UltimoNumero;
    }

    public void Reiniciar()
    {
        FechaActual = DateOnly.MinValue;
        UltimoNumero = 0;
    }
}