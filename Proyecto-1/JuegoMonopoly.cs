using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

/**
 * @file JuegoMonopoly.cs
 * @brief Nucleo de logica y coordinacion del juego Monopoly.
 *
 * Administra el estado completo de la partida:
 * - El tablero como una lista circular doblemente enlazada (LinkedList + MakeCircular()).
 * - Los participantes en una lista enlazada (LinkedList).
 * - Los mazos de cartas (Fortuna y Arca Comunal) como listas circulares rotativas.
 * - La comunicacion y difusion hacia espectadores e interfaces graficas (Godot).
 * - La integracion con hardware externo (Raspberry Pi Pico: RFID y dados).
 */

/**
 * @class AccionTurno
 * @brief Representa una solicitud de accion recibida desde la UI de Godot o la consola.
 */
public class AccionTurno
{
    /** @brief Tipo de accion solicitada ("tirar", "comprar", "comprarcasa", "terminar", etc.). */
    public string Tipo { get; set; } = "";

    /** @brief Indice opcional de casilla o parametro numerico asociado a la accion. */
    public int CasillaIndex { get; set; } = -1;

    /** @brief Identificador del jugador emisor de la accion. */
    public int JugadorId { get; set; } = 0;
}

/**
 * @class JuegoMonopoly
 * @brief Clase controladora principal implementada bajo el patron de diseno Singleton.
 *
 * Conecta las casillas del tablero, los mazos de eventos, las transacciones financieras
 * y las estructuras de datos enlazadas requeridas por la arquitectura del proyecto.
 */
public class JuegoMonopoly
{
    /** @brief Instancia unica de la clase para el patron Singleton. */
    private static JuegoMonopoly? instancia;

    /**
     * @brief Propiedad estatica para acceder a la instancia unica del juego.
     */
    public static JuegoMonopoly Instancia => instancia ??= new JuegoMonopoly();

    /** @brief Estructura de datos del tablero (Lista circular doblemente enlazada). */
    private LinkedList tablero;

    /** @brief Lista de jugadores registrados en la partida. */
    private LinkedList jugadores;

    /** @brief Mazo de cartas de evento de tipo Fortuna. */
    private LinkedList mazoFortuna;

    /** @brief Mazo de cartas de evento de tipo Arca Comunal. */
    private LinkedList mazoArcaComunal;

    /** @brief Contador global del turno en ejecucion. */
    private int turnoActual = 1;

    /** @brief Asignador incremental para los identificadores numericos de jugadores. */
    private int contadorJugadores = 1;

    /** @brief Generador de valores aleatorios para tiradas virtuales de dados. */
    private Random random = new Random();

    /**
     * @brief Obtiene el numero de turno global actual.
     * @return Entero con el turno en curso.
     */
    public int GetTurnoActual()
    {
        return this.turnoActual;
    }

    /**
     * @brief Establece el numero de turno global actual.
     * @param turnoActual Nuevo numero de turno.
     */
    public void SetTurnoActual(int turnoActual)
    {
        this.turnoActual = turnoActual;
    }

    /**
     * @brief Obtiene la lista enlazada del tablero de casillas.
     * @return LinkedList circular del tablero.
     */
    public LinkedList GetTablero()
    {
        return this.tablero;
    }

    /**
     * @brief Obtiene la lista enlazada de jugadores registrados.
     * @return LinkedList con los participantes.
     */
    public LinkedList GetJugadores()
    {
        return this.jugadores;
    }

    /**
     * @brief Obtiene el mazo circular de cartas de Fortuna.
     * @return LinkedList con la baraja de Fortuna.
     */
    public LinkedList GetMazoFortuna()
    {
        return this.mazoFortuna;
    }

    /**
     * @brief Obtiene el mazo circular de cartas de Arca Comunal.
     * @return LinkedList con la baraja de Arca Comunal.
     */
    public LinkedList GetMazoArcaComunal()
    {
        return this.mazoArcaComunal;
    }

    /**
     * @brief Constructor privado del Singleton. Inicializa estructuras, mazos y el tablero.
     */
    public JuegoMonopoly()
    {
        this.tablero = new LinkedList();
        this.jugadores = new LinkedList();
        this.mazoFortuna = MazoCartas.CrearMazoFortuna();
        this.mazoArcaComunal = MazoCartas.CrearMazoArcaComunal();
        InicializarTablero();
    }

