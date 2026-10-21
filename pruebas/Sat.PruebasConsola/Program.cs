using System.Text.Json;
using Sat.Api.Datos;
using Sat.Api.Dominio.Entidades;
using Sat.Api.Dominio.Estructuras;
using Sat.Api.Dominio.Reglas;
using Sat.Api.Servicios;
using Sat.Compartido.Contratos.Entrada;
using Sat.Compartido.Contratos.Salida;

// Contadores para el resumen final
int correctas = 0;
int fallidas = 0;

// Función local: compara el resultado obtenido con el esperado e imprime OK o FALLA
void Verificar(string descripcion, bool condicion)
{
    if (condicion)
    {
        correctas++;
        Console.WriteLine($"  OK     {descripcion}");
    }
    else
    {
        fallidas++;
        Console.WriteLine($"  FALLA  {descripcion}");
    }
}

// Función local: devuelve true si la acción lanza una excepción (para probar las reglas que deben fallar)
bool LanzaExcepcion(Action accion)
{
    try
    {
        accion();
        return false;
    }
    catch (Exception)
    {
        return true;
    }
}

// ---------------------------------------------------------------- ValidadorNit
Console.WriteLine("=== ValidadorNit ===");
Verificar("7378106 es válido (ejemplo del enunciado)", ValidadorNit.EsValido("7378106"));
Verificar("8338817 es válido (ejemplo del enunciado)", ValidadorNit.EsValido("8338817"));
Verificar("5123456 es INVÁLIDO (su verificador debería ser 3)", !ValidadorNit.EsValido("5123456"));
Verificar("1234560 es válido (caso 11 mod 11 = 0)", ValidadorNit.EsValido("1234560"));
Verificar("576002K es válido (verificador K)", ValidadorNit.EsValido("576002K"));
Verificar("576002k es válido (k minúscula)", ValidadorNit.EsValido("576002k"));
Verificar("' 737810-6 ' es válido (espacios y guion)", ValidadorNit.EsValido(" 737810-6 "));
Verificar("12A4 es inválido (letra en la base)", !ValidadorNit.EsValido("12A4"));
Verificar("7 es inválido (sin base)", !ValidadorNit.EsValido("7"));
Verificar("cadena vacía es inválida", !ValidadorNit.EsValido(""));
Verificar("null es inválido", !ValidadorNit.EsValido(null));

// ---------------------------------------------------------------- ExtractorFecha
Console.WriteLine("=== ExtractorFecha ===");
bool ok = ExtractorFecha.IntentarExtraer("Guatemala, 15/01/2025 15:25 hrs.", out DateTime fecha);
Verificar("extrae 15/01/2025 15:25 del ejemplo del enunciado",
    ok && fecha == new DateTime(2025, 1, 15, 15, 25, 0));

ok = ExtractorFecha.IntentarExtraer("Quetzaltenango, 3/2/2026 9:05", out fecha);
Verificar("acepta día, mes y hora de 1 dígito (03/02/2026 09:05)",
    ok && fecha == new DateTime(2026, 2, 3, 9, 5, 0));

ok = ExtractorFecha.IntentarExtraer("Guatemala, 20/10/2026", out fecha);
Verificar("acepta fecha sin hora (hora 00:00)", ok && fecha == new DateTime(2026, 10, 20));

Verificar("rechaza 31/02/2026 (fecha imposible)",
    !ExtractorFecha.IntentarExtraer("Guatemala, 31/02/2026 10:00", out fecha));
Verificar("rechaza 15/13/2026 (mes 13)",
    !ExtractorFecha.IntentarExtraer("Guatemala, 15/13/2026 10:00", out fecha));
Verificar("rechaza 24:00 (hora fuera de rango)",
    !ExtractorFecha.IntentarExtraer("Guatemala, 15/01/2026 24:00", out fecha));
Verificar("rechaza texto sin fecha",
    !ExtractorFecha.IntentarExtraer("sin fecha", out fecha));
Verificar("rechaza null",
    !ExtractorFecha.IntentarExtraer(null, out fecha));

