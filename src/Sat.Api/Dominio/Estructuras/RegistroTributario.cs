using Sat.Api.Dominio.Entidades;
using Sat.Api.Dominio.Reglas;

namespace Sat.Api.Dominio.Estructuras;

/// <summary>
/// TDA "Registro Tributario Unificado" (RTU): colección de contribuyentes indexada por NIT.
/// Internamente usa un Dictionary, pero eso queda oculto: desde afuera solo se ven sus operaciones.
/// </summary>
public class RegistroTributario
{
    // clave = NIT normalizado, valor = contribuyente
    private readonly Dictionary<string, Contribuyente> _contribuyentes =
        new Dictionary<string, Contribuyente>();

    public int Cantidad => _contribuyentes.Count;

    public bool Existe(string nit)
    {
        return _contribuyentes.ContainsKey(ValidadorNit.Normalizar(nit));
    }

    /// <summary>Devuelve el contribuyente o null si no existe.</summary>
    public Contribuyente? Buscar(string nit)
    {
        if (_contribuyentes.TryGetValue(ValidadorNit.Normalizar(nit), out Contribuyente? encontrado))
        {
            return encontrado;
        }

        return null;
    }

    public void Agregar(Contribuyente contribuyente)
    {
        if (Existe(contribuyente.Nit))
        {
            throw new InvalidOperationException($"El NIT {contribuyente.Nit} ya existe en el RTU.");
        }

        _contribuyentes.Add(contribuyente.Nit, contribuyente);
    }

    /// <summary>Devuelve una COPIA de los contribuyentes, para que nadie altere el RTU desde afuera.</summary>
    public List<Contribuyente> ObtenerTodos()
    {
        List<Contribuyente> copia = new List<Contribuyente>();
        foreach (Contribuyente contribuyente in _contribuyentes.Values)
        {
            copia.Add(contribuyente);
        }

        return copia;
    }

    public void Limpiar()
    {
        _contribuyentes.Clear();
    }
}