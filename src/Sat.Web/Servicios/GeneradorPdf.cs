using System.Globalization;
using Sat.Compartido.Contratos.Consultas;

namespace Sat.Web.Servicios;

/// <summary>
/// Arma los reportes PDF de cada petición usando DocumentoPdf (escritor propio).
/// Cada método público recibe la misma respuesta de la API que muestra la pantalla.
/// </summary>
public class GeneradorPdf
{
    private const string ColorTitulo = "#0d6efd";
    private const string ColorGris = "#6c757d";
    private const string ColorFondoEncabezado = "#e9ecef";
    private const string ColorFondoAlterno = "#f8f9fa";
    private const string ColorBorde = "#dee2e6";

    // ================================================================== Reportes
    public byte[] Contribuyente(ConsultaContribuyenteRespuesta r)
    {
        DocumentoPdf doc = new DocumentoPdf();
        Encabezado(doc, "Consultar contribuyente",
            $"{r.Nit} - {r.Nombre} · facturas {r.Tipo} del {Fecha(r.Desde)} al {Fecha(r.Hasta)}");

        if (r.Fechas.Count == 0)
        {
            Parrafo(doc, "No hay facturas autorizadas en ese rango.");
            return doc.Generar();
        }

        Subtitulo(doc, "Resumen por fecha");
        List<string[]> filas = new List<string[]>();
        foreach (TotalPorFecha fila in r.Fechas)
        {
            filas.Add(new[] { Fecha(fila.Fecha), fila.Cantidad.ToString(), Monto(fila.Monto) });
        }
        Tabla(doc, new[] { "Fecha", "Facturas autorizadas", "Monto facturado (Q)" },
              new double[] { 150, 150, 170 }, new[] { false, true, true }, filas,
              new[] { "Total del rango", r.CantidadTotal.ToString(), Monto(r.MontoTotal) });

        Subtitulo(doc, "Listado de facturas");
        List<string[]> facturas = new List<string[]>();
        foreach (FacturaResumen f in r.Facturas)
        {
            facturas.Add(new[]
            {
                f.FechaEmision.ToString("dd/MM/yyyy HH:mm"), f.Referencia, f.CodigoAprobacion.ToString(),
                f.NitEmisor, f.NitReceptor, Monto(f.Valor), Monto(f.Iva), Monto(f.Total)
            });
        }
        Tabla(doc, new[] { "Fecha", "Referencia", "Código aprobación", "NIT emisor", "NIT receptor", "Valor", "IVA", "Total" },
              new double[] { 76, 56, 92, 60, 60, 56, 50, 62 },
              new[] { false, false, false, false, false, true, true, true }, facturas, null, 8);

        return doc.Generar();
    }

    public byte[] ResumenIva(ResumenIvaRespuesta r)
    {
        DocumentoPdf doc = new DocumentoPdf();
        Encabezado(doc, "Resumen de IVA por fecha y NIT", $"Movimientos del {Fecha(r.Fecha)}");

        if (r.Nits.Count == 0)
        {
            Parrafo(doc, "No hay facturas autorizadas en esa fecha.");
            return doc.Generar();
        }

        List<string[]> filas = new List<string[]>();
        foreach (IvaPorNit fila in r.Nits)
        {
            filas.Add(new[] { fila.Nit, fila.Nombre, Monto(fila.IvaEmitido), Monto(fila.IvaRecibido) });
        }
        Tabla(doc, new[] { "NIT", "Nombre", "IVA emitido (Q)", "IVA recibido (Q)" },
              new double[] { 90, 200, 110, 112 }, new[] { false, false, true, true }, filas,
              new[] { "Total", "", Monto(r.TotalIvaEmitido), Monto(r.TotalIvaRecibido) });

        GraficaBarras(doc, DatosGrafica.DeResumenIva(r));
        return doc.Generar();
    }

    public byte[] ResumenRango(ResumenRangoRespuesta r, string descripcionTipo)
    {
        DocumentoPdf doc = new DocumentoPdf();
        Encabezado(doc, "Resumen por rango de fechas",
            $"{descripcionTipo} · del {Fecha(r.Desde)} al {Fecha(r.Hasta)}");

        if (r.Fechas.Count == 0)
        {
            Parrafo(doc, "No hay facturas autorizadas en ese rango.");
            return doc.Generar();
        }

        List<string[]> filas = new List<string[]>();
        foreach (TotalPorFecha fila in r.Fechas)
        {
            filas.Add(new[] { Fecha(fila.Fecha), fila.Cantidad.ToString(), Monto(fila.Monto) });
        }
        Tabla(doc, new[] { "Fecha", "Facturas", $"{descripcionTipo} (Q)" },
              new double[] { 150, 120, 200 }, new[] { false, true, true }, filas,
              new[] { "Total del rango", "", Monto(r.MontoTotal) });

        GraficaBarras(doc, DatosGrafica.DeResumenRango(r, descripcionTipo));
        return doc.Generar();
    }