// ---------------------------------------------------------------- ReglasFiscales
Console.WriteLine("=== ReglasFiscales ===");
Verificar("IVA de 100.00 = 12.00", ReglasFiscales.CalcularIva(100.00m) == 12.00m);
Verificar("IVA de 100.10 = 12.01 (12.012 redondeado)", ReglasFiscales.CalcularIva(100.10m) == 12.01m);
Verificar("IVA de 0.38 = 0.05 (0.0456 redondeado)", ReglasFiscales.CalcularIva(0.38m) == 0.05m);
Verificar("IVA 24.00 es correcto para 200.00", ReglasFiscales.IvaEsCorrecto(200.00m, 24.00m));
Verificar("IVA 24.01 es incorrecto para 200.00", !ReglasFiscales.IvaEsCorrecto(200.00m, 24.01m));
Verificar("TOTAL 112.00 es correcto (100 + 12)", ReglasFiscales.TotalEsCorrecto(100.00m, 12.00m, 112.00m));
Verificar("TOTAL 113.00 es incorrecto", !ReglasFiscales.TotalEsCorrecto(100.00m, 12.00m, 113.00m));
Verificar("VALOR 150.25 es válido", ReglasFiscales.ValorEsValido(150.25m));
Verificar("VALOR 0 es inválido", !ReglasFiscales.ValorEsValido(0m));
Verificar("VALOR -5.00 es inválido", !ReglasFiscales.ValorEsValido(-5.00m));
Verificar("VALOR 10.125 es inválido (3 decimales)", !ReglasFiscales.ValorEsValido(10.125m));

// ---------------------------------------------------------------- Contribuyente
Console.WriteLine("=== Contribuyente ===");
Contribuyente empresa = new Contribuyente(" 737810-6 ", "  Empresa ABC, S.A. ");
Verificar("guarda el NIT normalizado (7378106)", empresa.Nit == "7378106");
Verificar("guarda el nombre sin espacios sobrantes", empresa.Nombre == "Empresa ABC, S.A.");
empresa.ActualizarNombre("Empresa ABC Renovada");
Verificar("ActualizarNombre cambia el nombre", empresa.Nombre == "Empresa ABC Renovada");
Verificar("no permite crear un contribuyente con NIT inválido",
    LanzaExcepcion(() => new Contribuyente("5123456", "NIT malo")));

// ---------------------------------------------------------------- Factura
Console.WriteLine("=== Factura ===");
DateTime ahora = new DateTime(2026, 10, 20, 18, 30, 0);
Factura buena = new Factura("A1990", new DateTime(2025, 1, 15, 15, 25, 0), ahora,
                            "7378106", "8338817", 100.00m, 12.00m, 112.00m);
Verificar("una factura nueva no está aprobada", !buena.EstaAprobada);
Verificar("una factura nueva no tiene errores", !buena.TieneErrores());
buena.Aprobar(2026102000000001);
Verificar("Aprobar asigna el código", buena.EstaAprobada && buena.CodigoAprobacion == 2026102000000001);
Verificar("no se puede aprobar dos veces", LanzaExcepcion(() => buena.Aprobar(2026102000000002)));

Factura mala = new Factura("A1991", null, ahora, "5123456", "8338817", 100.00m, 13.00m, 113.00m);
mala.AgregarError(TipoError.NitEmisorInvalido);
mala.AgregarError(TipoError.IvaMalCalculado);
mala.AgregarError(TipoError.IvaMalCalculado); // repetido: no debe duplicarse
Verificar("registra varios errores sin repetirlos", mala.Errores.Count == 2);
Verificar("TieneError detecta un error concreto", mala.TieneError(TipoError.IvaMalCalculado));
Verificar("una factura con errores no puede aprobarse", LanzaExcepcion(() => mala.Aprobar(1)));

