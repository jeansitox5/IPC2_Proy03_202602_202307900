using Microsoft.AspNetCore.Mvc;
using Sat.Api.Datos;
using Sat.Api.Servicios;
using Sat.Compartido.Contratos.Salida;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- Servicios (inyección de dependencias)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Cuando el JSON del cuerpo o un parámetro de la URL no se pueden leer, ASP.NET responde 400.
        // Aquí reemplazamos su respuesta técnica (en inglés) por nuestro formato {"mensaje": "..."}.
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            string mensaje = contexto.HttpContext.Request.Method == "POST"
                ? "El mensaje no tiene el formato esperado: debe ser un arreglo JSON [ ... ] bien formado."
                : "Hay parámetros con formato inválido. Las fechas deben enviarse como yyyy-MM-dd.";

            return new BadRequestObjectResult(new MensajeRespuesta { Mensaje = mensaje });
        };
    });

// Carpeta donde se guardan los archivos JSON (se lee de appsettings.json)
string nombreCarpeta = builder.Configuration["Almacenamiento:Carpeta"] ?? "Almacenamiento";
string carpetaDatos = Path.Combine(builder.Environment.ContentRootPath, nombreCarpeta);

// Singleton = UNA sola instancia para toda la vida de la API (todos comparten los mismos datos)
builder.Services.AddSingleton<IAlmacenDatos>(new AlmacenDatosJson(carpetaDatos));
builder.Services.AddSingleton<ServicioRtu>();
builder.Services.AddSingleton<ServicioAutorizacion>();
builder.Services.AddSingleton<ServicioConsultas>();

// ---------------------------------------------------------------- Aplicación
var app = builder.Build();

app.MapControllers();

app.Run();