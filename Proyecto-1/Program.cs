using System;

// Punto de entrada: inicializa el juego de Monopoly con la lista circular de casillas,
// registra a los jugadores (vía consola o RFID) y ejecuta el ciclo de turnos interactivo.
internal class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==========================================================");
        Console.WriteLine("             🎮 MONOPOLY - EDICIÓN AYEDD 🎮               ");
        Console.WriteLine("==========================================================");

        var juego = JuegoMonopoly.Instancia;
        Console.WriteLine($"Tablero cargado con {juego.Tablero.Size()} casillas en lista circular.\n");

        Console.WriteLine("Seleccione el modo de juego:");
        Console.WriteLine("1. Modo Consola (Registro de jugadores manual)");
        Console.WriteLine("2. Modo Hardware (Raspberry Pi Pico: RFID + Dados)");
        Console.Write("Opción (1 o 2, por defecto 1): ");
        string? opcionModo = Console.ReadLine();

        ConexionPico? conexion = null;
        ControlDados? dadosHardware = null;

        if (opcionModo == "2")
        {
            Console.Write("Ingrese el puerto serial de la Pico (ej. COM3 o /dev/ttyACM0): ");
            string? puerto = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(puerto)) puerto = "COM3";

            conexion = new ConexionPico();
            conexion.IniciarConexion(puerto);

            try
            {
                Jugador[] jugadoresCapturados = Creador.CrearJugador(conexion);
                for (int i = 0; i < jugadoresCapturados.Length; i++)
                {
                    juego.RegistrarJugadorExistente(jugadoresCapturados[i]);
                }

                dadosHardware = new ControlDados(conexion);
                dadosHardware.IniciarModoDados();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error al conectar con la Pico]: {ex.Message}");
                Console.WriteLine("Cambiando a modo consola...\n");
            }
        }

        // Si no se usó la Pico o falló, registrar jugadores por consola
        if (juego.Jugadores.Size() == 0)
        {
            Console.Write("\n¿Cuántos jugadores participarán? (2-4): ");
            if (!int.TryParse(Console.ReadLine(), out int cantidad) || cantidad < 2)
            {
                cantidad = 2;
            }

            for (int i = 1; i <= cantidad; i++)
            {
                Console.Write($"Nombre del Jugador {i}: ");
                string? nombre = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(nombre)) nombre = $"Jugador_{i}";

                Jugador nuevo = new Jugador(i, nombre, balance_inicial: 1500)
                {
                    Posicion = juego.Tablero.GetHead()
                };
                juego.RegistrarJugadorExistente(nuevo);
            }
        }

        Console.WriteLine("\n==========================================================");
        Console.WriteLine("                 ¡COMIENZA LA PARTIDA!                    ");
        Console.WriteLine("==========================================================\n");

        try
        {
            int turnoGlobal = 1;

            // Bucle principal de la partida: continúa mientras haya más de un jugador con saldo positivo
            while (ContarJugadoresActivos(juego) > 1)
            {
                juego.TurnoActual = turnoGlobal;

                // Recorremos la lista enlazada de jugadores activos
                Node? nodoJugador = juego.Jugadores.GetHead();
                int totalJugadores = juego.Jugadores.Size();

                for (int idx = 0; idx < totalJugadores; idx++)
                {
                    if (nodoJugador?.GetData() is Jugador jugadorActual)
                    {
                        if (jugadorActual.Dinero <= 0)
                        {
                            nodoJugador = nodoJugador?.GetNext();
                            continue;
                        }

                        if (ContarJugadoresActivos(juego) <= 1)
                        {
                            break;
                        }

                        EjecutarTurnoJugador(juego, jugadorActual, dadosHardware);
                    }

                    nodoJugador = nodoJugador?.GetNext();
                }

                turnoGlobal++;
            }

            // Anuncio del ganador
            Jugador? ganador = ObtenerGanador(juego);
            Console.WriteLine("\n==========================================================");
            if (ganador != null)
            {
                Console.WriteLine($"🏆 ¡FELICITACIONES, {ganador.Nombre.ToUpper()}! ¡HAS GANADO LA PARTIDA!");
                Console.WriteLine($"Saldo final: ${ganador.Dinero} | Propiedades: {ganador.Propiedades.Size()}");
            }
            else
            {
                Console.WriteLine("La partida ha terminado.");
            }
            Console.WriteLine("==========================================================\n");

            // Generar informe final de transacciones
            Transaccion.ImprimirTransacciones();
            Console.WriteLine("📄 Se ha generado el reporte de transacciones en 'Reporte.txt'.");
        }
        finally
        {
            if (conexion != null && conexion.EstaConectado())
            {
                conexion.EnviarComando("STOP");
                conexion.CerrarConexion();
            }
        }
    }

    // Ejecuta el turno individual de un jugador con menú de acciones
    private static void EjecutarTurnoJugador(JuegoMonopoly juego, Jugador jugador, ControlDados? dadosHardware)
    {
        Console.WriteLine($"\n----------------------------------------------------------");
        Console.WriteLine($"🎲 TURNO #{juego.TurnoActual} DE: {jugador.Nombre.ToUpper()}");
        Console.WriteLine($"Saldo: ${jugador.Dinero} | Posición: {jugador.ObtenerCasillaActual()?.Nombre ?? "Salida"}");
        if (jugador.EnCarcel)
        {
            Console.WriteLine($"🔒 ¡Estás en la Cárcel! (Turnos cumplidos: {jugador.TurnosEnCarcel}/3)");
        }
        Console.WriteLine($"----------------------------------------------------------");

        bool turnoTerminado = false;
        bool yaTiroDados = false;

        while (!turnoTerminado)
        {
            Console.WriteLine("\nAcciones disponibles:");
            if (!yaTiroDados)
            {
                Console.WriteLine("  1. Tirar dados y avanzar");
            }
            if (jugador.EnCarcel)
            {
                Console.WriteLine("  2. Pagar fianza ($50) o usar carta para salir de la cárcel");
            }
            if (yaTiroDados && jugador.ObtenerCasillaActual() is Propiedad prop && !prop.TienePropietario() && jugador.Dinero >= prop.PrecioCompra)
            {
                Console.WriteLine($"  3. Comprar la propiedad actual ({prop.Nombre} por ${prop.PrecioCompra})");
            }
            if (jugador.ObtenerCasillaActual() is Propiedad miProp && miProp.Propietario == jugador && miProp.CantidadCasas < 5)
            {
                Console.WriteLine($"  4. Construir casa/hotel en {miProp.Nombre} (Costo: ${miProp.PrecioCompra / 2})");
            }
            Console.WriteLine("  5. Ver mi estado y propiedades");
            Console.WriteLine("  6. Ver tablero completo");
            Console.WriteLine("  7. Terminar turno");
            Console.Write("Selecciona una opción: ");

            string? eleccion = Console.ReadLine()?.Trim();

            switch (eleccion)
            {
                case "1":
                    if (yaTiroDados)
                    {
                        Console.WriteLine("⚠️ Ya tiraste los dados en este turno.");
                    }
                    else
                    {
                        if (dadosHardware != null && dadosHardware.EstaConectado())
                        {
                            Console.WriteLine("Esperando tirada desde la Pico 2 W...");
                            int casillasPico = -1;
                            while (casillasPico < 0)
                            {
                                casillasPico = dadosHardware.LeerCasillas();
                            }
                            Console.WriteLine($"🎲 Tirada obtenida de la Pico: {casillasPico}");
                            juego.MoverJugadorCasillas(jugador, casillasPico);
                        }
                        else
                        {
                            juego.TirarDados(jugador);
                        }
                        yaTiroDados = true;
                    }
                    break;

                case "2":
                    juego.SalirDeCarcelConPago(jugador);
                    break;

                case "3":
                    juego.ComprarPropiedad(jugador);
                    break;

                case "4":
                    juego.ComprarCasa(jugador);
                    break;

                case "5":
                    juego.VerEstado(jugador);
                    break;

                case "6":
                    juego.VerTablero(jugador);
                    break;

                case "7":
                    if (!yaTiroDados && !jugador.PierdeSiguienteTurno)
                    {
                        Console.WriteLine("⚠️ Debes tirar los dados antes de terminar tu turno.");
                    }
                    else
                    {
                        turnoTerminado = true;
                    }
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }

            // Si el jugador cayó en bancarrota durante una acción
            if (jugador.Dinero <= 0)
            {
                Console.WriteLine($"\n💀 {jugador.Nombre} ha quedado eliminado por bancarrota.");
                turnoTerminado = true;
            }
        }
    }

    // Cuenta cuántos jugadores aún tienen dinero positivo
    private static int ContarJugadoresActivos(JuegoMonopoly juego)
    {
        int activos = 0;
        Node? actual = juego.Jugadores.GetHead();
        for (int i = 0; i < juego.Jugadores.Size(); i++)
        {
            if (actual?.GetData() is Jugador j && j.Dinero > 0)
            {
                activos++;
            }
            actual = actual?.GetNext();
        }
        return activos;
    }

    // Encuentra al último jugador en pie
    private static Jugador? ObtenerGanador(JuegoMonopoly juego)
    {
        Node? actual = juego.Jugadores.GetHead();
        for (int i = 0; i < juego.Jugadores.Size(); i++)
        {
            if (actual?.GetData() is Jugador j && j.Dinero > 0)
            {
                return j;
            }
            actual = actual?.GetNext();
        }
        return null;
    }
}

