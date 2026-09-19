using System;
using System.IO;

/// <summary>
/// Representa a un participante del juego, ya sea registrado por RFID o por red/consola.
/// Mantiene su inventario de propiedades en una LinkedList propia y su posición en un Node del tablero.
/// </summary>
public class Jugador
{
    // Identificador único numérico (1, 2, 3, 4...).
    public int Id { get; set; }

    // Nombre visible del jugador.
    public string Nombre { get; set; }

    // UID de la tarjeta RFID, conservado como texto.
    public string IdTarjeta { get; set; }

    // Nodo del tablero donde se encuentra actualmente el jugador (lista circular).
    public Node? Posicion { get; set; }

    // Balance / Dinero que tiene el jugador.
    public int Balance { get; set; }

    // Propiedad Dinero como alias de Balance para compatibilidad en todo el código.
    public int Dinero
    {
        get => this.Balance;
        set => this.Balance = value;
    }

    // Inventario personal de propiedades, administrado mediante una LinkedList independiente.
    public LinkedList Propiedades { get; set; }

    // Cantidad de cartas de salir de la cárcel que posee el jugador.
    public int CartasSalirDeCarcel { get; set; }

    // Indica si el jugador está encarcelado.
    public bool EnCarcel { get; set; }

    // Contador de turnos transcurridos en la cárcel.
    public int TurnosEnCarcel { get; set; }

    // Indica si el jugador debe perder su siguiente turno por efecto de carta.
    public bool PierdeSiguienteTurno { get; set; }

    // Flujo de escritura TCP (StreamWriter) asociado a este cliente, o null si juega en consola.
    public StreamWriter? Writer { get; set; }

    // Constructor para registro físico vía RFID / Raspberry Pi Pico.
    public Jugador(string nombre, string idTarjeta, int balance_incial = 1500)
    {
        this.Id = 0;
        this.Nombre = nombre;
        this.IdTarjeta = idTarjeta;
        this.Posicion = null;
        this.Balance = balance_incial;
        this.Propiedades = new LinkedList();
        this.CartasSalirDeCarcel = 0;
        this.EnCarcel = false;
        this.TurnosEnCarcel = 0;
        this.PierdeSiguienteTurno = false;
        this.Writer = null;
    }

    // Constructor para servidor / red / terminal virtual.
    public Jugador(int id, string nombre, StreamWriter? writer = null, int balance_inicial = 1500)
    {
        this.Id = id;
        this.Nombre = nombre;
        this.IdTarjeta = string.Empty;
        this.Posicion = null;
        this.Balance = balance_inicial;
        this.Propiedades = new LinkedList();
        this.CartasSalirDeCarcel = 0;
        this.EnCarcel = false;
        this.TurnosEnCarcel = 0;
        this.PierdeSiguienteTurno = false;
        this.Writer = writer;
    }

    // Envía un mensaje al cliente. Si tiene conexión TCP activa, lo envía por el socket;
    // de lo contrario, lo muestra en la consola local del servidor.
    public void EnviarMensaje(string mensaje)
    {
        try
        {
            if (this.Writer != null)
            {
                this.Writer.WriteLine(mensaje);
            }
            else
            {
                Console.WriteLine($"[{this.Nombre}] {mensaje}");
            }
        }
        catch
        {
            // Captura de desconexión abrupta del socket
        }
    }

    // Devuelve el objeto Casilla almacenado dentro del Node actual de posición.
    public Casilla? ObtenerCasillaActual()
    {
        return this.Posicion?.GetData() as Casilla;
    }

    public override string ToString()
    {
        return this.Nombre;
    }
}