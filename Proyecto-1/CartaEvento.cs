using System;

/**
 * @file CartaEvento.cs
 * @brief Definicion del modelo de cartas de evento (Fortuna y Arca Comunal).
 *
 * Contiene el enumerador de efectos posibles y la clase CartaEvento.
 * Las cartas se organizan en dos listas circulares independientes que rotan
 * al robarse: cada carta robada desde la cabeza vuelve a insertarse al final.
 */

/**
 * @enum TipoEfectoCarta
 * @brief Tipos de efectos ejecutables al robar una carta de evento.
 *
 * Permite al motor de juego procesar consecuencias de dinero, movimiento,
 * sanciones o cartas especiales sin necesidad de subclases complejas.
 */
public enum TipoEfectoCarta
{
    /** @brief El jugador recibe un monto del banco. */
    GanarDinero,

    /** @brief El jugador abona un monto al banco. */
    PerderDinero,

    /** @brief Quien roba la carta paga un monto a cada uno de los demas jugadores. */
    PagarACadaJugador,

    /** @brief Cada uno de los otros jugadores paga un monto a quien robo la carta. */
    CobrarDeCadaJugador,

    /** @brief El jugador abona un monto multiplicado por el numero de propiedades que posee. */
    PerderDineroPorPropiedad,

    /** @brief Paga tarifas especificas por cada casa y por cada hotel que posea. */
    PerderDineroPorConstruccion,

    /** @brief Desplaza al jugador directamente al indice de casilla especificado. */
    MoverACasilla,

    /** @brief Desplaza al jugador una cantidad relativa de casillas (adelante o atras). */
    MoverCasillas,

    /** @brief Envia al jugador a la carcel inmediatamente sin cobrar bono de salida. */
    IrACarcel,

    /** @brief Concede al jugador una carta para librarse de la carcel gratuitamente. */
    SalirDeCarcelGratis,

    /** @brief Instruye al jugador a robar una carta complementaria del mazo indicado. */
    TomarOtraCarta
}

/**
 * @class CartaEvento
 * @brief Representa una carta individual perteneciente a los mazos de Fortuna o Arca Comunal.
 */
public class CartaEvento
{
    /** @brief Texto narrativo que se muestra al jugador al robar la carta. */
    private string descripcion;

    /** @brief Categoria de efecto que desencadena la carta. */
    private TipoEfectoCarta tipo;

    /** @brief Valor monetario involucrado (ganancia, perdida o pago). */
    private int monto;

    /** @brief Monto a cobrar por cada casa construida (para reparaciones). */
    private int montoPorCasa;

    /** @brief Monto a cobrar por cada hotel construido. */
    private int montoPorHotel;

    /** @brief Indice absoluto de casilla de destino (0 a 31). */
    private int casillaDestino;

    /** @brief Cantidad de casillas a avanzar (+) o retroceder (-). */
    private int cantidadCasillas;

    /** @brief Bandera que indica si el jugador pierde su siguiente turno. */
    private bool pierdeTurno;

    /** @brief Identificador del mazo al que pertenece (1: Fortuna, 2: Arca Comunal). */
    private int mazo;

    /**
     * @brief Obtiene el texto narrativo de la carta.
     * @return Descripcion textual para el usuario.
     */
    public string GetDescripcion()
    {
        return this.descripcion;
    }

    /**
     * @brief Modifica la descripcion de la carta.
     * @param descripcion Nueva descripcion textual.
     */
    public void SetDescripcion(string descripcion)
    {
        this.descripcion = descripcion;
    }

    /**
     * @brief Obtiene el tipo de efecto asociado.
     * @return Valor del enumerador TipoEfectoCarta.
     */
    public TipoEfectoCarta GetTipo()
    {
        return this.tipo;
    }

    /**
     * @brief Define el tipo de efecto asociado.
     * @param tipo Categoria de efecto a asignar.
     */
    public void SetTipo(TipoEfectoCarta tipo)
    {
        this.tipo = tipo;
    }

    /**
     * @brief Obtiene el importe o monto monetario.
     * @return Cantidad numerica en dinero.
     */
    public int GetMonto()
    {
        return this.monto;
    }

