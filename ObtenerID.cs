// Controla el modo de lectura RFID usando una ConexionPico compartida.
public class ObtenerID
{
    private readonly ConexionPico conexion;
    // Asocia el lector RFID con una conexion serial existente.
    public ObtenerID(ConexionPico conexion)
    {
        this.conexion = conexion;
    }
    // Indica si la conexion compartida esta disponible.
    public bool EstaConectado() => conexion.EstaConectado();
    /* Cambia la Pico al modo de tarjetas y limpia respuestas anteriores.
    Debe llamarse antes de comenzar a leer UID. */
    public void IniciarLecturaTarjetas()
    {
        conexion.LimpiarEntrada();
        conexion.EnviarComando("TARJETAS");
    }
    /* Lee y valida un UID. Devuelve texto vacio para mensajes de control,
    lineas invalidas o cuando todavia no hay una tarjeta disponible. */
    public string LeerTarjeta()
    {
        string linea = conexion.LeerLinea();

        if (string.IsNullOrWhiteSpace(linea) || EsMensajeDeControl(linea))
        {
            return string.Empty;
        }

        int inicioUid = linea.IndexOf("UID:", StringComparison.OrdinalIgnoreCase);
        if (inicioUid >= 0)
        {
            linea = linea.Substring(inicioUid + "UID:".Length).Trim();
        }

        string uid = linea.Replace(":", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase);

        if (uid.Length < 8 || uid.Length > 32)
        {
            Console.WriteLine($"[Pico] Línea ignorada como UID: {linea}");
            return string.Empty;
        }

        foreach (char caracter in uid)
        {
            if (!Uri.IsHexDigit(caracter))
            {
                Console.WriteLine($"[Pico] Línea ignorada como UID: {linea}");
                return string.Empty;
            }
        }

        return uid;
    }

    // Evita que ecos de comandos o respuestas de otro modo se guarden como UID.
    private static bool EsMensajeDeControl(string linea)
    {
        return linea.Equals("TARJETAS", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("DADOS", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("OK", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("LISTO", StringComparison.OrdinalIgnoreCase)
            || linea.Equals("STOP", StringComparison.OrdinalIgnoreCase)
            || linea.StartsWith("CASILLAS:", StringComparison.OrdinalIgnoreCase);
    }
}