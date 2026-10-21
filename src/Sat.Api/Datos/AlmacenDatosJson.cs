using System.Text.Json;
using System.Text.Json.Serialization;
using Sat.Api.Datos.Registros;
using Sat.Api.Dominio.Entidades;
using Sat.Api.Dominio.Estructuras;

namespace Sat.Api.Datos;

/// <summary>
/// Implementación de IAlmacenDatos con archivos JSON:
/// rtu.json, facturas.json y control.json dentro de una carpeta.
/// Al crearse, carga lo que haya en los archivos; Guardar() los reescribe.
/// </summary>
public class AlmacenDatosJson : IAlmacenDatos
{
    private const string ArchivoRtu = "rtu.json";
    private const string ArchivoFacturas = "facturas.json";
    private const string ArchivoControl = "control.json";

    // Opciones de JSON: con sangría (legible) y enums como texto ("IvaMalCalculado" en vez de 4)
    private static readonly JsonSerializerOptions _opcionesJson = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _carpeta;

    public RegistroTributario Rtu { get; } = new RegistroTributario();
    public LibroFacturas Facturas { get; } = new LibroFacturas();
    public ControlCorrelativo Correlativo { get; private set; } = new ControlCorrelativo();
    public object Candado { get; } = new object();

    public AlmacenDatosJson(string carpeta)
    {
        _carpeta = carpeta;
        Directory.CreateDirectory(_carpeta); // si ya existe, no hace nada
        Cargar();
    }

    // ------------------------------------------------------------------ Guardar
    public void Guardar()
    {
        // 1) RTU: dominio → registros
        List<ContribuyenteRegistro> contribuyentes = new List<ContribuyenteRegistro>();
        foreach (Contribuyente contribuyente in Rtu.ObtenerTodos())
        {
            contribuyentes.Add(new ContribuyenteRegistro
            {
                Nit = contribuyente.Nit,
                Nombre = contribuyente.Nombre
            });
        }
        EscribirArchivo(ArchivoRtu, contribuyentes);

        // 2) Facturas: dominio → registros
        List<FacturaRegistro> facturas = new List<FacturaRegistro>();
        foreach (Factura factura in Facturas.ObtenerTodas())
        {
            facturas.Add(new FacturaRegistro
            {
                Referencia = factura.Referencia,
                FechaEmision = factura.FechaEmision,
                FechaProcesamiento = factura.FechaProcesamiento,
                NitEmisor = factura.NitEmisor,
                NitReceptor = factura.NitReceptor,
                Valor = factura.Valor,
                Iva = factura.Iva,
                Total = factura.Total,
                CodigoAprobacion = factura.CodigoAprobacion,
                Errores = new List<TipoError>(factura.Errores)
            });
        }
        EscribirArchivo(ArchivoFacturas, facturas);

        // 3) Correlativo
        ControlRegistro control = new ControlRegistro
        {
            FechaActual = Correlativo.FechaActual,
            UltimoNumero = Correlativo.UltimoNumero
        };
        EscribirArchivo(ArchivoControl, control);
    }

    // ------------------------------------------------------------------ Inicializar
    public void Inicializar()
    {
        Rtu.Limpiar();
        Facturas.Limpiar();
        Correlativo.Reiniciar();

        BorrarArchivo(ArchivoRtu);
        BorrarArchivo(ArchivoFacturas);
        BorrarArchivo(ArchivoControl);
    }

    // ------------------------------------------------------------------ Cargar (privado)
    private void Cargar()
    {
        // 1) RTU: registros → dominio
        List<ContribuyenteRegistro>? contribuyentes = LeerArchivo<List<ContribuyenteRegistro>>(ArchivoRtu);
        if (contribuyentes != null)
        {
            foreach (ContribuyenteRegistro registro in contribuyentes)
            {
                Rtu.Agregar(new Contribuyente(registro.Nit, registro.Nombre));
            }
        }

        // 2) Facturas: registros → dominio (se reconstruyen con sus propios métodos)
        List<FacturaRegistro>? facturas = LeerArchivo<List<FacturaRegistro>>(ArchivoFacturas);
        if (facturas != null)
        {
            foreach (FacturaRegistro registro in facturas)
            {
                Factura factura = new Factura(registro.Referencia, registro.FechaEmision,
                    registro.FechaProcesamiento, registro.NitEmisor, registro.NitReceptor,
                    registro.Valor, registro.Iva, registro.Total);

                foreach (TipoError error in registro.Errores)
                {
                    factura.AgregarError(error);
                }

                if (registro.CodigoAprobacion.HasValue)
                {
                    factura.Aprobar(registro.CodigoAprobacion.Value);
                }

                Facturas.Agregar(factura);
            }
        }

        // 3) Correlativo
        ControlRegistro? control = LeerArchivo<ControlRegistro>(ArchivoControl);
        if (control != null)
        {
            Correlativo = new ControlCorrelativo(control.FechaActual, control.UltimoNumero);
        }
    }

    // ------------------------------------------------------------------ Utilidades de archivos
    /// <summary>Lee y convierte un archivo JSON al tipo T. Devuelve null si el archivo no existe.</summary>
    private T? LeerArchivo<T>(string nombreArchivo) where T : class
    {
        string ruta = Path.Combine(_carpeta, nombreArchivo);
        if (!File.Exists(ruta))
        {
            return null;
        }

        string json = File.ReadAllText(ruta);
        return JsonSerializer.Deserialize<T>(json, _opcionesJson);
    }

    /// <summary>Convierte el contenido a JSON y lo escribe (reemplazando el archivo).</summary>
    private void EscribirArchivo<T>(string nombreArchivo, T contenido)
    {
        string ruta = Path.Combine(_carpeta, nombreArchivo);
        string json = JsonSerializer.Serialize(contenido, _opcionesJson);
        File.WriteAllText(ruta, json);
    }

    private void BorrarArchivo(string nombreArchivo)
    {
        string ruta = Path.Combine(_carpeta, nombreArchivo);
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }
}