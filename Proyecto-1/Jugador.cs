using System;
using System.IO;

/// <summary>
/// Representa a un participante del juego, ya sea registrado por RFID o por red/consola.
/// Mantiene su inventario de propiedades en una LinkedList propia y su posición en un Node del tablero.
/// </summary>
public class Jugador
{
    // Campos privados
    private int id;
    private string nombre;
    private string idTarjeta;
    private Node? posicion;
    private int balance;
    private LinkedList propiedades;
    private int cartasSalirDeCarcel;
    private bool enCarcel;
    private int turnosEnCarcel;
    private bool pierdeSiguienteTurno;
    private StreamWriter? writer;

    // Métodos individuales para obtener y definir (Getters y Setters)
    public int GetId()
    {
        return this.id;
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public string GetNombre()
    {
        return this.nombre;
    }

    public void SetNombre(string nombre)
    {
        this.nombre = nombre;
    }

    public string GetIdTarjeta()
    {
        return this.idTarjeta;
    }

    public void SetIdTarjeta(string idTarjeta)
    {
        this.idTarjeta = idTarjeta;
    }

    public Node? GetPosicion()
    {
        return this.posicion;
    }

    public void SetPosicion(Node? posicion)
    {
        this.posicion = posicion;
    }

    public int GetBalance()
    {
        return this.balance;
    }

    public void SetBalance(int balance)
    {
        this.balance = balance;
    }

    public int GetDinero()
    {
        return this.balance;
    }

    public void SetDinero(int dinero)
    {
        this.balance = dinero;
    }

    public LinkedList GetPropiedades()
    {
        return this.propiedades;
    }

    public void SetPropiedades(LinkedList propiedades)
    {
        this.propiedades = propiedades;
    }

    public int GetCartasSalirDeCarcel()
    {
        return this.cartasSalirDeCarcel;
    }

    public void SetCartasSalirDeCarcel(int cartasSalirDeCarcel)
    {
        this.cartasSalirDeCarcel = cartasSalirDeCarcel;
    }

    public bool GetEnCarcel()
    {
        return this.enCarcel;
    }

    public void SetEnCarcel(bool enCarcel)
    {
        this.enCarcel = enCarcel;
    }

    public int GetTurnosEnCarcel()
    {
        return this.turnosEnCarcel;
    }

    public void SetTurnosEnCarcel(int turnosEnCarcel)
    {
        this.turnosEnCarcel = turnosEnCarcel;
    }

    public bool GetPierdeSiguienteTurno()
    {
        return this.pierdeSiguienteTurno;
    }

    public void SetPierdeSiguienteTurno(bool pierdeSiguienteTurno)
    {
        this.pierdeSiguienteTurno = pierdeSiguienteTurno;
    }

    public StreamWriter? GetWriter()
    {
        return this.writer;
    }

    public void SetWriter(StreamWriter? writer)
    {
        this.writer = writer;
    }

    // Constructor para registro físico vía RFID / Raspberry Pi Pico.
    public Jugador(string nombre, string idTarjeta, int balance_incial = 1500)
    {
        this.id = 0;
        this.nombre = nombre;
        this.idTarjeta = idTarjeta;
        this.posicion = null;
        this.balance = balance_incial;
        this.propiedades = new LinkedList();
        this.cartasSalirDeCarcel = 0;
        this.enCarcel = false;
        this.turnosEnCarcel = 0;
        this.pierdeSiguienteTurno = false;
        this.writer = null;
    }

    // Constructor para servidor / red / terminal virtual.
    public Jugador(int id, string nombre, StreamWriter? writer = null, int balance_inicial = 1500)
    {
        this.id = id;
        this.nombre = nombre;
        this.idTarjeta = string.Empty;
        this.posicion = null;
        this.balance = balance_inicial;
        this.propiedades = new LinkedList();
        this.cartasSalirDeCarcel = 0;
        this.enCarcel = false;
        this.turnosEnCarcel = 0;
        this.pierdeSiguienteTurno = false;
        this.writer = writer;
    }

    // Envía un mensaje al cliente. Si tiene conexión TCP activa, lo envía por el socket;
    // de lo contrario, lo muestra en la consola local del servidor.
    public void EnviarMensaje(string mensaje)
    {
        try
        {
            if (this.writer != null)
            {
                this.writer.WriteLine(mensaje);
            }
            else
            {
                Console.WriteLine($"[{this.nombre}] {mensaje}");
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
        return this.posicion?.GetData() as Casilla;
    }

    public override string ToString()
    {
        return this.nombre;
    }
}