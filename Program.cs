// Punto de entrada: prepara el tablero, registra jugadores y procesa dados.
internal class Program
{
    // La lista circular representa las posiciones del tablero.
    private readonly LinkedList HeadLista = new LinkedList();

    // Ejecuta el flujo principal y garantiza que la Pico reciba STOP al salir.
    public static void Main()
    {
        Program program = new Program();

        program.HeadLista.InsertFirst("Head");

        for (int i = 1; i <= 24; i++)
        {
            program.HeadLista.InsertAfterHead(i);
        }
        // El tablero se recorre de forma circular y cada nodo representa una posicion.
        program.HeadLista.MakeCircular();
        program.HeadLista.PrintCircular();

        // Una sola conexion serial es compartida por tarjetas y dados.
        ConexionPico conexion = new ConexionPico();
        conexion.IniciarConexion("/dev/ttyACM0");
        try
        {
            Creador.CrearJugador(conexion);

            // Cambia el modo de la Pico y espera una respuesta de tirada.
            ControlDados dados = new ControlDados(conexion);
            dados.IniciarModoDados();

            Console.WriteLine("Esperando una tirada de dados...");
            int casillas = -1;
            while (casillas < 0)
            {
                casillas = dados.LeerCasillas();
            }

            Console.WriteLine($"La tirada indica avanzar {casillas} casillas.");
        }
        finally
        {
            // STOP apaga los displays en la Pico antes de cerrar el puerto.
            conexion.EnviarComando("STOP");
            conexion.CerrarConexion();
        }
    }
}

