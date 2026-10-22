using System.Globalization;
using System.Text;

namespace Sat.Web.Servicios;

/// <summary>
/// Escritor de PDF propio y mínimo (sin bibliotecas externas): páginas tamaño carta con texto
/// (Helvetica normal y negrita), rectángulos de color y líneas. Usa un "cursor" vertical (Y)
/// que avanza de arriba hacia abajo y crea páginas nuevas cuando se acaba el espacio.
/// </summary>
public class DocumentoPdf
{
    public const double AnchoPagina = 612;   // 8.5 pulgadas × 72 puntos
    public const double AltoPagina = 792;    // 11 pulgadas × 72 puntos
    public const double Margen = 50;

    private readonly List<StringBuilder> _paginas = new List<StringBuilder>();

    /// <summary>Posición vertical actual, medida desde el borde SUPERIOR de la página.</summary>
    public double Y { get; set; }

    /// <summary>Ancho útil entre márgenes.</summary>
    public double AnchoUtil => AnchoPagina - 2 * Margen;

    public DocumentoPdf()
    {
        NuevaPagina();
    }

    public void NuevaPagina()
    {
        _paginas.Add(new StringBuilder());
        Y = Margen;
    }

    /// <summary>
    /// Si no caben "alto" puntos más en la página actual, empieza una nueva.
    /// Devuelve true si tuvo que crear una página nueva.
    /// </summary>
    public bool AsegurarEspacio(double alto)
    {
        if (Y + alto > AltoPagina - Margen)
        {
            NuevaPagina();
            return true;
        }

        return false;
    }

    // ================================================================== Dibujo
    /// <summary>Escribe texto con su línea base en (x, y), medidos desde la esquina superior izquierda.</summary>
    public void Texto(double x, double y, string texto, double tamano, bool negrita = false, string color = "#212529")
    {
        string fuente = negrita ? "F2" : "F1";
        Actual().Append($"{ColorRelleno(color)} BT /{fuente} {N(tamano)} Tf {N(x)} {N(AltoPagina - y)} Td ({Escapar(texto)}) Tj ET\n");
    }

    /// <summary>Escribe texto alineado a la derecha: termina exactamente en xDerecha.</summary>
    public void TextoDerecha(double xDerecha, double y, string texto, double tamano, bool negrita = false, string color = "#212529")
    {
        Texto(xDerecha - AnchoTexto(texto, tamano), y, texto, tamano, negrita, color);
    }

    /// <summary>Escribe texto centrado en xCentro.</summary>
    public void TextoCentrado(double xCentro, double y, string texto, double tamano, bool negrita = false, string color = "#212529")
    {
        Texto(xCentro - AnchoTexto(texto, tamano) / 2, y, texto, tamano, negrita, color);
    }

    /// <summary>Rectángulo relleno; (x, y) es su esquina SUPERIOR izquierda.</summary>
    public void Rectangulo(double x, double y, double ancho, double alto, string color)
    {
        Actual().Append($"{ColorRelleno(color)} {N(x)} {N(AltoPagina - y - alto)} {N(ancho)} {N(alto)} re f\n");
    }

    public void Linea(double x1, double y1, double x2, double y2, double grosor, string color)
    {
        Actual().Append($"{ColorTrazo(color)} {N(grosor)} w {N(x1)} {N(AltoPagina - y1)} m {N(x2)} {N(AltoPagina - y2)} l S\n");
    }

    /// <summary>Ancho aproximado de un texto en Helvetica (los dígitos miden exactamente 0.556 × tamaño).</summary>
    public static double AnchoTexto(string texto, double tamano)
    {
        double unidades = 0;
        foreach (char c in texto)
        {
            if (char.IsAsciiDigit(c)) unidades += 0.556;
            else if (c == '.' || c == ',' || c == ' ' || c == ':' || c == '/') unidades += 0.278;
            else if (char.IsUpper(c)) unidades += 0.667;
            else unidades += 0.5;
        }

        return unidades * tamano;
    }

