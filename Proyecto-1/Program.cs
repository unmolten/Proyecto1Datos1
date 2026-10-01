using System;
using System.Net.Sockets;
using System.Threading;

/**
 * @file Program.cs
 * @brief Punto de entrada principal y bucle del servidor para Monopoly.
 *
 * Configura los modos de juego (Consola o Hardware con Raspberry Pi Pico),
 * levanta el servidor de red TCP en un hilo secundario para los clientes de Godot,
 * gestiona el flujo de turnos interactivo y concluye con el reporte de transacciones.
 */

/**
 * @class Program
 * @brief Clase interna que alberga la funcion Main y la orquestacion de la partida.
 */
internal class Program
{
    /**
     * @brief Metodo de entrada principal de la aplicacion de servidor.
     */
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==========================================================");
        Console.WriteLine("               MONOPOLY - EDICION AYEDD                   ");
        Console.WriteLine("==========================================================");

        var juego = JuegoMonopoly.Instancia;
        Console.WriteLine($"Tablero cargado con {juego.GetTablero().Size()} casillas en lista circular.\n");

        // Servidor para interfaces graficas (Godot) en un hilo aparte
        Thread hiloGodot = new Thread(() => IniciarServidorGodot(juego));
        hiloGodot.IsBackground = true;
        hiloGodot.Start();

        Console.WriteLine("Seleccione el modo de juego:");
        Console.WriteLine("1. Modo Consola (Registro de jugadores manual)");
        Console.WriteLine("2. Modo Hardware (Raspberry Pi Pico: RFID + Dados)");
        Console.Write("Opcion (1 o 2, por defecto 1): ");
        string? opcionModo = Console.ReadLine();

        ConexionPico? conexion = null;
        ControlDados? dadosHardware = null;

        if (opcionModo == "2")
        {
            string puertoDefault = DetectarPuertoPico();
            string[] puertosDetectados = System.IO.Ports.SerialPort.GetPortNames();
            Console.WriteLine($"\n[Hardware] Puertos COM detectados: {string.Join(", ", puertosDetectados)}");
            Console.Write($"Ingrese el puerto serial de la Pico (Enter para {puertoDefault}): ");
            string? puerto = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(puerto)) puerto = puertoDefault;
            puerto = puerto.Trim().ToUpper();

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

