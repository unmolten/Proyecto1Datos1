using System;
using System.Threading;

/**
 * @file Dados.cs
 * @brief Controlador para la lectura y gestion de dados fisicos y virtuales.
 *
 * Se comunica con la Raspberry Pi Pico para solicitar tiradas de dados fisicos
 * y decodificar las respuestas seriales en valores numericos de avance (1 a 12).
 */

/**
 * @class ControlDados
 * @brief Administra la recepcion y decodificacion de los lanzamientos de dados.
 */
public class ControlDados
{
    /** @brief Instancia compartida del conector serial con la Pico. */
    private readonly ConexionPico conexion;

    /** @brief Valor del primer dado de la ultima tirada registrada. */
    private int ultimoDado1 = 0;

    /** @brief Valor del segundo dado de la ultima tirada registrada. */
    private int ultimoDado2 = 0;

    /** @brief Suma total de casillas de la ultima tirada. */
    private int ultimaTirada = 0;

    /**
     * @brief Constructor que vincula el controlador a una conexion serial.
     * @param conexion Instancia de conexion serial compartida.
     */
    public ControlDados(ConexionPico conexion)
    {
        this.conexion = conexion;
    }

    /**
     * @brief Verifica si la comunicacion serial con el hardware de dados esta activa.
     * @return true si la Pico esta conectada; false en caso contrario.
     */
    public bool EstaConectado() => conexion.EstaConectado();

    /**
     * @brief Obtiene el valor del primer dado de la tirada reciente.
     * @return Entero entre 1 y 6.
     */
    public int GetUltimoDado1() => ultimoDado1;

    /**
     * @brief Obtiene el valor del segundo dado de la tirada reciente.
     * @return Entero entre 1 y 6.
     */
    public int GetUltimoDado2() => ultimoDado2;

    /**
     * @brief Obtiene la suma total de casillas de la tirada reciente.
     * @return Entero entre 1 y 12.
     */
    public int GetUltimaTirada() => ultimaTirada;

    /**
     * @brief Envia la orden a la Raspberry Pi Pico para ingresar al modo de dados.
     */
    public void IniciarModoDados()
    {
        conexion.EnviarComando("DADOS");
    }

    /**
     * @brief Prepara el sistema de dados para un nuevo turno reiniciando buffers y registros.
     */
    public void IniciarNuevoTurno()
    {
        ultimoDado1 = 0;
        ultimoDado2 = 0;
        ultimaTirada = 0;
        conexion.EnviarComando("NUEVO_TURNO");
        Thread.Sleep(50);
        conexion.LimpiarEntrada();
    }

    /**
     * @brief Descarta tiradas anteriores que hayan quedado en el buffer serial antes de lanzar.
     */
    public void LimpiarTiradasPrevias()
    {
        ultimoDado1 = 0;
        ultimoDado2 = 0;
        ultimaTirada = 0;
        conexion.LimpiarEntrada();
    }

    /**
     * @brief Envia valores de dados virtuales o calculados a los displays fisicos de la Pico.
     * @param d1 Valor del primer dado.
     * @param d2 Valor del segundo dado.
     */
    public void EnviarResultadoDados(int d1, int d2)
    {
        ultimoDado1 = d1;
        ultimoDado2 = d2;
        ultimaTirada = d1 + d2;
        conexion.EnviarComando($"MOSTRAR:{d1},{d2}");
    }

