using Sat.Api.Datos;
using Sat.Api.Dominio.Entidades;
using Sat.Api.Dominio.Reglas;
using Sat.Compartido.Contratos.Consultas;
using Sat.Compartido.Contratos.Entrada;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Api.Servicios;

/// <summary>
/// Caso de uso "Actualizar el RTU":
///  - NIT válido y nuevo      → se crea el contribuyente
///  - NIT válido y existente  → se actualiza su nombre
///  - NIT inválido            → no afecta el RTU, solo se cuenta
/// </summary>
public class ServicioRtu
{
    private readonly IAlmacenDatos _almacen;

    // El almacén llega por el constructor (inyección de dependencias)
    public ServicioRtu(IAlmacenDatos almacen)
    {
        _almacen = almacen;
    }

    public RespuestaRtu Procesar(List<ContribuyenteEntrada> entradas)
    {
        RespuestaRtu respuesta = new RespuestaRtu();

        lock (_almacen.Candado) // una petición a la vez modifica los datos
        {
            foreach (ContribuyenteEntrada entrada in entradas)
            {
                string nit = entrada.Nit ?? "";
                string nombre = entrada.Nombre ?? "";

                if (!ValidadorNit.EsValido(nit))
                {
                    respuesta.NitsInvalidos++;
                    continue;
                }

                Contribuyente? existente = _almacen.Rtu.Buscar(nit);
                if (existente == null)
                {
                    _almacen.Rtu.Agregar(new Contribuyente(nit, nombre));
                    respuesta.ContribuyentesNuevos++;
                }
                else
                {
                    existente.ActualizarNombre(nombre);
                    respuesta.ContribuyentesActualizados++;
                }
            }

            _almacen.Guardar();
        }

        return respuesta;
    }

    /// <summary>Lista todos los contribuyentes del RTU (para los selectores del frontend).</summary>
    public List<ContribuyenteSalida> Listar()
    {
        List<ContribuyenteSalida> lista = new List<ContribuyenteSalida>();

        lock (_almacen.Candado)
        {
            foreach (Contribuyente contribuyente in _almacen.Rtu.ObtenerTodos())
            {
                lista.Add(new ContribuyenteSalida
                {
                    Nit = contribuyente.Nit,
                    Nombre = contribuyente.Nombre
                });
            }
        }

        return lista;
    }
}