using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Consultas;
using Sat.Web.Servicios;

namespace Sat.Web.Pages.Peticiones;

/// <summary>
/// Petición "Consultar contribuyente": facturas emitidas o recibidas por un NIT en un rango de fechas.
/// Los filtros viajan en la URL (formulario GET).
/// </summary>
public class ContribuyenteModel : PaginaApiModel
{
    // ---------------- Filtros (SupportsGet = true: se llenan también en peticiones GET)
    [BindProperty(SupportsGet = true)]
    public string? Nit { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Tipo { get; set; } = "emitidas";

    [BindProperty(SupportsGet = true)]
    public DateOnly? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? Hasta { get; set; }

    // ---------------- Datos para la vista
    public List<ContribuyenteSalida> Contribuyentes { get; private set; } = new List<ContribuyenteSalida>();
    public ConsultaContribuyenteRespuesta? Resultado { get; private set; }

    public ContribuyenteModel(ClienteApiSat api) : base(api)
    {
    }

    public async Task OnGetAsync()
    {
        // 1) Siempre: cargar los contribuyentes del RTU para la lista desplegable
        bool listaCargada = await IntentarAsync(async () =>
        {
            Contribuyentes = await Api.ListarContribuyentesAsync();
        });

        // 2) Primera visita (URL sin parámetros) o API caída: solo se muestra el formulario
        if (!listaCargada || Request.Query.Count == 0)
        {
            return;
        }

        // 3) Se presionó "Consultar": validar que estén todos los filtros
        if (string.IsNullOrWhiteSpace(Nit) || Desde == null || Hasta == null)
        {
            Error = "Complete todos los filtros: contribuyente, tipo de facturas y rango de fechas.";
            return;
        }

        // 4) Consultar a la API
        await IntentarAsync(async () =>
        {
            Resultado = await Api.ConsultarContribuyenteAsync(Nit, Tipo, Desde.Value, Hasta.Value);
        });
    }
}