// ---------------------------------------------------------------- RegistroTributario
Console.WriteLine("=== RegistroTributario (RTU) ===");
RegistroTributario rtu = new RegistroTributario();
rtu.Agregar(new Contribuyente("7378106", "Empresa ABC, S.A."));
rtu.Agregar(new Contribuyente("8338817", "Comercial XYZ, S.A."));
Verificar("Cantidad = 2", rtu.Cantidad == 2);
Verificar("Existe encuentra un NIT aunque venga con guion", rtu.Existe("737810-6"));
Verificar("Existe devuelve false para un NIT que no está", !rtu.Existe("1234560"));
Verificar("Buscar devuelve el contribuyente correcto", rtu.Buscar("8338817")?.Nombre == "Comercial XYZ, S.A.");
Verificar("Buscar devuelve null si no existe", rtu.Buscar("1234560") == null);
Verificar("no permite agregar un NIT repetido",
    LanzaExcepcion(() => rtu.Agregar(new Contribuyente("7378106", "Duplicado"))));
List<Contribuyente> copia = rtu.ObtenerTodos();
copia.Clear(); // vaciar la copia NO debe afectar al RTU
Verificar("ObtenerTodos entrega una copia (el RTU sigue con 2)", rtu.Cantidad == 2);
rtu.Limpiar();
Verificar("Limpiar deja el RTU vacío", rtu.Cantidad == 0);

// ---------------------------------------------------------------- LibroFacturas
Console.WriteLine("=== LibroFacturas ===");
LibroFacturas libro = new LibroFacturas();
libro.Agregar(buena); // aprobada, emitida el 15/01/2025
libro.Agregar(mala);  // rechazada, sin fecha válida
Verificar("Cantidad = 2 (guarda aprobadas y rechazadas)", libro.Cantidad == 2);
Verificar("A1990 figura como referencia aprobada", libro.ReferenciaYaAprobada("A1990"));
Verificar("A1991 (rechazada) NO ocupa la referencia", !libro.ReferenciaYaAprobada("A1991"));
List<Factura> enero = libro.ObtenerPorRango(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));
Verificar("rango de enero 2025 devuelve solo la factura con fecha", enero.Count == 1);
List<Factura> febrero = libro.ObtenerPorRango(new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28));
Verificar("rango de febrero 2025 está vacío", febrero.Count == 0);
libro.Limpiar();
Verificar("Limpiar vacía facturas y referencias",
    libro.Cantidad == 0 && !libro.ReferenciaYaAprobada("A1990"));

// ---------------------------------------------------------------- ControlCorrelativo
Console.WriteLine("=== ControlCorrelativo ===");
ControlCorrelativo control = new ControlCorrelativo();
DateTime dia1 = new DateTime(2026, 10, 20, 9, 0, 0);
Verificar("primer código del día termina en 00000001",
    control.GenerarCodigo(dia1) == 2026102000000001);
Verificar("segundo código del mismo día termina en 00000002",
    control.GenerarCodigo(dia1.AddHours(5)) == 2026102000000002);
DateTime dia2 = new DateTime(2026, 10, 21, 8, 0, 0);
Verificar("al cambiar de día el correlativo reinicia en 1",
    control.GenerarCodigo(dia2) == 2026102100000001);
ControlCorrelativo restaurado = new ControlCorrelativo(new DateOnly(2026, 10, 21), 50);
Verificar("un control restaurado continúa en 51 (no vuelve a 1)",
    restaurado.GenerarCodigo(dia2) == 2026102100000051);

// ---------------------------------------------------------------- Contratos JSON
Console.WriteLine("=== Contratos JSON ===");
string jsonRespuesta = JsonSerializer.Serialize(new RespuestaRtu { ContribuyentesNuevos = 2 });
Verificar("RespuestaRtu usa los nombres exactos del enunciado",
    jsonRespuesta == "{\"CONTRIBUYENTES_NUEVOS\":2,\"CONTRIBUYENTES_ACTUALIZADOS\":0,\"NITS_INVALIDOS\":0}");
List<ContribuyenteEntrada>? leidos = JsonSerializer.Deserialize<List<ContribuyenteEntrada>>(
    "[{\"NIT\": \"7378106\", \"nombre\": \"Empresa ABC, S.A.\"}]");
Verificar("ContribuyenteEntrada lee \"NIT\" y \"nombre\" del mensaje",
    leidos != null && leidos[0].Nit == "7378106" && leidos[0].Nombre == "Empresa ABC, S.A.");

