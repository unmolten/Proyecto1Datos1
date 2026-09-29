using System;
using System.IO;

/**
 * @file Jugador.cs
 * @brief Definicion del modelo de participante en la partida de Monopoly.
 *
 * Mantiene el estado financiero (saldo), la posicion actual en el tablero (Node),
 * el inventario de propiedades adquiridas (LinkedList propia), estado de carcel
 * y referencias de comunicacion de red.
 */

/**
 * @class Jugador
 * @brief Representa a un jugador del juego, registrado via RFID o consola/red.
 */
public class Jugador
{
    /** @brief Identificador numerico unico asignado al jugador (1, 2, 3...). */
    private int id;

    /** @brief Nombre o apodo del participante. */
    private string nombre;

    /** @brief Codigo identificador UID de la tarjeta fisica RFID (en formato hexadecimal). */
    private string idTarjeta;

    /** @brief Referencia al nodo de la lista enlazada del tablero donde se ubica el jugador. */
    private Node? posicion;

    /** @brief Dinero o balance disponible para compras, rentas y pagos. */
    private int balance;

    /** @brief Lista enlazada que contiene las propiedades adquiridas por el jugador. */
    private LinkedList propiedades;

    /** @brief Cantidad de cartas para librarse de la carcel gratuitamente que posee. */
    private int cartasSalirDeCarcel;

    /** @brief Indica si el jugador se encuentra actualmente recluido en prision. */
    private bool enCarcel;

    /** @brief Contador de turnos consecutivos que el jugador ha permanecido en carcel. */
    private int turnosEnCarcel;

    /** @brief Bandera que se activa si una carta u orden le hace perder su proximo turno. */
    private bool pierdeSiguienteTurno;

    /** @brief Flujo de salida TCP para emitir mensajes directos a su cliente conectado. */
    private StreamWriter? writer;

    /**
     * @brief Obtiene el ID del jugador.
     * @return Identificador numerico.
     */
    public int GetId()
    {
        return this.id;
    }

    /**
     * @brief Asigna el ID del jugador.
     * @param id Nuevo identificador numerico.
     */
    public void SetId(int id)
    {
        this.id = id;
    }

    /**
     * @brief Obtiene el nombre del jugador.
     * @return Nombre textual.
     */
    public string GetNombre()
    {
        return this.nombre;
    }

    /**
     * @brief Asigna el nombre del jugador.
     * @param nombre Nuevo nombre a asignar.
     */
    public void SetNombre(string nombre)
    {
        this.nombre = nombre;
    }

    /**
     * @brief Obtiene el identificador UID de la tarjeta RFID.
     * @return Cadena con el UID hexadecimal.
     */
    public string GetIdTarjeta()
    {
        return this.idTarjeta;
    }

    /**
     * @brief Asigna el UID de la tarjeta RFID.
     * @param idTarjeta Nuevo UID de tarjeta.
     */
    public void SetIdTarjeta(string idTarjeta)
    {
        this.idTarjeta = idTarjeta;
    }

    /**
     * @brief Obtiene la referencia al nodo de la casilla donde se encuentra el jugador.
     * @return Nodo actual de la lista circular del tablero.
     */
    public Node? GetPosicion()
    {
        return this.posicion;
    }

    /**
     * @brief Establece la casilla actual asignando su nodo correspondiente.
     * @param posicion Nuevo nodo de casilla.
     */
    public void SetPosicion(Node? posicion)
    {
        this.posicion = posicion;
    }

    /**
     * @brief Obtiene el balance financiero del jugador.
     * @return Saldo disponible.
     */
    public int GetBalance()
    {
        return this.balance;
    }

    /**
     * @brief Modifica el balance financiero del jugador.
     * @param balance Nuevo saldo disponible.
     */
    public void SetBalance(int balance)
    {
        this.balance = balance;
    }

    /**
     * @brief Metodo de conveniencia equivalente a GetBalance.
     * @return Saldo disponible.
     */
    public int GetDinero()
    {
        return this.balance;
    }

    /**
     * @brief Metodo de conveniencia equivalente a SetBalance.
     * @param dinero Nuevo saldo a fijar.
     */
    public void SetDinero(int dinero)
    {
        this.balance = dinero;
    }

    /**
     * @brief Obtiene la lista enlazada de propiedades adquiridas.
     * @return LinkedList con los bienes inmuebles del jugador.
     */
    public LinkedList GetPropiedades()
    {
        return this.propiedades;
    }

