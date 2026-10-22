using Sat.Api.Datos;
using Sat.Api.Dominio.Entidades;
using Sat.Api.Dominio.Reglas;
using Sat.Compartido.Contratos.Consultas;

namespace Sat.Api.Servicios;

/// <summary>
/// Consultas y reportes sobre las facturas almacenadas.
/// Todos los cálculos (agrupar, sumar, contar) son algoritmos propios, sin LINQ (decisión D6).
/// </summary>
public class ServicioConsultas
{
    public const string TipoEmitidas = "emitidas";
    public const string TipoRecibidas = "recibidas";
    public const string TipoTotal = "total";
    public const string TipoSinIva = "siniva";

    private readonly IAlmacenDatos _almacen;

    public ServicioConsultas(IAlmacenDatos almacen)
    {
        _almacen = almacen;
    }

    // ================================================================== 1) Consultar contribuyente
    /// <summary>
    /// Facturas AUTORIZADAS emitidas o recibidas por un NIT en un rango:
    /// por cada fecha, cantidad y monto; además el total del rango y el listado de facturas.
    /// </summary>
    public ConsultaContribuyenteRespuesta ConsultarContribuyente(string nit, string tipo, DateOnly desde, DateOnly hasta)
    {
        ValidarRango(desde, hasta);
        string tipoNormalizado = NormalizarTipo(tipo, TipoEmitidas, TipoRecibidas);
        string nitNormalizado = ValidadorNit.Normalizar(nit);

        ConsultaContribuyenteRespuesta respuesta = new ConsultaContribuyenteRespuesta
        {
            Nit = nitNormalizado,
            Tipo = tipoNormalizado,
            Desde = desde,
            Hasta = hasta
        };

        lock (_almacen.Candado)
        {
            respuesta.Nombre = _almacen.Rtu.Buscar(nitNormalizado)?.Nombre ?? "";

            // Paso 1: filtrar las facturas aprobadas de ese NIT (como emisor o como receptor)
            List<Factura> delContribuyente = new List<Factura>();
            foreach (Factura factura in _almacen.Facturas.ObtenerPorRango(desde, hasta))
            {
                if (!factura.EstaAprobada)
                {
                    continue;
                }

                string nitDeLaFactura = tipoNormalizado == TipoEmitidas ? factura.NitEmisor : factura.NitReceptor;
                if (nitDeLaFactura == nitNormalizado)
                {
                    delContribuyente.Add(factura);
                }
            }

            // Paso 2: agrupar por fecha y recorrer los días en orden
            Dictionary<DateOnly, List<Factura>> porFecha = AgruparPorFecha(delContribuyente);
            foreach (DateOnly dia in DiasConDatos(porFecha, desde, hasta))
            {
                TotalPorFecha fila = new TotalPorFecha { Fecha = dia };

                foreach (Factura factura in porFecha[dia])
                {
                    fila.Cantidad++;
                    fila.Monto += factura.Total;
                    respuesta.Facturas.Add(CrearResumen(factura));
                }

                respuesta.Fechas.Add(fila);
                respuesta.CantidadTotal += fila.Cantidad;
                respuesta.MontoTotal += fila.Monto;
            }
        }

        return respuesta;
    }

    // ================================================================== 2) Resumen de IVA por fecha y NIT
    /// <summary>
    /// Para una fecha: por cada NIT que participó en facturas autorizadas, su IVA emitido y su IVA recibido.
    /// </summary>
    public ResumenIvaRespuesta ResumenIva(DateOnly fecha)
    {
        ResumenIvaRespuesta respuesta = new ResumenIvaRespuesta { Fecha = fecha };

        lock (_almacen.Candado)
        {
            // Índice NIT → fila, para encontrar rápido la fila de cada NIT.
            // La lista respuesta.Nits conserva el orden en que aparecen los NITs.
            Dictionary<string, IvaPorNit> indice = new Dictionary<string, IvaPorNit>();

            foreach (Factura factura in _almacen.Facturas.ObtenerPorRango(fecha, fecha))
            {
                if (!factura.EstaAprobada)
                {
                    continue;
                }

                IvaPorNit emisor = ObtenerOCrearFila(indice, respuesta.Nits, factura.NitEmisor);
                emisor.IvaEmitido += factura.Iva;

                IvaPorNit receptor = ObtenerOCrearFila(indice, respuesta.Nits, factura.NitReceptor);
                receptor.IvaRecibido += factura.Iva;

                respuesta.TotalIvaEmitido += factura.Iva;
                respuesta.TotalIvaRecibido += factura.Iva;
            }
        }

        return respuesta;
    }

