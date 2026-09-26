using System;
using System.Net.Sockets;
using System.Threading;

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
        Console.WriteLine($"Tablero cargado con {juego.GetTablero().Size()} casillas en lista circular.\n");

        // Arrancamos el servidor para Godot en un hilo aparte, para que la
        // consola pueda seguir pidiendo cosas normal sin quedar bloqueada
        // esperando conexiones. Por ahora Godot solo VE la partida, no manda
        // nada (eso lo conectamos despues con los botones de la UI)
        Thread hiloGodot = new Thread(() => IniciarServidorGodot(juego));
        hiloGodot.IsBackground = true;
        hiloGodot.Start();

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
        if (juego.GetJugadores().Size() == 0)
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

                Jugador nuevo = new Jugador(i, nombre, balance_inicial: 1500);
                nuevo.SetPosicion(juego.GetTablero().GetHead());
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
                juego.SetTurnoActual(turnoGlobal);

                // Recorremos la lista enlazada de jugadores activos
                Node? nodoJugador = juego.GetJugadores().GetHead();
                int totalJugadores = juego.GetJugadores().Size();

                for (int idx = 0; idx < totalJugadores; idx++)
                {
                    if (nodoJugador?.GetData() is Jugador jugadorActual)
                    {
                        if (jugadorActual.GetDinero() <= 0)
                        {
                            nodoJugador = nodoJugador?.GetNext();
                            continue;
                        }

                        if (ContarJugadoresActivos(juego) <= 1)
                        {
                            break;
                        }

                        juego.AnunciarTurno(jugadorActual);
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
                Console.WriteLine($"🏆 ¡FELICITACIONES, {ganador.GetNombre().ToUpper()}! ¡HAS GANADO LA PARTIDA!");
                Console.WriteLine($"Saldo final: ${ganador.GetDinero()} | Propiedades: {ganador.GetPropiedades().Size()}");
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
        Console.WriteLine($"🎲 TURNO #{juego.GetTurnoActual()} DE: {jugador.GetNombre().ToUpper()}");
        Console.WriteLine($"Saldo: ${jugador.GetDinero()} | Posición: {jugador.ObtenerCasillaActual()?.GetNombre() ?? "Salida"}");
        if (jugador.GetEnCarcel())
        {
            Console.WriteLine($"🔒 ¡Estás en la Cárcel! (Turnos cumplidos: {jugador.GetTurnosEnCarcel()}/3)");
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
            if (jugador.GetEnCarcel())
            {
                Console.WriteLine("  2. Pagar fianza ($50) o usar carta para salir de la cárcel");
            }
            if (yaTiroDados && jugador.ObtenerCasillaActual() is Propiedad prop && !prop.TienePropietario() && jugador.GetDinero() >= prop.GetPrecioCompra())
            {
                Console.WriteLine($"  3. Comprar la propiedad actual ({prop.GetNombre()} por ${prop.GetPrecioCompra()})");
            }
            if (jugador.ObtenerCasillaActual() is Propiedad miProp && miProp.GetPropietario() == jugador && miProp.GetCantidadCasas() < 5)
            {
                Console.WriteLine($"  4. Construir casa/hotel en {miProp.GetNombre()} (Costo: ${miProp.GetPrecioCompra() / 2})");
            }
            Console.WriteLine("  5. Ver mi estado y propiedades");
            Console.WriteLine("  6. Ver tablero completo");
            Console.WriteLine("  7. Terminar turno");
            if (jugador.GetPropiedades().Size() > 0)
            {
                Console.WriteLine("  8. Gestionar mis propiedades (construir/vender/hipotecar/deshipotecar)");
            }
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
                    if (!yaTiroDados && !jugador.GetPierdeSiguienteTurno())
                    {
                        Console.WriteLine("⚠️ Debes tirar los dados antes de terminar tu turno.");
                    }
                    else
                    {
                        turnoTerminado = true;
                    }
                    break;

                case "8":
                    GestionarPropiedades(juego, jugador);
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }

            // Actualiza el HUD de dinero de Godot despues de CUALQUIER accion
            juego.AnunciarDinero(jugador);

            // Si el jugador cayó en bancarrota durante una acción
            if (jugador.GetDinero() <= 0)
            {
                Console.WriteLine($"\n💀 {jugador.GetNombre()} ha quedado eliminado por bancarrota.");
                juego.NotificarEliminacion(jugador);
                turnoTerminado = true;
            }
        }
    }

    // Submenú para hipotecar, deshipotecar, construir o vender casas en
    // CUALQUIERA de las propiedades del jugador, no solo la que está pisando
    private static void GestionarPropiedades(JuegoMonopoly juego, Jugador jugador)
    {
        if (jugador.GetPropiedades().Size() == 0)
        {
            Console.WriteLine("Todavía no tienes ninguna propiedad.");
            return;
        }

        Propiedad? elegida = SeleccionarPropiedad(jugador);
        if (elegida == null)
        {
            Console.WriteLine("Cancelado.");
            return;
        }

        string nivelActual = elegida.GetCantidadCasas() == 5 ? "Hotel" : elegida.GetCantidadCasas().ToString();
        Console.WriteLine($"\n-- {elegida.GetNombre()} --");
        Console.WriteLine($"Precio: ${elegida.GetPrecioCompra()} | Renta actual: ${elegida.CalcularRenta()} | Casas: {nivelActual} | Hipotecada: {(elegida.GetIsHipotecada() ? "Sí" : "No")}");
        Console.WriteLine("  1. Comprar casa/hotel");
        Console.WriteLine("  2. Vender casa/hotel");
        Console.WriteLine("  3. Hipotecar");
        Console.WriteLine("  4. Deshipotecar");
        Console.WriteLine("  5. Cancelar");
        Console.Write("Opción: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                juego.ComprarCasa(jugador, elegida);
                break;
            case "2":
                juego.VenderCasa(jugador, elegida);
                break;
            case "3":
                juego.HipotecarPropiedad(jugador, elegida);
                break;
            case "4":
                juego.DeshipotecarPropiedad(jugador, elegida);
                break;
            default:
                Console.WriteLine("Cancelado.");
                break;
        }
    }

    // Muestra la lista de propiedades del jugador numerada y devuelve la que elija.
    // Usa GetDataNode de la propia LinkedList del jugador, no un List de C#
    private static Propiedad? SeleccionarPropiedad(Jugador jugador)
    {
        int total = jugador.GetPropiedades().Size();

        Console.WriteLine("\nTus propiedades:");
        Node? temp = jugador.GetPropiedades().GetHead();
        for (int i = 0; i < total; i++)
        {
            if (temp?.GetData() is Propiedad p)
            {
                string nivel = p.GetCantidadCasas() == 5 ? "Hotel" : $"{p.GetCantidadCasas()} casas";
                string hip = p.GetIsHipotecada() ? " [HIPOTECADA]" : "";
                Console.WriteLine($"  {i + 1}. {p.GetNombre()} ({nivel}){hip}");
            }
            temp = temp?.GetNext();
        }

        Console.Write("Elige el número de la propiedad (0 para cancelar): ");
        if (!int.TryParse(Console.ReadLine(), out int idx) || idx <= 0 || idx > total)
        {
            return null;
        }

        return jugador.GetPropiedades().GetDataNode(idx - 1) as Propiedad;
    }

    // Cuenta cuántos jugadores aún tienen dinero positivo
    private static int ContarJugadoresActivos(JuegoMonopoly juego)
    {
        int activos = 0;
        Node? actual = juego.GetJugadores().GetHead();
        for (int i = 0; i < juego.GetJugadores().Size(); i++)
        {
            if (actual?.GetData() is Jugador j && j.GetDinero() > 0)
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
        Node? actual = juego.GetJugadores().GetHead();
        for (int i = 0; i < juego.GetJugadores().Size(); i++)
        {
            if (actual?.GetData() is Jugador j && j.GetDinero() > 0)
            {
                return j;
            }
            actual = actual?.GetNext();
        }
        return null;
    }

    // Se queda esperando conexiones de instancias de Godot y las va agregando
    // como espectadores. Corre en su propio hilo, separado de la consola
    private static void IniciarServidorGodot(JuegoMonopoly juego)
    {
        const int puertoGodot = 6767;
        TcpListener listener = new TcpListener(System.Net.IPAddress.Any, puertoGodot);
        listener.Start();
        Console.WriteLine($"[GODOT] Escuchando en el puerto {puertoGodot}, esperando a que se conecte la UI...\n");

        while (true)
        {
            TcpClient cliente = listener.AcceptTcpClient();
            var writer = new System.IO.StreamWriter(cliente.GetStream(), System.Text.Encoding.UTF8) { AutoFlush = true };
            juego.ConectarEspectadorGodot(writer);
            Console.WriteLine("[GODOT] Se conecto una instancia de Godot.");
        }
    }
}