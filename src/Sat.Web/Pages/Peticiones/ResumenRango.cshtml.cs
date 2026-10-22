using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Petición "Resumen por rango de fechas": valor autorizado por fecha, total o sin IVA.
/// </summary>
public class ResumenRangoModel : PaginaApiModel
{
    [BindProperty(SupportsGet = true)]
    public DateOnly? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? Hasta { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Tipo { get; set; } = "total";

    public ResumenRangoRespuesta? Resultado { get; private set; }

    /// <summary>Texto para los encabezados según el tipo elegido.</summary>
    public string DescripcionTipo => Tipo == "siniva" ? "Valor sin IVA" : "Valor total (con IVA)";

    public ResumenRangoModel(ClienteApiSat api) : base(api)
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
            Resultado = await Api.ResumenRangoAsync(Desde.Value, Hasta.Value, Tipo);
        });
    }
}