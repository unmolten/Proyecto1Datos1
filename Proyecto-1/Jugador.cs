/// Datos de un jugador y el UID de la tarjeta que lo identifica.
public class Jugador
{
    // Nombre visible del jugador.
    public string Nombre { get; set; }

    // UID de la tarjeta RFID, conservado como texto.
    public string IdTarjeta { get; set; }

    // Nodo del tablero donde se encuentra actualmente el jugador.
    public Node? Posicion { get; set; }

    // Balance / Dinero que tiene el jugador.
    public int Balance { get; set;}

    // Propiedades / La lista de propiedades que tiene el jugador
    public LinkedList Propiedades;

    // Cantidad de cartas de salir de la carcel que tiene el jugador
    public int CartasSalirDeCarcel;

    // Crea un jugador con su nombre y UID de tarjeta.
    public Jugador(string nombre, string idTarjeta, int balance_incial = 0)
    {
        this.Nombre = nombre;
        this.IdTarjeta = idTarjeta;
        this.Posicion = null;
        this.Balance = balance_incial;
        this.Propiedades = new LinkedList();
        this.CartasSalirDeCarcel = 0;
    }
}