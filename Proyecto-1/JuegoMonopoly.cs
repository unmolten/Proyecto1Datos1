using System;
using System.IO;
using System.Collections.Generic;

using System.Threading;

// Representa una acción de juego solicitada desde la UI de Godot o la consola
public class AccionTurno
{
    public string Tipo { get; set; } = "";
    public int CasillaIndex { get; set; } = -1;
    public int JugadorId { get; set; } = 0;
}

// PLANTILLA BASE: JuegoMonopoly
// 
// Esta clase coordina el juego de Monopoly en el servidor, conectando las casillas,
// los mazos de eventos, las transacciones y las listas enlazadas.
// 
// Conexión con Listas Enlazadas:
// - El tablero es una lista enlazada circular ('LinkedList' + 'MakeCircular()').
// - Los jugadores avanzan en el tablero de nodo en nodo con 'jugador.Posicion = jugador.Posicion.GetNext()'.
// - Cada jugador tiene su propia lista enlazada ('jugador.Propiedades') para guardar sus compras.
// - Los jugadores conectados se almacenan en una lista enlazada ('LinkedList').
// - Los mazos de cartas (Fortuna y Arca Comunal) son listas enlazadas circulares que rotan al robar.
public class JuegoMonopoly
{
    // Patrón Singleton: Una única instancia para coordinar el juego en el servidor
    private static JuegoMonopoly? instancia;
    public static JuegoMonopoly Instancia => instancia ??= new JuegoMonopoly();

    // Estructura de datos del tablero (Lista Enlazada Circular)
    private LinkedList tablero;

    // Lista de jugadores conectados al servidor (Lista Enlazada)
    private LinkedList jugadores;

    // Mazos de cartas de eventos (Listas Enlazadas Circulares)
    private LinkedList mazoFortuna;
    private LinkedList mazoArcaComunal;

    // Contador de turnos y asignador de IDs
    private int turnoActual = 1;
    private int contadorJugadores = 1;

    public int GetTurnoActual()
    {
        return this.turnoActual;
    }

    public void SetTurnoActual(int turnoActual)
    {
        this.turnoActual = turnoActual;
    }

    public LinkedList GetTablero()
    {
        return this.tablero;
    }

    public LinkedList GetJugadores()
    {
        return this.jugadores;
    }

    public LinkedList GetMazoFortuna()
    {
        return this.mazoFortuna;
    }

    public LinkedList GetMazoArcaComunal()
    {
        return this.mazoArcaComunal;
    }

    // Generador de números aleatorios para los dados
    private Random random = new Random();

    public JuegoMonopoly()
    {
        this.tablero = new LinkedList();
        this.jugadores = new LinkedList();
        this.mazoFortuna = MazoCartas.CrearMazoFortuna();
        this.mazoArcaComunal = MazoCartas.CrearMazoArcaComunal();
        InicializarTablero();
    }

    // Inicializa las casillas del tablero en la lista enlazada circular con sus clases polimórficas.
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