    public byte[] Reporte(ReporteRespuesta r)
    {
        DocumentoPdf doc = new DocumentoPdf();
        Encabezado(doc, "Reporte de autorizaciones", $"Del {Fecha(r.Desde)} al {Fecha(r.Hasta)}");

        if (r.Fechas.Count == 0)
        {
            Parrafo(doc, "No se recibieron facturas en ese rango.");
            return doc.Generar();
        }

        foreach (ReporteFecha f in r.Fechas)
        {
            List<string[]> filas = new List<string[]>
            {
                new[] { "Cantidad total de facturas recibidas", f.TotalRecibidas.ToString() },
                new[] { "   NIT emisor inválido", f.NitEmisorInvalido.ToString() },
                new[] { "   NIT receptor inválido", f.NitReceptorInvalido.ToString() },
                new[] { "   NIT emisor inexistente", f.NitEmisorInexistente.ToString() },
                new[] { "   NIT receptor inexistente", f.NitReceptorInexistente.ToString() },
                new[] { "   IVA mal calculado", f.IvaMalCalculado.ToString() },
                new[] { "   TOTAL mal calculado", f.TotalMalCalculado.ToString() },
                new[] { "   REFERENCIA duplicada", f.ReferenciaDuplicada.ToString() },
                new[] { "   Formato inválido (datos mal formados)", f.FormatoInvalido.ToString() },
                new[] { "Cantidad de facturas sin errores", f.SinErrores.ToString() },
                new[] { "Cantidad de emisores de facturas", f.CantidadEmisores.ToString() },
                new[] { "Cantidad de receptores de facturas", f.CantidadReceptores.ToString() }
            };

            doc.AsegurarEspacio(18 * (filas.Count + 2));   // el bloque de una fecha no se parte entre páginas
            Tabla(doc, new[] { $"FECHA: {Fecha(f.Fecha)}", "" }, new double[] { 330, 80 },
                  new[] { false, true }, filas);
        }

        return doc.Generar();
    }

    // ================================================================== Piezas reutilizables
    private static void Encabezado(DocumentoPdf doc, string titulo, string subtitulo)
    {
        doc.Texto(DocumentoPdf.Margen, doc.Y + 16, titulo, 18, true, ColorTitulo);
        doc.Texto(DocumentoPdf.Margen, doc.Y + 34, subtitulo, 10, false, ColorGris);
        doc.Texto(DocumentoPdf.Margen, doc.Y + 48,
            $"SAT · Autorización de DTE · Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}",
            8, false, ColorGris);
        doc.Linea(DocumentoPdf.Margen, doc.Y + 56, DocumentoPdf.AnchoPagina - DocumentoPdf.Margen, doc.Y + 56, 1, ColorTitulo);
        doc.Y += 72;
    }

    private static void Subtitulo(DocumentoPdf doc, string texto)
    {
        doc.AsegurarEspacio(40);
        doc.Texto(DocumentoPdf.Margen, doc.Y + 12, texto, 12, true);
        doc.Y += 20;
    }

    private static void Parrafo(DocumentoPdf doc, string texto)
    {
        doc.AsegurarEspacio(20);
        doc.Texto(DocumentoPdf.Margen, doc.Y + 12, texto, 10);
        doc.Y += 20;
    }

    /// <summary>
    /// Dibuja una tabla: fila de encabezado (con fondo), filas alternadas y, opcionalmente, fila de totales.
    /// Si la tabla no cabe, continúa en otra página repitiendo el encabezado.
    /// </summary>
    private static void Tabla(DocumentoPdf doc, string[] encabezados, double[] anchos, bool[] alinearDerecha,
                              List<string[]> filas, string[]? pie = null, double tamano = 9)
    {
        double altoFila = tamano + 8;

        doc.AsegurarEspacio(altoFila * 2);
        DibujarFila(doc, encabezados, anchos, alinearDerecha, altoFila, tamano, true, ColorFondoEncabezado);

        for (int i = 0; i < filas.Count; i++)
        {
            if (doc.AsegurarEspacio(altoFila))
            {
                DibujarFila(doc, encabezados, anchos, alinearDerecha, altoFila, tamano, true, ColorFondoEncabezado);
            }

            string? fondo = i % 2 == 1 ? ColorFondoAlterno : null;
            DibujarFila(doc, filas[i], anchos, alinearDerecha, altoFila, tamano, false, fondo);
        }

        if (pie != null)
        {
            doc.AsegurarEspacio(altoFila);
            DibujarFila(doc, pie, anchos, alinearDerecha, altoFila, tamano, true, ColorFondoEncabezado);
        }

        doc.Y += 14;
    }

