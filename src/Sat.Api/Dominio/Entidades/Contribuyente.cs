using Sat.Api.Dominio.Reglas;

namespace Sat.Api.Dominio.Entidades;

/// <summary>
/// Un contribuyente del RTU. Solo puede existir con un NIT válido.
/// </summary>
public class Contribuyente
{
    // Propiedades
    public string Nit { get; }                 // solo lectura: se asigna en el constructor y nunca cambia
    public string Nombre { get; private set; } // se lee desde afuera, pero solo esta clase la modifica

    // Constructor
    public Contribuyente(string nit, string nombre)
    {
        if (!ValidadorNit.EsValido(nit))
        {
            throw new ArgumentException($"El NIT '{nit}' no es válido.", nameof(nit));
        }

        Nit = ValidadorNit.Normalizar(nit);
        Nombre = nombre.Trim();
    }

    // Métodos
    public void ActualizarNombre(string nuevoNombre)
    {
        Nombre = nuevoNombre.Trim();
    }
}
