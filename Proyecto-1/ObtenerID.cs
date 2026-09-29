using System;

/**
 * @file ObtenerID.cs
 * @brief Modulo para la lectura y extraccion de identificadores de tarjetas RFID.
 *
 * Se comunica con la Raspberry Pi Pico mediante la conexion serial y procesa
 * las lineas recibidas para validar y extraer el UID hexadecimal de los tags RFID.
 */

/**
 * @class ObtenerID
 * @brief Gestiona la deteccion de tarjetas RFID a traves del lector RC522.
 */
public class ObtenerID
{
    /** @brief Referencia a la conexion serial compartida con la Pico. */
    private readonly ConexionPico conexion;

    /**
     * @brief Constructor que vincula el lector con la comunicacion serial existente.
     * @param conexion Instancia activa de ConexionPico.
     */
    public ObtenerID(ConexionPico conexion)
    {
        this.conexion = conexion;
    }

    /**
     * @brief Consulta si la conexion fisica con la Pico se encuentra disponible.
     * @return true si esta conectada; false en caso contrario.
     */
    public bool EstaConectado() => conexion.EstaConectado();

    /**
     * @brief Instruye a la Pico para ingresar en modo de deteccion de tarjetas RFID.
     */
    public void IniciarLecturaTarjetas()
    {
        // Se preserva el buffer para no perder una tarjeta presentada instantes antes
        conexion.EnviarComando("TARJETAS");
    }

    /**
     * @brief Lee del canal serial y extrae un UID de tarjeta valido si existe.
     *
     * Analiza formatos oficiales emitidos por el firmware de la Pico (ej: 0x5D8E421A o UID:...)
     * descartando respuestas de diagnostico, ecos de consola y ruido serial.
     * @return Cadena con el UID en mayusculas, o string.Empty si no se ha detectado tarjeta aun.
     */
    public string LeerTarjeta()
    {
        string linea = conexion.LeerLinea();

        if (string.IsNullOrWhiteSpace(linea) || EsMensajeDeControl(linea))
        {
            return string.Empty;
        }

        Console.WriteLine($"[Pico Serial RX]: \"{linea}\"");

        // Descartar mensajes de inicio, estado y reconexion de la Pico sin generar alertas
        if (linea.Contains("Initializing", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Ready for next", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Place your RFID", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Double-check", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Initialization successful", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Read error", StringComparison.OrdinalIgnoreCase) ||
            linea.Contains("Program stopped", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        // Si la linea contiene "0x", extraer los digitos hexadecimales contiguos
        int idx0x = linea.IndexOf("0x", StringComparison.OrdinalIgnoreCase);
        if (idx0x >= 0)
        {
            string despues = linea.Substring(idx0x + 2);
            string hex0x = "";
            foreach (char c in despues)
            {
                if (Uri.IsHexDigit(c)) hex0x += c;
                else break;
            }
            if (hex0x.Length >= 8 && hex0x.Length <= 32)
            {
                Console.WriteLine($"[RFID] Tarjeta detectada. UID: {hex0x.ToUpper()}");
                return hex0x.ToUpper();
            }
        }

        // Buscar prefijo UID:, Hex UID:, o UID
        int inicioUid = linea.IndexOf("UID:", StringComparison.OrdinalIgnoreCase);
        if (inicioUid >= 0)
        {
            linea = linea.Substring(inicioUid + "UID:".Length).Trim();
        }
        else
        {
            int idxHex = linea.IndexOf("Hex:", StringComparison.OrdinalIgnoreCase);
            if (idxHex >= 0)
            {
                linea = linea.Substring(idxHex + "Hex:".Length).Trim();
            }
        }

        string limpia = linea.Replace(":", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase);

        // Extraer la secuencia continua de digitos hexadecimales mas larga
        string candidato = "";
        string mejorCandidato = "";
        foreach (char c in limpia)
        {
            if (Uri.IsHexDigit(c))
            {
                candidato += c;
                if (candidato.Length > mejorCandidato.Length)
                {
                    mejorCandidato = candidato;
                }
            }
            else
            {
                candidato = "";
            }
        }

        string uid = mejorCandidato;

        if (uid.Length < 8 || uid.Length > 32)
        {
            return string.Empty;
        }

        return uid.ToUpper();
    }

    /**
     * @brief Comprueba si una linea recibida corresponde a un eco o mensaje interno del sistema.
     * @param linea Texto recibido por el puerto serial.
     * @return true si es mensaje de control a descartar; false si puede contener un UID.
     */
    private static bool EsMensajeDeControl(string linea)
    {
        return linea.Equals("TARJETAS", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("DADOS", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("OK", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("LISTO", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("STOP", StringComparison.OrdinalIgnoreCase)
            || linea.StartsWith("MODO:", StringComparison.OrdinalIgnoreCase)
            || linea.StartsWith("PICO_", StringComparison.OrdinalIgnoreCase)
            || linea.StartsWith("RFID_", StringComparison.OrdinalIgnoreCase)
            || linea.StartsWith("CASILLAS:", StringComparison.OrdinalIgnoreCase);
    }
}