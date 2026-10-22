using Sat.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- Servicios
builder.Services.AddRazorPages();

// Cliente HTTP tipado: cada vez que una página pida un ClienteApiSat,
// recibirá uno con su HttpClient apuntando a la URL de la API (appsettings.json)
string urlApi = builder.Configuration["ApiSat:UrlBase"] ?? "http://localhost:5100/";
builder.Services.AddHttpClient<ClienteApiSat>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi);
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

// Generadores de gráficas (SVG) y reportes (PDF): no guardan estado, una sola instancia basta
builder.Services.AddSingleton<GeneradorGraficas>();
builder.Services.AddSingleton<GeneradorPdf>();

// ---------------------------------------------------------------- Aplicación
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();