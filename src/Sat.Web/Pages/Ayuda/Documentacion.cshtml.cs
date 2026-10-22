using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web.Pages.Ayuda;

/// <summary>
/// Página de documentación: resume el programa y enlaza el ensayo en PDF si ya existe en wwwroot/docs.
/// </summary>
public class DocumentacionModel : PageModel
{
    public const string RutaEnsayo = "/docs/Ensayo_Proyecto3_202307900.pdf";

    private readonly IWebHostEnvironment _entorno;

    public bool ExisteEnsayo { get; private set; }

    // IWebHostEnvironment lo entrega ASP.NET: sabe dónde está la carpeta wwwroot
    public DocumentacionModel(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public void OnGet()
    {
        string rutaFisica = Path.Combine(_entorno.WebRootPath, "docs", "Ensayo_Proyecto3_202307900.pdf");
        ExisteEnsayo = System.IO.File.Exists(rutaFisica);
    }
}