    /**
     * @brief Lee y procesa las respuestas del puerto serial para extraer una tirada valida.
     *
     * Soporta multiples formatos de entrada como:
     * - DADOS:d1,d2,total
     * - CASILLAS:X
     * - "3 + 4 = 7"
     * - Numeros enteros directos (ej: "7")
     *
     * Descarta automaticamente mensajes de control, ecos de comandos y lecturas RFID.
     * @return El total de casillas a avanzar (1 a 12), o -1 si no hay tirada disponible aun.
     */
    public int LeerCasillas()
    {
        string linea = conexion.LeerLinea();

        if (string.IsNullOrWhiteSpace(linea))
        {
            return -1;
        }

        // Ignorar respuestas de control, ecos, mensajes del lector RFID y UIDs
        if (linea.Equals("DADOS", StringComparison.OrdinalIgnoreCase) ||
            linea.Equals("TARJETAS", StringComparison.OrdinalIgnoreCase) ||
            linea.Equals("OK", StringComparison.OrdinalIgnoreCase) ||
            linea.Equals("LISTO", StringComparison.OrdinalIgnoreCase) ||
            linea.Equals("STOP", StringComparison.OrdinalIgnoreCase) ||
            linea.Equals("NUEVO_TURNO", StringComparison.OrdinalIgnoreCase) ||
            linea.StartsWith("MODO:", StringComparison.OrdinalIgnoreCase) ||
            linea.StartsWith("PICO_", StringComparison.OrdinalIgnoreCase) ||
            linea.StartsWith("RFID_", StringComparison.OrdinalIgnoreCase) ||
            linea.StartsWith("UID", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("0x", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Initializing", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Ready for next", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Place your RFID", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Card Detected", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Double-check", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Read error", StringComparison.OrdinalIgnoreCase))
        {
            return -1;
        }

        // 1. Formato DADOS:d1,d2,casillas (o DADOS:d1,d2)
        int idxDados = linea.IndexOf("DADOS:", StringComparison.OrdinalIgnoreCase);
        if (idxDados >= 0)
        {
            string resto = linea.Substring(idxDados + "DADOS:".Length).Trim();
            string[] partes = resto.Split(',');
            if (partes.Length >= 2 && int.TryParse(partes[0], out int d1) && int.TryParse(partes[1], out int d2))
            {
                int total = d1 + d2;
                if (partes.Length >= 3 && int.TryParse(partes[2], out int t) && t > 0)
                {
                    total = t;
                }

                if (total >= 1 && total <= 12)
                {
                    ultimoDado1 = d1;
                    ultimoDado2 = d2;
                    ultimaTirada = total;
                    Console.WriteLine($"[Pico Dados] Tirada detectada: [{d1}] + [{d2}] = {total}");
                    return total;
                }
            }
            else if (partes.Length == 1 && int.TryParse(partes[0], out int soloTotal) && soloTotal >= 1 && soloTotal <= 12)
            {
                ultimaTirada = soloTotal;
                ultimoDado1 = Math.Max(1, Math.Min(6, soloTotal / 2));
                ultimoDado2 = soloTotal - ultimoDado1;
                Console.WriteLine($"[Pico Dados] Tirada total detectada: {soloTotal}");
                return soloTotal;
            }
        }

        // 2. Prefijo CASILLAS:X o variantes comunes
        string[] prefijos = { "CASILLAS:", "CASILLA:", "TIRADA:", "TOTAL:", "RESULTADO:", "VALOR:", "AVANZA:" };
        foreach (var prefijo in prefijos)
        {
            int idx = linea.IndexOf(prefijo, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                string resto = linea.Substring(idx + prefijo.Length).Trim();
                string numStr = "";
                foreach (char c in resto)
                {
                    if (char.IsDigit(c)) numStr += c;
                    else if (numStr.Length > 0) break;
                }

                if (int.TryParse(numStr, out int casillas) && casillas >= 1 && casillas <= 12)
                {
                    ultimaTirada = casillas;
                    if (ultimoDado1 <= 0 || ultimoDado2 <= 0)
                    {
                        ultimoDado1 = Math.Max(1, Math.Min(6, casillas / 2));
                        ultimoDado2 = casillas - ultimoDado1;
                    }
                    Console.WriteLine($"[Pico Dados] Tirada detectada ({prefijo}): {casillas}");
                    return casillas;
                }
            }
        }

        // 3. Expresiones de suma tipo "3 + 4 = 7" o "3+4"
        int idxMas = linea.IndexOf('+');
        if (idxMas > 0 && idxMas < linea.Length - 1)
        {
            int d1 = ExtraerUltimoNumero(linea.Substring(0, idxMas));
            int d2 = ExtraerPrimerNumero(linea.Substring(idxMas + 1));
            if (d1 >= 1 && d1 <= 6 && d2 >= 1 && d2 <= 6)
            {
                int totalSuma = d1 + d2;
                ultimoDado1 = d1;
                ultimoDado2 = d2;
                ultimaTirada = totalSuma;
                Console.WriteLine($"[Pico Dados] Dados combinados [{d1} + {d2}] = {totalSuma}");
                return totalSuma;
            }
        }

        // 4. Lectura directa de un numero entero limpio (ej. "7")
        string trimmed = linea.Trim();
        bool esSoloDigitos = trimmed.Length > 0 && trimmed.Length <= 2;
        foreach (char c in trimmed)
        {
            if (!char.IsDigit(c)) { esSoloDigitos = false; break; }
        }
        if (esSoloDigitos && int.TryParse(trimmed, out int valorDirecto) && valorDirecto >= 1 && valorDirecto <= 12)
        {
            ultimaTirada = valorDirecto;
            if (ultimoDado1 <= 0 || ultimoDado2 <= 0)
            {
                ultimoDado1 = Math.Max(1, Math.Min(6, valorDirecto / 2));
                ultimoDado2 = valorDirecto - ultimoDado1;
            }
            Console.WriteLine($"[Pico Dados] Tirada directa detectada: {valorDirecto}");
            return valorDirecto;
        }

        return -1;
    }

    /**
     * @brief Extrae el primer numero entero presente en una cadena.
     * @param texto Cadena de origen.
     * @return El numero extraido o -1 si no contiene digitos.
     */
    private static int ExtraerPrimerNumero(string texto)
    {
        string num = "";
        foreach (char c in texto)
        {
            if (char.IsDigit(c)) num += c;
            else if (num.Length > 0) break;
        }
        return int.TryParse(num, out int res) ? res : -1;
    }

    /**
     * @brief Extrae el ultimo numero entero presente en una cadena.
     * @param texto Cadena de origen.
     * @return El numero extraido o -1 si no contiene digitos.
     */
    private static int ExtraerUltimoNumero(string texto)
    {
        string num = "";
        for (int i = texto.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(texto[i])) num = texto[i] + num;
            else if (num.Length > 0) break;
        }
        return int.TryParse(num, out int res) ? res : -1;
    }
}