    /**
     * @brief Asigna una nueva lista enlazada de propiedades.
     * @param propiedades LinkedList con las propiedades.
     */
    public void SetPropiedades(LinkedList propiedades)
    {
        this.propiedades = propiedades;
    }

    /**
     * @brief Obtiene la cantidad de cartas de salir de carcel que retiene el jugador.
     * @return Numero de cartas disponibles.
     */
    public int GetCartasSalirDeCarcel()
    {
        return this.cartasSalirDeCarcel;
    }

    /**
     * @brief Modifica la cantidad de cartas de salir de carcel.
     * @param cartasSalirDeCarcel Cantidad de cartas.
     */
    public void SetCartasSalirDeCarcel(int cartasSalirDeCarcel)
    {
        this.cartasSalirDeCarcel = cartasSalirDeCarcel;
    }

    /**
     * @brief Consulta si el jugador se encuentra preso.
     * @return true si esta en la carcel; false si esta libre.
     */
    public bool GetEnCarcel()
    {
        return this.enCarcel;
    }

    /**
     * @brief Cambia el estado de prision del jugador.
     * @param enCarcel true para encarcelar; false para liberar.
     */
    public void SetEnCarcel(bool enCarcel)
    {
        this.enCarcel = enCarcel;
    }

    /**
     * @brief Obtiene los turnos cumplidos en prision.
     * @return Cantidad de turnos encarcelado (0 a 3).
     */
    public int GetTurnosEnCarcel()
    {
        return this.turnosEnCarcel;
    }

    /**
     * @brief Modifica el contador de turnos en prision.
     * @param turnosEnCarcel Numero de turnos.
     */
    public void SetTurnosEnCarcel(int turnosEnCarcel)
    {
        this.turnosEnCarcel = turnosEnCarcel;
    }

    /**
     * @brief Consulta si el jugador tiene sancion de perder el proximo turno.
     * @return true si pierde el proximo turno; false en caso normal.
     */
    public bool GetPierdeSiguienteTurno()
    {
        return this.pierdeSiguienteTurno;
    }

    /**
     * @brief Asigna la penalizacion de perdida de turno.
     * @param pierdeSiguienteTurno Estado booleano de la sancion.
     */
    public void SetPierdeSiguienteTurno(bool pierdeSiguienteTurno)
    {
        this.pierdeSiguienteTurno = pierdeSiguienteTurno;
    }

    /**
     * @brief Obtiene el flujo de salida de red para comunicacion con el cliente.
     * @return StreamWriter del socket o null si juega en consola local.
     */
    public StreamWriter? GetWriter()
    {
        return this.writer;
    }

    /**
     * @brief Configura el flujo de salida de red.
     * @param writer Instancia de StreamWriter correspondiente al socket.
     */
    public void SetWriter(StreamWriter? writer)
    {
        this.writer = writer;
    }

    /**
     * @brief Constructor disenado para registro fisico mediante lector RFID.
     * @param nombre Nombre del jugador.
     * @param idTarjeta Codigo hexadecimal del tag RFID.
     * @param balance_incial Dinero inicial (por defecto 1500).
     */
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

    /**
     * @brief Constructor disenado para conexion de red o consola virtual.
     * @param id Identificador numerico asignado.
     * @param nombre Nombre o apodo.
     * @param writer Canal de transmision de red (opcional).
     * @param balance_inicial Dinero de partida (por defecto 1500).
     */
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

    /**
     * @brief Transmite un mensaje al participante.
     *
     * Si cuenta con una conexion TCP activa en writer, lo envia a traves del socket;
     * de lo contrario, lo imprime en la consola local del servidor.
     * @param mensaje Texto que describe el evento o solicitud de juego.
     */
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
            // Ignorar excepciones por desconexion repentina del socket
        }
    }

    /**
     * @brief Obtiene el objeto Casilla actual contenido dentro del Node de posicion.
     * @return La Casilla donde reposa el jugador, o null si aun no ha iniciado.
     */
    public Casilla? ObtenerCasillaActual()
    {
        return this.posicion?.GetData() as Casilla;
    }

    /**
     * @brief Representacion en texto del jugador (su nombre).
     * @return Cadena con el nombre del jugador.
     */
    public override string ToString()
    {
        return this.nombre;
    }
}