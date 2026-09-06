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
    private string fechaYHora; //Fecha con formato "dd/MM/yyyy-hh:mm:ss"
    private string tipo; //Tipo de 
    private string jugadorOrigen;
    private string jugadorDestino;
    private string descripcion;

    public Transaccion(int monto, int turno, string tipo, string jugadorOrigen, string jugadorDestino, string descripcion)
    {
        this.transaccionID = Interlocked.Increment(ref refID);
        this.monto = monto;
        this.turno = turno;
        this.fechaYHora = DateAndTime.Now.ToString("dd/MM/yyyy-hh:mm:ss");
        this.tipo = tipo;
        this.jugadorOrigen = jugadorOrigen;
        this.jugadorDestino = jugadorDestino;
        this.descripcion = descripcion;
    }
}