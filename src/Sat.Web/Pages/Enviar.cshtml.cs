using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sat.Compartido.Contratos.Salida;
using Sat.Web.Servicios;

namespace Sat.Web.Pages;

/// <summary>
/// Pantalla "Enviar mensaje": sube un archivo .json y lo envía SIN MODIFICARLO a la API,
/// ya sea como actualización del RTU o como solicitudes de autorización.
/// </summary>
public class EnviarModel : PaginaApiModel
{
    public const string TipoRtu = "rtu";
    public const string TipoAutorizaciones = "autorizaciones";

    // Opciones para mostrar el JSON de respuesta con sangría
    private static readonly JsonSerializerOptions _opcionesFormato = new JsonSerializerOptions { WriteIndented = true };

    // ---------------- Datos que vienen del formulario ([BindProperty] los llena al hacer POST)
    [BindProperty]
    public IFormFile? Archivo { get; set; }

    [BindProperty]
    public string TipoMensaje { get; set; } = TipoRtu;

    // ---------------- Datos que la página muestra después de enviar
    public string? NombreArchivo { get; private set; }
    public string? ContenidoEnviado { get; private set; }
    public string? RespuestaJson { get; private set; }
    public RespuestaRtu? RespuestaRtu { get; private set; }
    public RespuestaAutorizaciones? RespuestaAutorizaciones { get; private set; }

    public EnviarModel(ClienteApiSat api) : base(api)
    {
    }

    /// <summary>GET: solo muestra el formulario vacío.</summary>
    public void OnGet()
    {
    }

    /// <summary>POST: se ejecuta al presionar "Enviar a la API".</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        // 1) Validar el archivo
        if (Archivo == null || Archivo.Length == 0)
        {
            Error = "Seleccione un archivo .json antes de enviar.";
            return Page();
        }

        if (!Path.GetExtension(Archivo.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            Error = "El archivo debe tener extensión .json.";
            return Page();
        }

        // 2) Leer el contenido completo del archivo como texto
        NombreArchivo = Archivo.FileName;
        using (StreamReader lector = new StreamReader(Archivo.OpenReadStream()))
        {
            ContenidoEnviado = await lector.ReadToEndAsync();
        }

        // 3) Enviarlo a la API según el tipo elegido
        await IntentarAsync(async () =>
        {
            if (TipoMensaje == TipoAutorizaciones)
            {
                RespuestaAutorizaciones = await Api.EnviarAutorizacionesAsync(ContenidoEnviado);
                RespuestaJson = JsonSerializer.Serialize(RespuestaAutorizaciones, _opcionesFormato);
            }
            else
            {
                RespuestaRtu = await Api.EnviarRtuAsync(ContenidoEnviado);
                RespuestaJson = JsonSerializer.Serialize(RespuestaRtu, _opcionesFormato);
            }
        });

        return Page();
    }
}