// ---------------------------------------------------------------- ServicioRtu + AlmacenDatosJson
Console.WriteLine("=== ServicioRtu + AlmacenDatosJson ===");
// Carpeta temporal y única, para no tocar los datos reales de la API
string carpetaPrueba = Path.Combine(Path.GetTempPath(), "sat_pruebas_" + Guid.NewGuid().ToString("N"));
AlmacenDatosJson almacen = new AlmacenDatosJson(carpetaPrueba);
ServicioRtu servicioRtu = new ServicioRtu(almacen);

List<ContribuyenteEntrada> mensajeRtu = new List<ContribuyenteEntrada>
{
    new ContribuyenteEntrada { Nit = "7378106", Nombre = "Empresa ABC, S.A." },
    new ContribuyenteEntrada { Nit = "8338817", Nombre = "Comercial XYZ, S.A." },
    new ContribuyenteEntrada { Nit = "5123456", Nombre = "Servicios Integrales del Norte" }
};
RespuestaRtu r1 = servicioRtu.Procesar(mensajeRtu);
Verificar("ejemplo del enunciado → 2 nuevos, 0 actualizados, 1 inválido",
    r1.ContribuyentesNuevos == 2 && r1.ContribuyentesActualizados == 0 && r1.NitsInvalidos == 1);

RespuestaRtu r2 = servicioRtu.Procesar(new List<ContribuyenteEntrada>
{
    new ContribuyenteEntrada { Nit = "7378106", Nombre = "Empresa ABC Renovada" }
});
Verificar("un NIT existente se actualiza (1 actualizado)", r2.ContribuyentesActualizados == 1);
Verificar("Listar devuelve 2 contribuyentes", servicioRtu.Listar().Count == 2);
Verificar("se creó el archivo rtu.json", File.Exists(Path.Combine(carpetaPrueba, "rtu.json")));

// Guardamos también una factura y el correlativo para probar su persistencia
long codigoPrueba = almacen.Correlativo.GenerarCodigo(new DateTime(2026, 10, 21, 10, 0, 0));
Factura facturaAprobada = new Factura("A1990", new DateTime(2025, 1, 15, 15, 25, 0), DateTime.Now,
                                      "7378106", "8338817", 100.00m, 12.00m, 112.00m);
facturaAprobada.Aprobar(codigoPrueba);
Factura facturaRechazada = new Factura("A1991", null, DateTime.Now,
                                       "5123456", "8338817", 100.00m, 13.00m, 113.00m);
facturaRechazada.AgregarError(TipoError.NitEmisorInvalido);
facturaRechazada.AgregarError(TipoError.IvaMalCalculado);
almacen.Facturas.Agregar(facturaAprobada);
almacen.Facturas.Agregar(facturaRechazada);
almacen.Guardar();

// Un almacén NUEVO sobre la misma carpeta simula "reiniciar la API"
AlmacenDatosJson recargado = new AlmacenDatosJson(carpetaPrueba);
Verificar("tras recargar, el RTU conserva 2 contribuyentes", recargado.Rtu.Cantidad == 2);
Verificar("tras recargar, conserva el nombre actualizado",
    recargado.Rtu.Buscar("7378106")?.Nombre == "Empresa ABC Renovada");
Verificar("tras recargar, conserva las 2 facturas", recargado.Facturas.Cantidad == 2);
Verificar("tras recargar, A1990 sigue aprobada", recargado.Facturas.ReferenciaYaAprobada("A1990"));
List<Factura> todas = recargado.Facturas.ObtenerTodas();
Verificar("tras recargar, la rechazada conserva sus 2 errores", todas[1].Errores.Count == 2);
Verificar("tras recargar, el correlativo continúa en 2",
    recargado.Correlativo.GenerarCodigo(new DateTime(2026, 10, 21, 11, 0, 0)) == 2026102100000002);

recargado.Inicializar();
Verificar("Inicializar vacía el RTU y las facturas",
    recargado.Rtu.Cantidad == 0 && recargado.Facturas.Cantidad == 0);
