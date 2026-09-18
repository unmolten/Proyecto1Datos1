/*
MazoCartas se encarga de crear TODAS las cartas de cada mazo (Fortuna y Arca Comunal) de una sola vez,
randomizar el orden de cada mazo apenas se crea, guardar cada mazo dentro de una LinkedList, y robar
una carta: la saca de la cabeza y la vuelve a insertar al final

Uso esperado desde donde se maneje la partida :

    LinkedList mazoFortuna = MazoCartas.CrearMazoFortuna();
    LinkedList mazoArca    = MazoCartas.CrearMazoArcaComunal();

    Cuando un jugador cae en una casilla de Fortuna:

        CartaEvento carta = MazoCartas.RobarCarta(mazoFortuna);
        jugador.EnviarMensaje(carta.Descripcion);

    luego, segun carta.Tipo, se aplica el efecto
*/

using System;
using System.Collections.Generic;

public static class MazoCartas
{
    private static readonly Random random = new Random();

/*
    ---------------------------------------------------------------
    El formato es siempre:

    new CartaEvento("texto de la descripcion para el jugador", TipoEfectoCarta.tipo, parametros...)

    Segun el tipo, los parametros que importan son:

    GanarDinero - PerderDinero - PerderDineroPorPropiedad - PagarACadaJugador - CobrarDeCadaJugador
        monto: numero

    PerderDineroPorConstruccion
        montoPorCasa: numero
        montoPorHotel: numero

    MoverACasilla
        casillaDestino: indice de la casilla, 0 a 31

    MoverCasillas
        cantidadCasillas: numero, positivo avanza, negativo retrocede

    IrACarcel - SalirDeCarcelGratis
        no necesitan parametros extra
    ---------------------------------------------------------------
*/