        // LISTA CIRCULAR: Conecta la cola con la cabeza para que el tablero dé vueltas continuas
        this.tablero.MakeCircular();
    }

    // Registra a un nuevo cliente en la partida y lo posiciona en la cabeza de la lista (Salida).
    public Jugador RegistrarJugador(StreamWriter writer)
    {
        Jugador nuevo = new Jugador(contadorJugadores, $"Jugador_{contadorJugadores}", writer);
        nuevo.SetPosicion(this.tablero.GetHead()); // Inicia en la cabeza de la lista enlazada
        contadorJugadores++;

        this.jugadores.InsertEnd(nuevo);
        Broadcast($"[SISTEMA] {nuevo.GetNombre()} se ha unido a la partida.");
        GodotBroadcast($"jugador/{nuevo.GetId()}/activar");
        return nuevo;
    }

    // Registra a un jugador ya existente (creado por RFID o consola) en la lista de jugadores.
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
        Broadcast($"[SISTEMA] {jugador.GetNombre()} (ID: {jugador.GetId()}) está listo en el tablero.");
        GodotBroadcast($"jugador/{jugador.GetId()}/activar");
    }

    // ---------------- CONEXION CON GODOT ----------------
    //
    // Esto es aparte de los jugadores de consola/RFID de arriba, un espectador
    // de Godot solo recibe info, todavia no manda nada (eso lo conectamos
    // despues con los botones). Por eso no son Jugador, son solo un StreamWriter
    // guardado en una lista

    // Conexiones de las instancias de Godot que estan viendo la partida
    // HAY QUE MODIFICAR ESTO PORQUE ESTA HECHO CON LISTAS DE C#, NO LISTA ENLAZADA
    private List<StreamWriter> espectadoresGodot = new List<StreamWriter>();

    // Quien tiene el turno ahora mismo (para poder sincronizar a un
    // espectador de Godot que se conecta a mitad de partida)
    private Jugador? jugadorEnTurno = null;

    // Program.cs llama esto cada vez que una instancia de Godot se conecta.
    // Godot no sabe NADA por su cuenta (ni siquiera qué propiedades existen),
    // solo refleja lo que el servidor le manda, asi que aqui le mandamos
    // TODO el estado, en el orden correcto para que lo pueda armar solo
    public void ConectarEspectadorGodot(StreamWriter writer)
    {
        lock (this.espectadoresGodot)
        {
            this.espectadoresGodot.Add(writer);
        }

        // 1) Los datos fijos de cada propiedad: nombre, precio, renta base
        // y grupo de color. Sin esto Godot ni siquiera sabria que existen,
        // asi que esto se manda SIEMPRE primero, antes que cualquier otra cosa
        Node? nodoDatos = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (nodoDatos?.GetData() is Propiedad prop)
            {
                EnviarAEspectador(writer, $"propiedad/{prop.GetPosicion()}/{prop.GetNombre()}/{prop.GetPrecioCompra()}/{prop.GetAlquilerBase()}/{prop.GetColorGrupo()}");
            }
            nodoDatos = nodoDatos?.GetNext();
        }

        // 2) El estado actual de cada propiedad: dueño, casas y si esta
        // hipotecada (en ese orden, porque construir/hipotecar en Godot
        // dependen de que el dueño ya este puesto primero)
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

        // 3) Los jugadores que ya estaban registrados y donde estan parados
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

        // 4) Por último, de quién es el turno ahora mismo (para el aro de
        // color y el HUD de dinero), si la partida ya empezó
        if (this.jugadorEnTurno != null)
        {
            EnviarAEspectador(writer, $"jugador/{this.jugadorEnTurno.GetId()}/turno");
            EnviarAEspectador(writer, $"jugador/{this.jugadorEnTurno.GetId()}/dinero/{this.jugadorEnTurno.GetDinero()}");

            Casilla? cActual = this.jugadorEnTurno.ObtenerCasillaActual();
            bool puedeComprar = cActual is Propiedad pr && !pr.TienePropietario() && this.jugadorEnTurno.GetDinero() >= pr.GetPrecioCompra();
            int precio = (cActual is Propiedad pr2) ? pr2.GetPrecioCompra() : 0;
            string nom = cActual?.GetNombre() ?? "";
            EnviarAEspectador(writer, $"turno/acciones/1/{(puedeComprar ? 1 : 0)}/{precio}/{nom}/0/{(this.jugadorEnTurno.GetEnCarcel() ? 1 : 0)}");
        }
    }

    // Manda una linea nada mas a un espectador (usado para la sincronizacion inicial)
    private void EnviarAEspectador(StreamWriter writer, string mensaje)
    {
        try { writer.WriteLine(mensaje); } catch { /* se limpia sola cuando falle el broadcast normal */ }
    }

    // Manda una linea a TODAS las instancias de Godot conectadas
    // esto usa el protocolo que ya entiende networkclient.gd (jugador/id/accion/...)
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
                    // se cayo la conexion, la sacamos de la lista
                    this.espectadoresGodot.RemoveAt(i);
                }
            }
        }
    }

    // ---------------- INTEGRACIÓN HARDWARE RFID / DADOS ----------------
    private ConexionPico? conexionPico;
    private ObtenerID? lectorRfid;

    public void ConfigurarHardware(ConexionPico? conexion)
    {
        this.conexionPico = conexion;
        if (conexion != null)
        {
            this.lectorRfid = new ObtenerID(conexion);
        }
    }

    public ConexionPico? GetConexionPico()
    {
        return this.conexionPico;
    }

    // ---------------- COLA DE ACCIONES DE TURNOS (GODOT + CONSOLA) ----------------
    // Cola implementada con la lista enlazada propia + Monitor para bloqueo entre hilos
    private LinkedList colaAcciones = new LinkedList();
    private readonly object colaLock = new object();

    public void EncolarAccion(AccionTurno accion)
    {
        lock (colaLock)
        {
            colaAcciones.InsertEnd(accion);
            Monitor.Pulse(colaLock); // Despierta al hilo que espera en EsperarAccion
        }
    }

    public AccionTurno EsperarAccion(Jugador jugador)
    {
        while (true)
        {
            AccionTurno accion;
            lock (colaLock)
            {
                // Espera bloqueante hasta que haya al menos un elemento en la cola
                while (colaAcciones.IsEmpty())
                {
                    Monitor.Wait(colaLock);
                }
                // Saca el primer elemento (FIFO) de la lista enlazada
                Node? nodo = colaAcciones.DeleteFirst();
                accion = (AccionTurno)nodo!.GetData();
            }
            // Acciones generales o asignadas al jugador en turno
            if (accion.JugadorId == 0 || accion.JugadorId == jugador.GetId())
            {
                return accion;
            }
        }
    }

    // Procesa comandos enviados por los clientes Godot vía TCP (ej: accion/1/tirar)
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

        if (tipo == "rfid" || tipo == "rfid_confirmar" || tipo == "confirmarpago")
        {
            ConfirmarPagoRfid(jugadorId);
            return;
        }
        if (tipo == "cancelarpago")
        {
            CancelarPagoRfid(jugadorId);
            return;
        }

        EncolarAccion(new AccionTurno
        {
            JugadorId = jugadorId,
            Tipo = tipo,
            CasillaIndex = arg
        });
    }

    // ---------------- AUTORIZACIÓN DE PAGO VIA RFID (ESTILO COMPRA CON TELÉFONO) ----------------
    private volatile bool pagoRfidConfirmado = false;
    private volatile bool pagoRfidCancelado = false;

    public void ConfirmarPagoRfid(int jugadorId)
    {
        pagoRfidConfirmado = true;
    }

    public void CancelarPagoRfid(int jugadorId)
    {
        pagoRfidCancelado = true;
    }

    public bool AutorizarPagoRFID(Jugador jugador, int monto, string concepto)
    {
        pagoRfidConfirmado = false;
        pagoRfidCancelado = false;

        Console.WriteLine("\n==========================================================");
        Console.WriteLine("📲 [AUTORIZACIÓN DE PAGO REQUERIDA (RFID / CONTACTLESS)]");
        Console.WriteLine($"   Jugador: {jugador.GetNombre()} (Saldo actual: ${jugador.GetDinero()})");
        Console.WriteLine($"   Monto a pagar: ${monto}");
        Console.WriteLine($"   Concepto: {concepto}");
        Console.WriteLine("   -> Acerca tu tarjeta RFID al lector físico, o presiona");
        Console.WriteLine("      [Pagar con RFID / NFC] en Godot, o escribe 'p' en consola...");
        Console.WriteLine("==========================================================");

        // Notifica a todas las pantallas de Godot para abrir el modal de pago
        GodotBroadcast($"pago/solicitar/{jugador.GetId()}/{monto}/{concepto}");

        bool hardwareDisponible = this.conexionPico != null && this.conexionPico.EstaConectado();
        if (hardwareDisponible && this.lectorRfid != null)
        {
            try
            {
                this.lectorRfid.IniciarLecturaTarjetas();
            }
            catch { }
        }

        while (!pagoRfidConfirmado && !pagoRfidCancelado)
        {
            if (hardwareDisponible && this.lectorRfid != null)
            {
                try
                {
                    string uid = this.lectorRfid.LeerTarjeta();
                    if (!string.IsNullOrEmpty(uid))
                    {
                        Console.WriteLine($"💳 [Pico RFID] ¡Tarjeta física detectada! UID: {uid}. Pago aprobado.");
                        pagoRfidConfirmado = true;
                        break;
                    }
                }
                catch { }
            }

            Thread.Sleep(50);
        }

        if (pagoRfidConfirmado)
        {
            Console.WriteLine($"✅ [PAGO APROBADO] Pago de ${monto} completado exitosamente para {jugador.GetNombre()}.\n");
            GodotBroadcast($"pago/exito/{jugador.GetId()}/{monto}");
            Thread.Sleep(300);
            return true;
        }
        else
        {
            Console.WriteLine($"❌ [PAGO CANCELADO] La transacción de ${monto} fue cancelada.\n");
            GodotBroadcast($"pago/cancelado/{jugador.GetId()}");
            return false;
        }
    }

    // Sincroniza con Godot qué botones de acción están habilitados en el turno actual
    public void EnviarEstadoAcciones(Jugador jugador, bool yaTiroDados)
    {
        bool puedeTirar = !yaTiroDados && !jugador.GetPierdeSiguienteTurno();
        bool puedeComprar = false;
        int precioCompra = 0;
        string nombrePropiedad = "";

        Casilla? casilla = jugador.ObtenerCasillaActual();
        if (yaTiroDados && casilla is Propiedad prop && !prop.TienePropietario() && jugador.GetDinero() >= prop.GetPrecioCompra())
        {
            puedeComprar = true;
            precioCompra = prop.GetPrecioCompra();
            nombrePropiedad = prop.GetNombre();
        }

        bool puedeTerminar = yaTiroDados || jugador.GetPierdeSiguienteTurno();
        bool enCarcel = jugador.GetEnCarcel();

        GodotBroadcast($"turno/acciones/{(puedeTirar ? 1 : 0)}/{(puedeComprar ? 1 : 0)}/{precioCompra}/{nombrePropiedad}/{(puedeTerminar ? 1 : 0)}/{(enCarcel ? 1 : 0)}");
    }

    // Busca una propiedad en el tablero circular por su posición
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

    // Avisa a Godot en que casilla quedo el jugador despues de moverse
    // se llama siempre que jugador.Posicion cambia y ya se asento
    private void AnunciarPosicion(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();
        if (actual != null)
        {
            GodotBroadcast($"jugador/{jugador.GetId()}/mover/casilla/{actual.GetPosicion()}");
        }
    }

    // Avisa a Godot de quien es el turno actual (para el aro de color) y de
    // paso le manda su dinero actual (para que el HUD arranque con el numero correcto)
    public void AnunciarTurno(Jugador jugador)
    {
        this.jugadorEnTurno = jugador;
        GodotBroadcast($"jugador/{jugador.GetId()}/turno");
        AnunciarDinero(jugador);
    }

    // Avisa a Godot cuanto dinero tiene un jugador. El HUD de la esquina solo
    // le hace caso a esto si es del jugador que tiene el turno actual, pero
    // eso lo decide Godot, aqui simplemente se manda
    public void AnunciarDinero(Jugador jugador)
    {
        GodotBroadcast($"jugador/{jugador.GetId()}/dinero/{jugador.GetDinero()}");
    }

    // El Program.cs llama esto cuando un jugador queda eliminado por bancarrota
    public void NotificarEliminacion(Jugador jugador)
    {
        GodotBroadcast($"jugador/{jugador.GetId()}/desactivar");
    }

    // Remueve a un jugador cuando se desconecta.
    public void DesconectarJugador(Jugador jugador)
    {
        this.jugadores.Delete(jugador);
        Broadcast($"[SISTEMA] {jugador.GetNombre()} ha salido de la partida.");
    }

    // Envía un mensaje a todos los jugadores conectados recorriendo la lista enlazada.
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

    // Tirar dados, gestionar cárcel, avanzar en la lista circular y activar la casilla polimórficamente.
    public void TirarDados(Jugador jugador)
    {
        // 1. Verifica si debe perder su turno por efecto de carta
        if (jugador.GetPierdeSiguienteTurno())
        {
            jugador.SetPierdeSiguienteTurno(false);
            jugador.EnviarMensaje("⏳ Perdiste este turno debido a un evento anterior.");
            Broadcast($"📢 {jugador.GetNombre()} perdió su turno.", jugador);
            return;
        }

        // 2. Tirar dos dados
        int dado1 = random.Next(1, 7);
        int dado2 = random.Next(1, 7);
        int total = dado1 + dado2;

        jugador.EnviarMensaje($"🎲 Tiraste: [{dado1}] + [{dado2}] = {total}");

        // 3. Manejo de estado en la cárcel
        if (jugador.GetEnCarcel())
        {
            if (dado1 == dado2)
            {
                jugador.SetEnCarcel(false);
                jugador.SetTurnosEnCarcel(0);
                jugador.EnviarMensaje("🎉 ¡Sacaste dobles! Quedas libre de la Cárcel.");
                Broadcast($"📢 {jugador.GetNombre()} sacó dobles y salió libre de la Cárcel.", jugador);
            }
            else
            {
                jugador.SetTurnosEnCarcel(jugador.GetTurnosEnCarcel() + 1);
                jugador.EnviarMensaje($"🔒 No sacaste dobles. Sigues en la Cárcel ({jugador.GetTurnosEnCarcel()}/3 turnos).");

                if (jugador.GetTurnosEnCarcel() >= 3)
                {
                    jugador.EnviarMensaje("⚠️ Cumpliste 3 turnos en prisión. Debes pagar fianza de $50 para salir.");
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
                jugador.EnviarMensaje("💵 ¡Pasaste por Salida! Cobraste $200 de bono.");
                Broadcast($"📢 {jugador.GetNombre()} pasó por Salida y cobró $200.", jugador);
            }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"📍 Ahora estás en: {actual?.GetNombre()} ({actual?.GetTipo()})");
        AnunciarPosicion(jugador);

        // POLIMORFISMO: Se delega la acción a la casilla en la que aterrizó
        actual?.Accion(jugador);
    }

    // Comprar la propiedad de la casilla actual.
    public void ComprarPropiedad(Jugador jugador)
    {
        Casilla? casilla = jugador.ObtenerCasillaActual();
        if (casilla == null) return;

        if (casilla is not Propiedad propiedad)
        {
            jugador.EnviarMensaje("❌ Esta casilla no es una propiedad comprable.");
            return;
        }

        if (propiedad.TienePropietario())
        {
            string dueño = propiedad.GetPropietario() == jugador ? "ya te pertenece" : $"le pertenece a {propiedad.GetPropietario()!.GetNombre()}";
            jugador.EnviarMensaje($"❌ Esta propiedad {dueño}.");
            return;
        }

        if (jugador.GetDinero() < propiedad.GetPrecioCompra())
        {
            jugador.EnviarMensaje($"❌ Saldo insuficiente. Tienes ${jugador.GetDinero()} y cuesta ${propiedad.GetPrecioCompra()}.");
            return;
        }

        // VERIFICACIÓN RFID OBLIGATORIA (estilo Apple Pay / contactless con teléfono)
        if (!AutorizarPagoRFID(jugador, propiedad.GetPrecioCompra(), $"Compra de '{propiedad.GetNombre()}'"))
        {
            jugador.EnviarMensaje("❌ Compra cancelada: no se autorizó el pago vía RFID.");
            return;
        }

        // Ejecuta la transacción de compra
        new Transaccion(propiedad.GetPrecioCompra(), GetTurnoActual(), "Compra de propiedad", jugador, null);
        propiedad.SetPropietario(jugador);

        // USO DE LISTA ENLAZADA: Guardamos la casilla en el inventario del jugador
        jugador.GetPropiedades().InsertEnd(propiedad);

        jugador.EnviarMensaje($"🎉 ¡Has comprado '{propiedad.GetNombre()}' por ${propiedad.GetPrecioCompra()}!");
        jugador.EnviarMensaje($"Saldo restante: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} compró '{propiedad.GetNombre()}'!", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprar/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    // Revisa si el jugador es dueño de TODAS las propiedades de ese grupo de color.
    // Los "Tren" nunca cuentan para monopolio (en el Monopoly real los ferrocarriles
    // tampoco admiten casas), por eso ComprarCasa los rechaza directamente.
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

    // Atajo: construye en la propiedad donde el jugador está parado actualmente
    // (se usa cuando cae en una propiedad suya y quiere construir ahí mismo).
    public void ComprarCasa(Jugador jugador)
    {
        if (jugador.ObtenerCasillaActual() is Propiedad propiedad)
        {
            ComprarCasa(jugador, propiedad);
        }
        else
        {
            jugador.EnviarMensaje("❌ Debes estar en una propiedad para construir aquí.");
        }
    }

    // Comprar una casa u hotel en una propiedad especifica (ya no solo la actual,
    // asi se puede construir en cualquier propiedad propia desde el menu de gestión).
    // Ahora exige tener el monopolio completo del grupo de color.
    public void ComprarCasa(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("❌ Esa propiedad no te pertenece.");
            return;
        }

        if (propiedad.GetColorGrupo() == "Tren")
        {
            jugador.EnviarMensaje("❌ Los ferrocarriles no admiten casas.");
            return;
        }

        if (propiedad.GetIsHipotecada())
        {
            jugador.EnviarMensaje("❌ No puedes construir en una propiedad hipotecada.");
            return;
        }

        if (!TieneMonopolio(jugador, propiedad.GetColorGrupo()))
        {
            jugador.EnviarMensaje($"❌ Necesitas ser dueño de TODAS las propiedades del grupo '{propiedad.GetColorGrupo()}' para construir aquí.");
            return;
        }

        if (propiedad.GetCantidadCasas() >= 5)
        {
            jugador.EnviarMensaje("❌ Esta propiedad ya tiene un Hotel construido (nivel máximo).");
            return;
        }

        int costoCasa = propiedad.GetPrecioCompra() / 2;
        if (jugador.GetDinero() < costoCasa)
        {
            jugador.EnviarMensaje($"❌ Saldo insuficiente. Construir cuesta ${costoCasa} y tienes ${jugador.GetDinero()}.");
            return;
        }

        // VERIFICACIÓN RFID OBLIGATORIA
        if (!AutorizarPagoRFID(jugador, costoCasa, $"Construcción en '{propiedad.GetNombre()}'"))
        {
            jugador.EnviarMensaje("❌ Construcción cancelada: no se autorizó el pago vía RFID.");
            return;
        }

        new Transaccion(costoCasa, GetTurnoActual(), "Pago al banco", jugador, null);
        propiedad.SetCantidadCasas(propiedad.GetCantidadCasas() + 1);
        string mejora = propiedad.GetCantidadCasas() == 5 ? "un Hotel" : $"la casa #{propiedad.GetCantidadCasas()}";
        jugador.EnviarMensaje($"🏗️ ¡Construiste {mejora} en '{propiedad.GetNombre()}' por ${costoCasa}!");
        jugador.EnviarMensaje($"Nueva renta: ${propiedad.CalcularRenta()}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} construyó {mejora} en '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprarcasa/{propiedad.GetCantidadCasas()}/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    // Vende una casa/hotel (bajar un nivel). Se recupera la mitad de lo que
    // costó construirla, igual que la regla clásica del Monopoly de mesa.
    public void VenderCasa(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("❌ Esa propiedad no te pertenece.");
            return;
        }

        if (propiedad.GetCantidadCasas() <= 0)
        {
            jugador.EnviarMensaje("❌ Esta propiedad no tiene casas para vender.");
            return;
        }

        int costoCasa = propiedad.GetPrecioCompra() / 2;
        int reembolso = costoCasa / 2;

        propiedad.SetCantidadCasas(propiedad.GetCantidadCasas() - 1);
        new Transaccion(reembolso, GetTurnoActual(), "Ganancia por evento", jugador, null);

        string quedo = propiedad.GetCantidadCasas() == 0 ? "sin casas" : $"{propiedad.GetCantidadCasas()} casas";
        jugador.EnviarMensaje($"🏚️ Vendiste una mejora de '{propiedad.GetNombre()}' y recibiste ${reembolso}. Ahora tiene {quedo}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} vendió una mejora en '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprarcasa/{propiedad.GetCantidadCasas()}/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    // Hipoteca una propiedad: el banco te da la mitad del precio de compra,
    // pero deja de cobrar renta hasta que se deshipoteque. No se puede
    // hipotecar si todavía tiene casas construidas encima.
    public void HipotecarPropiedad(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("❌ Esa propiedad no te pertenece.");
            return;
        }

        if (propiedad.GetIsHipotecada())
        {
            jugador.EnviarMensaje("❌ Esa propiedad ya está hipotecada.");
            return;
        }

        if (propiedad.GetCantidadCasas() > 0)
        {
            jugador.EnviarMensaje("❌ Debes vender las casas/hotel antes de hipotecar esta propiedad.");
            return;
        }

        int valorHipoteca = propiedad.GetPrecioCompra() / 2;
        propiedad.SetIsHipotecada(true);
        new Transaccion(valorHipoteca, GetTurnoActual(), "Ganancia por evento", jugador, null);

        jugador.EnviarMensaje($"🏦 Hipotecaste '{propiedad.GetNombre()}' y recibiste ${valorHipoteca}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} hipotecó '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/hipotecar/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    // Deshipoteca una propiedad: se paga lo que te dieron por la hipoteca
    // más un 10% de interés (la regla clásica del Monopoly de mesa).
    public void DeshipotecarPropiedad(Jugador jugador, Propiedad propiedad)
    {
        if (propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("❌ Esa propiedad no te pertenece.");
            return;
        }

        if (!propiedad.GetIsHipotecada())
        {
            jugador.EnviarMensaje("❌ Esa propiedad no está hipotecada.");
            return;
        }

        int costo = (int)(propiedad.GetPrecioCompra() / 2 * 1.1);
        if (jugador.GetDinero() < costo)
        {
            jugador.EnviarMensaje($"❌ Necesitas ${costo} para deshipotecarla y tienes ${jugador.GetDinero()}.");
            return;
        }

        // VERIFICACIÓN RFID OBLIGATORIA
        if (!AutorizarPagoRFID(jugador, costo, $"Deshipotecar '{propiedad.GetNombre()}'"))
        {
            jugador.EnviarMensaje("❌ Deshipoteca cancelada: no se autorizó el pago vía RFID.");
            return;
        }

        new Transaccion(costo, GetTurnoActual(), "Pago al banco", jugador, null);
        propiedad.SetIsHipotecada(false);

        jugador.EnviarMensaje($"🏦 Deshipotecaste '{propiedad.GetNombre()}' por ${costo}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} deshipotecó '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/deshipotecar/casilla/{propiedad.GetPosicion()}");
        AnunciarDinero(jugador);
    }

    // Mueve al jugador a una casilla específica por su índice numérico (0 a 31).
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
        jugador.EnviarMensaje($"📍 Te moviste a: {actual?.GetNombre()} ({actual?.GetTipo()})");
        AnunciarPosicion(jugador);
        actual?.Accion(jugador);
    }

    // Mueve al jugador un número relativo de casillas (positivo hacia adelante, negativo hacia atrás).
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
        jugador.EnviarMensaje($"📍 Ahora estás en: {actual?.GetNombre()} ({actual?.GetTipo()})");
        AnunciarPosicion(jugador);
        actual?.Accion(jugador);
    }

    // Envía a un jugador directamente a la Cárcel (Casilla 8).
    public void EnviarACarcel(Jugador jugador)
    {
        jugador.SetEnCarcel(true);
        jugador.SetTurnosEnCarcel(0);
        MoverJugadorACasilla(jugador, 8); // Casilla 8 es la Cárcel
        jugador.EnviarMensaje("🔒 Has sido encerrado en la Cárcel.");
        Broadcast($"🚨 {jugador.GetNombre()} fue enviado a la Cárcel.", jugador);
    }

    // Salir de la cárcel pagando fianza o usando carta.
    public void SalirDeCarcelConPago(Jugador jugador)
    {
        if (!jugador.GetEnCarcel())
        {
            jugador.EnviarMensaje("ℹ️ No estás en la Cárcel.");
            return;
        }

        if (jugador.GetCartasSalirDeCarcel() > 0)
        {
            jugador.SetCartasSalirDeCarcel(jugador.GetCartasSalirDeCarcel() - 1);
            jugador.SetEnCarcel(false);
            jugador.SetTurnosEnCarcel(0);
            jugador.EnviarMensaje("🎟️ ¡Usaste tu carta de Salir de la Cárcel Gratis y quedas en libertad!");
            Broadcast($"📢 {jugador.GetNombre()} usó una carta y salió de la Cárcel.", jugador);
            return;
        }

        int fianza = 50;
        if (jugador.GetDinero() < fianza)
        {
            jugador.EnviarMensaje($"❌ No tienes suficiente dinero (${jugador.GetDinero()}) para pagar la fianza (${fianza}).");
            return;
        }

        // VERIFICACIÓN RFID OBLIGATORIA
        if (!AutorizarPagoRFID(jugador, fianza, "Fianza para salir de la Cárcel"))
        {
            jugador.EnviarMensaje("❌ Salida cancelada: no se autorizó el pago de la fianza vía RFID.");
            return;
        }

        new Transaccion(fianza, GetTurnoActual(), "Pago al banco", jugador, null);
        jugador.SetEnCarcel(false);
        jugador.SetTurnosEnCarcel(0);
        jugador.EnviarMensaje($"💵 Pagaste ${fianza} de fianza y has salido de la Cárcel. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} pagó la fianza y salió de la Cárcel.", jugador);
        AnunciarDinero(jugador);
    }

    // Consultar estado del jugador y recorrer su lista enlazada de propiedades.
    public void VerEstado(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();

        jugador.EnviarMensaje($"\n=== ESTADO DE {jugador.GetNombre()} ===");
        jugador.EnviarMensaje($"Dinero: ${jugador.GetDinero()}");
        jugador.EnviarMensaje($"Casilla actual: {actual?.GetNombre() ?? "Ninguna"}");
        jugador.EnviarMensaje($"En Cárcel: {(jugador.GetEnCarcel() ? $"Sí ({jugador.GetTurnosEnCarcel()}/3)" : "No")}");
        jugador.EnviarMensaje($"Cartas Salir de Cárcel: {jugador.GetCartasSalirDeCarcel()}");
        jugador.EnviarMensaje($"Propiedades compradas ({jugador.GetPropiedades().Size()}):");

        // Recorrido de la lista enlazada de propiedades del jugador
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

    // Mostrar el tablero recorriendo la lista circular.
    public void VerTablero(Jugador jugador)
    {
        jugador.EnviarMensaje("\n=== TABLERO (LISTA CIRCULAR) ===");

        Node? actual = this.tablero.GetHead();
        int total = this.tablero.Size();

        // Recorremos la lista circular una sola vuelta (total nodos)
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