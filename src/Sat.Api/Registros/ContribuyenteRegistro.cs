namespace Sat.Api.Datos.Registros;

/// <summary>
/// Forma en que un contribuyente se guarda en rtu.json.
/// Es una clase "de transporte": solo datos, sin reglas.
/// </summary>
public class ContribuyenteRegistro
{
    public string Nit { get; set; } = "";
    public string Nombre { get; set; } = "";
}