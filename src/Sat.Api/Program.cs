// 1) Crea el "constructor" de la aplicación web: lee configuración (appsettings.json, puertos, etc.).
var builder = WebApplication.CreateBuilder(args);

// 2) Registra servicios. AddControllers() activa el soporte para clases Controller,
//    que son las que atenderán las rutas como /api/rtu o /api/procesar.
builder.Services.AddControllers();

// 3) Construye la aplicación con todo lo registrado arriba.
var app = builder.Build();

// 4) Conecta cada ruta HTTP con el método del Controller que la atiende.
app.MapControllers();

// 5) Arranca el servidor y se queda escuchando peticiones hasta que presiones Ctrl + C.
app.Run();