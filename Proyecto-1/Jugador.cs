/// Datos de un jugador y el UID de la tarjeta que lo identifica.
public class Jugador
{
    // Nombre visible del jugador.
    public string Nombre { get; set; }

    // UID de la tarjeta RFID, conservado como texto.
    public string IdTarjeta { get; set; }

    // Balance / Dinero que tiene el jugador.
    public int Balance { get; set;}

    // Crea un jugador con su nombre y UID de tarjeta.
    public Jugador(string nombre, string idTarjeta)
    {
        Nombre = nombre;
        IdTarjeta = idTarjeta;
        Balance = 0;
    }
}