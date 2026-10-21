using Sat.Api.Datos;
using Sat.Api.Dominio.Entidades;
using Sat.Api.Dominio.Reglas;
using Sat.Compartido.Contratos.Entrada;
using Sat.Compartido.Contratos.Salida;

namespace Sat.Api.Servicios;

/// <summary>
/// Caso de uso "Autorizar facturas" (ver el diagrama de actividades del plan).
/// Por cada solicitud: construye la Factura, aplica TODAS las validaciones y,
/// si no tiene errores, la aprueba con el siguiente código del correlativo diario.
/// </summary>
public class ServicioAutorizacion
{
    private const int LargoMaximoReferencia = 40;

    private readonly IAlmacenDatos _almacen;

    public ServicioAutorizacion(IAlmacenDatos almacen)
    {
        _almacen = almacen;
    }

    public RespuestaAutorizaciones Procesar(List<FacturaEntrada> entradas)
    {
        RespuestaAutorizaciones respuesta = new RespuestaAutorizaciones();
        ResultadoAutorizaciones resultado = respuesta.Resultado;

        lock (_almacen.Candado)
        {
            foreach (FacturaEntrada entrada in entradas)
            {
                DateTime ahora = DateTime.Now; // momento en que se procesa ESTA solicitud

                Factura factura = ConstruirFactura(entrada, ahora);
                Validar(factura);

                if (factura.TieneErrores())
                {
                    // La respuesta solo cuenta estos dos motivos de rechazo (estructura del enunciado)
                    if (factura.TieneError(TipoError.NitEmisorInexistente))
                    {
                        resultado.RechazadasPorNitEmisorInexistente++;
                    }
                    if (factura.TieneError(TipoError.NitReceptorInexistente))
                    {
                        resultado.RechazadasPorNitReceptorInexistente++;
                    }
                }
                else
                {
                    long codigo = _almacen.Correlativo.GenerarCodigo(ahora);
                    factura.Aprobar(codigo);

                    resultado.Aprobadas++;
                    resultado.Detalle.Add(new DetalleAprobacion
                    {
                        Referencia = factura.Referencia,
                        CodigoAprobacion = codigo
                    });
                }

                // Se guarda SIEMPRE (aprobada o rechazada): el reporte necesita ambas
                _almacen.Facturas.Agregar(factura);
            }

            _almacen.Guardar();
        }

        return respuesta;
    }

    /// <summary>
    /// Convierte el DTO en una entidad: extrae la fecha con Regex, normaliza NITs y referencia.
    /// No valida nada todavía: solo traduce.
    /// </summary>
    private Factura ConstruirFactura(FacturaEntrada entrada, DateTime ahora)
    {
        DateTime? fechaEmision = null;
        if (ExtractorFecha.IntentarExtraer(entrada.Tiempo, out DateTime fechaExtraida))
        {
            fechaEmision = fechaExtraida;
        }

        string referencia = (entrada.Referencia ?? "").Trim();
        string nitEmisor = ValidadorNit.Normalizar(entrada.NitEmisor);
        string nitReceptor = ValidadorNit.Normalizar(entrada.NitReceptor);

        return new Factura(referencia, fechaEmision, ahora, nitEmisor, nitReceptor,
                           entrada.Valor, entrada.Iva, entrada.Total);
    }

    /// <summary>
    /// Aplica todas las reglas del enunciado y registra CADA error encontrado (decisión D3).
    /// </summary>
    private void Validar(Factura factura)
    {
        // 1) Formato (decisión D7): fecha legible, referencia de 1 a 40 caracteres, VALOR positivo con 2 decimales
        bool referenciaValida = factura.Referencia.Length > 0
                                && factura.Referencia.Length <= LargoMaximoReferencia;

        if (factura.FechaEmision == null || !referenciaValida || !ReglasFiscales.ValorEsValido(factura.Valor))
        {
            factura.AgregarError(TipoError.FormatoInvalido);
        }

        // 2) NIT emisor: primero que sea válido; solo si es válido se busca en el RTU
        if (!ValidadorNit.EsValido(factura.NitEmisor))
        {
            factura.AgregarError(TipoError.NitEmisorInvalido);
        }
        else if (!_almacen.Rtu.Existe(factura.NitEmisor))
        {
            factura.AgregarError(TipoError.NitEmisorInexistente);
        }

        // 3) NIT receptor: misma lógica
        if (!ValidadorNit.EsValido(factura.NitReceptor))
        {
            factura.AgregarError(TipoError.NitReceptorInvalido);
        }
        else if (!_almacen.Rtu.Existe(factura.NitReceptor))
        {
            factura.AgregarError(TipoError.NitReceptorInexistente);
        }

        // 4) IVA = REDONDEAR(VALOR * 0.12, 2)
        if (!ReglasFiscales.IvaEsCorrecto(factura.Valor, factura.Iva))
        {
            factura.AgregarError(TipoError.IvaMalCalculado);
        }

        // 5) TOTAL = VALOR + IVA
        if (!ReglasFiscales.TotalEsCorrecto(factura.Valor, factura.Iva, factura.Total))
        {
            factura.AgregarError(TipoError.TotalMalCalculado);
        }

        // 6) REFERENCIA única entre las facturas aprobadas (decisión D4)
        if (_almacen.Facturas.ReferenciaYaAprobada(factura.Referencia))
        {
            factura.AgregarError(TipoError.ReferenciaDuplicada);
        }
    }
}