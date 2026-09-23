using System;
using System.IO;
using System.Collections.Generic;

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
    private List<StreamWriter> espectadoresGodot = new List<StreamWriter>();

    // Program.cs llama esto cada vez que una instancia de Godot se conecta
    public void ConectarEspectadorGodot(StreamWriter writer)
    {
        lock (this.espectadoresGodot)
        {
            this.espectadoresGodot.Add(writer);
        }

        // Como la partida puede que ya haya empezado, le mandamos el estado
        // actual para que no arranque con el tablero vacio

        // Sincroniza a los jugadores que ya estaban
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

        // Sincroniza las casas/hoteles que ya se hayan construido
        Node? nodoCasilla = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (nodoCasilla?.GetData() is Propiedad p && p.GetCantidadCasas() > 0)
            {
                EnviarAEspectador(writer, $"jugador/0/comprarcasa/{p.GetCantidadCasas()}/casilla/{p.GetPosicion()}");
            }
            nodoCasilla = nodoCasilla?.GetNext();
        }
    }

    // Manda una linea nada mas a un espectador (usado para la sincronizacion inicial)
    private void EnviarAEspectador(StreamWriter writer, string mensaje)
    {
        try { writer.WriteLine(mensaje); } catch { /* se limpia sola cuando falle el broadcast normal */ }
    }

    // Manda una linea a TODAS las instancias de Godot conectadas
    // esto usa el protocolo que ya entiende networkclient.gd (jugador/id/accion/...)
    private void GodotBroadcast(string mensaje)
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

        // Ejecuta la transacción de compra
        new Transaccion(propiedad.GetPrecioCompra(), GetTurnoActual(), "Compra de propiedad", jugador, null);
        propiedad.SetPropietario(jugador);

        // USO DE LISTA ENLAZADA: Guardamos la casilla en el inventario del jugador
        jugador.GetPropiedades().InsertEnd(propiedad);

        jugador.EnviarMensaje($"🎉 ¡Has comprado '{propiedad.GetNombre()}' por ${propiedad.GetPrecioCompra()}!");
        jugador.EnviarMensaje($"Saldo restante: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} compró '{propiedad.GetNombre()}'!", jugador);
    }

    // Comprar una casa u hotel en la propiedad actual si le pertenece al jugador.
    public void ComprarCasa(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();
        if (actual is not Propiedad propiedad || propiedad.GetPropietario() != jugador)
        {
            jugador.EnviarMensaje("❌ Debes estar en una propiedad que te pertenezca para construir.");
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

        new Transaccion(costoCasa, GetTurnoActual(), "Pago al banco", jugador, null);
        propiedad.SetCantidadCasas(propiedad.GetCantidadCasas() + 1);
        string mejora = propiedad.GetCantidadCasas() == 5 ? "un Hotel" : $"la casa #{propiedad.GetCantidadCasas()}";
        jugador.EnviarMensaje($"🏗️ ¡Construiste {mejora} en '{propiedad.GetNombre()}' por ${costoCasa}!");
        jugador.EnviarMensaje($"Nueva renta: ${propiedad.CalcularRenta()}. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} construyó {mejora} en '{propiedad.GetNombre()}'.", jugador);
        GodotBroadcast($"jugador/{jugador.GetId()}/comprarcasa/{propiedad.GetCantidadCasas()}/casilla/{propiedad.GetPosicion()}");
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

        new Transaccion(fianza, GetTurnoActual(), "Pago al banco", jugador, null);
        jugador.SetEnCarcel(false);
        jugador.SetTurnosEnCarcel(0);
        jugador.EnviarMensaje($"💵 Pagaste ${fianza} de fianza y has salido de la Cárcel. Saldo: ${jugador.GetDinero()}");
        Broadcast($"📢 {jugador.GetNombre()} pagó la fianza y salió de la Cárcel.", jugador);
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