    // ================================================================== 3) Resumen por rango de fechas
    /// <summary>
    /// Por cada fecha del rango, el valor autorizado: TOTAL (tipo "total") o VALOR sin IVA (tipo "siniva").
    /// </summary>
    public ResumenRangoRespuesta ResumenRango(DateOnly desde, DateOnly hasta, string tipo)
    {
        ValidarRango(desde, hasta);
        string tipoNormalizado = NormalizarTipo(tipo, TipoTotal, TipoSinIva);

        ResumenRangoRespuesta respuesta = new ResumenRangoRespuesta
        {
            Desde = desde,
            Hasta = hasta,
            Tipo = tipoNormalizado
        };

        lock (_almacen.Candado)
        {
            List<Factura> aprobadas = new List<Factura>();
            foreach (Factura factura in _almacen.Facturas.ObtenerPorRango(desde, hasta))
            {
                if (factura.EstaAprobada)
                {
                    aprobadas.Add(factura);
                }
            }

            Dictionary<DateOnly, List<Factura>> porFecha = AgruparPorFecha(aprobadas);
            foreach (DateOnly dia in DiasConDatos(porFecha, desde, hasta))
            {
                TotalPorFecha fila = new TotalPorFecha { Fecha = dia };

                foreach (Factura factura in porFecha[dia])
                {
                    fila.Cantidad++;
                    fila.Monto += tipoNormalizado == TipoTotal ? factura.Total : factura.Valor;
                }

                respuesta.Fechas.Add(fila);
                respuesta.MontoTotal += fila.Monto;
            }
        }

        return respuesta;
    }

    // ================================================================== 4) Reporte de autorizaciones
    /// <summary>
    /// Reporte del enunciado: por cada fecha, facturas recibidas, conteo de cada tipo de error,
    /// facturas sin errores y cantidad de emisores y receptores distintos.
    /// </summary>
    public ReporteRespuesta Reporte(DateOnly desde, DateOnly hasta)
    {
        ValidarRango(desde, hasta);
        ReporteRespuesta respuesta = new ReporteRespuesta { Desde = desde, Hasta = hasta };

        lock (_almacen.Candado)
        {
            // Aquí entran TODAS las facturas (aprobadas y rechazadas)
            Dictionary<DateOnly, List<Factura>> porFecha = AgruparPorFecha(_almacen.Facturas.ObtenerPorRango(desde, hasta));

            foreach (DateOnly dia in DiasConDatos(porFecha, desde, hasta))
            {
                List<Factura> delDia = porFecha[dia];
                ReporteFecha fila = new ReporteFecha { Fecha = dia, TotalRecibidas = delDia.Count };

                // HashSet: cada NIT se cuenta una sola vez aunque aparezca en varias facturas
                HashSet<string> emisores = new HashSet<string>();
                HashSet<string> receptores = new HashSet<string>();

                foreach (Factura factura in delDia)
                {
                    if (factura.TieneError(TipoError.NitEmisorInvalido)) fila.NitEmisorInvalido++;
                    if (factura.TieneError(TipoError.NitReceptorInvalido)) fila.NitReceptorInvalido++;
                    if (factura.TieneError(TipoError.NitEmisorInexistente)) fila.NitEmisorInexistente++;
                    if (factura.TieneError(TipoError.NitReceptorInexistente)) fila.NitReceptorInexistente++;
                    if (factura.TieneError(TipoError.IvaMalCalculado)) fila.IvaMalCalculado++;
                    if (factura.TieneError(TipoError.TotalMalCalculado)) fila.TotalMalCalculado++;
                    if (factura.TieneError(TipoError.ReferenciaDuplicada)) fila.ReferenciaDuplicada++;
                    if (factura.TieneError(TipoError.FormatoInvalido)) fila.FormatoInvalido++;

                    if (!factura.TieneErrores())
                    {
                        fila.SinErrores++;
                    }

                    if (factura.NitEmisor != "")
                    {
                        emisores.Add(factura.NitEmisor);
                    }
                    if (factura.NitReceptor != "")
                    {
                        receptores.Add(factura.NitReceptor);
                    }
                }

                fila.CantidadEmisores = emisores.Count;
                fila.CantidadReceptores = receptores.Count;
                respuesta.Fechas.Add(fila);
            }
        }

        return respuesta;
    }

