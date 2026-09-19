// Punto de entrada: prepara el tablero, registra jugadores y procesa dados.
internal class Program
{
    // La lista circular representa las posiciones del tablero.
    private readonly LinkedList HeadLista = new LinkedList();
    private Jugador[] jugadores = Array.Empty<Jugador>();

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
            program.jugadores = Creador.CrearJugador(conexion);

            Node salida = program.HeadLista.GetNodeAt(0);
            for (int i = 0; i < program.jugadores.Length; i++)
            {
                program.jugadores[i].Posicion = salida;
            }

            // Cambia el modo de la Pico y espera una respuesta de tirada.
            ControlDados dados = new ControlDados(conexion);
            dados.IniciarModoDados();

            int turno = 0;
            while (program.HayJugadoresActivos())
            {
                Jugador jugadorActual = program.jugadores[turno];
                turno = (turno + 1) % program.jugadores.Length;

                if (jugadorActual.Balance <= 0)
                {
                    continue;
                }

                Console.WriteLine($"Turno de {jugadorActual.Nombre}. Saldo: " +
                    $"${jugadorActual.Balance}");
                Console.WriteLine("Esperando una tirada de dados...");

                int casillas = -1;
                while (casillas < 0)
                {
                    casillas = dados.LeerCasillas();
                }

                MoverJugador(jugadorActual, casillas);
                Console.WriteLine($"{jugadorActual.Nombre} ahora esta en: " +
                    $"{jugadorActual.Posicion?.GetData()}");
            }

            Console.WriteLine("Todos los jugadores estan en bancarrota.");
        }
        finally
        {
            // STOP apaga los displays en la Pico antes de cerrar el puerto.
            conexion.EnviarComando("STOP");
            conexion.CerrarConexion();
        }
    }

    private bool HayJugadoresActivos()
    {
        for (int i = 0; i < jugadores.Length; i++)
        {
            if (jugadores[i].Balance > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void MoverJugador(Jugador jugador, int casillas)
    {
        for (int i = 0; i < casillas; i++)
        {
            jugador.Posicion = jugador.Posicion?.GetNext();
        }
    }
}

