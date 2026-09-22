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
    public int TurnoActual { get; set; } = 1;
    private int contadorJugadores = 1;

    // Generador de números aleatorios para los dados
    private Random random = new Random();

    // Getters para acceder al tablero, jugadores y mazos
    public LinkedList Tablero => this.tablero;
    public LinkedList Jugadores => this.jugadores;
    public LinkedList MazoFortuna => this.mazoFortuna;
    public LinkedList MazoArcaComunal => this.mazoArcaComunal;

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
        Jugador nuevo = new Jugador(contadorJugadores, $"Jugador_{contadorJugadores}", writer)
        {
            Posicion = this.tablero.GetHead() // Inicia en la cabeza de la lista enlazada
        };
        contadorJugadores++;

        this.jugadores.InsertEnd(nuevo);
        Broadcast($"[SISTEMA] {nuevo.Nombre} se ha unido a la partida.");
        GodotBroadcast($"jugador/{nuevo.Id}/activar");
        return nuevo;
    }

    // Registra a un jugador ya existente (creado por RFID o consola) en la lista de jugadores.
    public void RegistrarJugadorExistente(Jugador jugador)
    {
        if (jugador.Id == 0)
        {
            jugador.Id = contadorJugadores++;
        }
        if (jugador.Posicion == null)
        {
            jugador.Posicion = this.tablero.GetHead();
        }
        this.jugadores.InsertEnd(jugador);
        Broadcast($"[SISTEMA] {jugador.Nombre} (ID: {jugador.Id}) está listo en el tablero.");
        GodotBroadcast($"jugador/{jugador.Id}/activar");
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
                EnviarAEspectador(writer, $"jugador/{j.Id}/activar");
                if (casillaJugador != null)
                {
                    EnviarAEspectador(writer, $"jugador/{j.Id}/mover/casilla/{casillaJugador.Posicion}");
                }
            }
            nodoJugador = nodoJugador?.GetNext();
        }

        // Sincroniza las casas/hoteles que ya se hayan construido
        Node? nodoCasilla = this.tablero.GetHead();
        for (int i = 0; i < this.tablero.Size(); i++)
        {
            if (nodoCasilla?.GetData() is Propiedad p && p.CantidadCasas > 0)
            {
                EnviarAEspectador(writer, $"jugador/0/comprarcasa/{p.CantidadCasas}/casilla/{p.Posicion}");
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
            GodotBroadcast($"jugador/{jugador.Id}/mover/casilla/{actual.Posicion}");
        }
    }

    // El Program.cs llama esto cuando un jugador queda eliminado por bancarrota
    public void NotificarEliminacion(Jugador jugador)
    {
        GodotBroadcast($"jugador/{jugador.Id}/desactivar");
    }

    // Remueve a un jugador cuando se desconecta.
    public void DesconectarJugador(Jugador jugador)
    {
        this.jugadores.Delete(jugador);
        Broadcast($"[SISTEMA] {jugador.Nombre} ha salido de la partida.");
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
                if (excluir == null || j.Id != excluir.Id)
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
        if (jugador.PierdeSiguienteTurno)
        {
            jugador.PierdeSiguienteTurno = false;
            jugador.EnviarMensaje("⏳ Perdiste este turno debido a un evento anterior.");
            Broadcast($"📢 {jugador.Nombre} perdió su turno.", jugador);
            return;
        }

        // 2. Tirar dos dados
        int dado1 = random.Next(1, 7);
        int dado2 = random.Next(1, 7);
        int total = dado1 + dado2;

        jugador.EnviarMensaje($"🎲 Tiraste: [{dado1}] + [{dado2}] = {total}");

        // 3. Manejo de estado en la cárcel
        if (jugador.EnCarcel)
        {
            if (dado1 == dado2)
            {
                jugador.EnCarcel = false;
                jugador.TurnosEnCarcel = 0;
                jugador.EnviarMensaje("🎉 ¡Sacaste dobles! Quedas libre de la Cárcel.");
                Broadcast($"📢 {jugador.Nombre} sacó dobles y salió libre de la Cárcel.", jugador);
            }
            else
            {
                jugador.TurnosEnCarcel++;
                jugador.EnviarMensaje($"🔒 No sacaste dobles. Sigues en la Cárcel ({jugador.TurnosEnCarcel}/3 turnos).");

                if (jugador.TurnosEnCarcel >= 3)
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
            jugador.Posicion = jugador.Posicion?.GetNext();

            // Bono por pasar por Salida (Casilla 0) antes de llegar al destino
            if (jugador.ObtenerCasillaActual()?.Posicion == 0 && i < total - 1)
            {
                new Transaccion(200, TurnoActual, "Premio por pasar por inicio", jugador, null);
                jugador.EnviarMensaje("💵 ¡Pasaste por Salida! Cobraste $200 de bono.");
                Broadcast($"📢 {jugador.Nombre} pasó por Salida y cobró $200.", jugador);
            }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"📍 Ahora estás en: {actual?.Nombre} ({actual?.Tipo})");
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
            string dueño = propiedad.Propietario == jugador ? "ya te pertenece" : $"le pertenece a {propiedad.Propietario!.Nombre}";
            jugador.EnviarMensaje($"❌ Esta propiedad {dueño}.");
            return;
        }

        if (jugador.Dinero < propiedad.PrecioCompra)
        {
            jugador.EnviarMensaje($"❌ Saldo insuficiente. Tienes ${jugador.Dinero} y cuesta ${propiedad.PrecioCompra}.");
            return;
        }

        // Ejecuta la transacción de compra
        new Transaccion(propiedad.PrecioCompra, TurnoActual, "Compra de propiedad", jugador, null);
        propiedad.Propietario = jugador;

        // USO DE LISTA ENLAZADA: Guardamos la casilla en el inventario del jugador
        jugador.Propiedades.InsertEnd(propiedad);

        jugador.EnviarMensaje($"🎉 ¡Has comprado '{propiedad.Nombre}' por ${propiedad.PrecioCompra}!");
        jugador.EnviarMensaje($"Saldo restante: ${jugador.Dinero}");
        Broadcast($"📢 {jugador.Nombre} compró '{propiedad.Nombre}'!", jugador);
    }

    // Comprar una casa u hotel en la propiedad actual si le pertenece al jugador.
    public void ComprarCasa(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();
        if (actual is not Propiedad propiedad || propiedad.Propietario != jugador)
        {
            jugador.EnviarMensaje("❌ Debes estar en una propiedad que te pertenezca para construir.");
            return;
        }

        if (propiedad.CantidadCasas >= 5)
        {
            jugador.EnviarMensaje("❌ Esta propiedad ya tiene un Hotel construido (nivel máximo).");
            return;
        }

        int costoCasa = propiedad.PrecioCompra / 2;
        if (jugador.Dinero < costoCasa)
        {
            jugador.EnviarMensaje($"❌ Saldo insuficiente. Construir cuesta ${costoCasa} y tienes ${jugador.Dinero}.");
            return;
        }

        new Transaccion(costoCasa, TurnoActual, "Pago al banco", jugador, null);
        propiedad.CantidadCasas++;
        string mejora = propiedad.CantidadCasas == 5 ? "un Hotel" : $"la casa #{propiedad.CantidadCasas}";
        jugador.EnviarMensaje($"🏗️ ¡Construiste {mejora} en '{propiedad.Nombre}' por ${costoCasa}!");
        jugador.EnviarMensaje($"Nueva renta: ${propiedad.CalcularRenta()}. Saldo: ${jugador.Dinero}");
        Broadcast($"📢 {jugador.Nombre} construyó {mejora} en '{propiedad.Nombre}'.", jugador);
        GodotBroadcast($"jugador/{jugador.Id}/comprarcasa/{propiedad.CantidadCasas}/casilla/{propiedad.Posicion}");
    }

    // Mueve al jugador a una casilla específica por su índice numérico (0 a 31).
    public void MoverJugadorACasilla(Jugador jugador, int posicionDestino)
    {
        if (jugador.Posicion == null)
        {
            jugador.Posicion = this.tablero.GetHead();
        }

        int total = this.tablero.Size();
        for (int i = 0; i < total; i++)
        {
            if (jugador.ObtenerCasillaActual()?.Posicion == posicionDestino)
            {
                break;
            }
            jugador.Posicion = jugador.Posicion?.GetNext();
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"📍 Te moviste a: {actual?.Nombre} ({actual?.Tipo})");
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
                jugador.Posicion = jugador.Posicion?.GetNext();
            }
        }
        else
        {
            for (int i = 0; i < Math.Abs(cantidad); i++)
            {
                jugador.Posicion = jugador.Posicion?.GetPrevious();
            }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"📍 Ahora estás en: {actual?.Nombre} ({actual?.Tipo})");
        AnunciarPosicion(jugador);
        actual?.Accion(jugador);
    }

    // Envía a un jugador directamente a la Cárcel (Casilla 8).
    public void EnviarACarcel(Jugador jugador)
    {
        jugador.EnCarcel = true;
        jugador.TurnosEnCarcel = 0;
        MoverJugadorACasilla(jugador, 8); // Casilla 8 es la Cárcel
        jugador.EnviarMensaje("🔒 Has sido encerrado en la Cárcel.");
        Broadcast($"🚨 {jugador.Nombre} fue enviado a la Cárcel.", jugador);
    }

    // Salir de la cárcel pagando fianza o usando carta.
    public void SalirDeCarcelConPago(Jugador jugador)
    {
        if (!jugador.EnCarcel)
        {
            jugador.EnviarMensaje("ℹ️ No estás en la Cárcel.");
            return;
        }

        if (jugador.CartasSalirDeCarcel > 0)
        {
            jugador.CartasSalirDeCarcel--;
            jugador.EnCarcel = false;
            jugador.TurnosEnCarcel = 0;
            jugador.EnviarMensaje("🎟️ ¡Usaste tu carta de Salir de la Cárcel Gratis y quedas en libertad!");
            Broadcast($"📢 {jugador.Nombre} usó una carta y salió de la Cárcel.", jugador);
            return;
        }

        int fianza = 50;
        if (jugador.Dinero < fianza)
        {
            jugador.EnviarMensaje($"❌ No tienes suficiente dinero (${jugador.Dinero}) para pagar la fianza (${fianza}).");
            return;
        }

        new Transaccion(fianza, TurnoActual, "Pago al banco", jugador, null);
        jugador.EnCarcel = false;
        jugador.TurnosEnCarcel = 0;
        jugador.EnviarMensaje($"💵 Pagaste ${fianza} de fianza y has salido de la Cárcel. Saldo: ${jugador.Dinero}");
        Broadcast($"📢 {jugador.Nombre} pagó la fianza y salió de la Cárcel.", jugador);
    }

    // Consultar estado del jugador y recorrer su lista enlazada de propiedades.
    public void VerEstado(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();

        jugador.EnviarMensaje($"\n=== ESTADO DE {jugador.Nombre} ===");
        jugador.EnviarMensaje($"Dinero: ${jugador.Dinero}");
        jugador.EnviarMensaje($"Casilla actual: {actual?.Nombre ?? "Ninguna"}");
        jugador.EnviarMensaje($"En Cárcel: {(jugador.EnCarcel ? $"Sí ({jugador.TurnosEnCarcel}/3)" : "No")}");
        jugador.EnviarMensaje($"Cartas Salir de Cárcel: {jugador.CartasSalirDeCarcel}");
        jugador.EnviarMensaje($"Propiedades compradas ({jugador.Propiedades.Size()}):");

        // Recorrido de la lista enlazada de propiedades del jugador
        Node? temp = jugador.Propiedades.GetHead();
        int totalPropiedades = jugador.Propiedades.Size();
        for (int i = 0; i < totalPropiedades; i++)
        {
            if (temp?.GetData() is Propiedad p)
            {
                string nivel = p.CantidadCasas == 5 ? "Hotel" : $"{p.CantidadCasas} casas";
                jugador.EnviarMensaje($"  - {p.Nombre} (Grupo: {p.ColorGrupo}, Renta: ${p.CalcularRenta()}, {nivel})");
            }
            else if (temp?.GetData() is Casilla c)
            {
                jugador.EnviarMensaje($"  - {c.Nombre} (Renta: ${c.Renta})");
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
                if (c is Propiedad p && p.Propietario != null)
                {
                    dueño = $" [Dueño: {p.Propietario.Nombre}]";
                }
                jugador.EnviarMensaje($"[{c.Posicion}] {c.Nombre} ({c.Tipo}){dueño}");
            }
            actual = actual?.GetNext();
        }
    }
}