/*
CartaEvento es la plantilla utilizada para simular las cartas
de evento de Monopoly (tanto las "Fortuna" como las ArcaComunal").

Todas las cartas de un mazo se crean UNA sola vez al iniciar la partida
(MazoCartas.cs) y se guardan dentro de una LinkedList: la misma lista
circular doblemente enlazada que ya existe en el proyecto. Como pide la
arquitectura, cada carta que se robe (se saca de la cabeza) debe volver a
insertarse al final de la lista, por lo que el orden de las cartas se
randomiza una sola vez, apenas se crea el mazo, y despues cada mazo
simplemente rota.

Existen DOS mazos independientes (cada uno su propia LinkedList):
  -Fortuna
  -Arca Comunal
*/

using System;

// Tipo de efecto que aplica una CartaEvento al jugador que se la roba
// Sirve para que el resto del juego sepa qué hacer con Monto / CasillaDestino /
// CantidadCasillas sin tener que crear una subclase distinta por cada carta
public enum TipoEfectoCarta
{
    GanarDinero,                   // El jugador recibe un monto del banco
    PerderDinero,                  // El jugador paga un monto al banco
    PagarACadaJugador,             // Quien roba la carta paga un monto a cada uno de los demas jugadores
    CobrarDeCadaJugador,           // Cada uno de los demas jugadores le paga un monto a quien robo la carta
    PerderDineroPorPropiedad,      // El jugador paga un monto multiplicado por la cantidad de propiedades que tiene
    PerderDineroPorConstruccion,   // El jugador paga MontoPorCasa por cada casa y MontoPorHotel por cada hotel que tenga
    MoverACasilla,                 // El jugador se mueve directo a la casilla de indice CasillaDestino
    MoverCasillas,                 // El jugador avanza o retrocede +- CantidadCasillas
    IrACarcel,                     // El jugador va directo a la carcel, sin cobrar salida
    SalirDeCarcelGratis,           // El jugador se queda con esta carta hasta que la use para salir de la carcel
    TomarOtraCarta                 // El jugador toma otra carta de uno de los dos mazos de cartas de arca comunal o fortuna
}

// Carta de evento (Fortuna o Arca Comunal)
public class CartaEvento
{
    // Texto que se le muestra al jugador al robar la carta
    public string Descripcion { get; set; }

    // Que tipo de efecto aplica esta carta (enum TipoEfectoCarta)
    public TipoEfectoCarta Tipo { get; set; }

    // Monto en dinero involucrado en el efecto, 0 si el
    // efecto de la carta no involucra dinero
    public int Monto { get; set; }

    // Costo POR CADA casa que el jugador tenga construida, solo se usa si el
    // Tipo es PerderDineroPorConstruccion, 0 si no aplica
    public int MontoPorCasa { get; set; }
 
    // Costo POR CADA hotel que el jugador tenga construido, solo se usa si el
    // Tipo es PerderDineroPorConstruccion, 0 si no aplica
    public int MontoPorHotel { get; set; }

    // Casilla de destino, solo se usa si el Tipo es MoverACasilla
    // Se deja en -1 si no aplica
    public int CasillaDestino { get; set; }

    // Cantidad de casillas a avanzar/retroceder, solo se usa si Tipo es
    // MoverCasillas (positivo avanza, negativo retrocede)
    // Se deja en 0 si no aplica
    public int CantidadCasillas { get; set; }

    // true si ademas del efecto de Tipo, la carta hace que el jugador
    // pierda su siguiente turno, se puede combinar con cualquier Tipo
    // (por ejemplo: pagar dinero y perder turno, o moverse y perder turno)
    public bool PierdeTurno { get; set; }

    // Numero de mazo al que robar las tarjetas (1 para Fortuna, 2 Arca),
    // 0 si no aplica
    public int Mazo { get; set; }

    // Famosisimo constructor
    public CartaEvento(string descripcion, TipoEfectoCarta tipo, int monto = 0, int casillaDestino = -1, int cantidadCasillas = 0,
    int montoPorCasa = 0, int montoPorHotel = 0, bool pierdeTurno = false, int mazo = 0)
    {
        this.Descripcion = descripcion;
        this.Tipo = tipo;
        this.Monto = monto;
        this.CasillaDestino = casillaDestino;
        this.CantidadCasillas = cantidadCasillas;
        this.MontoPorCasa = montoPorCasa;
        this.MontoPorHotel = montoPorHotel;
        this.PierdeTurno = pierdeTurno;
        this.Mazo = mazo;
    }

    // Representacion en texto de la carta (lo que se le manda al jugador)
    public override string ToString()
    {
        return this.Descripcion;
    }
}