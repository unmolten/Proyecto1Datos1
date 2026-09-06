/*
En este archivo se manejan las transacciones de la partida
incluyendo instanciacion de las mismas, Almacenamiento e
impresion 
*/

using Microsoft.VisualBasic;

class Transaccion
{
    private static int refID = 0; //ID de referencia para cada instancia
    private int transaccionID; //ID propio de cada instancia
    private int monto; //Monto transferido de jugadorOrigen a jugadorDestino
    private int turno; //Turno en el que el jugador genero la transaccion
    private string fechaYHora; //Fecha con formato "dd/MM/yyyy//hh:mm:ss"
    private string tipo; //Tipo de transaccion (predefinida)
    private string jugadorOrigen;
    private string jugadorDestino;
    private string descripcion; //Descripcion autogenerada segun el tipo de transaccion

    public Transaccion(int monto, int turno, string tipo, string jugadorOrigen, string jugadorDestino, string descripcion)
    {
        this.transaccionID = Interlocked.Increment(ref refID);
        this.monto = monto;
        this.turno = turno;
        this.fechaYHora = DateAndTime.Now.ToString("dd/MM/yyyy-hh:mm:ss");
        this.tipo = tipo;
        this.jugadorOrigen = jugadorOrigen;
        this.jugadorDestino = jugadorDestino;
        this.descripcion = GenerarDescripcion();
        Console.WriteLine(this.descripcion);
    }

    // Metodo para autogenerar descripciones segun el tipo
    private string GenerarDescripcion()
    {
        
        string mensajeDescripcion;
        

        //Mensajes en mayuscula deben ser reemplazados por el nombre de la propiedad correspondiente
        switch (this.tipo)
        {
            case "Compra de propiedad":
                mensajeDescripcion = $"{jugadorOrigen} ha comprado la propiedad PROPIEDAD por {monto}";
                break;
            case "Pago de alquiler":
                mensajeDescripcion = $"{jugadorOrigen} ha pagado {monto} a {jugadorDestino} por el alquiler de la propiedad PROPIEDAD";
                break;
            case "Pago al banco":
                mensajeDescripcion = $"{jugadorOrigen} ha pagado {monto} al banco";
                break;
            case "Pago entre jugadores":
                mensajeDescripcion = $"{jugadorOrigen} ha pagado {monto} a {jugadorDestino}";
                break;
            case "Ganancia por evento":
                mensajeDescripcion = $"{jugadorOrigen} ha ganado {monto} por el evento EVENTO";
                break;
            case "Perdida por evento":
                mensajeDescripcion = $"{jugadorOrigen} ha perdido {monto} por el evento EVENTO";
                break;
            case "Premio por pasar por inicio":
                mensajeDescripcion = $"{jugadorOrigen} ha recibido {monto} por pasar por inicio";
                break;
            default:
                mensajeDescripcion = "Error";
                break;
        }

        return mensajeDescripcion;
    }
}


class PRUEBA //Esta clase es meramente de prueba, debe ser eliminada para su funcionamiento final
{
    static void Main(string[] args)
    {
        Transaccion trans1 = new Transaccion(1000, 3, "Pago al banco", "Jugador1", "Jugador 2", "ETC");
    }
}