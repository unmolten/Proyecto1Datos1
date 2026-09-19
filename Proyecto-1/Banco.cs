static class Banco
{
        // Metodo para procesar la transaccion dentro del juego

    public static void ProcesarTransaccion(Jugador jugadorOrigen, Jugador jugadorDestino, int monto, string tipo)
    {
        switch (tipo)
        {
            case "Compra de propiedad":

                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de comprar propiedad cuando jugadorOrigen es null");
                    return;
                }

                jugadorOrigen.Balance -= monto;

                break;
            case "Pago de alquiler":

                if (jugadorOrigen == null || jugadorDestino == null)
                {
                    Console.WriteLine("ERROR: Intento de pagar alquiler cuando jugadorOrigen o jugadorDestino es null");
                    return;
                }

                jugadorOrigen.Balance -= monto;
                jugadorDestino.Balance += monto;
                break;
            case "Pago al banco":

                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de pagar al banco cuando jugadorOrigen es null");
                    return;
                }

                jugadorOrigen.Balance -= monto;
                break;
            case "Pago entre jugadores":

                if (jugadorOrigen == null || jugadorDestino == null)
                {
                    Console.WriteLine("ERROR: Intento de pago entre jugadores cuando jugadorOrigen o jugadorDestino es null");
                    return;
                }

                jugadorOrigen.Balance -= monto;
                jugadorDestino.Balance += monto;
                break;
            case "Ganancia por evento":
                
                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de ganancia por evento cuando jugadorOrigen es null");
                    return;
                }

                jugadorOrigen.Balance += monto;
                break;
            case "Perdida por evento":

                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de perdida por evento cuando jugadorOrigen es null");
                    return;
                }

                jugadorOrigen.Balance -= monto;
                break;
            case "Premio por pasar por inicio":

                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de premio por pasar por inicio cuando jugadorOrigen es null");
                    return;
                }

                jugadorOrigen.Balance += monto;
                break;
            default:
                Console.WriteLine("ERROR: Tipo de transaccion no reconocido");
                break;
        }
    }

    private static void TransferirPropiedad(Jugador nuevoPropietario)
    {
        
    }
}