    /**
     * @brief Crea las 32 casillas polimorficas del tablero y cierra la circularidad doble.
     */
    private void InicializarTablero()
    {
        this.tablero.InsertEnd(new CasillaEspecial(0, "Salida (GO)", "Salida"));
        this.tablero.InsertEnd(new Propiedad(1, "Casa de tierra", "Propiedad", 60, 5, colorGrupo: "Marron"));
        this.tablero.InsertEnd(new Propiedad(2, "Cueva provisional", "Propiedad", 60, 5, colorGrupo: "Marron"));
        this.tablero.InsertEnd(new CasillaEvento(3, "Arca Comunal", "ArcaComunal"));
        this.tablero.InsertEnd(new Propiedad(4, "Tren de la aldea", "Propiedad", 200, 25, colorGrupo: "Tren"));
        this.tablero.InsertEnd(new Propiedad(5, "Puesto de saqueador", "Propiedad", 100, 10, colorGrupo: "Celeste"));
        this.tablero.InsertEnd(new Propiedad(6, "Aldea esmeraldil", "Propiedad", 100, 10, colorGrupo: "Celeste"));
        this.tablero.InsertEnd(new CasillaEspecial(7, "Impuesto sobre la renta", "Impuesto"));
        this.tablero.InsertEnd(new CasillaEspecial(8, "Carcel", "Carcel"));
        this.tablero.InsertEnd(new Propiedad(9, "Geoda de amatista", "Propiedad", 140, 15, colorGrupo: "Rosa"));
        this.tablero.InsertEnd(new Propiedad(10, "Mina de oro", "Propiedad", 140, 15, colorGrupo: "Rosa"));
        this.tablero.InsertEnd(new CasillaEvento(11, "Fortuna", "Fortuna"));
        this.tablero.InsertEnd(new Propiedad(12, "Tren a las minas", "Propiedad", 200, 25, colorGrupo: "Tren"));
        this.tablero.InsertEnd(new Propiedad(13, "Runa oceanica", "Propiedad", 180, 20, colorGrupo: "Naranja"));
        this.tablero.InsertEnd(new Propiedad(14, "Barco hundido", "Propiedad", 180, 20, colorGrupo: "Naranja"));
        this.tablero.InsertEnd(new Propiedad(15, "Monumento oceanico", "Propiedad", 200, 24, colorGrupo: "Naranja"));
        this.tablero.InsertEnd(new CasillaEspecial(16, "Parada Libre", "ParadaLibre"));
        this.tablero.InsertEnd(new Propiedad(17, "Templo del desierto", "Propiedad", 220, 20, colorGrupo: "Rojo"));
        this.tablero.InsertEnd(new CasillaEvento(18, "Arca Comunal", "ArcaComunal"));
        this.tablero.InsertEnd(new Propiedad(19, "Trial Chamber", "Propiedad", 220, 20, colorGrupo: "Rojo"));
        this.tablero.InsertEnd(new Propiedad(20, "Tren a los portales", "Propiedad", 200, 25, colorGrupo: "Tren"));
        this.tablero.InsertEnd(new Propiedad(21, "Ciudad Antigua", "Propiedad", 240, 25, colorGrupo: "Amarillo"));
        this.tablero.InsertEnd(new Propiedad(22, "Portal al Nether", "Propiedad", 240, 25, colorGrupo: "Amarillo"));
        this.tablero.InsertEnd(new Propiedad(23, "Portal al End", "Propiedad", 260, 28, colorGrupo: "Amarillo"));
        this.tablero.InsertEnd(new CasillaEspecial(24, "Vaya a la carcel", "VayaALaCarcel"));
        this.tablero.InsertEnd(new Propiedad(25, "Charco de lava", "Propiedad", 280, 30, colorGrupo: "Verde"));
        this.tablero.InsertEnd(new Propiedad(26, "Fortaleza del Nether", "Propiedad", 280, 30, colorGrupo: "Verde"));
        this.tablero.InsertEnd(new Propiedad(27, "Bastion del Nether", "Propiedad", 300, 32, colorGrupo: "Verde"));
        this.tablero.InsertEnd(new Propiedad(28, "Tren a las Farlands", "Propiedad", 200, 25, colorGrupo: "Tren"));
        this.tablero.InsertEnd(new Propiedad(29, "Ciudad del End", "Propiedad", 350, 35, colorGrupo: "Azul"));
        this.tablero.InsertEnd(new CasillaEvento(30, "Fortuna", "Fortuna"));
        this.tablero.InsertEnd(new Propiedad(31, "Barco del End", "Propiedad", 350, 35, colorGrupo: "Azul"));

        // Asegura que el tablero sea completamente circular
        this.tablero.MakeCircular();
    }

    /**
     * @brief Registra a un nuevo cliente en la partida asignandole la posicion de Salida.
     * @param writer Flujo de salida de red correspondiente al socket del jugador.
     * @return La nueva instancia de Jugador creada.
     */
    public Jugador RegistrarJugador(StreamWriter writer)
    {
        Jugador nuevo = new Jugador(contadorJugadores, $"Jugador_{contadorJugadores}", writer);
        nuevo.SetPosicion(this.tablero.GetHead());
        contadorJugadores++;

        this.jugadores.InsertEnd(nuevo);
        Broadcast($"[SISTEMA] {nuevo.GetNombre()} se ha unido a la partida.");
        GodotBroadcast($"jugador/{nuevo.GetId()}/activar");
        return nuevo;
    }

    /**
     * @brief Registra un jugador ya instanciado (por RFID o consola) en la estructura del juego.
     * @param jugador Instancia de Jugador a registrar.
     */
    public void RegistrarJugadorExistente(Jugador jugador)
    {
        if (jugador.GetId() == 0)
        {
            jugador.SetId(contadorJugadores++);
        }
        if (jugador.GetPosicion() == null)
        {
            jugador.SetPosicion(this.tablero.GetHead());
        }
        this.jugadores.InsertEnd(jugador);
        Broadcast($"[SISTEMA] {jugador.GetNombre()} (ID: {jugador.GetId()}) esta listo en el tablero.");
        GodotBroadcast($"jugador/{jugador.GetId()}/activar");
    }

    /** @brief Lista de canales de salida hacia clientes espectadores de Godot conectados. */
    private List<StreamWriter> espectadoresGodot = new List<StreamWriter>();

    /** @brief Referencia al jugador que ostenta el turno en este momento. */
    private Jugador? jugadorEnTurno = null;

    /**
     * @brief Sincroniza el estado completo del juego con un cliente Godot recien conectado.
     *
     * Envia en orden:
     * 1. Propiedades y sus datos estaticos (precio, renta, grupo).
     * 2. Estado de dueños, casas construidas e hipotecas.
     * 3. Participantes y sus casillas de ubicacion.
     * 4. Turno actual y saldo del jugador activo.
     * @param writer Canal de transmision de red de la nueva sesion de Godot.
     */
    public void ConectarEspectadorGodot(StreamWriter writer)
    {
        lock (this.espectadoresGodot)
        {
            this.espectadoresGodot.Add(writer);
        }

        // 1. Datos estaticos de las propiedades
        Node? nodoDatos = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (nodoDatos?.GetData() is Propiedad prop)
            {
                EnviarAEspectador(writer, $"propiedad/{prop.GetPosicion()}/{prop.GetNombre()}/{prop.GetPrecioCompra()}/{prop.GetAlquilerBase()}/{prop.GetColorGrupo()}");
            }
            nodoDatos = nodoDatos?.GetNext();
        }

