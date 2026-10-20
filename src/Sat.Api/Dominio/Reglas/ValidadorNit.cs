namespace Sat.Api.Dominio.Reglas;

/// <summary>
/// Valida NITs guatemaltecos con el algoritmo de módulo 11 del enunciado.
/// Es estática porque no guarda estado: recibe un NIT y responde.
/// </summary>
public static class ValidadorNit
{
    // Un NIT tiene de 1 a 20 dígitos de base + 1 verificador.
    private const int LargoMinimo = 2;
    private const int LargoMaximo = 21;

    /// <summary>Quita espacios y guiones, y pasa a mayúsculas (k → K).</summary>
    public static string Normalizar(string? nit)
    {
        if (nit == null)
        {
            return "";
        }

        return nit.Trim().Replace(" ", "").Replace("-", "").ToUpper();
    }

    /// <summary>Devuelve true si el NIT tiene formato correcto y su verificador coincide.</summary>
    public static bool EsValido(string? nit)
    {
        string limpio = Normalizar(nit);

        // 1) Largo permitido
        if (limpio.Length < LargoMinimo || limpio.Length > LargoMaximo)
        {
            return false;
        }

        // 2) Separar base y verificador
        string baseNit = limpio.Substring(0, limpio.Length - 1);
        char verificador = limpio[limpio.Length - 1];

        // 3) La base solo puede tener dígitos
        for (int i = 0; i < baseNit.Length; i++)
        {
            if (!char.IsAsciiDigit(baseNit[i]))
            {
                return false;
            }
        }

        // 4) Comparar con el verificador calculado
        return CalcularVerificador(baseNit) == verificador;
    }

    /// <summary>Calcula el verificador ('0'..'9' o 'K') de una base formada solo por dígitos.</summary>
    private static char CalcularVerificador(string baseNit)
    {
        int suma = 0;
        int posicion = 2; // la posición 1 es el verificador, que no se multiplica

        // Recorre de derecha a izquierda
        for (int i = baseNit.Length - 1; i >= 0; i--)
        {
            int digito = baseNit[i] - '0';
            suma += digito * posicion;
            posicion++;
        }

        int resultado = (11 - (suma % 11)) % 11;

        return resultado == 10 ? 'K' : (char)('0' + resultado);
    }
}