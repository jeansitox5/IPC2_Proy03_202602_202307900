using Sat.Api.Dominio.Entidades;

namespace Sat.Api.Datos.Registros;

/// <summary>
/// Forma en que una factura se guarda en facturas.json.
/// </summary>
public class FacturaRegistro
{
    public string Referencia { get; set; } = "";
    public DateTime? FechaEmision { get; set; }
    public DateTime FechaProcesamiento { get; set; }
    public string NitEmisor { get; set; } = "";
    public string NitReceptor { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public long? CodigoAprobacion { get; set; }
    public List<TipoError> Errores { get; set; } = new List<TipoError>();
}