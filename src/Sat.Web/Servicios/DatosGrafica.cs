using System.Globalization;
using Sat.Compartido.Contratos.Consultas;

namespace Sat.Web.Servicios;

/// <summary>
/// Todo lo necesario para dibujar una gráfica de barras: título, etiquetas del eje X y series.
/// Los métodos estáticos la arman a partir de las respuestas de la API; así la pantalla (SVG)
/// y el PDF dibujan exactamente los mismos datos.
/// </summary>
public class DatosGrafica
{
    public const string ColorEmitido = "#0d6efd";
    public const string ColorRecibido = "#fd7e14";
    public const string ColorValor = "#198754";

    /// <summary>A partir de cuántas fechas se usan etiquetas cortas (dd/MM).</summary>
    private const int MaximoEtiquetasLargas = 8;

    public string Titulo { get; }
    public List<string> Etiquetas { get; } = new List<string>();
    public List<SerieGrafica> Series { get; } = new List<SerieGrafica>();

    public DatosGrafica(string titulo)
    {
        Titulo = titulo;
    }

    /// <summary>Barras agrupadas: por cada NIT, IVA emitido e IVA recibido.</summary>
    public static DatosGrafica DeResumenIva(ResumenIvaRespuesta resumen)
    {
        DatosGrafica datos = new DatosGrafica(
            $"IVA emitido y recibido por NIT · {resumen.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
        SerieGrafica emitido = new SerieGrafica("IVA emitido", ColorEmitido);
        SerieGrafica recibido = new SerieGrafica("IVA recibido", ColorRecibido);

        foreach (IvaPorNit fila in resumen.Nits)
        {
            datos.Etiquetas.Add(fila.Nit);
            emitido.Valores.Add(fila.IvaEmitido);
            recibido.Valores.Add(fila.IvaRecibido);
        }

        datos.Series.Add(emitido);
        datos.Series.Add(recibido);
        return datos;
    }

    /// <summary>Barras simples: valor autorizado por fecha (total o sin IVA).</summary>
    public static DatosGrafica DeResumenRango(ResumenRangoRespuesta resumen, string descripcionTipo)
    {
        DatosGrafica datos = new DatosGrafica($"{descripcionTipo} por fecha");
        SerieGrafica valores = new SerieGrafica(descripcionTipo, ColorValor);

        // Con muchas fechas, la etiqueta corta (dd/MM) evita que los textos se encimen
        string formato = resumen.Fechas.Count > MaximoEtiquetasLargas ? "dd/MM" : "dd/MM/yyyy";

        foreach (TotalPorFecha fila in resumen.Fechas)
        {
            datos.Etiquetas.Add(fila.Fecha.ToString(formato, CultureInfo.InvariantCulture));
            valores.Valores.Add(fila.Monto);
        }

        datos.Series.Add(valores);
        return datos;
    }
}