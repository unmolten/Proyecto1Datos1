/*
Este es un archivo de prueba, para manejar los modulos
que realizo.

CAMBIAR EL NAMESPACE SEGUN SEA NECESARIO
*/


class TEST
{
    static void Main(string[] args)
    {
        //Instancia de prueba
        Transaccion trans1 = new Transaccion(1000, 3, "Pago al banco", "Jugador1", "Jugador 2");
        Transaccion trans2 = new Transaccion(2000, 5, "Pago de alquiler", "Carlos", "Sofia");
        Transaccion trans3 = new Transaccion(4000, 7, "Ganancia por evento", "Maria", "Destino");

        //Prueba de metodo estatico ImprimirTransacciones
        Transaccion.ImprimirTransacciones();
    }
}