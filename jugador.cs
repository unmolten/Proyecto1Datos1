public class Jugador
{
    public string Nombre { get; set; }
    public string IdTarjeta { get; set; }

    public Jugador(string nombre, string idTarjeta)
    {
        Nombre = nombre;
        IdTarjeta = idTarjeta;
    }
}