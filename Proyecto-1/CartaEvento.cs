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
    // Campos privados
    private string descripcion;
    private TipoEfectoCarta tipo;
    private int monto;
    private int montoPorCasa;
    private int montoPorHotel;
    private int casillaDestino;
    private int cantidadCasillas;
    private bool pierdeTurno;
    private int mazo;

    // Métodos individuales para obtener y definir (Getters y Setters)
    public string GetDescripcion()
    {
        return this.descripcion;
    }

    public void SetDescripcion(string descripcion)
    {
        this.descripcion = descripcion;
    }

    public TipoEfectoCarta GetTipo()
    {
        return this.tipo;
    }

    public void SetTipo(TipoEfectoCarta tipo)
    {
        this.tipo = tipo;
    }

    public int GetMonto()
    {
        return this.monto;
    }

    public void SetMonto(int monto)
    {
        this.monto = monto;
    }

    public int GetMontoPorCasa()
    {
        return this.montoPorCasa;
    }

    public void SetMontoPorCasa(int montoPorCasa)
    {
        this.montoPorCasa = montoPorCasa;
    }

    public int GetMontoPorHotel()
    {
        return this.montoPorHotel;
    }

    public void SetMontoPorHotel(int montoPorHotel)
    {
        this.montoPorHotel = montoPorHotel;
    }

    public int GetCasillaDestino()
    {
        return this.casillaDestino;
    }

    public void SetCasillaDestino(int casillaDestino)
    {
        this.casillaDestino = casillaDestino;
    }

    public int GetCantidadCasillas()
    {
        return this.cantidadCasillas;
    }

    public void SetCantidadCasillas(int cantidadCasillas)
    {
        this.cantidadCasillas = cantidadCasillas;
    }

    public bool GetPierdeTurno()
    {
        return this.pierdeTurno;
    }

    public void SetPierdeTurno(bool pierdeTurno)
    {
        this.pierdeTurno = pierdeTurno;
    }

    public int GetMazo()
    {
        return this.mazo;
    }

    public void SetMazo(int mazo)
    {
        this.mazo = mazo;
    }

    // Famosisimo constructor
    public CartaEvento(string descripcion, TipoEfectoCarta tipo, int monto = 0, int casillaDestino = -1, int cantidadCasillas = 0,
    int montoPorCasa = 0, int montoPorHotel = 0, bool pierdeTurno = false, int mazo = 0)
    {
        this.descripcion = descripcion;
        this.tipo = tipo;
        this.monto = monto;
        this.casillaDestino = casillaDestino;
        this.cantidadCasillas = cantidadCasillas;
        this.montoPorCasa = montoPorCasa;
        this.montoPorHotel = montoPorHotel;
        this.pierdeTurno = pierdeTurno;
        this.mazo = mazo;
    }

    // Representacion en texto de la carta (lo que se le manda al jugador)
    public override string ToString()
    {
        return this.descripcion;
    }
}