    // Construye el mazo de Fortuna con todas sus cartas random
    public static LinkedList CrearMazoFortuna()
    {
        List<CartaEvento> cartas = new List<CartaEvento>
        {
            new CartaEvento("¡No te acerques! Un Warden acecha, retrocede lentamente 2 casillas.",
            TipoEfectoCarta.MoverCasillas, cantidadCasillas: -2),

            new CartaEvento("¡Corre! Una horda de Zombies, corres 4 casillas.",
            TipoEfectoCarta.MoverCasillas, cantidadCasillas: 4),

            new CartaEvento("¡Descanso! Ve a Parada Libre y pierde un turno, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 16, pierdeTurno: true),

            new CartaEvento("¡Están cruzando la dimensión! Ve al Portal al Nether, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 22),

            new CartaEvento("¡Moriste! Te toma un turno completo recuperar tus cosas y pierdes 100 esmeraldas en el camino.",
            TipoEfectoCarta.PerderDinero, monto: 100, pierdeTurno: true),

            new CartaEvento("¡Muy muy lejano! Ve al tren a las Farlands, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 28),

            new CartaEvento("¡Visita a los infortunados! Ve a la Cárcel de visita, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 8),

            new CartaEvento("¡Es muy tarde! Pasa la noche en la Aldea Esmeraldil, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 6),

            new CartaEvento("¡De turismo! Toma el Tren a las minas, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 12),

            new CartaEvento("¡Primera clase! Visita el Barco del End, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 31),

            new CartaEvento("¡Segunda oportunidad! Toma otra carta de Fortuna.",
            TipoEfectoCarta.TomarOtraCarta, mazo: 1),

            new CartaEvento("¡Cambio de ritmo! Toma una carta de Arca Comunal.",
            TipoEfectoCarta.TomarOtraCarta, mazo: 2),

            new CartaEvento("¡Fuiste tú! Te acusan de explotar con dinamita las pertenencias de los demás, ve a la cárcel, no cobras Salida.",
            TipoEfectoCarta.IrACarcel),

            new CartaEvento("¡Para ti! Obtienes una carta de salir de la cárcel gratis.",
            TipoEfectoCarta.SalirDeCarcelGratis),
        };

        return ConstruirMazoRandomizado(cartas);
    }

    // Construye el mazo de Arca Comunal con todas sus cartas random
    public static LinkedList CrearMazoArcaComunal()
    {
        List<CartaEvento> cartas = new List<CartaEvento>
        {
            new CartaEvento("¡Héroe de la aldea! Acabaste con los asaltos, los aldeanos te dan 100 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 100),
            
            new CartaEvento("¡Robaste nuestras cosas! Vaya directo a la cárcel sin cobrar Salida.",
            TipoEfectoCarta.IrACarcel),
            
            new CartaEvento("¡Impuestos! Has pasado mucho tiempo en las minas, le debes 25 esmeraldas a cada jugador.",
            TipoEfectoCarta.PagarACadaJugador, monto: 25),
            
            new CartaEvento("¡Creepers! Paga 25 esmeraldas por cada propiedad adquirida para reparar los daños.",
            TipoEfectoCarta.PerderDineroPorPropiedad, monto: 25),
            
            new CartaEvento("¡Están en llamas! Paga 25 por cada casa y 100 por cada hotel para reparar los daños.",
            TipoEfectoCarta.PerderDineroPorConstruccion, montoPorCasa: 25, montoPorHotel: 100),
            
            new CartaEvento("¡Prestacion de servicios! Por tu ayuda recolectando recursos, todos te pagan 25 esmeraldas.",
            TipoEfectoCarta.CobrarDeCadaJugador, monto: 25),

            new CartaEvento("¡Un diamante! Encuentras un diamante en el suelo, lo vendes por 50 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 50),

            new CartaEvento("¡Feliz cumpleaños! Hoy cumples años, todos te regalan 20 esmeraldas.",
            TipoEfectoCarta.CobrarDeCadaJugador, monto: 20),

            new CartaEvento("¡Estafa! Un aldeano te vende un cartón pintado como diamante, perdiste 50 esmeraldas.",
            TipoEfectoCarta.PerderDinero, monto: 50),

            new CartaEvento("¡Veterinario! Tu lobo necesita una operacion, gastas 150 esmeraldas.",
            TipoEfectoCarta.PerderDinero, monto: 150),

            new CartaEvento("¡Michi! Tu gato te trajo un pescado y 25 esmeraldas mientras dormias.",
            TipoEfectoCarta.GanarDinero, monto: 25),

            new CartaEvento("¡Mira qué estilo! Compras una nueva armadura, gastas 50 esmeraldas.",
            TipoEfectoCarta.PerderDinero, monto: 50),

            new CartaEvento("¡Lotería! Ganas un concurso de comer pastel, te dan 20 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 20),

            new CartaEvento("¡Caridad! Te regalan 150 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 150),

            new CartaEvento("¡Para ti! Obtienes una carta de salir de la cárcel gratis.",
            TipoEfectoCarta.SalirDeCarcelGratis),
        };

        return ConstruirMazoRandomizado(cartas);
    }

    // Recibe la lista de cartas ya creadas, las mezcla (por fisher yates) y
    // arma con ellas una LinkedList circular lista para usarse como mazo
    private static LinkedList ConstruirMazoRandomizado(List<CartaEvento> cartas)
    {
        // Randomiza el orden (este es el fisher yates, comienza desde el final
        // de la lista, elije un elemento random de la lista para intercambiarlo
        // a la posicion final donde comenzó el for. Se intercambian y se pasa a
        // la siguiente iteracion, lo que significa que la carta que haya quedado
        // de ultima seguirá de ultima fija)
        for (int i = cartas.Count - 1; i > 0; i--)
        {
            // Elije una posicion random
            int j = random.Next(i + 1);

            (cartas[i], cartas[j]) = (cartas[j], cartas[i]);

            // Esta parte es para evitar hacer:
            // CartaEvento temp = cartas[i];
            // cartas[i] = cartas[j];
            // cartas[j] = temp;
            // es lo mismo, pero mas corto y eficiente
        }

        // Arma la lista enlazada circular con el orden ya randomizado
        LinkedList mazo = new LinkedList();

        foreach (CartaEvento carta in cartas)
        {
            mazo.InsertEnd(carta);
        }

        mazo.MakeCircular();
        return mazo;
    }

    // Roba la carta que esta en la cabeza del mazo y la vuelve a insertar
    // al final
    // Devuelve la CartaEvento robada
    public static CartaEvento RobarCarta(LinkedList mazo)
    {
        Node? nodo = mazo.DeleteFirst();

        if (nodo == null || nodo.GetData() is not CartaEvento carta)
        {
            throw new InvalidOperationException("El mazo esta vacio o contiene datos invalidos.");
        }

        // Lleva la carta sacada al final
        mazo.InsertEnd(carta);

        return carta;
    }
}