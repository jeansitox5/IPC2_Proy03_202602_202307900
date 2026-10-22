namespace Sat.Web.Servicios;

/// <summary>
/// Excepción propia: la API respondió, pero con un código de error (400, 404, 500...).
/// Hereda de Exception y agrega el código HTTP recibido.
/// </summary>
public class ErrorApiException : Exception
{
    public int CodigoEstado { get; }

    public ErrorApiException(string mensaje, int codigoEstado) : base(mensaje)
    {
        CodigoEstado = codigoEstado;
    }
}