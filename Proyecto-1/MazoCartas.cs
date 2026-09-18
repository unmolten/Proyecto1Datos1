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

    GanarDinero - PerderDinero - PagarACadaJugador - CobrarDeCadaJugador
        monto: numero

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
            // agregar aquí las cartas
        };

        return ConstruirMazoRandomizado(cartas);
    }

    // Construye el mazo de Arca Comunal con todas sus cartas random
    public static LinkedList CrearMazoArcaComunal()
    {
        List<CartaEvento> cartas = new List<CartaEvento>
        {
            // agregar aqui las cartas
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