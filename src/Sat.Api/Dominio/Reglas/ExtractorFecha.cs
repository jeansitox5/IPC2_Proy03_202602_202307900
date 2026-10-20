using System.Text.RegularExpressions;

namespace Sat.Api.Dominio.Reglas;

/// <summary>
/// Extrae la fecha y hora del campo TIEMPO usando una expresión regular.
/// Ignora todo lo demás (lugar, "hrs.", etc.), como pide el enunciado.
/// </summary>
public static class ExtractorFecha
{
    // dd/mm/yyyy  y, opcionalmente, espacios + hh:mi
    // Grupos: 1=día 2=mes 3=año 4=hora 5=minuto
    private static readonly Regex _patron =
        new Regex(@"(\d{1,2})/(\d{1,2})/(\d{4})(?:\s+(\d{1,2}):(\d{2}))?");

    /// <summary>
    /// Intenta extraer la fecha. Devuelve true si lo logró y deja el resultado en "fecha".
    /// </summary>
    public static bool IntentarExtraer(string? texto, out DateTime fecha)
    {
        fecha = DateTime.MinValue; // valor por defecto obligatorio para un parámetro out

        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        Match coincidencia = _patron.Match(texto);
        if (!coincidencia.Success)
        {
            return false;
        }

        int dia = int.Parse(coincidencia.Groups[1].Value);
        int mes = int.Parse(coincidencia.Groups[2].Value);
        int anio = int.Parse(coincidencia.Groups[3].Value);

        int hora = 0;
        int minuto = 0;
        if (coincidencia.Groups[4].Success) // la hora es opcional
        {
            hora = int.Parse(coincidencia.Groups[4].Value);
            minuto = int.Parse(coincidencia.Groups[5].Value);
        }

        // Validar antes de construir el DateTime (si no, lanzaría una excepción)
        if (anio < 1 || mes < 1 || mes > 12)
        {
            return false;
        }
        if (dia < 1 || dia > DateTime.DaysInMonth(anio, mes))
        {
            return false;
        }
        if (hora > 23 || minuto > 59)
        {
            return false;
        }

        fecha = new DateTime(anio, mes, dia, hora, minuto, 0);
        return true;
    }
}