    /**
     * @brief Asigna el importe o monto monetario.
     * @param monto Cantidad de dinero asignada.
     */
    public void SetMonto(int monto)
    {
        this.monto = monto;
    }

    /**
     * @brief Obtiene el costo por casa para efectos de construccion.
     * @return Importe asignado por casa.
     */
    public int GetMontoPorCasa()
    {
        return this.montoPorCasa;
    }

    /**
     * @brief Modifica el costo por casa para efectos de construccion.
     * @param montoPorCasa Importe a fijar por casa.
     */
    public void SetMontoPorCasa(int montoPorCasa)
    {
        this.montoPorCasa = montoPorCasa;
    }

    /**
     * @brief Obtiene el costo por hotel para efectos de reparacion.
     * @return Importe asignado por hotel.
     */
    public int GetMontoPorHotel()
    {
        return this.montoPorHotel;
    }

    /**
     * @brief Modifica el costo por hotel para efectos de reparacion.
     * @param montoPorHotel Importe a fijar por hotel.
     */
    public void SetMontoPorHotel(int montoPorHotel)
    {
        this.montoPorHotel = montoPorHotel;
    }

    /**
     * @brief Obtiene el indice de casilla hacia donde se debe desplazar el jugador.
     * @return Indice del tablero (0 a 31) o -1 si no aplica.
     */
    public int GetCasillaDestino()
    {
        return this.casillaDestino;
    }

    /**
     * @brief Establece el indice de casilla de destino.
     * @param casillaDestino Indice valido del tablero.
     */
    public void SetCasillaDestino(int casillaDestino)
    {
        this.casillaDestino = casillaDestino;
    }

    /**
     * @brief Obtiene el numero relativo de casillas a mover.
     * @return Entero positivo para avanzar o negativo para retroceder.
     */
    public int GetCantidadCasillas()
    {
        return this.cantidadCasillas;
    }

    /**
     * @brief Establece el numero relativo de casillas a mover.
     * @param cantidadCasillas Cantidad de casillas.
     */
    public void SetCantidadCasillas(int cantidadCasillas)
    {
        this.cantidadCasillas = cantidadCasillas;
    }

    /**
     * @brief Indica si la carta ocasiona la perdida del turno posterior.
     * @return true si el jugador debe perder turno; false en caso contrario.
     */
    public bool GetPierdeTurno()
    {
        return this.pierdeTurno;
    }

    /**
     * @brief Configura la penalizacion de perdida de turno.
     * @param pierdeTurno Estado booleano de la penalizacion.
     */
    public void SetPierdeTurno(bool pierdeTurno)
    {
        this.pierdeTurno = pierdeTurno;
    }

    /**
     * @brief Obtiene el identificador del mazo de procedencia.
     * @return 1 para Fortuna, 2 para Arca Comunal.
     */
    public int GetMazo()
    {
        return this.mazo;
    }

    /**
     * @brief Asigna el identificador del mazo.
     * @param mazo 1 para Fortuna, 2 para Arca Comunal.
     */
    public void SetMazo(int mazo)
    {
        this.mazo = mazo;
    }

    /**
     * @brief Constructor completo para instanciar una carta de evento.
     * @param descripcion Mensaje visible para el jugador.
     * @param tipo Tipo de efecto a ejecutar.
     * @param monto Monto monetario involucrado (opcional, por defecto 0).
     * @param casillaDestino Indice de casilla hacia la que se mueve (opcional, por defecto -1).
     * @param cantidadCasillas Desplazamiento relativo (opcional, por defecto 0).
     * @param montoPorCasa Tarifa por casa para reparaciones (opcional, por defecto 0).
     * @param montoPorHotel Tarifa por hotel para reparaciones (opcional, por defecto 0).
     * @param pierdeTurno Si el jugador pierde su siguiente turno (opcional, por defecto false).
     * @param mazo Identificador de mazo si roba otra carta (opcional, por defecto 0).
     */
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

    /**
     * @brief Representacion en cadena de texto de la carta.
     * @return La descripcion de la carta.
     */
    public override string ToString()
    {
        return this.descripcion;
    }
}