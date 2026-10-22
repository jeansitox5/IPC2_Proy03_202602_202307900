using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Reporte de autorizaciones del enunciado: por cada fecha, facturas recibidas, errores,
/// facturas sin errores y emisores/receptores distintos. Permite descargarlo en PDF.
/// </summary>
public class ReporteModel : PaginaApiModel
{
    private readonly GeneradorPdf _pdf;

    [BindProperty(SupportsGet = true)]
    public DateOnly? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? Hasta { get; set; }

    public ReporteRespuesta? Resultado { get; private set; }

    public ReporteModel(ClienteApiSat api, GeneradorPdf pdf) : base(api)
    {
        _pdf = pdf;
    }

    public async Task OnGetAsync()
    {
        await CargarAsync();
    }

    public async Task<IActionResult> OnGetPdfAsync()
    {
        await CargarAsync();
        if (Resultado == null)
        {
            return Page();
        }

        byte[] archivo = _pdf.Reporte(Resultado);
        return File(archivo, "application/pdf", $"Reporte_{Resultado.Desde:yyyyMMdd}_{Resultado.Hasta:yyyyMMdd}.pdf");
    }

    private async Task CargarAsync()
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
