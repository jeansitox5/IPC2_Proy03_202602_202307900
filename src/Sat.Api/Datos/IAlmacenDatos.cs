using Sat.Api.Dominio.Estructuras;

namespace Sat.Api.Datos;

/// <summary>
/// Contrato de la capa de datos: da acceso a los TDAs y permite guardarlos o borrarlos.
/// Los servicios dependen de esta INTERFAZ, no de cómo se guardan los datos.
/// </summary>
public interface IAlmacenDatos
{
    RegistroTributario Rtu { get; }
    LibroFacturas Facturas { get; }
    ControlCorrelativo Correlativo { get; }

    /// <summary>Objeto para "lock": evita que dos peticiones modifiquen los datos al mismo tiempo.</summary>
    object Candado { get; }

    /// <summary>Escribe el estado actual en el almacenamiento permanente.</summary>
    void Guardar();

    /// <summary>Borra todos los datos (memoria y almacenamiento) y deja el sistema vacío.</summary>
    void Inicializar();
}