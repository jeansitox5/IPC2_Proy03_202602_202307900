using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Petición "Resumen de IVA por fecha y NIT": IVA emitido y recibido por cada NIT en una fecha,
/// con gráfica de barras agrupadas y descarga en PDF.
/// </summary>
public class ResumenIvaModel : PaginaApiModel
{
    private readonly GeneradorGraficas _graficas;
    private readonly GeneradorPdf _pdf;

    [BindProperty(SupportsGet = true)]
    public DateOnly? Fecha { get; set; }

    public ResumenIvaRespuesta? Resultado { get; private set; }
    public string? GraficaSvg { get; private set; }

    public ResumenIvaModel(ClienteApiSat api, GeneradorGraficas graficas, GeneradorPdf pdf) : base(api)
    {
        _graficas = graficas;
        _pdf = pdf;
    }

    public async Task OnGetAsync()
    {
        await CargarAsync();

        if (Resultado != null && Resultado.Nits.Count > 0)
        {
            GraficaSvg = _graficas.Barras(DatosGrafica.DeResumenIva(Resultado));
        }
    }

    public async Task<IActionResult> OnGetPdfAsync()
    {
        await CargarAsync();
        if (Resultado == null)
        {
            return Page();
        }

        byte[] archivo = _pdf.ResumenIva(Resultado);
        return File(archivo, "application/pdf", $"ResumenIva_{Resultado.Fecha:yyyyMMdd}.pdf");
    }

    private async Task CargarAsync()
    {
        if (Request.Query.Count == 0)
        {
            return; // primera visita: solo el formulario
        }

        if (Fecha == null)
        {
            Error = "Seleccione una fecha.";
            return;
        }

        await IntentarAsync(async () =>
        {
            Resultado = await Api.ResumenIvaAsync(Fecha.Value);
        });
    }
}