using System;
using System.IO;

// Clase Jugador:
// Representa a cada participante conectado a través de un socket TCP en el servidor.
// Integra dos conceptos fundamentales de estructuras de datos:
// 1. Puntero a la posición actual en el tablero (un 'Node' dentro de la lista circular).
// 2. Inventario personal de propiedades, administrado mediante una 'LinkedList' independiente.
public class Jugador
{
    // Identificador único numérico asignado por el servidor.
    public int Id { get; set; }

    // Nombre visible del jugador (por defecto "Jugador_N", pero modificable mediante comando NOMBRE).
    public string Nombre { get; set; }

    // Cantidad de dinero disponible (inicia en $1500, según las reglas estándar de Monopoly).
    public int Dinero { get; set; }

    // PUNTERO AL TABLERO (LISTA ENLAZADA):
    // Apunta directamente al nodo actual de la lista circular donde está ubicado el jugador.
    // Al tirar los dados, este puntero se actualiza iterativamente: Posicion = Posicion.GetNext().
    public Node? Posicion { get; set; }

    // ESTRUCTURA DE DATOS DE PROPIEDADES (LISTA ENLAZADA):
    // Cada vez que el jugador compra una casilla, se almacena en esta lista enlazada propia.
    // Esto permite administrar dinámicamente sus activos sin límites de un arreglo estático.
    public LinkedList Propiedades { get; set; }

    // Indica si el jugador se encuentra privado de su libertad en la casilla de la Cárcel.
    public bool EnCarcel { get; set; }

    // Flujo de escritura TCP (StreamWriter) asociado a este cliente.
    // Permite al servidor enviar respuestas y notificaciones directas al terminal del jugador.
    public StreamWriter? Writer { get; set; }

    // Constructor del Jugador.
    public Jugador(int id, string nombre, StreamWriter? writer = null)
    {
        this.Id = id;
        this.Nombre = nombre;
        this.Dinero = 1500;
        this.Posicion = null;
        this.Propiedades = new LinkedList();
        this.EnCarcel = false;
        this.Writer = writer;
    }

    // Envía un mensaje de texto directamente a través del socket TCP al cliente correspondiente.
    // Incluye manejo de excepciones en caso de que el cliente se desconecte repentinamente.
    public void EnviarMensaje(string mensaje)
    {
        try
        {
            if (this.Writer != null)
            {
                this.Writer.WriteLine(mensaje);
            }
        }
        catch
        {
            // Si el socket fue cerrado por el cliente, se captura de manera segura
        }
    }

    // Método de conveniencia para desempaquetar el objeto 'Casilla' almacenado dentro del 'Node' actual.
    public Casilla? ObtenerCasillaActual()
    {
        return this.Posicion?.GetData() as Casilla;
    }
}
