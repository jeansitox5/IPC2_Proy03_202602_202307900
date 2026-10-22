using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages;

/// <summary>
/// Página de inicio: presenta el sistema y verifica si la API está disponible.
/// </summary>
public class IndexModel : PaginaApiModel
{
    public bool ApiConectada { get; private set; }
    public int CantidadContribuyentes { get; private set; }

    public IndexModel(ClienteApiSat api) : base(api)
    {
    }

    public async Task OnGetAsync()
    {
        ApiConectada = await IntentarAsync(async () =>
        {
            List<ContribuyenteSalida> contribuyentes = await Api.ListarContribuyentesAsync();
            CantidadContribuyentes = contribuyentes.Count;
        });
    }
}