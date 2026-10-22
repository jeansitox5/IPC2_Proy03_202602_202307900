using System.Globalization;
using System.Net;
using System.Text;

namespace Sat.Web.Servicios;

/// <summary>
/// Dibuja gráficas de barras como SVG (texto) con un algoritmo propio, sin bibliotecas externas.
/// Con una serie: barras simples. Con varias series: barras agrupadas con leyenda.
/// </summary>
public class GeneradorGraficas
{
    // Dimensiones del dibujo (unidades del viewBox; el SVG se escala al ancho disponible)
    private const double Ancho = 760;
    private const double Alto = 380;
    private const double MargenIzquierdo = 80;
    private const double MargenDerecho = 20;
    private const double MargenSuperior = 55;
    private const double MargenInferior = 60;
    private const int DivisionesEje = 5;

    public string Barras(DatosGrafica datos)
    {
        string titulo = datos.Titulo;
        List<string> etiquetas = datos.Etiquetas;
        List<SerieGrafica> series = datos.Series;

        double anchoArea = Ancho - MargenIzquierdo - MargenDerecho;
        double altoArea = Alto - MargenSuperior - MargenInferior;
        double baseY = MargenSuperior + altoArea;          // línea del eje X (valor 0)

        decimal maximoEje = CalcularMaximoEje(MayorValor(series));

        StringBuilder svg = new StringBuilder();
        svg.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {N(Ancho)} {N(Alto)}\" width=\"100%\" role=\"img\" font-family=\"Arial, sans-serif\">");
        svg.Append($"<rect x=\"0\" y=\"0\" width=\"{N(Ancho)}\" height=\"{N(Alto)}\" fill=\"#ffffff\" />");

        // 1) Título
        svg.Append($"<text x=\"{N(Ancho / 2)}\" y=\"24\" text-anchor=\"middle\" font-size=\"16\" font-weight=\"bold\" fill=\"#212529\">{Texto(titulo)}</text>");

        // 2) Líneas guía horizontales con su valor a la izquierda
        for (int i = 0; i <= DivisionesEje; i++)
        {
            decimal valorLinea = maximoEje * i / DivisionesEje;
            double y = baseY - altoArea * i / DivisionesEje;
            svg.Append($"<line x1=\"{N(MargenIzquierdo)}\" y1=\"{N(y)}\" x2=\"{N(Ancho - MargenDerecho)}\" y2=\"{N(y)}\" stroke=\"#dee2e6\" stroke-width=\"1\" />");
            svg.Append($"<text x=\"{N(MargenIzquierdo - 8)}\" y=\"{N(y + 4)}\" text-anchor=\"end\" font-size=\"11\" fill=\"#6c757d\">{valorLinea.ToString("N0", CultureInfo.InvariantCulture)}</text>");
        }

        // 3) Barras: un grupo por etiqueta, una barra por serie dentro del grupo
        int grupos = etiquetas.Count;
        if (grupos > 0)
        {
            double anchoGrupo = anchoArea / grupos;
            double anchoBarra = Math.Min(60, anchoGrupo * 0.7 / series.Count);

            for (int g = 0; g < grupos; g++)
            {
                double inicioGrupo = MargenIzquierdo + g * anchoGrupo + (anchoGrupo - anchoBarra * series.Count) / 2;

                for (int s = 0; s < series.Count; s++)
                {
                    decimal valor = series[s].Valores[g];
                    double altoBarra = maximoEje == 0 ? 0 : altoArea * (double)(valor / maximoEje);
                    double x = inicioGrupo + s * anchoBarra;
                    double y = baseY - altoBarra;

                    svg.Append($"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"{N(anchoBarra - 2)}\" height=\"{N(altoBarra)}\" fill=\"{series[s].Color}\" />");
                    svg.Append($"<text x=\"{N(x + (anchoBarra - 2) / 2)}\" y=\"{N(y - 4)}\" text-anchor=\"middle\" font-size=\"10\" fill=\"#212529\">{valor.ToString("N2", CultureInfo.InvariantCulture)}</text>");
                }

                // Etiqueta del grupo debajo del eje
                svg.Append($"<text x=\"{N(MargenIzquierdo + g * anchoGrupo + anchoGrupo / 2)}\" y=\"{N(baseY + 18)}\" text-anchor=\"middle\" font-size=\"11\" fill=\"#212529\">{Texto(etiquetas[g])}</text>");
            }
        }

        // 4) Ejes
        svg.Append($"<line x1=\"{N(MargenIzquierdo)}\" y1=\"{N(MargenSuperior)}\" x2=\"{N(MargenIzquierdo)}\" y2=\"{N(baseY)}\" stroke=\"#495057\" stroke-width=\"1.5\" />");
        svg.Append($"<line x1=\"{N(MargenIzquierdo)}\" y1=\"{N(baseY)}\" x2=\"{N(Ancho - MargenDerecho)}\" y2=\"{N(baseY)}\" stroke=\"#495057\" stroke-width=\"1.5\" />");

        // 5) Leyenda (solo si hay más de una serie)
        if (series.Count > 1)
        {
            double xLeyenda = MargenIzquierdo;
            for (int s = 0; s < series.Count; s++)
            {
                svg.Append($"<rect x=\"{N(xLeyenda)}\" y=\"{N(Alto - 22)}\" width=\"12\" height=\"12\" fill=\"{series[s].Color}\" />");
                svg.Append($"<text x=\"{N(xLeyenda + 18)}\" y=\"{N(Alto - 12)}\" font-size=\"12\" fill=\"#212529\">{Texto(series[s].Nombre)}</text>");
                xLeyenda += 160;
            }
        }

        svg.Append("</svg>");
        return svg.ToString();
    }

    /// <summary>
    /// Redondea el valor máximo hacia arriba a un número "cómodo" para el eje (1, 2, 5 o 10 × potencia de 10).
    /// Ej.: 336 → 500; 36 → 50; 1 200 → 2 000. Lo usa también GeneradorPdf.
    /// </summary>
    public static decimal CalcularMaximoEje(decimal maximo)
    {
        if (maximo <= 0)
        {
            return 1;
        }

        double potencia = Math.Pow(10, Math.Floor(Math.Log10((double)maximo)));
        double[] factores = { 1, 2, 5, 10 };

        foreach (double factor in factores)
        {
            decimal candidato = (decimal)(factor * potencia);
            if (candidato >= maximo)
            {
                return candidato;
            }
        }

        return (decimal)(10 * potencia);
    }

    /// <summary>El mayor valor entre todas las series (0 si no hay datos).</summary>
    public static decimal MayorValor(List<SerieGrafica> series)
    {
        decimal mayor = 0;
        foreach (SerieGrafica serie in series)
        {
            foreach (decimal valor in serie.Valores)
            {
                if (valor > mayor)
                {
                    mayor = valor;
                }
            }
        }

        return mayor;
    }

    /// <summary>Número con punto decimal, sin importar la configuración regional (el SVG lo exige).</summary>
    private static string N(double valor)
    {
        return valor.ToString("0.##", CultureInfo.InvariantCulture);
    }

    /// <summary>Escapa caracteres especiales (&lt;, &amp;, comillas) para que el texto no rompa el SVG.</summary>
    private static string Texto(string texto)
    {
        return WebUtility.HtmlEncode(texto);
    }
}