using System;
using System.Collections.Generic;
using System.IO;

// PLANTILLA BASE: JuegoMonopoly
// 
// Esta clase sirve como una estructura limpia y básica (template) para que el equipo
// pueda implementar e iterar las reglas completas de Monopoly más adelante.
// 
// Conexión con Listas Enlazadas:
// - El tablero es una lista enlazada circular ('LinkedList' + 'MakeCircular()').
// - Los jugadores avanzan en el tablero de nodo en nodo con 'jugador.Posicion = jugador.Posicion.GetNext()'.
// - Cada jugador tiene su propia lista enlazada ('jugador.Propiedades') para guardar sus compras.
public class JuegoMonopoly
{
    // Patrón Singleton: Una única instancia para coordinar el juego en el servidor
    private static JuegoMonopoly? instancia;
    public static JuegoMonopoly Instancia => instancia ??= new JuegoMonopoly();

    // Estructura de datos del tablero (Lista Enlazada Circular)
    private LinkedList tablero;

    // Lista de jugadores conectados al servidor
    private List<Jugador> jugadores;

    // Contador para asignar IDs a los nuevos jugadores
    private int contadorJugadores = 1;

    // Generador de números aleatorios para los dados
    private Random random = new Random();

    // Getters para acceder al tablero y los jugadores
    public LinkedList Tablero => this.tablero;
    public List<Jugador> Jugadores => this.jugadores;

    public JuegoMonopoly()
    {
        this.tablero = new LinkedList();
        this.jugadores = new List<Jugador>();
        InicializarTablero();
    }

    // Inicializa las casillas iniciales del tablero en la lista enlazada y la hace circular.
    private void InicializarTablero()
    {

//    public Propiedad(int posicion, string nombre, string tipo, int precioCompra, int alquilerBase, Jugador propietario, string colorGrupo)
//    : base(posicion, nombre, tipo)

        // TODO: Modificar, agregar o personalizar las casillas del tablero a su gusto
        this.tablero.InsertEnd(new CasillaEspecial(0, "Salida (GO)", "Salida"));
        this.tablero.InsertEnd(new Propiedad(1, "Casa de tierra", "Propiedad", 60, 5));
        this.tablero.InsertEnd(new Propiedad(2, "Cueva provisional", "Propiedad", 60, 5));
        this.tablero.InsertEnd(new CasillaEvento(3, "Arca Comunal", "ArcaComunal"));
        this.tablero.InsertEnd(new Propiedad(4, "Tren de la aldea", "Propiedad", 200, 25));
        this.tablero.InsertEnd(new Propiedad(5, "Puesto de saqueador", "Propiedad", 100, 10));
        this.tablero.InsertEnd(new Propiedad(6, "Aldea esmeraldil", "Propiedad", 100, 10));
        this.tablero.InsertEnd(new CasillaEspecial(7, "Impuesto sobre la renta", "Impuesto"));
        this.tablero.InsertEnd(new CasillaEspecial(8, "Carcel", "Carcel"));
        this.tablero.InsertEnd(new Propiedad(9, "Geoda de amatista", "Propiedad", 140, 15));
        this.tablero.InsertEnd(new Propiedad(10, "Mina de oro", "Propiedad", 140, 15));
        this.tablero.InsertEnd(new CasillaEvento(11, "Fortuna", "Fortuna"));
        this.tablero.InsertEnd(new Propiedad(12, "Tren a las minas", "Propiedad", 200, 25));
        this.tablero.InsertEnd(new Propiedad(13, "Runa oceanica", "Propiedad", 180, 20));
        this.tablero.InsertEnd(new Propiedad(14, "Barco hundido", "Propiedad", 180, 20));
        this.tablero.InsertEnd(new Propiedad(15, "Monumento oceanico", "Propiedad", 200, 24));
        this.tablero.InsertEnd(new CasillaEspecial(16, "Parada Libre", "ParadaLibre"));
        this.tablero.InsertEnd(new Propiedad(17, "Templo del desierto", "Propiedad", 220, 20));
        this.tablero.InsertEnd(new CasillaEvento(18, "Arca Comunal", "ArcaComunal"));
        this.tablero.InsertEnd(new Propiedad(19, "Trial Chamber", "Propiedad", 220, 20));
        this.tablero.InsertEnd(new Propiedad(20, "Tren a los portales", "Propiedad", 200, 25));
        this.tablero.InsertEnd(new Propiedad(21, "Ciudad Antigua", "Propiedad", 240, 25));
        this.tablero.InsertEnd(new Propiedad(22, "Portal al Nether", "Propiedad", 240, 25));
        this.tablero.InsertEnd(new Propiedad(23, "Portal al End", "Propiedad", 260, 28));
        this.tablero.InsertEnd(new CasillaEspecial(24, "Vaya a la carcel", "VayaALaCarcel"));
        this.tablero.InsertEnd(new Propiedad(25, "Charco de lava", "Propiedad", 280, 30));
        this.tablero.InsertEnd(new Propiedad(26, "Fortaleza del Nether", "Propiedad", 280, 30));
        this.tablero.InsertEnd(new Propiedad(27, "Bastion del Nether", "Propiedad", 300, 32));
        this.tablero.InsertEnd(new Propiedad(28, "Tren a los portales", "Propiedad", 200, 25));
        this.tablero.InsertEnd(new Propiedad(29, "Ciudad del End", "Propiedad", 350, 35));
        this.tablero.InsertEnd(new CasillaEvento(30, "Fortuna", "Fortuna"));
        this.tablero.InsertEnd(new Propiedad(31, "Barco del End", "Propiedad", 350, 35));        

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

        this.jugadores.Add(nuevo);
        Broadcast($"[SISTEMA] {nuevo.Nombre} se ha unido a la partida.");
        return nuevo;
    }

