namespace Sat.Web.Servicios;

/// <summary>
/// Una serie de datos para una gráfica de barras: nombre (para la leyenda), color y un valor por etiqueta.
/// </summary>
public class SerieGrafica
{
    public string Nombre { get; }
    public string Color { get; }
    public List<decimal> Valores { get; } = new List<decimal>();

    public SerieGrafica(string nombre, string color)
    {
        Nombre = nombre;
        Color = color;
    }
}