    // ================================================================== Métodos auxiliares (privados)
    /// <summary>Lanza ArgumentException si el rango está invertido.</summary>
    private static void ValidarRango(DateOnly desde, DateOnly hasta)
    {
        if (desde > hasta)
        {
            throw new ArgumentException("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");
        }
    }

    /// <summary>Pasa el tipo a minúsculas y verifica que sea una de las dos opciones permitidas.</summary>
    private static string NormalizarTipo(string tipo, string opcion1, string opcion2)
    {
        string normalizado = (tipo ?? "").Trim().ToLower();
        if (normalizado != opcion1 && normalizado != opcion2)
        {
            throw new ArgumentException($"El tipo debe ser '{opcion1}' o '{opcion2}'.");
        }

        return normalizado;
    }

    /// <summary>
    /// Agrupa facturas por su fecha de emisión: fecha → lista de facturas de ese día.
    /// Recorre la lista UNA sola vez.
    /// </summary>
    private static Dictionary<DateOnly, List<Factura>> AgruparPorFecha(List<Factura> facturas)
    {
        Dictionary<DateOnly, List<Factura>> grupos = new Dictionary<DateOnly, List<Factura>>();

        foreach (Factura factura in facturas)
        {
            if (factura.FechaEmision == null)
            {
                continue;
            }

            DateOnly dia = DateOnly.FromDateTime(factura.FechaEmision.Value);

            if (!grupos.TryGetValue(dia, out List<Factura>? delDia))
            {
                delDia = new List<Factura>();   // primera factura de ese día: se crea su grupo
                grupos.Add(dia, delDia);
            }

            delDia.Add(factura);
        }

        return grupos;
    }

    /// <summary>
    /// Devuelve, en orden cronológico, los días del rango que tienen facturas.
    /// Recorre el rango día por día usando DayNumber (días transcurridos desde el 01/01/0001).
    /// </summary>
    private static List<DateOnly> DiasConDatos(Dictionary<DateOnly, List<Factura>> grupos, DateOnly desde, DateOnly hasta)
    {
        List<DateOnly> dias = new List<DateOnly>();

        for (int numero = desde.DayNumber; numero <= hasta.DayNumber; numero++)
        {
            DateOnly dia = DateOnly.FromDayNumber(numero);
            if (grupos.ContainsKey(dia))
            {
                dias.Add(dia);
            }
        }

        return dias;
    }

    /// <summary>Busca la fila de un NIT en el índice; si no existe, la crea y la agrega a la lista.</summary>
    private IvaPorNit ObtenerOCrearFila(Dictionary<string, IvaPorNit> indice, List<IvaPorNit> filas, string nit)
    {
        if (!indice.TryGetValue(nit, out IvaPorNit? fila))
        {
            fila = new IvaPorNit
            {
                Nit = nit,
                Nombre = _almacen.Rtu.Buscar(nit)?.Nombre ?? ""
            };
            indice.Add(nit, fila);
            filas.Add(fila);
        }

        return fila;
    }

    /// <summary>Convierte una factura aprobada en su DTO de listado.</summary>
    private static FacturaResumen CrearResumen(Factura factura)
    {
        return new FacturaResumen
        {
            Referencia = factura.Referencia,
            FechaEmision = factura.FechaEmision ?? DateTime.MinValue,
            CodigoAprobacion = factura.CodigoAprobacion ?? 0,
            NitEmisor = factura.NitEmisor,
            NitReceptor = factura.NitReceptor,
            Valor = factura.Valor,
            Iva = factura.Iva,
            Total = factura.Total
        };
    }
}