Verificar("Inicializar borra los archivos", !File.Exists(Path.Combine(carpetaPrueba, "rtu.json")));

Directory.Delete(carpetaPrueba, true); // limpieza de la carpeta temporal

// ---------------------------------------------------------------- ServicioAutorizacion
Console.WriteLine("=== ServicioAutorizacion ===");
string carpetaAut = Path.Combine(Path.GetTempPath(), "sat_pruebas_" + Guid.NewGuid().ToString("N"));
AlmacenDatosJson almacenAut = new AlmacenDatosJson(carpetaAut);
new ServicioRtu(almacenAut).Procesar(mensajeRtu); // RTU con 7378106 y 8338817
ServicioAutorizacion servicioAut = new ServicioAutorizacion(almacenAut);

// Prefijo del código de hoy: yyyymmdd × 10^8 (el servicio usa la fecha actual)
long prefijoHoy = long.Parse(DateTime.Now.ToString("yyyyMMdd")) * 100_000_000L;

// Función local: crea una solicitud válida; cada prueba cambia solo lo que necesita
FacturaEntrada Solicitud(string referencia, string emisor = "7378106", string receptor = "8338817",
                         decimal valor = 100.00m, decimal iva = 12.00m, decimal total = 112.00m,
                         string tiempo = "Guatemala, 15/01/2025 15:25 hrs.")
{
    return new FacturaEntrada
    {
        Tiempo = tiempo, Referencia = referencia, NitEmisor = emisor, NitReceptor = receptor,
        Valor = valor, Iva = iva, Total = total
    };
}

// a) Ejemplo del enunciado: 2 facturas correctas
RespuestaAutorizaciones ra = servicioAut.Procesar(new List<FacturaEntrada>
{
    Solicitud("A1990"),
    Solicitud("A1991", valor: 200.00m, iva: 24.00m, total: 224.00m)
});
Verificar("ejemplo del enunciado → 2 aprobadas", ra.Resultado.Aprobadas == 2);
Verificar("detalle con 2 aprobaciones en orden",
    ra.Resultado.Detalle.Count == 2 && ra.Resultado.Detalle[0].Referencia == "A1990");
Verificar("códigos yyyymmdd00000001 y yyyymmdd00000002",
    ra.Resultado.Detalle[0].CodigoAprobacion == prefijoHoy + 1
    && ra.Resultado.Detalle[1].CodigoAprobacion == prefijoHoy + 2);

// b) Cada tipo de error por separado
RespuestaAutorizaciones rb = servicioAut.Procesar(new List<FacturaEntrada>
{
    Solicitud("B002", emisor: "1234560"),                          // emisor válido pero no está en el RTU
    Solicitud("B003", receptor: "576002K"),                        // receptor válido pero no está en el RTU
    Solicitud("B004", emisor: "5123456"),                          // emisor con NIT inválido
    Solicitud("B005", iva: 13.00m, total: 113.00m),                // IVA mal calculado
    Solicitud("B006", total: 115.00m),                             // TOTAL mal calculado
    Solicitud("A1990"),                                            // referencia ya aprobada
    Solicitud("B007", tiempo: "Guatemala, 31/02/2025 09:35 hrs.")  // fecha imposible
});
Verificar("ninguna de las 7 solicitudes con error se aprueba", rb.Resultado.Aprobadas == 0);
Verificar("1 rechazada por NIT emisor inexistente", rb.Resultado.RechazadasPorNitEmisorInexistente == 1);
Verificar("1 rechazada por NIT receptor inexistente", rb.Resultado.RechazadasPorNitReceptorInexistente == 1);
Verificar("detalleAprobaciones queda vacío", rb.Resultado.Detalle.Count == 0);

// Revisamos los errores registrados en el libro (las últimas 7 facturas)
List<Factura> libroAut = almacenAut.Facturas.ObtenerTodas();
Verificar("B002 → NitEmisorInexistente", libroAut[2].TieneError(TipoError.NitEmisorInexistente));
Verificar("B003 → NitReceptorInexistente", libroAut[3].TieneError(TipoError.NitReceptorInexistente));
Verificar("B004 → NitEmisorInvalido (y NO inexistente)",
    libroAut[4].TieneError(TipoError.NitEmisorInvalido) && !libroAut[4].TieneError(TipoError.NitEmisorInexistente));
