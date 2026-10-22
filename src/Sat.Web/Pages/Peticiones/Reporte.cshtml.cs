using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Reporte de autorizaciones del enunciado: por cada fecha, facturas recibidas, errores,
/// facturas sin errores y emisores/receptores distintos.
/// </summary>
public class ReporteModel : PaginaApiModel
{
    [BindProperty(SupportsGet = true)]
    public DateOnly? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? Hasta { get; set; }

    public ReporteRespuesta? Resultado { get; private set; }

    public ReporteModel(ClienteApiSat api) : base(api)
    {
    }

    public async Task OnGetAsync()
    {
        if (Request.Query.Count == 0)
        {
            return;
        }

        if (Desde == null || Hasta == null)
        {
            Error = "Seleccione el rango de fechas.";
            return;
        }

        await IntentarAsync(async () =>
        {
            Resultado = await Api.ReporteAsync(Desde.Value, Hasta.Value);
        });
    }
}
