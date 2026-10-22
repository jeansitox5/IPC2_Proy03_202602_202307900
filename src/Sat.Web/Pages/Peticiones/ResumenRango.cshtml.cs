using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Petición "Resumen por rango de fechas": valor autorizado por fecha, total o sin IVA,
/// con gráfica de barras y descarga en PDF.
/// </summary>
public class ResumenRangoModel : PaginaApiModel
{
    private readonly GeneradorGraficas _graficas;
    private readonly GeneradorPdf _pdf;

    [BindProperty(SupportsGet = true)]
    public DateOnly? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? Hasta { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Tipo { get; set; } = "total";

    public ResumenRangoRespuesta? Resultado { get; private set; }
    public string? GraficaSvg { get; private set; }

    /// <summary>Texto para los encabezados según el tipo elegido.</summary>
    public string DescripcionTipo => Tipo == "siniva" ? "Valor sin IVA" : "Valor total (con IVA)";

    public ResumenRangoModel(ClienteApiSat api, GeneradorGraficas graficas, GeneradorPdf pdf) : base(api)
    {
        _graficas = graficas;
        _pdf = pdf;
    }

    public async Task OnGetAsync()
    {
        await CargarAsync();

        if (Resultado != null && Resultado.Fechas.Count > 0)
        {
            GraficaSvg = _graficas.Barras(DatosGrafica.DeResumenRango(Resultado, DescripcionTipo));
        }
    }

    public async Task<IActionResult> OnGetPdfAsync()
    {
        await CargarAsync();
        if (Resultado == null)
        {
            return Page();
        }

        byte[] archivo = _pdf.ResumenRango(Resultado, DescripcionTipo);
        return File(archivo, "application/pdf", $"ResumenRango_{Resultado.Desde:yyyyMMdd}_{Resultado.Hasta:yyyyMMdd}.pdf");
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
            Resultado = await Api.ResumenRangoAsync(Desde.Value, Hasta.Value, Tipo);
        });
    }
}