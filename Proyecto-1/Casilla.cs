using System;

// Clase Casilla:
// Representa cada uno de los espacios físicos que conforman el tablero de Monopoly.
// Cada objeto 'Casilla' se almacena dentro del campo 'data' de un 'Node' en la lista enlazada del tablero.
// Clase abstracta que hereda a las clases Propiedad, CasillaEvento y CasillaEspecial.
public abstract class Casilla
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
    // - "Fortuna": Otorga eventos aleatorios (premios, multas, saltos).
    // - "ArcaComunal": Otorga eventos aleatorios (premios, multas, saltos).
    public string Tipo { get; set; }

    // Constructor para inicializar una casilla con sus atributos básicos.
    public Casilla(int posicion, string nombre, string tipo)
    {
        this.Posicion = posicion;
        this.Nombre = nombre;
        this.Tipo = tipo;
    }

    // Metodo virtual, para las clases heredadas ocurre:
    // Propiedad: Verifica si la propiedad tiene o no dueno y permite comprar o cobrar un alquiler respectivamente
    // CasillaEvento: Permite obtener alguna instancia de CartaEvento segun su tipo ("Fortuna"/"ArcaComunal")
    // CasillaEspecial: Ejecuta una accion especifica para las casillas segun su tipo
    public virtual void Accion() {}
}
public class Propiedad : Casilla
{
    // Precio de compra de la casilla si es de tipo "Propiedad".
    int PrecioCompra { get; set; }

    // Monto de renta que debe pagar un jugador si cae en esta casilla y pertenece a otro jugador.
    // En casillas de "Impuesto", representa el monto a pagar al fisco.
    int AlquilerBase { get; set; }

    // Referencia al jugador que es dueño de esta propiedad.
    // Si es null, significa que la propiedad está disponible para ser comprada.
    Jugador? Propietario { get; set; }
    
    // Verifica si la propiedad se encuentra hipotecada
    bool IsHipotecada { get; set; }

    string ColorGrupo { get; set; }

    int CantidadCasas { get; set; }   // 0 a 5

    public Propiedad(int posicion, string nombre, string tipo, int precioCompra, int alquilerBase, Jugador propietario, string colorGrupo)
    : base(posicion, nombre, tipo)
    {
        this.PrecioCompra = precioCompra;
        this.AlquilerBase = alquilerBase;
        this.Propietario = propietario;
        this.IsHipotecada = false;
        this.ColorGrupo = colorGrupo;
    }

    // Verifica si la casilla tiene un propietario.
    private bool TienePropietario()
    {
        return this.Propietario != null;
    }

    public override void Accion()
    {
        if (TienePropietario())
        {
            int montoAlquiler;
            montoAlquiler = AlquilerBase;   
        }
        else
        {
            //preguntar si desea comprar la propiedad
        }
    }

    // 

    // Representación en texto formateado de la casilla para mostrar en la consola.
    public override string ToString()
    {
        if (this.Tipo == "Propiedad")
        {
            string dueño = this.Propietario != null ? this.Propietario.Nombre : "Sin dueño";
            return $"[{this.Posicion}] {this.Nombre} (Propiedad) - Precio: ${this.PrecioCompra} | Renta: ${this.AlquilerBase} | Dueño: {dueño}";
        }
        else
        {
            return $"[{this.Posicion}] {this.Nombre} ({this.Tipo})";
        }
    }

    public int NumeroCasas() {return CantidadCasas;}
}
public class CasillaEvento : Casilla
{
    string tipoEvento;
    public CasillaEvento(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
        
    }

    public override void Accion()
    {
        
    }
}
public class CasillaEspecial : Casilla
{
    
    public CasillaEspecial(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
        
    }

    public override void Accion()
    {
        
    }
}
