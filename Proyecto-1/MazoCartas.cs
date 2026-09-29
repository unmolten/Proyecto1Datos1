using System;

/**
 * @file MazoCartas.cs
 * @brief Fabricador y administrador de los mazos de cartas de evento.
 *
 * Crea de manera estatica las barajas completas de cartas para Fortuna y Arca Comunal,
 * las baraja al inicio mediante el algoritmo de Fisher-Yates, las estructura en una
 * LinkedList circular y provee el mecanismo de rotacion: cada carta robada de la cabeza
 * se reinserta al final del mazo.
 */

/**
 * @class MazoCartas
 * @brief Clase utilitaria estatica para la creacion y manipulacion de cartas de evento.
 */
public static class MazoCartas
{
    /** @brief Generador de aleatoriedad para el barajado inicial de cartas. */
    private static readonly Random random = new Random();

    /**
     * @brief Instancia y baraja el mazo completo de cartas de Fortuna.
     * @return LinkedList circular con todas las cartas de Fortuna barajadas.
     */
    public static LinkedList CrearMazoFortuna()
    {
        LinkedList mazo = new LinkedList();

        mazo.InsertEnd(new CartaEvento("¡No te acerques! Un Warden acecha, retrocede lentamente 2 casillas.",
            TipoEfectoCarta.MoverCasillas, cantidadCasillas: -2));

        mazo.InsertEnd(new CartaEvento("¡Corre! Una horda de Zombies, corres 4 casillas.",
            TipoEfectoCarta.MoverCasillas, cantidadCasillas: 4));

        mazo.InsertEnd(new CartaEvento("¡Descanso! Ve a Parada Libre y pierde un turno, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 16, pierdeTurno: true));

        mazo.InsertEnd(new CartaEvento("¡Están cruzando la dimensión! Ve al Portal al Nether, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 22));

        mazo.InsertEnd(new CartaEvento("¡Moriste! Te toma un turno completo recuperar tus cosas y pierdes 100 esmeraldas en el camino.",
            TipoEfectoCarta.PerderDinero, monto: 100, pierdeTurno: true));

        mazo.InsertEnd(new CartaEvento("¡Muy muy lejano! Ve al tren a las Farlands, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 28));

        mazo.InsertEnd(new CartaEvento("¡Visita a los infortunados! Ve a la Cárcel de visita, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 8));

        mazo.InsertEnd(new CartaEvento("¡Es muy tarde! Pasa la noche en la Aldea Esmeraldil, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 6));

        mazo.InsertEnd(new CartaEvento("¡De turismo! Toma el Tren a las minas, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 12));

        mazo.InsertEnd(new CartaEvento("¡Primera clase! Visita el Barco del End, si pasas por Salida, cobra el dinero.",
            TipoEfectoCarta.MoverACasilla, casillaDestino: 31));

        mazo.InsertEnd(new CartaEvento("¡Segunda oportunidad! Toma otra carta de Fortuna.",
            TipoEfectoCarta.TomarOtraCarta, mazo: 1));

        mazo.InsertEnd(new CartaEvento("¡Cambio de ritmo! Toma una carta de Arca Comunal.",
            TipoEfectoCarta.TomarOtraCarta, mazo: 2));

        mazo.InsertEnd(new CartaEvento("¡Fuiste tú! Te acusan de explotar con dinamita las pertenencias de los demás, ve a la cárcel, no cobras Salida.",
            TipoEfectoCarta.IrACarcel));

        mazo.InsertEnd(new CartaEvento("¡Para ti! Obtienes una carta de salir de la cárcel gratis.",
            TipoEfectoCarta.SalirDeCarcelGratis));

        return ConstruirMazoRandomizado(mazo);
    }

    /**
     * @brief Instancia y baraja el mazo completo de cartas de Arca Comunal.
     * @return LinkedList circular con todas las cartas de Arca Comunal barajadas.
     */
    public static LinkedList CrearMazoArcaComunal()
    {
        LinkedList mazo = new LinkedList();

        mazo.InsertEnd(new CartaEvento("¡Héroe de la aldea! Acabaste con los asaltos, los aldeanos te dan 100 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 100));

        mazo.InsertEnd(new CartaEvento("¡Robaste nuestras cosas! Vaya directo a la cárcel sin cobrar Salida.",
            TipoEfectoCarta.IrACarcel));

        mazo.InsertEnd(new CartaEvento("¡Impuestos! Has pasado mucho tiempo en las minas, le debes 25 esmeraldas a cada jugador.",
            TipoEfectoCarta.PagarACadaJugador, monto: 25));

        mazo.InsertEnd(new CartaEvento("¡Creepers! Paga 25 esmeraldas por cada propiedad adquirida para reparar los daños.",
            TipoEfectoCarta.PerderDineroPorPropiedad, monto: 25));

        mazo.InsertEnd(new CartaEvento("¡Están en llamas! Paga 25 por cada casa y 100 por cada hotel para reparar los daños.",
            TipoEfectoCarta.PerderDineroPorConstruccion, montoPorCasa: 25, montoPorHotel: 100));

        mazo.InsertEnd(new CartaEvento("¡Prestacion de servicios! Por tu ayuda recolectando recursos, todos te pagan 25 esmeraldas.",
            TipoEfectoCarta.CobrarDeCadaJugador, monto: 25));

        mazo.InsertEnd(new CartaEvento("¡Un diamante! Encuentras un diamante en el suelo, lo vendes por 50 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 50));

        mazo.InsertEnd(new CartaEvento("¡Feliz cumpleaños! Hoy cumples años, todos te regalan 20 esmeraldas.",
            TipoEfectoCarta.CobrarDeCadaJugador, monto: 20));

        mazo.InsertEnd(new CartaEvento("¡Estafa! Un aldeano te vende un cartón pintado como diamante, perdiste 50 esmeraldas.",
            TipoEfectoCarta.PerderDinero, monto: 50));

        mazo.InsertEnd(new CartaEvento("¡Veterinario! Tu lobo necesita una operacion, gastas 150 esmeraldas.",
            TipoEfectoCarta.PerderDinero, monto: 150));

        mazo.InsertEnd(new CartaEvento("¡Michi! Tu gato te trajo un pescado y 25 esmeraldas mientras dormias.",
            TipoEfectoCarta.GanarDinero, monto: 25));

        mazo.InsertEnd(new CartaEvento("¡Mira qué estilo! Compras una nueva armadura, gastas 50 esmeraldas.",
            TipoEfectoCarta.PerderDinero, monto: 50));

        mazo.InsertEnd(new CartaEvento("¡Lotería! Ganas un concurso de comer pastel, te dan 20 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 20));

        mazo.InsertEnd(new CartaEvento("¡Caridad! Te regalan 150 esmeraldas.",
            TipoEfectoCarta.GanarDinero, monto: 150));

        mazo.InsertEnd(new CartaEvento("¡Para ti! Obtienes una carta de salir de la cárcel gratis.",
            TipoEfectoCarta.SalirDeCarcelGratis));

        return ConstruirMazoRandomizado(mazo);
    }

    /**
     * @brief Baraja los elementos de la lista enlazada aplicando el algoritmo de Fisher-Yates
     * y garantiza su enlace circular.
     * @param mazo Lista lineal con las cartas recien creadas.
     * @return La misma lista mazo con sus elementos permutados aleatoriamente y enlazada de forma circular.
     */
    private static LinkedList ConstruirMazoRandomizado(LinkedList mazo)
    {
        for (int i = mazo.Size() - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);

            Node nodoI = mazo.GetNodeAt(i);
            Node nodoJ = mazo.GetNodeAt(j);

            object temp = nodoI.GetData();
            nodoI.SetData(nodoJ.GetData());
            nodoJ.SetData(temp);
        }

        mazo.MakeCircular();
        return mazo;
    }

    /**
     * @brief Extrae la carta en la cabeza de la lista y la reinserta al final de la misma.
     *
     * Cumple con la especificacion del diseno donde los mazos rotan indefinidamente.
     * @param mazo Lista enlazada circular que contiene las cartas del mazo.
     * @return La CartaEvento obtenida para ser aplicada al jugador.
     * @throws InvalidOperationException Si el mazo no contiene elementos validos.
     */
    public static CartaEvento RobarCarta(LinkedList mazo)
    {
        Node? nodo = mazo.DeleteFirst();

        if (nodo == null || nodo.GetData() is not CartaEvento carta)
        {
            throw new InvalidOperationException("El mazo esta vacio o contiene datos invalidos.");
        }

        // Reinserta la carta extraida en la cola del mazo para rotacion continua
        mazo.InsertEnd(carta);

        return carta;
    }
}