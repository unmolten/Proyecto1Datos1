// Controla el modo de dados de la Pico mediante la conexion serial compartida.
public class ControlDados
{
    private readonly ConexionPico conexion;
    // Asocia el controlador de dados con la conexion serial.
    public ControlDados(ConexionPico conexion)
    {
        this.conexion = conexion;
    }
    // Indica si la conexion compartida esta disponible.
    public bool EstaConectado() => conexion.EstaConectado();
    // Cambia la Pico al modo dados y limpia respuestas anteriores.
    public void IniciarModoDados()
    {
        conexion.LimpiarEntrada();
        conexion.EnviarComando("DADOS");
    }
    /* Devuelve las casillas de una tirada. Acepta CASILLAS:7 o 7 y devuelve
    -1 cuando la linea recibida no representa una tirada valida. */
    public int LeerCasillas()
    {
        string linea = conexion.LeerLinea();
 
        if (int.TryParse(linea, out int valorDirecto))
        {
            return valorDirecto;
        }

        if (linea.StartsWith("CASILLAS:", StringComparison.OrdinalIgnoreCase))
        {
            string valor = linea.Substring("CASILLAS:".Length).Trim();
            if (int.TryParse(valor, out int casillas))
            {
                return casillas;
            }
        }
 
        return -1;
    }
}
 