    // Remueve a un jugador cuando se desconecta.
    public void DesconectarJugador(Jugador jugador)
    {
        this.jugadores.Remove(jugador);
        Broadcast($"[SISTEMA] {jugador.Nombre} ha salido de la partida.");
    }

    // Envía un mensaje a todos los jugadores conectados.
    public void Broadcast(string mensaje, Jugador? excluir = null)
    {
        foreach (var j in this.jugadores)
        {
            if (excluir == null || j.Id != excluir.Id)
            {
                j.EnviarMensaje(mensaje);
            }
        }
    }

    // PLANTILLA: Tirar dados y avanzar por la lista enlazada.
    public void TirarDados(Jugador jugador)
    {
        // 1. Tirar dos dados
        int dado1 = random.Next(1, 7);
        int dado2 = random.Next(1, 7);
        int total = dado1 + dado2;

        jugador.EnviarMensaje($"🎲 Tiraste: [{dado1}] + [{dado2}] = {total}");

        // 2. RECORRIDO DE LA LISTA ENLAZADA CIRCULAR:
        // Avanzamos 'total' nodos hacia adelante utilizando 'GetNext()'
        for (int i = 0; i < total; i++)
        {
            jugador.Posicion = jugador.Posicion?.GetNext();

            // TODO: Implementar lógica de bono al pasar por Salida
            // Ejemplo: if (jugador.Posicion == this.tablero.GetHead()) { jugador.Dinero += 200; }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"📍 Ahora estás en: {actual?.Nombre} ({actual?.Tipo})");

        // TODO: Implementar las reglas de cada casilla (cobro de renta a rivales, cárcel, impuestos, etc.)
    }

    // PLANTILLA: Comprar la propiedad de la casilla actual.
    public void ComprarPropiedad(Jugador jugador)
    {
        Casilla? casilla = jugador.ObtenerCasillaActual();
        if (casilla == null) return;

        // TODO: Agregar validaciones completas (si el jugador tiene suficiente dinero, si ya tiene dueño, etc.)
        if (casilla.EsPropiedad() && !casilla.TienePropietario())
        {
            casilla.Propietario = jugador;

            // USO DE LISTA ENLAZADA: Guardamos la casilla en el inventario del jugador
            jugador.Propiedades.InsertEnd(casilla);

            jugador.EnviarMensaje($"🎉 ¡Has comprado {casilla.Nombre}!");
            Broadcast($"📢 {jugador.Nombre} compró {casilla.Nombre}!", jugador);
        }
        else
        {
            jugador.EnviarMensaje("❌ Esta casilla no se puede comprar o ya tiene dueño.");
        }
    }

    // PLANTILLA: Consultar estado del jugador y recorrer su lista enlazada de propiedades.
    public void VerEstado(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();

        jugador.EnviarMensaje($"\n=== ESTADO DE {jugador.Nombre} ===");
        jugador.EnviarMensaje($"Dinero: ${jugador.Dinero}");
        jugador.EnviarMensaje($"Casilla actual: {actual?.Nombre ?? "Ninguna"}");
        jugador.EnviarMensaje($"Propiedades compradas ({jugador.Propiedades.Size()}):");

        // Recorrido lineal simple de la lista enlazada de propiedades del jugador
        Node? temp = jugador.Propiedades.GetHead();
        while (temp != null)
        {
            if (temp.GetData() is Casilla c)
            {
                jugador.EnviarMensaje($"  - {c.Nombre} (Renta: ${c.Renta})");
            }
            temp = temp.GetNext();
        }

        // TODO: Personalizar el formato o agregar más estadísticas (turnos, bancarrota, etc.)
    }

    // PLANTILLA: Mostrar el tablero recorriendo la lista circular.
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
                string dueño = c.Propietario != null ? $" [Dueño: {c.Propietario.Nombre}]" : "";
                jugador.EnviarMensaje($"[{c.Posicion}] {c.Nombre} ({c.Tipo}){dueño}");
            }
            actual = actual?.GetNext();
        }

        // TODO: Agregar formato visual, precios, o mostrar en qué casilla está cada jugador
    }

    // PLANTILLA: Salir de la cárcel pagando fianza.
    public void SalirDeCarcelConPago(Jugador jugador)
    {
        // TODO: Implementar la lógica completa de la cárcel (costo de fianza, turnos en espera, etc.)
        jugador.EnviarMensaje("ℹ️ [TODO]: Implementar lógica de fianza para la cárcel.");
    }
}
