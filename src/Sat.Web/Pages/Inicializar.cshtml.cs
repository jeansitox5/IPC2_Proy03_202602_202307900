using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Salida;
using Sat.Web.Servicios;

namespace Sat.Web.Pages;

/// <summary>
/// Pantalla "Inicializar": envía a la API la orden de borrar todos los datos.
/// </summary>
public class InicializarModel : PaginaApiModel
{
    public string? Mensaje { get; private set; }

    public InicializarModel(ClienteApiSat api) : base(api)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await IntentarAsync(async () =>
        {
            MensajeRespuesta respuesta = await Api.InicializarAsync();
            Mensaje = respuesta.Mensaje;
        });

        return Page();
    }
}