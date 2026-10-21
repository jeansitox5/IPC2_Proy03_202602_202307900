using Sat.Api.Dominio.Entidades;

namespace Sat.Api.Dominio.Estructuras;

/// <summary>
/// TDA "Libro de facturas": guarda TODAS las facturas recibidas (aprobadas y rechazadas)
/// y un índice de referencias aprobadas para detectar duplicados rápidamente.
/// </summary>
public class LibroFacturas
{
    private readonly List<Factura> _facturas = new List<Factura>();
    private readonly HashSet<string> _referenciasAprobadas = new HashSet<string>();

    public int Cantidad => _facturas.Count;

    /// <summary>Agrega una factura ya procesada (aprobada o rechazada).</summary>
    public void Agregar(Factura factura)
    {
        _facturas.Add(factura);

        if (factura.EstaAprobada)
        {
            _referenciasAprobadas.Add(factura.Referencia);
        }
    }

    /// <summary>true si ya existe una factura APROBADA con esa referencia (decisión D4).</summary>
    public bool ReferenciaYaAprobada(string referencia)
    {
        return _referenciasAprobadas.Contains(referencia);
    }

    /// <summary>
    /// Facturas cuya fecha de emisión está entre "desde" y "hasta" (ambas inclusive).
    /// Las facturas sin fecha válida no entran en ningún rango.
    /// </summary>
    public List<Factura> ObtenerPorRango(DateOnly desde, DateOnly hasta)
    {
        List<Factura> resultado = new List<Factura>();

        foreach (Factura factura in _facturas)
        {
            if (factura.FechaEmision == null)
            {
                continue; // salta a la siguiente factura
            }

            DateOnly fecha = DateOnly.FromDateTime(factura.FechaEmision.Value);
            if (fecha >= desde && fecha <= hasta)
            {
                resultado.Add(factura);
            }
        }

        return resultado;
    }

    /// <summary>Copia de todas las facturas (para guardarlas en archivo).</summary>
    public List<Factura> ObtenerTodas()
    {
        return new List<Factura>(_facturas);
    }

    public void Limpiar()
    {
        _facturas.Clear();
        _referenciasAprobadas.Clear();
    }
}