        // 2. Estado dinamico de cada propiedad (dueño, mejoras, hipotecas)
        Node? nodoCasilla = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (nodoCasilla?.GetData() is Propiedad p && p.GetPropietario() != null)
            {
                int dueñoId = p.GetPropietario()!.GetId();
                EnviarAEspectador(writer, $"jugador/{dueñoId}/comprar/casilla/{p.GetPosicion()}");

                if (p.GetCantidadCasas() > 0)
                {
                    EnviarAEspectador(writer, $"jugador/{dueñoId}/comprarcasa/{p.GetCantidadCasas()}/casilla/{p.GetPosicion()}");
                }

                if (p.GetIsHipotecada())
                {
                    EnviarAEspectador(writer, $"jugador/{dueñoId}/hipotecar/casilla/{p.GetPosicion()}");
                }
            }
            nodoCasilla = nodoCasilla?.GetNext();
        }

        // 3. Jugadores registrados y su posicion actual en el tablero
        Node? nodoJugador = this.jugadores.GetHead();
        for (int i = 0; i < this.jugadores.Size(); i++)
        {
            if (nodoJugador?.GetData() is Jugador j)
            {
                Casilla? casillaJugador = j.ObtenerCasillaActual();
                EnviarAEspectador(writer, $"jugador/{j.GetId()}/activar");
                if (casillaJugador != null)
                {
                    EnviarAEspectador(writer, $"jugador/{j.GetId()}/mover/casilla/{casillaJugador.GetPosicion()}");
                }
            }
            nodoJugador = nodoJugador?.GetNext();
        }

        // 4. Sincronizacion del turno activo y fondos
        if (this.jugadorEnTurno != null)
        {
            EnviarAEspectador(writer, $"jugador/{this.jugadorEnTurno.GetId()}/turno");
            EnviarAEspectador(writer, $"jugador/{this.jugadorEnTurno.GetId()}/dinero/{this.jugadorEnTurno.GetDinero()}");

            Casilla? cActual = this.jugadorEnTurno.ObtenerCasillaActual();
            bool puedeComprar = cActual is Propiedad pr && !pr.TienePropietario();
            int precio = (cActual is Propiedad pr2) ? pr2.GetPrecioCompra() : 0;
            string nom = cActual?.GetNombre() ?? "";

            bool puedeConstruir = false;
            int costoConstruir = 0;
            string nomConstruir = "";

            if (cActual is Propiedad miP && miP.GetPropietario() == this.jugadorEnTurno && miP.GetCantidadCasas() < 5 && miP.GetColorGrupo() != "Tren" && !miP.GetIsHipotecada() && TieneMonopolio(this.jugadorEnTurno, miP.GetColorGrupo()))
            {
                puedeConstruir = true;
                costoConstruir = miP.GetPrecioCompra() / 2;
                nomConstruir = miP.GetNombre();
            }

            EnviarAEspectador(writer, $"turno/acciones/1/{(puedeComprar ? 1 : 0)}/{precio}/{nom}/0/{(this.jugadorEnTurno.GetEnCarcel() ? 1 : 0)}/{(puedeConstruir ? 1 : 0)}/{costoConstruir}/{nomConstruir}");
        }
    }

    /**
     * @brief Transmite un comando de texto a un espectador individual de Godot.
     * @param writer Canal de salida del socket.
     * @param mensaje Cadena de protocolo.
     */
    private void EnviarAEspectador(StreamWriter writer, string mensaje)
    {
        try { writer.WriteLine(mensaje); } catch { }
    }

    /**
     * @brief Transmite un mensaje del protocolo a todas las ventanas e instancias de Godot activas.
     * @param mensaje Cadena formateada para el cliente grafico.
     */
    public void GodotBroadcast(string mensaje)
    {
        lock (this.espectadoresGodot)
        {
            for (int i = this.espectadoresGodot.Count - 1; i >= 0; i--)
            {
                try
                {
                    this.espectadoresGodot[i].WriteLine(mensaje);
                }
                catch
                {
                    this.espectadoresGodot.RemoveAt(i);
                }
            }
        }
    }

    /** @brief Referencia al gestor de la conexion serial con la Pico. */
    private ConexionPico? conexionPico;

    /** @brief Referencia al lector de tarjetas RFID. */
    private ObtenerID? lectorRfid;

    /** @brief Referencia al controlador de dados digitales de la Pico. */
    private ControlDados? controlDados;

    /**
     * @brief Configura las dependencias de hardware serial de la Raspberry Pi Pico.
     * @param conexion Instancia activa de ConexionPico.
     * @param dados Instancia de ControlDados (opcional).
     */
    public void ConfigurarHardware(ConexionPico? conexion, ControlDados? dados = null)
    {
        this.conexionPico = conexion;
        if (conexion != null)
        {
            this.lectorRfid = new ObtenerID(conexion);
            this.controlDados = dados ?? new ControlDados(conexion);
        }
    }

    /**
     * @brief Obtiene la conexion serial configurada con la Pico.
     * @return Instancia de ConexionPico o null si no se configuro.
     */
    public ConexionPico? GetConexionPico()
    {
        return this.conexionPico;
    }

    /**
     * @brief Obtiene el controlador de dados configurado.
     * @return Instancia de ControlDados o null si no se configuro.
     */
    public ControlDados? GetControlDados()
    {
        return this.controlDados;
    }

    /** @brief Cola FIFO de solicitudes de accion de turnos implementada sobre LinkedList. */
    private LinkedList colaAcciones = new LinkedList();

    /** @brief Objeto de bloqueo para exclusion mutua en la cola de acciones. */
    private readonly object colaLock = new object();

    /**
     * @brief Agrega una nueva accion a la cola e interactua con hilos en espera mediante Monitor.
     * @param accion Instancia de AccionTurno a encolar.
     */
    public void EncolarAccion(AccionTurno accion)
    {
        lock (colaLock)
        {
            colaAcciones.InsertEnd(accion);
            Monitor.Pulse(colaLock);
        }
    }

    /**
     * @brief Limpia todas las acciones pendientes en la cola.
     */
    public void LimpiarColaAcciones()
    {
        lock (colaLock)
        {
            while (!colaAcciones.IsEmpty())
            {
                colaAcciones.DeleteFirst();
            }
        }
    }

    /**
     * @brief Espera de forma bloqueante hasta que arribe una accion valida para el jugador en turno.
     * @param jugador Participante activo en el turno.
     * @return La AccionTurno recibida.
     */
    public AccionTurno EsperarAccion(Jugador jugador)
    {
        while (true)
        {
            AccionTurno accion;
            lock (colaLock)
            {
                while (colaAcciones.IsEmpty())
                {
                    Monitor.Wait(colaLock);
                }
                Node? nodo = colaAcciones.DeleteFirst();
                accion = (AccionTurno)nodo!.GetData();
            }
            if (accion.JugadorId == 0 || accion.JugadorId == jugador.GetId())
            {
                return accion;
            }
            else
            {
                Console.WriteLine($"[COLA] Descartada accion '{accion.Tipo}' del jugador {accion.JugadorId} (turno actual: {jugador.GetNombre()}).");
            }
        }
    }

    /**
     * @brief Intenta extraer de forma no bloqueante una accion de la cola si esta disponible.
     * @param jugador Participante activo en el turno.
     * @return La AccionTurno si habia una en cola; de lo contrario null.
     */
    public AccionTurno? IntentarObtenerAccion(Jugador jugador)
    {
        lock (colaLock)
        {
            if (colaAcciones.IsEmpty())
            {
                return null;
            }

            // Scan the queue for a matching action; discard stale actions from other players
            Node? nodo = colaAcciones.GetHead();
            int total = colaAcciones.Size();
            for (int i = 0; i < total; i++)
            {
                if (nodo?.GetData() is AccionTurno accion)
                {
                    if (accion.JugadorId == 0 || accion.JugadorId == jugador.GetId())
                    {
                        // Found a valid action — remove it and return
                        colaAcciones.Delete(accion);
                        return accion;
                    }
                    else
                    {
                        // Stale action from a different player — discard it to prevent head-blocking
                        Node? siguiente = nodo.GetNext();
                        colaAcciones.Delete(accion);
                        Console.WriteLine($"[COLA] Descartada accion '{accion.Tipo}' del jugador {accion.JugadorId} (turno actual: {jugador.GetNombre()}).");
                        nodo = siguiente;
                        total--;
                        i--;
                        continue;
                    }
                }
                nodo = nodo?.GetNext();
            }

            return null;
        }
    }

    /**
     * @brief Desglosa y procesa cadenas de accion enviadas desde la red (ej: "accion/1/tirar").
     * @param comando Cadena con formato delimitado por barras inclinadas.
     */
    public void ProcesarComandoCliente(string comando)
    {
        string[] partes = comando.Split('/');
        if (partes.Length < 3 || partes[0] != "accion")
        {
            return;
        }

        if (!int.TryParse(partes[1], out int jugadorId))
        {
            return;
        }

        string tipo = partes[2].ToLower();
        int arg = -1;
        if (partes.Length >= 4)
        {
            int.TryParse(partes[3], out arg);
        }

        EncolarAccion(new AccionTurno
        {
            JugadorId = jugadorId,
            Tipo = tipo,
            CasillaIndex = arg
        });
    }

    /** @brief Indicador volatil de confirmacion de cobro RFID. */
    private volatile bool pagoRfidConfirmado = false;

    /**
     * @brief Permite autorizar el pago por consola cuando se juega en modo sin hardware.
     */
    public void ConfirmarPagoRfidConsola()
    {
        bool hardwareDisponible = this.conexionPico != null && this.conexionPico.EstaConectado();
        if (!hardwareDisponible)
        {
            pagoRfidConfirmado = true;
        }
    }

    /**
     * @brief Bloquea la ejecucion hasta que el jugador valide la transaccion pasando su tarjeta RFID fisica.
     *
     * Compara estrictamente el UID leido con el UID registrado del participante.
     * Notifica a la interfaz de Godot para mostrar el modal de espera.
     * @param jugador Jugador que debe abonar el monto.
     * @param monto Importe a pagar.
     * @param concepto Motivo del cobro.
     */
    public void AutorizarPagoRFID(Jugador jugador, int monto, string concepto)
    {
        pagoRfidConfirmado = false;

        Console.WriteLine("\n==========================================================");
        Console.WriteLine("[PAGO RFID REQUERIDO]");
        Console.WriteLine($"   Jugador: {jugador.GetNombre()} (Saldo actual: ${jugador.GetDinero()})");
        Console.WriteLine($"   Monto a pagar: ${monto}");
        Console.WriteLine($"   Concepto: {concepto}");
        Console.WriteLine($"   -> {jugador.GetNombre()}, pasa tu tarjeta RFID por el lector para confirmar.");
        Console.WriteLine("==========================================================");

        // Notifica a Godot para que abra el modal
        GodotBroadcast($"pago/solicitar/{jugador.GetId()}/{monto}/{concepto}");

        bool hardwareDisponible = this.conexionPico != null && this.conexionPico.EstaConectado();
        string tarjetaJugador = jugador.GetIdTarjeta();
        bool tieneTarjeta = !string.IsNullOrEmpty(tarjetaJugador);

        if (hardwareDisponible && this.lectorRfid != null)
        {
            try
            {
                this.lectorRfid.IniciarLecturaTarjetas();
            }
            catch { }
        }

        // Bucle de espera bloqueante de la tarjeta RFID correcta
        while (!pagoRfidConfirmado)
        {
            if (hardwareDisponible && this.lectorRfid != null && tieneTarjeta)
            {
                try
                {
                    string uid = this.lectorRfid.LeerTarjeta();
                    if (!string.IsNullOrEmpty(uid))
                    {
                        if (uid.Equals(tarjetaJugador, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"[RFID] Tarjeta de {jugador.GetNombre()} detectada. UID: {uid}. Pago autorizado.");
                            pagoRfidConfirmado = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"[RFID] Tarjeta incorrecta (UID: {uid}). Se necesita la tarjeta de {jugador.GetNombre()}.");
                            GodotBroadcast($"pago/tarjetaincorrecta/{jugador.GetId()}");
                        }
                    }
                }
                catch { }
            }

            Thread.Sleep(50);
        }

        Console.WriteLine($"[PAGO APROBADO] Pago de ${monto} completado para {jugador.GetNombre()}.\n");
        GodotBroadcast($"pago/exito/{jugador.GetId()}/{monto}");
        Thread.Sleep(300);

        // Retorna la Pico a modo dados tras culminar el cobro
        if (hardwareDisponible && this.controlDados != null)
        {
            try
            {
                this.controlDados.IniciarModoDados();
            }
            catch { }
        }
    }

    /**
     * @brief Envia el estado de disponibilidad de los botones de accion a Godot para el turno vigente.
     * @param jugador Participante en turno.
     * @param yaTiroDados Indica si ya efectuo el lanzamiento de dados en este turno.
     */
    public void EnviarEstadoAcciones(Jugador jugador, bool yaTiroDados)
    {
        bool puedeTirar = !yaTiroDados && !jugador.GetPierdeSiguienteTurno();
        bool puedeComprar = false;
        int precioCompra = 0;
        string nombrePropiedad = "";

        Casilla? casilla = jugador.ObtenerCasillaActual();
        if (yaTiroDados && casilla is Propiedad prop && !prop.TienePropietario())
        {
            puedeComprar = true;
            precioCompra = prop.GetPrecioCompra();
            nombrePropiedad = prop.GetNombre();
        }

        bool puedeTerminar = yaTiroDados || jugador.GetPierdeSiguienteTurno();
        bool enCarcel = jugador.GetEnCarcel();

        bool puedeConstruir = false;
        int costoConstruir = 0;
        string nombreConstruir = "";
        if (casilla is Propiedad miProp && miProp.GetPropietario() == jugador && miProp.GetCantidadCasas() < 5 && miProp.GetColorGrupo() != "Tren" && !miProp.GetIsHipotecada() && TieneMonopolio(jugador, miProp.GetColorGrupo()))
        {
            puedeConstruir = true;
            costoConstruir = miProp.GetPrecioCompra() / 2;
            nombreConstruir = miProp.GetNombre();
        }

        GodotBroadcast($"turno/acciones/{(puedeTirar ? 1 : 0)}/{(puedeComprar ? 1 : 0)}/{precioCompra}/{nombrePropiedad}/{(puedeTerminar ? 1 : 0)}/{(enCarcel ? 1 : 0)}/{(puedeConstruir ? 1 : 0)}/{costoConstruir}/{nombreConstruir}");
    }

    /**
     * @brief Localiza una propiedad en la lista circular segun su numero de posicion.
     * @param index Indice de la casilla buscada (0 a 31).
     * @return Referencia a la Propiedad o null si no se encuentra o no es propiedad.
     */
    public Propiedad? ObtenerPropiedadPorIndex(int index)
    {
        Node? temp = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (temp?.GetData() is Propiedad p && p.GetPosicion() == index)
            {
                return p;
            }
            temp = temp?.GetNext();
        }
        return null;
    }

    /**
     * @brief Emite a los clientes de Godot la coordenada o casilla actual del jugador.
     * @param jugador Participante cuya posicion cambio.
     */
    private void AnunciarPosicion(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();
        if (actual != null)
        {
            GodotBroadcast($"jugador/{jugador.GetId()}/mover/casilla/{actual.GetPosicion()}");
        }
    }

    /**
     * @brief Notifica el cambio de turno a las interfaces de red y actualiza saldos.
     * @param jugador Participante que asume el turno.
     */
    public void AnunciarTurno(Jugador jugador)
    {
        this.jugadorEnTurno = jugador;
        GodotBroadcast($"jugador/{jugador.GetId()}/turno");
        AnunciarDinero(jugador);
    }

    /**
     * @brief Transmite el saldo actual del jugador a las interfaces conectadas.
     * @param jugador Participante a consultar.
     */
    public void AnunciarDinero(Jugador jugador)
    {
        GodotBroadcast($"jugador/{jugador.GetId()}/dinero/{jugador.GetDinero()}");
    }

    /**
     * @brief Envia el aviso de retiro por bancarrota para ocultar la ficha del jugador en pantalla.
     * @param jugador Participante eliminado.
     */
    public void NotificarEliminacion(Jugador jugador)
    {
        GodotBroadcast($"jugador/{jugador.GetId()}/desactivar");
    }

    /**
     * @brief Retira a un participante de la lista de jugadores conectados.
     * @param jugador Participante a remover.
     */
    public void DesconectarJugador(Jugador jugador)
    {
        this.jugadores.Delete(jugador);
        Broadcast($"[SISTEMA] {jugador.GetNombre()} ha salido de la partida.");
    }

    /**
     * @brief Emite un mensaje de texto general a todos los participantes conectados.
     * @param mensaje Contenido textual a difundir.
     * @param excluir Participante opcional a omitir de la difusion.
     */
    public void Broadcast(string mensaje, Jugador? excluir = null)
    {
        Node? actual = this.jugadores.GetHead();
        int total = this.jugadores.Size();

        for (int i = 0; i < total; i++)
        {
            if (actual?.GetData() is Jugador j)
            {
                if (excluir == null || j.GetId() != excluir.GetId())
                {
                    j.EnviarMensaje(mensaje);
                }
            }
            actual = actual?.GetNext();
        }
    }

    /**
     * @brief Ejecuta el lanzamiento de dados, avance en la lista circular y accion de la casilla.
     *
     * Valida penalizaciones de turno previo, comprueba reglas de liberacion o permanencia
     * en prision, otorga bono de $200 si pasa por la casilla de Salida (nodo 0), y activa
     * de forma polimorfica el metodo Accion() de la casilla destino.
     * @param jugador Participante que lanza los dados.
     * @param totalExterno Suma de dados proveniente de la Pico (-1 para generar virtual).
     * @param d1Externo Valor de dado 1 del hardware (-1 si no aplica).
     * @param d2Externo Valor de dado 2 del hardware (-1 si no aplica).
     */
    public void TirarDados(Jugador jugador, int totalExterno = -1, int d1Externo = -1, int d2Externo = -1)
    {
        // 1. Verifica si debe perder su turno por efecto de carta
        if (jugador.GetPierdeSiguienteTurno())
        {
            jugador.SetPierdeSiguienteTurno(false);
            jugador.EnviarMensaje("[AVISO] Perdiste este turno debido a un evento anterior.");
            Broadcast($"[AVISO] {jugador.GetNombre()} perdio su turno.", jugador);
            return;
        }

        // 2. Determinar valores de los dados
        bool esHardware = totalExterno > 0;
        int dado1;
        int dado2;
        int total;

        if (esHardware)
        {
            total = totalExterno;
            if (d1Externo > 0 && d2Externo > 0)
            {
                dado1 = d1Externo;
                dado2 = d2Externo;
            }
            else
            {
                dado1 = Math.Max(1, Math.Min(6, total / 2));
                dado2 = total - dado1;
            }
            jugador.EnviarMensaje($"[DADOS] Dados fisicos: [{dado1}] + [{dado2}] = {total} casillas.");
            Broadcast($"[DADOS] {jugador.GetNombre()} tiro dados fisicos: [{dado1}] + [{dado2}] = {total}", jugador);
        }
        else
        {
            dado1 = random.Next(1, 7);
            dado2 = random.Next(1, 7);
            total = dado1 + dado2;
            jugador.EnviarMensaje($"[DADOS] Tiraste: [{dado1}] + [{dado2}] = {total}");
            Broadcast($"[DADOS] {jugador.GetNombre()} tiro: [{dado1}] + [{dado2}] = {total}", jugador);
        }

        // Sincronizar con los displays de la Pico si hay hardware conectado
        if (this.controlDados != null && this.controlDados.EstaConectado())
        {
            try
            {
                this.controlDados.EnviarResultadoDados(dado1, dado2);
            }
            catch { }
        }

        // Notificar a Godot para que muestre visualmente los dados a todos los jugadores
        GodotBroadcast($"dados/{jugador.GetId()}/{dado1}/{dado2}/{total}");

        // 3. Manejo de estado en la carcel
        if (jugador.GetEnCarcel())
        {
            if (dado1 == dado2)
            {
                jugador.SetEnCarcel(false);
                jugador.SetTurnosEnCarcel(0);
                jugador.EnviarMensaje("[CARCEL] Sacaste dobles. Quedas libre de la Carcel.");
                Broadcast($"[AVISO] {jugador.GetNombre()} saco dobles y salio libre de la Carcel.", jugador);
            }
            else
            {
                jugador.SetTurnosEnCarcel(jugador.GetTurnosEnCarcel() + 1);
                jugador.EnviarMensaje($"[CARCEL] No sacaste dobles. Sigues en la Carcel ({jugador.GetTurnosEnCarcel()}/3 turnos).");

                if (jugador.GetTurnosEnCarcel() >= 3)
                {
                    jugador.EnviarMensaje("[CARCEL] Cumpliste 3 turnos en prision. Debes pagar fianza de $50 para salir.");
                    SalirDeCarcelConPago(jugador);
                }
                return;
            }
        }

        // 4. RECORRIDO DE LA LISTA ENLAZADA CIRCULAR:
        // Avanzamos 'total' nodos hacia adelante utilizando 'GetNext()'
        for (int i = 0; i < total; i++)
        {
            jugador.SetPosicion(jugador.GetPosicion()?.GetNext());

            // Bono por pasar por Salida (Casilla 0) antes de llegar al destino
            if (jugador.ObtenerCasillaActual()?.GetPosicion() == 0 && i < total - 1)
            {
                new Transaccion(200, GetTurnoActual(), "Premio por pasar por inicio", jugador, null);
                jugador.EnviarMensaje("[SALIDA] Pasaste por Salida. Cobraste $200 de bono.");
                Broadcast($"[AVISO] {jugador.GetNombre()} paso por Salida y cobro $200.", jugador);
            }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"[POSICION] Ahora estas en: {actual?.GetNombre()} ({actual?.GetTipo()})");
        AnunciarPosicion(jugador);

        // Delegacion polimorfica de accion
        actual?.Accion(jugador);
    }

    /**
     * @brief Permite al jugador adquirir la propiedad correspondiente a la casilla donde esta situado.
     *
     * Requiere confirmacion con tarjeta RFID, persiste la transaccion, almacena la casilla
     * en el inventario propio del jugador (LinkedList) y valida situaciones de bancarrota.
     * @param jugador Participante que efectua la compra.
     */
    public void ComprarPropiedad(Jugador jugador)
    {
        Casilla? casilla = jugador.ObtenerCasillaActual();
        if (casilla == null) return;

        if (casilla is not Propiedad propiedad)
        {
            jugador.EnviarMensaje("[ERROR] Esta casilla no es una propiedad comprable.");
            return;
        }

        if (propiedad.TienePropietario())
        {
            string dueño = propiedad.GetPropietario() == jugador ? "ya te pertenece" : $"le pertenece a {propiedad.GetPropietario()!.GetNombre()}";
            jugador.EnviarMensaje($"[ERROR] Esta propiedad {dueño}.");
            return;
        }

        // Verificacion RFID obligatoria
        AutorizarPagoRFID(jugador, propiedad.GetPrecioCompra(), $"Compra de '{propiedad.GetNombre()}'");

        new Transaccion(propiedad.GetPrecioCompra(), GetTurnoActual(), "Compra de propiedad", jugador, null);
        propiedad.SetPropietario(jugador);

        // Guardamos la casilla en el inventario del jugador (LinkedList propia)
        jugador.GetPropiedades().InsertEnd(propiedad);

        jugador.EnviarMensaje($"[COMPRA] Has comprado '{propiedad.GetNombre()}' por ${propiedad.GetPrecioCompra()}.");
        jugador.EnviarMensaje($"Saldo restante: ${jugador.GetDinero()}");
        Broadcast($"[AVISO] {jugador.GetNombre()} compro '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprar/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);

        // Bancarrota si el saldo resulto negativo
        if (jugador.GetDinero() < 0)
        {
            jugador.EnviarMensaje("[BANCARROTA] No tenias suficiente dinero para esta compra.");
            Broadcast($"[BANCARROTA] {jugador.GetNombre()} ha caido en bancarrota.", jugador);
            NotificarEliminacion(jugador);
        }
    }

    /**
     * @brief Verifica si un jugador es propietario de la totalidad de bienes de un grupo de color.
     * @param jugador Participante a verificar.
     * @param colorGrupo Nombre del grupo de color.
     * @return true si posee todas las propiedades del grupo; false en caso contrario.
     */
    public bool TieneMonopolio(Jugador jugador, string colorGrupo)
    {
        Node? actual = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (actual?.GetData() is Propiedad p && p.GetColorGrupo() == colorGrupo && p.GetPropietario() != jugador)
            {
                return false;
            }
            actual = actual?.GetNext();
        }
        return true;
    }

    /**
     * @brief Metodo de conveniencia para construir en la propiedad actual del jugador.
     * @param jugador Participante que construye.
     */
    public void ComprarCasa(Jugador jugador)
    {
        if (jugador.ObtenerCasillaActual() is Propiedad propiedad)
        {
            ComprarCasa(jugador, propiedad);
        }
        else
        {
            jugador.EnviarMensaje("[ERROR] Debes estar en una propiedad para construir aqui.");
        }
    }

    /**
     * @brief Construye una casa o asciende a hotel en una propiedad especifica.
     *
     * Requiere que el jugador tenga el monopolio completo del grupo y que la propiedad
     * no este hipotecada. Requiere validacion RFID del costo.
     * @param jugador Participante que edifica.
     * @param propiedad Propiedad a mejorar.
     */
    public void ComprarCasa(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("[ERROR] Esa propiedad no te pertenece.");
            return;
        }

        if (propiedad.GetColorGrupo() == "Tren")
        {
            jugador.EnviarMensaje("[ERROR] Los ferrocarriles no admiten casas.");
            return;
        }

        if (propiedad.GetIsHipotecada())
        {
            jugador.EnviarMensaje("[ERROR] No puedes construir en una propiedad hipotecada.");
            return;
        }

        if (!TieneMonopolio(jugador, propiedad.GetColorGrupo()))
        {
            jugador.EnviarMensaje($"[ERROR] Necesitas ser dueño de todas las propiedades del grupo '{propiedad.GetColorGrupo()}' para construir aqui.");
            return;
        }

        if (propiedad.GetCantidadCasas() >= 5)
        {
            jugador.EnviarMensaje("[ERROR] Esta propiedad ya tiene un Hotel construido (nivel maximo).");
            return;
        }

        int costoCasa = propiedad.GetPrecioCompra() / 2;

        AutorizarPagoRFID(jugador, costoCasa, $"Construccion en '{propiedad.GetNombre()}'");

        new Transaccion(costoCasa, GetTurnoActual(), "Pago al banco", jugador, null);
        propiedad.SetCantidadCasas(propiedad.GetCantidadCasas() + 1);
        string mejora = propiedad.GetCantidadCasas() == 5 ? "un Hotel" : $"la casa #{propiedad.GetCantidadCasas()}";
        jugador.EnviarMensaje($"[CONSTRUCCION] Construiste {mejora} en '{propiedad.GetNombre()}' por ${costoCasa}.");
        jugador.EnviarMensaje($"Nueva renta: ${propiedad.CalcularRenta()}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"[AVISO] {jugador.GetNombre()} construyo {mejora} en '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprarcasa/{propiedad.GetCantidadCasas()}/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);

        if (jugador.GetDinero() < 0)
        {
            jugador.EnviarMensaje("[BANCARROTA] No tenias suficiente dinero para esta construccion.");
            Broadcast($"[BANCARROTA] {jugador.GetNombre()} ha caido en bancarrota.", jugador);
            NotificarEliminacion(jugador);
        }
    }

    /**
     * @brief Vende una casa u hotel de una propiedad recuperando la mitad de su costo.
     * @param jugador Participante dueño del inmueble.
     * @param propiedad Propiedad a desmejorar.
     */
    public void VenderCasa(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("[ERROR] Esa propiedad no te pertenece.");
            return;
        }

        if (propiedad.GetCantidadCasas() <= 0)
        {
            jugador.EnviarMensaje("[ERROR] Esta propiedad no tiene casas para vender.");
            return;
        }

        int costoCasa = propiedad.GetPrecioCompra() / 2;
        int reembolso = costoCasa / 2;

        propiedad.SetCantidadCasas(propiedad.GetCantidadCasas() - 1);
        new Transaccion(reembolso, GetTurnoActual(), "Ganancia por evento", jugador, null);

        string quedo = propiedad.GetCantidadCasas() == 0 ? "sin casas" : $"{propiedad.GetCantidadCasas()} casas";
        jugador.EnviarMensaje($"[VENTA] Vendiste una mejora de '{propiedad.GetNombre()}' y recibiste ${reembolso}. Ahora tiene {quedo}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"[AVISO] {jugador.GetNombre()} vendio una mejora en '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprarcasa/{propiedad.GetCantidadCasas()}/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    /**
     * @brief Hipoteca una propiedad transfiriendo el 50% de su valor al dueño y congelando su renta.
     * @param jugador Participante dueño.
     * @param propiedad Propiedad a hipotecar.
     */
    public void HipotecarPropiedad(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("[ERROR] Esa propiedad no te pertenece.");
            return;
        }

        if (propiedad.GetIsHipotecada())
        {
            jugador.EnviarMensaje("[ERROR] Esa propiedad ya esta hipotecada.");
            return;
        }

        if (propiedad.GetCantidadCasas() > 0)
        {
            jugador.EnviarMensaje("[ERROR] Debes vender las casas u hotel antes de hipotecar esta propiedad.");
            return;
        }

        int valorHipoteca = propiedad.GetPrecioCompra() / 2;
        propiedad.SetIsHipotecada(true);
        new Transaccion(valorHipoteca, GetTurnoActual(), "Ganancia por evento", jugador, null);

        jugador.EnviarMensaje($"[HIPOTECA] Hipotecaste '{propiedad.GetNombre()}' y recibiste ${valorHipoteca}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"[AVISO] {jugador.GetNombre()} hipoteco '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/hipotecar/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    /**
     * @brief Deshipoteca una propiedad abonando el valor recibido mas un 10% de interes reglamentario.
     * @param jugador Participante dueño.
     * @param propiedad Propiedad a deshipotecar.
     */
    public void DeshipotecarPropiedad(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("[ERROR] Esa propiedad no te pertenece.");
            return;
        }

        if (!propiedad.GetIsHipotecada())
        {
            jugador.EnviarMensaje("[ERROR] Esa propiedad no esta hipotecada.");
            return;
        }

        int costo = (int)(propiedad.GetPrecioCompra() / 2 * 1.1);

        AutorizarPagoRFID(jugador, costo, $"Deshipotecar '{propiedad.GetNombre()}'");

        new Transaccion(costo, GetTurnoActual(), "Pago al banco", jugador, null);
        propiedad.SetIsHipotecada(false);

        jugador.EnviarMensaje($"[DESHIPOTECA] Deshipotecaste '{propiedad.GetNombre()}' por ${costo}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"[AVISO] {jugador.GetNombre()} deshipoteco '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/deshipotecar/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);

        if (jugador.GetDinero() < 0)
        {
            jugador.EnviarMensaje("[BANCARROTA] No tenias suficiente dinero para deshipotecar.");
            Broadcast($"[BANCARROTA] {jugador.GetNombre()} ha caido en bancarrota.", jugador);
            NotificarEliminacion(jugador);
        }
    }

    /**
     * @brief Desplaza a un jugador directamente a un indice absoluto de casilla (0 a 31).
     * @param jugador Participante a desplazar.
     * @param posicionDestino Indice de casilla objetivo.
     */
    public void MoverJugadorACasilla(Jugador jugador, int posicionDestino)
    {
        if (jugador.GetPosicion() == null)
        {
            jugador.SetPosicion(this.tablero.GetHead());
        }

        int total = this.tablero.Size();
        for (int i = 0; i < total; i++)
        {
            if (jugador.ObtenerCasillaActual()?.GetPosicion() == posicionDestino)
            {
                break;
            }
            jugador.SetPosicion(jugador.GetPosicion()?.GetNext());
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"[POSICION] Te moviste a: {actual?.GetNombre()} ({actual?.GetTipo()})");
        AnunciarPosicion(jugador);
        actual?.Accion(jugador);
    }

    /**
     * @brief Desplaza al jugador una cantidad relativa de casillas hacia adelante o hacia atras.
     * @param jugador Participante a mover.
     * @param cantidad Cantidad positiva para avanzar o negativa para retroceder.
     */
    public void MoverJugadorCasillas(Jugador jugador, int cantidad)
    {
        if (cantidad >= 0)
        {
            for (int i = 0; i < cantidad; i++)
            {
                jugador.SetPosicion(jugador.GetPosicion()?.GetNext());
            }
        }
        else
        {
            for (int i = 0; i < Math.Abs(cantidad); i++)
            {
                jugador.SetPosicion(jugador.GetPosicion()?.GetPrevious());
            }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"[POSICION] Ahora estas en: {actual?.GetNombre()} ({actual?.GetTipo()})");
        AnunciarPosicion(jugador);
        actual?.Accion(jugador);
    }

    /**
     * @brief Envia al jugador a prision (Casilla 8) y activa su bandera de condena.
     * @param jugador Participante recluido.
     */
    public void EnviarACarcel(Jugador jugador)
    {
        jugador.SetEnCarcel(true);
        jugador.SetTurnosEnCarcel(0);
        MoverJugadorACasilla(jugador, 8);
        jugador.EnviarMensaje("[CARCEL] Has sido encerrado en la Carcel.");
        Broadcast($"[AVISO] {jugador.GetNombre()} fue enviado a la Carcel.", jugador);
    }

    /**
     * @brief Gestiona la liberacion de prision mediante pago de fianza o uso de carta de salida.
     * @param jugador Participante preso.
     */
    public void SalirDeCarcelConPago(Jugador jugador)
    {
        if (!jugador.GetEnCarcel())
        {
            jugador.EnviarMensaje("[INFO] No estas en la Carcel.");
            return;
        }

        if (jugador.GetCartasSalirDeCarcel() > 0)
        {
            jugador.SetCartasSalirDeCarcel(jugador.GetCartasSalirDeCarcel() - 1);
            jugador.SetEnCarcel(false);
            jugador.SetTurnosEnCarcel(0);
            jugador.EnviarMensaje("[CARCEL] Usaste tu carta de Salir de la Carcel Gratis y quedas en libertad.");
            Broadcast($"[AVISO] {jugador.GetNombre()} uso una carta y salio de la Carcel.", jugador);
            return;
        }

        int fianza = 50;

        AutorizarPagoRFID(jugador, fianza, "Fianza para salir de la Carcel");

        new Transaccion(fianza, GetTurnoActual(), "Pago al banco", jugador, null);
        jugador.SetEnCarcel(false);
        jugador.SetTurnosEnCarcel(0);
        jugador.EnviarMensaje($"[CARCEL] Pagaste ${fianza} de fianza y has salido de la Carcel. Saldo: ${jugador.GetDinero()}");
        Broadcast($"[AVISO] {jugador.GetNombre()} pago la fianza y salio de la Carcel.", jugador);
        AnunciarDinero(jugador);

        if (jugador.GetDinero() < 0)
        {
            jugador.EnviarMensaje("[BANCARROTA] No tenias suficiente dinero para la fianza.");
            Broadcast($"[BANCARROTA] {jugador.GetNombre()} ha caido en bancarrota.", jugador);
            NotificarEliminacion(jugador);
        }
    }

    /**
     * @brief Imprime en la consola el desglose patrimonial, fondos y propiedades del jugador.
     * @param jugador Participante a consultar.
     */
    public void VerEstado(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();

        jugador.EnviarMensaje($"\n=== ESTADO DE {jugador.GetNombre()} ===");
        jugador.EnviarMensaje($"Dinero: ${jugador.GetDinero()}");
        jugador.EnviarMensaje($"Casilla actual: {actual?.GetNombre() ?? "Ninguna"}");
        jugador.EnviarMensaje($"En Carcel: {(jugador.GetEnCarcel() ? $"Si ({jugador.GetTurnosEnCarcel()}/3)" : "No")}");
        jugador.EnviarMensaje($"Cartas Salir de Carcel: {jugador.GetCartasSalirDeCarcel()}");
        jugador.EnviarMensaje($"Propiedades compradas ({jugador.GetPropiedades().Size()}):");

        // Recorrido secuencial de la lista enlazada de propiedades del jugador
        Node? temp = jugador.GetPropiedades().GetHead();
        int totalPropiedades = jugador.GetPropiedades().Size();
        for (int i = 0; i < totalPropiedades; i++)
        {
            if (temp?.GetData() is Propiedad p)
            {
                string nivel = p.GetCantidadCasas() == 5 ? "Hotel" : $"{p.GetCantidadCasas()} casas";
                jugador.EnviarMensaje($"  - {p.GetNombre()} (Grupo: {p.GetColorGrupo()}, Renta: ${p.CalcularRenta()}, {nivel})");
            }
            else if (temp?.GetData() is Casilla c)
            {
                jugador.EnviarMensaje($"  - {c.GetNombre()} (Renta: ${c.GetRenta()})");
            }
            temp = temp?.GetNext();
        }
    }

    /**
     * @brief Recorre la lista circular del tablero e imprime cada una de sus casillas y propietarios.
     * @param jugador Participante que solicita la visualizacion.
     */
    public void VerTablero(Jugador jugador)
    {
        jugador.EnviarMensaje("\n=== TABLERO (LISTA CIRCULAR) ===");

        Node? actual = this.tablero.GetHead();
        int total = this.tablero.Size();

        for (int i = 0; i < total; i++)
        {
            if (actual?.GetData() is Casilla c)
            {
                string dueño = "";
                if (c is Propiedad p && p.GetPropietario() != null)
                {
                    dueño = $" [Dueño: {p.GetPropietario()!.GetNombre()}]";
                }
                jugador.EnviarMensaje($"[{c.GetPosicion()}] {c.GetNombre()} ({c.GetTipo()}){dueño}");
            }
            actual = actual?.GetNext();
        }
    }
}