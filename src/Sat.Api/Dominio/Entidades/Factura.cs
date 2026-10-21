namespace Sat.Api.Dominio.Entidades;

/// <summary>
/// Una factura recibida por el servicio. Guarda sus datos, sus errores y,
/// si fue aprobada, su código de autorización.
/// Regla principal: solo se aprueba si NO tiene errores.
/// </summary>
public class Factura
{
    // Campo privado: la lista real de errores. Nadie de afuera puede modificarla directamente.
    private readonly List<TipoError> _errores = new List<TipoError>();

    // Datos recibidos (solo lectura)
    public string Referencia { get; }
    public DateTime? FechaEmision { get; }          // null si TIEMPO no traía una fecha válida
    public DateTime FechaProcesamiento { get; }     // momento en que el servicio la procesó
    public string NitEmisor { get; }
    public string NitReceptor { get; }
    public decimal Valor { get; }
    public decimal Iva { get; }
    public decimal Total { get; }

    // Estado que cambia durante el proceso
    public long? CodigoAprobacion { get; private set; } // null mientras no esté aprobada

    // Propiedades calculadas
    public IReadOnlyList<TipoError> Errores => _errores;          // vista de solo lectura
    public bool EstaAprobada => CodigoAprobacion.HasValue;

    public Factura(string referencia, DateTime? fechaEmision, DateTime fechaProcesamiento,
                   string nitEmisor, string nitReceptor, decimal valor, decimal iva, decimal total)
    {
        Referencia = referencia;
        FechaEmision = fechaEmision;
        FechaProcesamiento = fechaProcesamiento;
        NitEmisor = nitEmisor;
        NitReceptor = nitReceptor;
        Valor = valor;
        Iva = iva;
        Total = total;
    }

    /// <summary>Registra un error (sin repetirlo). No se permite en una factura ya aprobada.</summary>
    public void AgregarError(TipoError tipo)
    {
        if (EstaAprobada)
        {
            throw new InvalidOperationException("No se puede agregar errores a una factura aprobada.");
        }

        if (!_errores.Contains(tipo))
        {
            _errores.Add(tipo);
        }
    }

    public bool TieneErrores()
    {
        return _errores.Count > 0;
    }

    public bool TieneError(TipoError tipo)
    {
        return _errores.Contains(tipo);
    }

    /// <summary>Asigna el código de autorización. Solo si no tiene errores y no estaba aprobada.</summary>
    public void Aprobar(long codigo)
    {
        if (TieneErrores())
        {
            throw new InvalidOperationException("Una factura con errores no puede aprobarse.");
        }

        if (EstaAprobada)
        {
            throw new InvalidOperationException("La factura ya estaba aprobada.");
        }

        CodigoAprobacion = codigo;
    }
}