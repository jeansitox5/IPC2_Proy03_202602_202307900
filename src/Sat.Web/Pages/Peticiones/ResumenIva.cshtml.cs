using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Petición "Resumen de IVA por fecha y NIT": IVA emitido y recibido por cada NIT en una fecha.
/// </summary>
public class ResumenIvaModel : PaginaApiModel
{
    [BindProperty(SupportsGet = true)]
    public DateOnly? Fecha { get; set; }

    public ResumenIvaRespuesta? Resultado { get; private set; }

    public ResumenIvaModel(ClienteApiSat api) : base(api)
    {
    }

    public async Task OnGetAsync()
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