    // ================================================================== Generación del archivo
    /// <summary>Arma el archivo PDF completo (con "Página i de n" al pie) y lo devuelve como bytes.</summary>
    public byte[] Generar()
    {
        // Pie de página en todas las páginas
        for (int i = 0; i < _paginas.Count; i++)
        {
            string pie = $"Página {i + 1} de {_paginas.Count}";
            _paginas[i].Append($"{ColorRelleno("#6c757d")} BT /F1 8 Tf {N(AnchoPagina - Margen - AnchoTexto(pie, 8))} {N(25)} Td ({Escapar(pie)}) Tj ET\n");
        }

        // Objetos del PDF: 1 catálogo, 2 árbol de páginas, 3 y 4 fuentes, luego (página, contenido) por cada página
        List<string> objetos = new List<string>();
        StringBuilder hijos = new StringBuilder();
        for (int i = 0; i < _paginas.Count; i++)
        {
            hijos.Append($"{5 + i * 2} 0 R ");
        }

        objetos.Add("<< /Type /Catalog /Pages 2 0 R >>");
        objetos.Add($"<< /Type /Pages /Kids [ {hijos}] /Count {_paginas.Count} >>");
        objetos.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        objetos.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

        for (int i = 0; i < _paginas.Count; i++)
        {
            int numeroContenido = 6 + i * 2;
            string contenido = _paginas[i].ToString();
            objetos.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(AnchoPagina)} {N(AltoPagina)}] " +
                        $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {numeroContenido} 0 R >>");
            objetos.Add($"<< /Length {Encoding.Latin1.GetByteCount(contenido)} >>\nstream\n{contenido}endstream");
        }

        // Escritura con la tabla de posiciones (xref) que el PDF exige
        using MemoryStream salida = new MemoryStream();
        List<long> posiciones = new List<long>();
        Escribir(salida, "%PDF-1.4\n");

        for (int i = 0; i < objetos.Count; i++)
        {
            posiciones.Add(salida.Position);
            Escribir(salida, $"{i + 1} 0 obj\n{objetos[i]}\nendobj\n");
        }

        long inicioXref = salida.Position;
        Escribir(salida, $"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n");
        foreach (long posicion in posiciones)
        {
            Escribir(salida, $"{posicion:D10} 00000 n \n");
        }
        Escribir(salida, $"trailer\n<< /Size {objetos.Count + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF");

        return salida.ToArray();
    }

    // ================================================================== Auxiliares privados
    private StringBuilder Actual()
    {
        return _paginas[_paginas.Count - 1];
    }

    private static void Escribir(MemoryStream salida, string texto)
    {
        byte[] bytes = Encoding.Latin1.GetBytes(texto);
        salida.Write(bytes, 0, bytes.Length);
    }

    /// <summary>Escapa \ ( ) y reemplaza caracteres que la fuente estándar no puede mostrar.</summary>
    private static string Escapar(string texto)
    {
        StringBuilder resultado = new StringBuilder();
        foreach (char c in texto)
        {
            if (c == '\\' || c == '(' || c == ')')
            {
                resultado.Append('\\').Append(c);
            }
            else if (c > 255)
            {
                resultado.Append('-');   // ej. "—" no existe en la fuente estándar
            }
            else
            {
                resultado.Append(c);
            }
        }

        return resultado.ToString();
    }

    private static string ColorRelleno(string hex)
    {
        return $"{Componentes(hex)} rg";
    }

    private static string ColorTrazo(string hex)
    {
        return $"{Componentes(hex)} RG";
    }

    /// <summary>Convierte "#RRGGBB" a "r g b" con valores de 0 a 1.</summary>
    private static string Componentes(string hex)
    {
        int r = Convert.ToInt32(hex.Substring(1, 2), 16);
        int g = Convert.ToInt32(hex.Substring(3, 2), 16);
        int b = Convert.ToInt32(hex.Substring(5, 2), 16);
        return $"{N(r / 255.0)} {N(g / 255.0)} {N(b / 255.0)}";
    }

    private static string N(double valor)
    {
        return valor.ToString("0.###", CultureInfo.InvariantCulture);
    }
}