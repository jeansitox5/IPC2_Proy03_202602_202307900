using Sat.Api.Datos;
using Sat.Api.Servicios;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- Servicios (inyección de dependencias)
builder.Services.AddControllers();

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