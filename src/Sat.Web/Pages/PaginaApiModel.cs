using Microsoft.AspNetCore.Mvc.RazorPages;
using Sat.Web.Servicios;

namespace Sat.Web.Pages;

/// <summary>
/// Clase base ABSTRACTA para todas las páginas que llaman a la API.
/// Centraliza el manejo de errores para no repetir el mismo try/catch en cada página.
/// </summary>
public abstract class PaginaApiModel : PageModel
{
    protected readonly ClienteApiSat Api;

    /// <summary>Mensaje de error para mostrar en la página (null si todo salió bien).</summary>
    public string? Error { get; protected set; }

    protected PaginaApiModel(ClienteApiSat api)
    {
        Api = api;
    }

    /// <summary>
    /// Ejecuta una llamada a la API. Si falla, guarda un mensaje claro en Error y devuelve false.
    /// </summary>
    protected async Task<bool> IntentarAsync(Func<Task> llamada)
    {
        try
        {
            await llamada();
            return true;
        }
        catch (ErrorApiException ex)
        {
            Error = ex.Message;
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con la API. Verifique que Sat.Api esté corriendo en http://localhost:5100.";
        }
        catch (TaskCanceledException)
        {
            Error = "La API tardó demasiado en responder.";
        }

        return false;
    }
}