        // Si no se uso la Pico o fallo la conexion, registrar participantes por consola
        if (juego.GetJugadores().Size() == 0)
        {
            Console.Write("\n¿Cuantos jugadores participaran? (2-4): ");
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

        juego.ConfigurarHardware(conexion, dadosHardware);
        IniciarLectorConsola(juego);

        Console.WriteLine("\n==========================================================");
        Console.WriteLine("                 ¡COMIENZA LA PARTIDA!                    ");
        Console.WriteLine("==========================================================\n");

        try
        {
            int turnoGlobal = 1;
            int turnoGlobalMax = 20;

            // Bucle principal de la partida mientras queden al menos dos competidores con saldo o se llegue al maximo de turnos definidos
            while (ContarJugadoresActivos(juego) > 1 || turnoGlobal >= turnoGlobalMax)
            {
                juego.SetTurnoActual(turnoGlobal);

                // Recorrido de la lista enlazada de jugadores
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

            // Anuncio del participante victorioso
            Jugador? ganador = ObtenerGanador(juego);
            Console.WriteLine("\n==========================================================");
            if (ganador != null)
            {
                Console.WriteLine($"[VICTORIA] ¡FELICITACIONES, {ganador.GetNombre().ToUpper()}! ¡HAS GANADO LA PARTIDA!");
                Console.WriteLine($"Saldo final: ${ganador.GetDinero()} | Propiedades: {ganador.GetPropiedades().Size()}");
            }
            else
            {
                Console.WriteLine("La partida ha terminado.");
            }
            Console.WriteLine("==========================================================\n");

            // Persistencia del reporte general de transacciones
            Transaccion.ImprimirTransacciones();
            Console.WriteLine("[REPORTE] Se ha generado el reporte de transacciones en 'Reporte.txt'.");
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

    /**
     * @brief Ejecuta el ciclo de decisiones y acciones correspondientes al turno de un jugador.
     * @param juego Instancia del controlador global del juego.
     * @param jugador Participante que posee el turno.
     * @param dadosHardware Controlador de dados fisicos (si esta disponible).
     */
    private static void EjecutarTurnoJugador(JuegoMonopoly juego, Jugador jugador, ControlDados? dadosHardware)
    {
        Console.WriteLine("\n----------------------------------------------------------");
        Console.WriteLine($"[TURNO] #{juego.GetTurnoActual()} DE: {jugador.GetNombre().ToUpper()}");
        Console.WriteLine($"Saldo: ${jugador.GetDinero()} | Posicion: {jugador.ObtenerCasillaActual()?.GetNombre() ?? "Salida"}");
        if (jugador.GetEnCarcel())
        {
            Console.WriteLine($"[CARCEL] ¡Estas en la Carcel! (Turnos cumplidos: {jugador.GetTurnosEnCarcel()}/3)");
        }
        Console.WriteLine("----------------------------------------------------------");

        bool turnoTerminado = false;
        bool yaTiroDados = false;

        juego.LimpiarColaAcciones();
        dadosHardware?.IniciarNuevoTurno();

        while (!turnoTerminado)
        {
            juego.EnviarEstadoAcciones(jugador, yaTiroDados);

            Console.WriteLine("\nAcciones disponibles (Consola o UI de Godot):");
            if (!yaTiroDados)
            {
                Console.WriteLine("  1. Tirar dados y avanzar (o presiona el boton fisico en la Pico)");
            }
            if (jugador.GetEnCarcel())
            {
                Console.WriteLine("  2. Pagar fianza ($50) o usar carta para salir de la carcel");
            }
            if (yaTiroDados && jugador.ObtenerCasillaActual() is Propiedad prop && !prop.TienePropietario())
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
            Console.WriteLine("  9. Buscar transacciones (comando: buscar <todos|origen|destino|tipo> [valor] [antiguo|reciente])");
            Console.WriteLine(" 10. Imprimir transacciones en Reporte.txt");
            Console.WriteLine(" 11. Imprimir todas las transacciones en terminal");
            Console.Write("Esperando accion (Consola, Godot o boton en la Pico)...: ");

            AccionTurno? accion = null;
            while (accion == null)
            {
                accion = juego.IntentarObtenerAccion(jugador);
                if (accion != null) break;

                if (!yaTiroDados && dadosHardware != null && dadosHardware.EstaConectado())
                {
                    int tiradaBoton = dadosHardware.LeerCasillas();
                    if (tiradaBoton > 0)
                    {
                        accion = new AccionTurno { Tipo = "tirar", CasillaIndex = tiradaBoton };
                        break;
                    }
                }

                Thread.Sleep(50);
            }

            Console.WriteLine($"\n[ACCION RECIBIDA: {accion.Tipo.ToUpper()}]");

            string tipoAccion = accion.Tipo.Trim();
            string[] partesAccion = tipoAccion.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string comandoAccion = partesAccion.Length > 0 ? partesAccion[0].ToLowerInvariant() : "";
            switch (comandoAccion)
            {
                case "1":
                case "tirar":
                    if (yaTiroDados)
                    {
                        Console.WriteLine("[AVISO] Ya tiraste los dados en este turno.");
                    }
                    else
                    {
                        int casillasPico = (accion.CasillaIndex > 0) ? accion.CasillaIndex : -1;
                        int d1 = dadosHardware?.GetUltimoDado1() ?? -1;
                        int d2 = dadosHardware?.GetUltimoDado2() ?? -1;

                        if (casillasPico <= 0 && dadosHardware != null && dadosHardware.EstaConectado())
                        {
                            Console.WriteLine("[DADOS] Esperando tirada desde la Pico (presiona el boton en la Pico, o escribe 'v' para tirada virtual)...");
                            while (casillasPico < 0)
                            {
                                casillasPico = dadosHardware.LeerCasillas();
                                if (casillasPico > 0)
                                {
                                    d1 = dadosHardware.GetUltimoDado1();
                                    d2 = dadosHardware.GetUltimoDado2();
                                    break;
                                }

                                AccionTurno? accionManual = juego.IntentarObtenerAccion(jugador);
                                if (accionManual != null)
                                {
                                    if (int.TryParse(accionManual.Tipo, out int numManual) && numManual >= 1 && numManual <= 12)
                                    {
                                        casillasPico = numManual;
                                        Console.WriteLine($"[DADOS] Tirada manual por consola ingresada: {casillasPico}");
                                        break;
                                    }
                                    else if (accionManual.Tipo.Equals("v", StringComparison.OrdinalIgnoreCase) ||
                                             accionManual.Tipo.Equals("tirar", StringComparison.OrdinalIgnoreCase) ||
                                             accionManual.Tipo.Equals("1", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Console.WriteLine("[DADOS] Usando tirada virtual de respaldo...");
                                        casillasPico = -999;
                                        break;
                                    }
                                }

                                Thread.Sleep(50);
                            }
                        }

                        if (casillasPico == -999)
                        {
                            juego.TirarDados(jugador);
                        }
                        else if (casillasPico > 0)
                        {
                            Console.WriteLine($"[DADOS] Tirada fisica procesada: [{d1}] + [{d2}] = {casillasPico}");
                            juego.TirarDados(jugador, casillasPico, d1, d2);
                        }
                        else
                        {
                            juego.TirarDados(jugador);
                        }
                        yaTiroDados = true;
                    }
                    break;

                case "2":
                case "salircarcel":
                    juego.SalirDeCarcelConPago(jugador);
                    break;

                case "3":
                case "comprar":
                    juego.ComprarPropiedad(jugador);
                    break;

                case "4":
                case "comprarcasa":
                    if (accion.CasillaIndex >= 0 && juego.ObtenerPropiedadPorIndex(accion.CasillaIndex) is Propiedad propDirecta)
                    {
                        juego.ComprarCasa(jugador, propDirecta);
                    }
                    else
                    {
                        juego.ComprarCasa(jugador);
                    }
                    break;

                case "vendercasa":
                    if (accion.CasillaIndex >= 0 && juego.ObtenerPropiedadPorIndex(accion.CasillaIndex) is Propiedad propVender)
                    {
                        juego.VenderCasa(jugador, propVender);
                    }
                    break;

                case "hipotecar":
                    if (accion.CasillaIndex >= 0 && juego.ObtenerPropiedadPorIndex(accion.CasillaIndex) is Propiedad propHip)
                    {
                        juego.HipotecarPropiedad(jugador, propHip);
                    }
                    break;

                case "deshipotecar":
                    if (accion.CasillaIndex >= 0 && juego.ObtenerPropiedadPorIndex(accion.CasillaIndex) is Propiedad propDeship)
                    {
                        juego.DeshipotecarPropiedad(jugador, propDeship);
                    }
                    break;

                case "5":
                    juego.VerEstado(jugador);
                    break;

                case "6":
                    juego.VerTablero(jugador);
                    break;

                case "7":
                case "terminar":
                case "terminarturno":
                    if (!yaTiroDados && !jugador.GetPierdeSiguienteTurno())
                    {
                        Console.WriteLine("[AVISO] Debes tirar los dados antes de terminar tu turno.");
                    }
                    else
                    {
                        turnoTerminado = true;
                    }
                    break;

                case "8":
                    GestionarPropiedades(juego, jugador);
                    break;

                case "9":
                case "buscar":
                    BuscarTransacciones(tipoAccion);
                    break;

                case "10":
                    Transaccion.ImprimirTransacciones();
                    Console.WriteLine("[REPORTE] Transacciones agregadas a 'Reporte.txt'.");
                    break;

                case "11":
                    Transaccion.BuscarTransaccion(null, "", "RecienteAAntiguo");
                    break;

                default:
                    Console.WriteLine("Opcion no valida.");
                    break;
            }

            juego.EnviarEstadoAcciones(jugador, yaTiroDados);
            juego.AnunciarDinero(jugador);

            if (jugador.GetDinero() <= 0)
            {
                Console.WriteLine($"\n[ELIMINACION] {jugador.GetNombre()} ha quedado eliminado por bancarrota.");
                juego.NotificarEliminacion(jugador);
                turnoTerminado = true;
            }
        }

        dadosHardware?.LimpiarTiradasPrevias();
        juego.LimpiarColaAcciones();
    }

    /**
     * @brief Busca transacciones usando un comando de una sola linea recibido por consola.
     * @param comando Texto con formato "buscar <campo> [valor] [antiguo|reciente]" o "9 <campo> [valor] [antiguo|reciente]".
     */
    private static void BuscarTransacciones(string comando)
    {
        string[] partes = comando.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length < 2)
        {
            Console.WriteLine("Uso: buscar <todos|origen|destino|tipo> [valor] [antiguo|reciente]");
            return;
        }

        string campo = partes[1].ToLowerInvariant();
        string ordenar = "RecienteAAntiguo";
        int cantidadArgumentos = partes.Length;
        if (partes.Length > 2)
        {
            string ordenSolicitado = partes[^1].ToLowerInvariant();
            if (ordenSolicitado is "antiguo" or "antiguoareciente" or "antiguo-a-reciente")
            {
                ordenar = "AntiguoAReciente";
                cantidadArgumentos--;
            }
            else if (ordenSolicitado is "reciente" or "recienteaantiguo" or "reciente-a-antiguo")
            {
                ordenar = "RecienteAAntiguo";
                cantidadArgumentos--;
            }
        }

        if (campo == "todos")
        {
            if (cantidadArgumentos != 2)
            {
                Console.WriteLine("Uso: buscar todos [antiguo|reciente]");
                return;
            }

            Transaccion.BuscarTransaccion(null, "", ordenar);
            return;
        }

        string? atributo = campo switch
        {
            "origen" or "jugadororigen" => "jugadorOrigen",
            "destino" or "jugadordestino" => "jugadorDestino",
            "tipo" => "tipo",
            _ => null
        };

        if (atributo == null || cantidadArgumentos < 3)
        {
            Console.WriteLine("Uso: buscar <todos|origen|destino|tipo> [valor] [antiguo|reciente]");
            return;
        }

        string valor = string.Join(' ', partes[2..cantidadArgumentos]);
        Transaccion.BuscarTransaccion(atributo, valor, ordenar);
    }

    /**
     * @brief Despliega el menu para hipotecar, deshipotecar, edificar o vender en propiedades propias.
     * @param juego Instancia del juego.
     * @param jugador Participante en turno.
     */
    private static void GestionarPropiedades(JuegoMonopoly juego, Jugador jugador)
    {
        if (jugador.GetPropiedades().Size() == 0)
        {
            Console.WriteLine("Todavia no tienes ninguna propiedad.");
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
        Console.WriteLine($"Precio: ${elegida.GetPrecioCompra()} | Renta actual: ${elegida.CalcularRenta()} | Casas: {nivelActual} | Hipotecada: {(elegida.GetIsHipotecada() ? "Si" : "No")}");
        Console.WriteLine("  1. Comprar casa/hotel");
        Console.WriteLine("  2. Vender casa/hotel");
        Console.WriteLine("  3. Hipotecar");
        Console.WriteLine("  4. Deshipotecar");
        Console.WriteLine("  5. Cancelar");
        Console.Write("Opcion: ");

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

    /**
     * @brief Presenta la lista de propiedades adquiridas por el participante y permite elegir una.
     * @param jugador Participante a consultar.
     * @return La propiedad elegida o null si cancela.
     */
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

        Console.Write("Elige el numero de la propiedad (0 para cancelar): ");
        if (!int.TryParse(Console.ReadLine(), out int idx) || idx <= 0 || idx > total)
        {
            return null;
        }

        return jugador.GetPropiedades().GetDataNode(idx - 1) as Propiedad;
    }

    /**
     * @brief Determina la cantidad de participantes con saldo disponible en la partida.
     * @param juego Instancia del juego.
     * @return Conteo de participantes con dinero > 0.
     */
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

    /**
     * @brief Localiza al ultimo participante con saldo solvente al finalizar la partida.
     * @param juego Instancia del juego.
     * @return El jugador ganador, o null si ninguno califica.
     */
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

    /**
     * @brief Inicia el servidor socket TCP para la conexion y difusion hacia clientes de Godot.
     * @param juego Instancia de JuegoMonopoly a vincular.
     */
    private static void IniciarServidorGodot(JuegoMonopoly juego)
    {
        const int puertoGodot = 6767;
        TcpListener listener = new TcpListener(System.Net.IPAddress.Any, puertoGodot);
        listener.Start();

        Console.WriteLine("\n==========================================================");
        Console.WriteLine($"[GODOT] Servidor de espectadores activo en el puerto {puertoGodot}");
        Console.WriteLine("        - En esta misma PC: 127.0.0.1");

        LinkedList ips = ObtenerIpsLocales();
        if (!ips.IsEmpty())
        {
            Console.WriteLine("        - Para otras PCs en la misma red o VPN:");
            Node? nodoIp = ips.GetHead();
            for (int idx = 0; idx < ips.Size(); idx++)
            {
                Console.WriteLine($"          -> IP: {(string)nodoIp!.GetData()}");
                nodoIp = nodoIp.GetNext();
            }
        }
        Console.WriteLine("==========================================================\n");

        while (true)
        {
            try
            {
                TcpClient cliente = listener.AcceptTcpClient();
                string endpoint = cliente.Client.RemoteEndPoint?.ToString() ?? "desconocido";
                var writer = new System.IO.StreamWriter(cliente.GetStream(), System.Text.Encoding.UTF8) { AutoFlush = true };
                juego.ConectarEspectadorGodot(writer);
                Console.WriteLine($"[GODOT] Cliente conectado desde {endpoint}.");

                Thread hiloLectura = new Thread(() => LeerComandosCliente(cliente, juego));
                hiloLectura.IsBackground = true;
                hiloLectura.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GODOT] Error aceptando cliente: {ex.Message}");
            }
        }
    }

    /**
     * @brief Hilo de recepcion continua de comandos enviados desde un cliente de red TCP.
     * @param cliente Socket del cliente conectado.
     * @param juego Instancia del juego para despachar los comandos recibidos.
     */
    private static void LeerComandosCliente(TcpClient cliente, JuegoMonopoly juego)
    {
        try
        {
            using var reader = new System.IO.StreamReader(cliente.GetStream(), System.Text.Encoding.UTF8);
            string? linea;
            while ((linea = reader.ReadLine()) != null)
            {
                linea = linea.Trim();
                if (!string.IsNullOrEmpty(linea))
                {
                    juego.ProcesarComandoCliente(linea);
                }
            }
        }
        catch { }
    }

    /**
     * @brief Inicializa el hilo de lectura por consola para admitir entradas concurrentes.
     * @param juego Instancia del juego.
     */
    private static void IniciarLectorConsola(JuegoMonopoly juego)
    {
        Thread t = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    string? linea = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(linea))
                    {
                        linea = linea.Trim();
                        if (linea.Equals("p", StringComparison.OrdinalIgnoreCase) || linea.Equals("y", StringComparison.OrdinalIgnoreCase))
                        {
                            juego.ConfirmarPagoRfidConsola();
                        }
                        else
                        {
                            juego.EncolarAccion(new AccionTurno { Tipo = linea });
                        }
                    }
                }
                catch { break; }
            }
        });
        t.IsBackground = true;
        t.Start();
    }

    /**
     * @brief Enumera las direcciones IPv4 de las interfaces de red locales activas.
     * @return LinkedList conteniendo las cadenas con las direcciones IP.
     */
    private static LinkedList ObtenerIpsLocales()
    {
        LinkedList ips = new LinkedList();
        try
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                    ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ips.InsertEnd(ip.Address.ToString());
                        }
                    }
                }
            }
        }
        catch { }
        return ips;
    }

    /**
     * @brief Busca e identifica el puerto COM asignado a la Raspberry Pi Pico.
     * @return Nombre del puerto detectado o "COM4" por defecto.
     */
    private static string DetectarPuertoPico()
    {
        try
        {
            string[] puertos = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var p in puertos)
            {
                if (p.Equals("COM4", StringComparison.OrdinalIgnoreCase)) return p;
            }
            if (puertos.Length > 0)
            {
                return puertos[0];
            }
        }
        catch { }
        return "COM4";
    }
}