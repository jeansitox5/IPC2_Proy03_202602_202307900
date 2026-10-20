using Sat.Api.Dominio.Reglas;

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

// ---------------------------------------------------------------- Resumen
Console.WriteLine();
Console.WriteLine($"Resultado: {correctas} correctas, {fallidas} fallidas.");