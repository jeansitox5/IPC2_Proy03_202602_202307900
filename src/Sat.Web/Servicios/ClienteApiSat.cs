using System.Globalization;
using System.Text;
using System.Text.Json;
using Sat.Compartido.Contratos.Consultas;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Web.Servicios;

/// <summary>
/// Único punto del frontend que habla con la API (Servicio 2) por HTTP.
/// Las páginas nunca arman URLs ni leen JSON por su cuenta: le piden todo a esta clase.
/// </summary>
public class ClienteApiSat
{
    private readonly HttpClient _http;

    // El HttpClient llega ya configurado (con la URL base de la API) desde Program.cs
    public ClienteApiSat(HttpClient http)
    {
        _http = http;
    }

    // ================================================================== Envío de mensajes (POST)
    /// <summary>Envía el contenido de un archivo de RTU tal cual, sin modificarlo.</summary>
    public async Task<RespuestaRtu> EnviarRtuAsync(string json)
    {
        return await EnviarJsonAsync<RespuestaRtu>("api/rtu", json);
    }

    /// <summary>Envía el contenido de un archivo de autorizaciones tal cual, sin modificarlo.</summary>
    public async Task<RespuestaAutorizaciones> EnviarAutorizacionesAsync(string json)
    {
        return await EnviarJsonAsync<RespuestaAutorizaciones>("api/procesar", json);
    }

    /// <summary>Deja la API sin datos.</summary>
    public async Task<MensajeRespuesta> InicializarAsync()
    {
        HttpResponseMessage respuesta = await _http.PostAsync("api/inicializar", null);
        return await LeerRespuestaAsync<MensajeRespuesta>(respuesta);
    }

    // ================================================================== Consultas (GET)
    public async Task<List<ContribuyenteSalida>> ListarContribuyentesAsync()
    {
        return await ObtenerAsync<List<ContribuyenteSalida>>("api/rtu");
    }

    public async Task<ConsultaContribuyenteRespuesta> ConsultarContribuyenteAsync(
        string nit, string tipo, DateOnly desde, DateOnly hasta)
    {
        string ruta = $"api/consultadatos?nit={Uri.EscapeDataString(nit)}&tipo={tipo}"
                    + $"&desde={FormatoFecha(desde)}&hasta={FormatoFecha(hasta)}";
        return await ObtenerAsync<ConsultaContribuyenteRespuesta>(ruta);
    }

    public async Task<ResumenIvaRespuesta> ResumenIvaAsync(DateOnly fecha)
    {
        return await ObtenerAsync<ResumenIvaRespuesta>($"api/resumeniva?fecha={FormatoFecha(fecha)}");
    }

    public async Task<ResumenRangoRespuesta> ResumenRangoAsync(DateOnly desde, DateOnly hasta, string tipo)
    {
        string ruta = $"api/resumenrango?desde={FormatoFecha(desde)}&hasta={FormatoFecha(hasta)}&tipo={tipo}";
        return await ObtenerAsync<ResumenRangoRespuesta>(ruta);
    }

    public async Task<ReporteRespuesta> ReporteAsync(DateOnly desde, DateOnly hasta)
    {
        return await ObtenerAsync<ReporteRespuesta>($"api/reporte?desde={FormatoFecha(desde)}&hasta={FormatoFecha(hasta)}");
    }

    // ================================================================== Métodos auxiliares (privados)
    /// <summary>POST con un cuerpo JSON (texto) y lectura de la respuesta como T.</summary>
    private async Task<T> EnviarJsonAsync<T>(string ruta, string json)
    {
        using StringContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage respuesta = await _http.PostAsync(ruta, contenido);
        return await LeerRespuestaAsync<T>(respuesta);
    }

    /// <summary>GET y lectura de la respuesta como T.</summary>
    private async Task<T> ObtenerAsync<T>(string ruta)
    {
        HttpResponseMessage respuesta = await _http.GetAsync(ruta);
        return await LeerRespuestaAsync<T>(respuesta);
    }

    /// <summary>
    /// Si la API respondió con éxito (2xx), convierte el JSON en T.
    /// Si respondió con error, lanza ErrorApiException con el mensaje más claro posible.
    /// </summary>
    private static async Task<T> LeerRespuestaAsync<T>(HttpResponseMessage respuesta)
    {
        string cuerpo = await respuesta.Content.ReadAsStringAsync();
        int codigo = (int)respuesta.StatusCode;

        if (!respuesta.IsSuccessStatusCode)
        {
            throw new ErrorApiException(ExtraerMensajeDeError(cuerpo, codigo), codigo);
        }

        T? resultado = JsonSerializer.Deserialize<T>(cuerpo);
        if (resultado == null)
        {
            throw new ErrorApiException("La API devolvió una respuesta vacía.", codigo);
        }

        return resultado;
    }

    /// <summary>Intenta leer {"mensaje": "..."}; si no existe, arma un mensaje según el código.</summary>
    private static string ExtraerMensajeDeError(string cuerpo, int codigo)
    {
        try
        {
            MensajeRespuesta? error = JsonSerializer.Deserialize<MensajeRespuesta>(cuerpo);
            if (error != null && error.Mensaje != "")
            {
                return error.Mensaje;
            }
        }
        catch (JsonException)
        {
            // El cuerpo no era JSON: se usa el mensaje genérico de abajo
        }

        if (codigo == 400)
        {
            return "La API rechazó la petición (400): el contenido no tiene el formato JSON esperado.";
        }

        return $"La API respondió con el código de error {codigo}.";
    }

    /// <summary>Fecha en el formato que espera la API: yyyy-MM-dd.</summary>
    private static string FormatoFecha(DateOnly fecha)
    {
        return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}