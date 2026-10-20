namespace Sat.Api.Dominio.Reglas;

/// <summary>
/// Reglas de cálculo del IVA y del TOTAL. Usa decimal (nunca double) para dinero.
/// </summary>
public static class ReglasFiscales
{
    public const decimal TasaIva = 0.12m;

    /// <summary>IVA = REDONDEAR(VALOR × 0.12, 2), redondeo "escolar" (0.5 sube).</summary>
    public static decimal CalcularIva(decimal valor)
    {
        return Math.Round(valor * TasaIva, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>true si el IVA recibido es igual al calculado.</summary>
    public static bool IvaEsCorrecto(decimal valor, decimal iva)
    {
        return iva == CalcularIva(valor);
    }

    /// <summary>true si TOTAL = VALOR + IVA (con el IVA que trae la factura).</summary>
    public static bool TotalEsCorrecto(decimal valor, decimal iva, decimal total)
    {
        return total == valor + iva;
    }

    /// <summary>true si el VALOR es positivo y tiene como máximo 2 decimales.</summary>
    public static bool ValorEsValido(decimal valor)
    {
        if (valor <= 0)
        {
            return false;
        }

        decimal centavos = valor * 100;
        return centavos == Math.Truncate(centavos); // si quedan decimales, había más de 2
    }
}