    private static void DibujarFila(DocumentoPdf doc, string[] celdas, double[] anchos, bool[] alinearDerecha,
                                    double altoFila, double tamano, bool negrita, string? fondo)
    {
        double anchoTotal = 0;
        foreach (double ancho in anchos)
        {
            anchoTotal += ancho;
        }

        if (fondo != null)
        {
            doc.Rectangulo(DocumentoPdf.Margen, doc.Y, anchoTotal, altoFila, fondo);
        }

        double x = DocumentoPdf.Margen;
        double lineaBase = doc.Y + altoFila - 5;
        for (int c = 0; c < celdas.Length; c++)
        {
            if (alinearDerecha[c])
            {
                doc.TextoDerecha(x + anchos[c] - 4, lineaBase, celdas[c], tamano, negrita);
            }
            else
            {
                doc.Texto(x + 4, lineaBase, celdas[c], tamano, negrita);
            }
            x += anchos[c];
        }

        doc.Linea(DocumentoPdf.Margen, doc.Y + altoFila, DocumentoPdf.Margen + anchoTotal, doc.Y + altoFila, 0.5, ColorBorde);
        doc.Y += altoFila;
    }

    /// <summary>Gráfica de barras (simples o agrupadas) dibujada con rectángulos y líneas del PDF.</summary>
    private static void GraficaBarras(DocumentoPdf doc, DatosGrafica datos)
    {
        const double altoArea = 180;
        const double margenIzquierdo = 55;
        double anchoArea = doc.AnchoUtil - margenIzquierdo;
        double x0 = DocumentoPdf.Margen + margenIzquierdo;

        doc.AsegurarEspacio(altoArea + 90);
        doc.Texto(DocumentoPdf.Margen, doc.Y + 12, datos.Titulo, 12, true);
        double baseY = doc.Y + 30 + altoArea;   // línea del eje X

        decimal maximoEje = GeneradorGraficas.CalcularMaximoEje(GeneradorGraficas.MayorValor(datos.Series));

        // Líneas guía y valores del eje Y
        for (int i = 0; i <= 5; i++)
        {
            double y = baseY - altoArea * i / 5;
            decimal valor = maximoEje * i / 5;
            doc.Linea(x0, y, x0 + anchoArea, y, 0.5, ColorBorde);
            doc.TextoDerecha(x0 - 6, y + 3, valor.ToString("N0", CultureInfo.InvariantCulture), 7, false, ColorGris);
        }

        // Barras
        int grupos = datos.Etiquetas.Count;
        double anchoGrupo = anchoArea / grupos;
        double anchoBarra = Math.Min(45, anchoGrupo * 0.7 / datos.Series.Count);

        for (int g = 0; g < grupos; g++)
        {
            double inicioGrupo = x0 + g * anchoGrupo + (anchoGrupo - anchoBarra * datos.Series.Count) / 2;

            for (int s = 0; s < datos.Series.Count; s++)
            {
                decimal valor = datos.Series[s].Valores[g];
                double altoBarra = altoArea * (double)(valor / maximoEje);
                double x = inicioGrupo + s * anchoBarra;

                doc.Rectangulo(x, baseY - altoBarra, anchoBarra - 2, altoBarra, datos.Series[s].Color);
                doc.TextoCentrado(x + (anchoBarra - 2) / 2, baseY - altoBarra - 3,
                    valor.ToString("N2", CultureInfo.InvariantCulture), 7);
            }

            doc.TextoCentrado(x0 + g * anchoGrupo + anchoGrupo / 2, baseY + 12, datos.Etiquetas[g], 8);
        }

        // Ejes
        doc.Linea(x0, baseY - altoArea, x0, baseY, 1, "#495057");
        doc.Linea(x0, baseY, x0 + anchoArea, baseY, 1, "#495057");

        // Leyenda
        double xLeyenda = x0;
        foreach (SerieGrafica serie in datos.Series)
        {
            doc.Rectangulo(xLeyenda, baseY + 22, 9, 9, serie.Color);
            doc.Texto(xLeyenda + 13, baseY + 30, serie.Nombre, 8);
            xLeyenda += 130;
        }

        doc.Y = baseY + 45;
    }

    private static string Fecha(DateOnly fecha)
    {
        return fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    private static string Monto(decimal monto)
    {
        return monto.ToString("N2", CultureInfo.InvariantCulture);
    }
}