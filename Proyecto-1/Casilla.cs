using System;

// Clase Casilla:
// Representa cada uno de los espacios físicos que conforman el tablero de Monopoly.
// Cada objeto 'Casilla' se almacena dentro del campo 'data' de un 'Node' en la lista enlazada del tablero.
public class Casilla
{
    // Posición o índice numérico en el tablero (de 0 a TotalCasillas - 1).
    public int Posicion { get; set; }

    // Nombre visible de la casilla (ej. "Salida (GO)", "Avenida Mediterráneo", "Cárcel").
    public string Nombre { get; set; }

    // Tipo de casilla que define su comportamiento dentro del juego:
    // - "Salida": Otorga bono de dinero al caer o pasar.
    // - "Propiedad": Puede ser comprada por un jugador y cobrar renta a los rivales.
    // - "Carcel": Zona de espera o visita.
    // - "Impuesto": Descuenta una tarifa fija al jugador.
    // - "ParadaLibre": Casilla neutral donde no ocurre penalización ni cobro.
    // - "VayaALaCarcel": Arresta al jugador y lo teletransporta al nodo de la Cárcel.
    // - "Suerte": Otorga eventos aleatorios (premios, multas, saltos).
    public string Tipo { get; set; }

    // Precio de compra de la casilla si es de tipo "Propiedad".
    public int Precio { get; set; }

    // Monto de renta que debe pagar un jugador si cae en esta casilla y pertenece a otro jugador.
    // En casillas de "Impuesto", representa el monto a pagar al fisco.
    public int Renta { get; set; }

    // Referencia al jugador que es dueño de esta propiedad.
    // Si es null, significa que la propiedad está disponible para ser comprada.
    public Jugador? Propietario { get; set; }

    // Constructor para inicializar una casilla con sus atributos básicos.
    public Casilla(int posicion, string nombre, string tipo, int precio = 0, int renta = 0)
    {
        this.Posicion = posicion;
        this.Nombre = nombre;
        this.Tipo = tipo;
        this.Precio = precio;
        this.Renta = renta;
        this.Propietario = null;
    }

    // Determina si la casilla es un bien inmueble que puede comprarse y generar rentas.
    public bool EsPropiedad()
    {
        return this.Tipo == "Propiedad";
    }

    // Determina si esta propiedad ya fue adquirida por algún jugador de la partida.
    public bool TienePropietario()
    {
        return this.Propietario != null;
    }

    // Representación en texto formateado de la casilla para mostrar en la consola.
    public override string ToString()
    {
        if (this.Tipo == "Propiedad")
        {
            string dueño = this.Propietario != null ? this.Propietario.Nombre : "Sin dueño";
            return $"[{this.Posicion}] {this.Nombre} (Propiedad) - Precio: ${this.Precio} | Renta: ${this.Renta} | Dueño: {dueño}";
        }
        else
        {
            return $"[{this.Posicion}] {this.Nombre} ({this.Tipo})";
        }
    }
}
