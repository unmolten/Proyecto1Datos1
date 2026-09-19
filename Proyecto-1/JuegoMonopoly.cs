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

    // Constantes del juego
    private const int BONO_SALIDA = 200;
    private const int FIANZA_CARCEL = 50;

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
        // NOTA: los nombres de color deben coincidir EXACTAMENTE con las claves
        // del switch en Propiedad.ObtenerMultiplicador (incluyendo espacios).
        this.tablero.InsertEnd(new CasillaEspecial(0, "Salida (GO)", "Salida"));

        this.tablero.InsertEnd(new Propiedad(1, "Casa de tierra", "Propiedad", 60, 5, colorGrupo: "Marrón"));
        this.tablero.InsertEnd(new Propiedad(2, "Cueva provisional", "Propiedad", 60, 5, colorGrupo: "Marrón"));
        this.tablero.InsertEnd(new CasillaEvento(3, "Arca Comunal", "ArcaComunal"));
        this.tablero.InsertEnd(new Propiedad(4, "Tren de la aldea", "Propiedad", 200, 25, colorGrupo: "Ferrocarril"));
        this.tablero.InsertEnd(new Propiedad(5, "Puesto de saqueador", "Propiedad", 100, 10, colorGrupo: "Celeste"));
        this.tablero.InsertEnd(new Propiedad(6, "Aldea esmeraldil", "Propiedad", 100, 10, colorGrupo: "Celeste"));
        this.tablero.InsertEnd(new CasillaEspecial(7, "Impuesto sobre la renta", "Impuesto") { Monto = 200 });
        this.tablero.InsertEnd(new CasillaEspecial(8, "Carcel", "Carcel"));
        this.tablero.InsertEnd(new Propiedad(9, "Geoda de amatista", "Propiedad", 140, 15, colorGrupo: "Morado"));
        this.tablero.InsertEnd(new Propiedad(10, "Mina de oro", "Propiedad", 140, 15, colorGrupo: "Morado"));
        this.tablero.InsertEnd(new CasillaEvento(11, "Fortuna", "Fortuna"));
        this.tablero.InsertEnd(new Propiedad(12, "Tren a las minas", "Propiedad", 200, 25, colorGrupo: "Ferrocarril"));
        this.tablero.InsertEnd(new Propiedad(13, "Runa oceanica", "Propiedad", 180, 20, colorGrupo: "Naranja"));
        this.tablero.InsertEnd(new Propiedad(14, "Barco hundido", "Propiedad", 180, 20, colorGrupo: "Naranja"));
        this.tablero.InsertEnd(new Propiedad(15, "Monumento oceanico", "Propiedad", 200, 24, colorGrupo: "Naranja"));
        this.tablero.InsertEnd(new CasillaEspecial(16, "Parada Libre", "ParadaLibre"));
        this.tablero.InsertEnd(new Propiedad(17, "Templo del desierto", "Propiedad", 220, 20, colorGrupo: "Rojo"));
        this.tablero.InsertEnd(new CasillaEvento(18, "Arca Comunal", "ArcaComunal"));
        this.tablero.InsertEnd(new Propiedad(19, "Trial Chamber", "Propiedad", 220, 20, colorGrupo: "Rojo"));
        this.tablero.InsertEnd(new Propiedad(20, "Tren a los portales", "Propiedad", 200, 25, colorGrupo: "Ferrocarril"));
        this.tablero.InsertEnd(new Propiedad(21, "Ciudad Antigua", "Propiedad", 240, 25, colorGrupo: "Amarillo"));
        this.tablero.InsertEnd(new Propiedad(22, "Portal al Nether", "Propiedad", 240, 25, colorGrupo: "Amarillo"));
        this.tablero.InsertEnd(new Propiedad(23, "Portal al End", "Propiedad", 260, 28, colorGrupo: "Amarillo"));
        this.tablero.InsertEnd(new CasillaEspecial(24, "Vaya a la carcel", "VayaALaCarcel"));
        this.tablero.InsertEnd(new Propiedad(25, "Charco de lava", "Propiedad", 280, 30, colorGrupo: "Verde"));
        this.tablero.InsertEnd(new Propiedad(26, "Fortaleza del Nether", "Propiedad", 280, 30, colorGrupo: "Verde"));
        this.tablero.InsertEnd(new Propiedad(27, "Bastion del Nether", "Propiedad", 300, 32, colorGrupo: "Verde"));
        this.tablero.InsertEnd(new Propiedad(28, "Tren a las Farlands", "Propiedad", 200, 25, colorGrupo: "Ferrocarril"));
        this.tablero.InsertEnd(new Propiedad(29, "Ciudad del End", "Propiedad", 350, 35, colorGrupo: "Azul Oscuro"));
        this.tablero.InsertEnd(new CasillaEvento(30, "Fortuna", "Fortuna"));
        this.tablero.InsertEnd(new Propiedad(31, "Barco del End", "Propiedad", 350, 35, colorGrupo: "Azul Oscuro"));
        this.tablero.InsertEnd(new CasillaEspecial(32, "Impuesto de Lujo", "Impuesto") { Monto = 100 });

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

    // Tirar dados y avanzar por la lista enlazada.
    public void TirarDados(Jugador jugador)
    {
        // 1. Tirar dos dados
        int dado1 = random.Next(1, 7);
        int dado2 = random.Next(1, 7);
        int total = dado1 + dado2;

        jugador.EnviarMensaje($"Tiraste: [{dado1}] + [{dado2}] = {total}");

        // 2. RECORRIDO DE LA LISTA ENLAZADA CIRCULAR:
        // Avanzamos 'total' nodos hacia adelante utilizando 'GetNext()'
        for (int i = 0; i < total; i++)
        {
            jugador.Posicion = jugador.Posicion?.GetNext();

            // Bono al pasar por Salida (cabeza de la lista circular).
            // Nota: si cae exactamente en la cabeza, también se le da el bono,
            // lo cual es consistente con la regla clásica (caer en GO = $200).
            if (jugador.Posicion == this.tablero.GetHead())
            {
                jugador.Dinero += BONO_SALIDA;
                jugador.EnviarMensaje($"Pasaste por Salida. Recibes ${BONO_SALIDA}.");
            }
        }

        Casilla? actual = jugador.ObtenerCasillaActual();
        jugador.EnviarMensaje($"Ahora estás en: {actual?.Nombre} ({actual?.Tipo})");

        // Procesar las reglas de la casilla donde cayó
        ProcesarCasilla(jugador, actual);
    }

    // Aplica las reglas de la casilla donde cayó el jugador.
    private void ProcesarCasilla(Jugador jugador, Casilla? casilla)
    {
        if (casilla == null) return;

        switch (casilla.Tipo)
        {
            case "Propiedad":
                ProcesarPropiedad(jugador, (Propiedad)casilla);
                break;

            case "Impuesto":
                int monto = ((CasillaEspecial)casilla).Monto;
                jugador.Dinero -= monto;
                jugador.EnviarMensaje($"Pagaste ${monto} de impuesto.");
                Broadcast($"{jugador.Nombre} pagó ${monto} de impuesto.", jugador);
                break;

            case "VayaALaCarcel":
                MoverJugadorACarcel(jugador);
                break;

            case "Fortuna":
                // TODO: robar una CartaEvento del mazo Fortuna
                jugador.EnviarMensaje("Fortuna: (pendiente de implementar mazo de cartas).");
                break;

            case "ArcaComunal":
                // TODO: robar una CartaEvento del mazo Arca Comunal
                jugador.EnviarMensaje("Arca Comunal: (pendiente de implementar mazo de cartas).");
                break;

            case "ParadaLibre":
                jugador.EnviarMensaje("Parada Libre. No pasa nada.");
                break;

            case "Carcel":
                jugador.EnviarMensaje("Solo de visita. No pasa nada.");
                break;

            case "Salida":
                // El bono ya se dio durante el recorrido, pero si cae justo aquí
                // ya se le sumó igual. No hacemos nada extra.
                break;
        }
    }

    // Reglas específicas al caer en una Propiedad.
    private void ProcesarPropiedad(Jugador jugador, Propiedad prop)
    {
        if (!prop.TienePropietario())
        {
            jugador.EnviarMensaje($"{prop.Nombre} está disponible por ${prop.PrecioCompra}. Usa 'comprar' para adquirirla.");
            return;
        }

        if (prop.Propietario == jugador)
        {
            jugador.EnviarMensaje($"{prop.Nombre} ya es tuya.");
            return;
        }

        if (prop.IsHipotecada)
        {
            jugador.EnviarMensaje($"{prop.Nombre} está hipotecada. No pagas renta.");
            return;
        }

        // Calcular renta: ferrocarril o propiedad de color.
        int renta;
        bool monopolio = false;

        if (prop.EsFerrocarril())
        {
            int cantidadFC = ContarFerrocarriles(prop.Propietario!);
            renta = prop.CalcularRenta(ferrocarrilesDelDueño: cantidadFC);

            jugador.EnviarMensaje($"Pagaste ${renta} de renta a {prop.Propietario!.Nombre} por {prop.Nombre} " +
                                  $"({cantidadFC} ferrocarril(es) en su poder).");
            Broadcast($"{jugador.Nombre} pagó ${renta} de renta a {prop.Propietario.Nombre} por {prop.Nombre}.",
                      excluir: jugador);
        }
        else
        {
            monopolio = TieneMonopolio(prop.Propietario!, prop.ColorGrupo);
            renta = prop.CalcularRenta(monopolio);

            jugador.EnviarMensaje($"Pagaste ${renta} de renta a {prop.Propietario!.Nombre} por {prop.Nombre}" +
                                  (monopolio ? " (con monopolio)" : "") + ".");
            Broadcast($"{jugador.Nombre} pagó ${renta} de renta a {prop.Propietario.Nombre} por {prop.Nombre}.",
                      excluir: jugador);
        }

        // Mover dinero
        jugador.Dinero -= renta;
        prop.Propietario!.Dinero += renta;
    }

    // Cuenta cuántos ferrocarriles posee un jugador en todo el tablero.
    private int ContarFerrocarriles(Jugador dueño)
    {
        int contador = 0;

        Node? actual = this.tablero.GetHead();
        int size = this.tablero.Size();

        for (int i = 0; i < size && actual != null; i++)
        {
            if (actual.GetData() is Propiedad p
                && p.EsFerrocarril()
                && p.Propietario == dueño)
            {
                contador++;
            }
            actual = actual.GetNext();
        }

        return contador;
    }

    // Devuelve true si 'dueño' posee TODAS las propiedades del 'colorGrupo' en el tablero.
    // Recorre la lista enlazada circular una sola vuelta.
    private bool TieneMonopolio(Jugador dueño, string colorGrupo)
    {
        // Los ferrocarriles no forman monopolio de color para efectos de renta.
        if (colorGrupo == "Ferrocarril") return false;

        int total = 0;
        int poseidas = 0;

        Node? actual = this.tablero.GetHead();
        int size = this.tablero.Size();

        for (int i = 0; i < size && actual != null; i++)
        {
            if (actual.GetData() is Propiedad p && p.ColorGrupo == colorGrupo)
            {
                total++;
                if (p.Propietario == dueño) poseidas++;
            }
            actual = actual.GetNext();
        }

        return total > 0 && total == poseidas;
    }

    // Teletransporta al jugador a la casilla de la Cárcel (buscándola por Tipo).
    private void MoverJugadorACarcel(Jugador jugador)
    {
        Node? actual = this.tablero.GetHead();
        int size = this.tablero.Size();

        for (int i = 0; i < size && actual != null; i++)
        {
            if (actual.GetData() is Casilla c && c.Tipo == "Carcel")
            {
                jugador.Posicion = actual;
                jugador.EnviarMensaje("Vas a la cárcel.");
                Broadcast($"{jugador.Nombre} fue enviado a la cárcel.", jugador);
                return;
            }
            actual = actual.GetNext();
        }
    }

    // Comprar la propiedad de la casilla actual.
    public void ComprarPropiedad(Jugador jugador)
    {
        Casilla? casilla = jugador.ObtenerCasillaActual();
        if (casilla == null) return;

        if (casilla is not Propiedad prop)
        {
            jugador.EnviarMensaje("Esta casilla no se puede comprar.");
            return;
        }

        if (prop.TienePropietario())
        {
            jugador.EnviarMensaje("Esta propiedad ya tiene dueño.");
            return;
        }

        if (prop.PrecioCompra <= 0)
        {
            jugador.EnviarMensaje("Esta casilla no está en venta.");
            return;
        }

        if (jugador.Dinero < prop.PrecioCompra)
        {
            jugador.EnviarMensaje($"No te alcanza. Necesitas ${prop.PrecioCompra} y tienes ${jugador.Dinero}.");
            return;
        }

        jugador.Dinero -= prop.PrecioCompra;
        prop.Propietario = jugador;

        // USO DE LISTA ENLAZADA: Guardamos la casilla en el inventario del jugador
        jugador.Propiedades.InsertEnd(prop);

        jugador.EnviarMensaje($"Has comprado {prop.Nombre} por ${prop.PrecioCompra}.");
        Broadcast($"{jugador.Nombre} compró {prop.Nombre} por ${prop.PrecioCompra}.", jugador);
    }

    // Consultar estado del jugador y recorrer su lista enlazada de propiedades.
    public void VerEstado(Jugador jugador)
    {
        Casilla? actual = jugador.ObtenerCasillaActual();

        jugador.EnviarMensaje($"\n=== ESTADO DE {jugador.Nombre} ===");
        jugador.EnviarMensaje($"Dinero: ${jugador.Dinero}");
        jugador.EnviarMensaje($"Casilla actual: {actual?.Nombre ?? "Ninguna"}");
        jugador.EnviarMensaje($"Propiedades compradas ({jugador.Propiedades.Size()}):");

        Node? temp = jugador.Propiedades.GetHead();
        while (temp != null)
        {
            if (temp.GetData() is Propiedad p)
            {
                int rentaActual;
                string detalle;

                if (p.EsFerrocarril())
                {
                    int cantidadFC = ContarFerrocarriles(jugador);
                    rentaActual = p.CalcularRenta(ferrocarrilesDelDueño: cantidadFC);
                    detalle = $"{cantidadFC} ferrocarril(es)";
                }
                else
                {
                    bool mono = TieneMonopolio(jugador, p.ColorGrupo);
                    rentaActual = p.CalcularRenta(mono);
                    detalle = p.ColorGrupo;
                }

                string estado = p.IsHipotecada ? " [HIPOTECADA]" : "";
                string casas = (!p.EsFerrocarril() && p.CantidadCasas > 0)
                    ? (p.CantidadCasas == 5 ? " [HOTEL]" : $" [{p.CantidadCasas} casa(s)]")
                    : "";

                jugador.EnviarMensaje($"  - [{p.Posicion}] {p.Nombre} ({detalle}) - Renta actual: ${rentaActual}{casas}{estado}");
            }
            temp = temp.GetNext();
        }
    }

    // Mostrar el tablero recorriendo la lista circular.
    public void VerTablero(Jugador jugador)
    {
        jugador.EnviarMensaje("\n=== TABLERO (LISTA CIRCULAR) ===");

        Node? actual = this.tablero.GetHead();
        int total = this.tablero.Size();

        for (int i = 0; i < total && actual != null; i++)
        {
            if (actual.GetData() is Casilla c)
            {
                string dueño = "";
                if (c is Propiedad p && p.Propietario != null)
                {
                    dueño = $" [Dueño: {p.Propietario.Nombre}]";
                }
                jugador.EnviarMensaje($"[{c.Posicion}] {c.Nombre} ({c.Tipo}){dueño}");
            }
            actual = actual.GetNext();
        }
    }

    // Salir de la cárcel pagando fianza.
    public void SalirDeCarcelConPago(Jugador jugador)
    {
        // Validación básica: solo puede pagar si está en la cárcel.
        Casilla? actual = jugador.ObtenerCasillaActual();
        if (actual == null || actual.Tipo != "Carcel")
        {
            jugador.EnviarMensaje("No estás en la cárcel.");
            return;
        }

        if (jugador.Dinero < FIANZA_CARCEL)
        {
            jugador.EnviarMensaje($"No tienes suficiente para la fianza (${FIANZA_CARCEL}).");
            return;
        }

        jugador.Dinero -= FIANZA_CARCEL;
        jugador.EnviarMensaje($"Pagaste ${FIANZA_CARCEL} de fianza. Puedes salir de la cárcel en tu próximo turno.");
        Broadcast($"{jugador.Nombre} pagó la fianza y saldrá de la cárcel.", jugador);

        // TODO: Implementar control de turnos (si queda libre inmediatamente o espera un turno).
    }
}