Verificar("B005 → solo IvaMalCalculado (TOTAL coherente con su IVA)",
    libroAut[5].Errores.Count == 1 && libroAut[5].TieneError(TipoError.IvaMalCalculado));
Verificar("B006 → TotalMalCalculado", libroAut[6].TieneError(TipoError.TotalMalCalculado));
Verificar("A1990 repetida → ReferenciaDuplicada", libroAut[7].TieneError(TipoError.ReferenciaDuplicada));
Verificar("B007 → FormatoInvalido y sin fecha de emisión",
    libroAut[8].TieneError(TipoError.FormatoInvalido) && libroAut[8].FechaEmision == null);

// c) Una referencia rechazada se puede reenviar corregida (decisión D4)
RespuestaAutorizaciones rc = servicioAut.Procesar(new List<FacturaEntrada> { Solicitud("B005") });
Verificar("B005 corregida se aprueba con el código 3 del día",
    rc.Resultado.Aprobadas == 1 && rc.Resultado.Detalle[0].CodigoAprobacion == prefijoHoy + 3);

// d) Duplicado dentro del MISMO mensaje: la primera se aprueba, la segunda no
RespuestaAutorizaciones rd = servicioAut.Procesar(new List<FacturaEntrada> { Solicitud("C001"), Solicitud("C001") });
Verificar("C001 dos veces en un mismo mensaje → solo 1 aprobada", rd.Resultado.Aprobadas == 1);

// e) La respuesta respeta los nombres del enunciado
string jsonAut = JsonSerializer.Serialize(rc);
Verificar("JSON con resultadoAutorizaciones y nombres exactos",
    jsonAut.Contains("\"resultadoAutorizaciones\"")
    && jsonAut.Contains("\"cantidadAutorizacionesAprobadas\":1")
    && jsonAut.Contains("\"cantidadAutorizacionesRechazadasPorNitEmisorInexistente\":0")
    && jsonAut.Contains("\"cantidadAutorizacionesRechazadasPorNitReceptorInexistente\":0")
    && jsonAut.Contains("\"detalleAprobaciones\":[{\"referencia\":\"B005\",\"codigoAprobacion\":" + (prefijoHoy + 3) + "}]"));

// f) FacturaEntrada lee el JSON del enunciado (incluso con VALOR entre comillas)
List<FacturaEntrada>? solicitudesLeidas = JsonSerializer.Deserialize<List<FacturaEntrada>>(
    "[{\"TIEMPO\": \"Guatemala, 15/01/2025 15:25 hrs.\", \"REFERENCIA\": \"A1990\", " +
    "\"NIT_EMISOR\": \"7378106\", \"NIT_RECEPTOR\": \"8338817\", \"VALOR\": \"100.00\", \"IVA\": 12.00, \"TOTAL\": 112.00}]");
Verificar("FacturaEntrada lee TIEMPO, REFERENCIA, NITs y montos",
    solicitudesLeidas != null && solicitudesLeidas[0].NitReceptor == "8338817"
    && solicitudesLeidas[0].Valor == 100.00m && solicitudesLeidas[0].Total == 112.00m);

// g) Persistencia: tras "reiniciar", el correlativo continúa
AlmacenDatosJson almacenAut2 = new AlmacenDatosJson(carpetaAut);
RespuestaAutorizaciones rg = new ServicioAutorizacion(almacenAut2).Procesar(
    new List<FacturaEntrada> { Solicitud("D001") });
Verificar("tras reiniciar, el siguiente código es el 5 del día",
    rg.Resultado.Detalle[0].CodigoAprobacion == prefijoHoy + 5);

Directory.Delete(carpetaAut, true);

// ---------------------------------------------------------------- Resumen
Console.WriteLine();
Console.WriteLine($"Resultado: {correctas} correctas, {fallidas} fallidas.");