using System;
using System.Collections.Generic;
using System.Threading;
// Coordina la captura de nombres y tarjetas para crear jugadores.
class Creador
{
    /* Espera la conexion, solicita la cantidad de jugadores, lee sus nombres
    y asigna un UID RFID valido a cada jugador. */
    public static void CrearJugador(ConexionPico conexion)
    {
        // La conexion se recibe desde Program para compartir el mismo puerto
        // con ControlDados y cerrarlo una sola vez al final.
        ObtenerID lector = new ObtenerID(conexion);
        

        // IniciarConexion trabaja en segundo plano; aqui esperamos a que este lista.
        Console.WriteLine("Esperando a que la Pico 2 W se conecte...");
        while (!lector.EstaConectado())
        {
            Thread.Sleep(300);
        }
        Console.WriteLine("Lector listo.\n");
        lector.IniciarLecturaTarjetas();

        List<Jugador> listaJugadores = new List<Jugador>();

        Console.Write("¿Cuántos jugadores van a jugar?: ");
        if (!int.TryParse(Console.ReadLine(), out int cantidad) || cantidad <= 0)
        {
            Console.WriteLine("Cantidad invalida.");
            return;
        }

        // Cada jugador se crea solo despues de recibir un UID validado.
        for (int i = 1; i <= cantidad; i++)
        {
            Console.Write($"\nIngrese el nombre del Jugador {i}: ");
            string? nombreIngresado = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombreIngresado))
            {
                Console.WriteLine("El nombre no puede estar vacio.");
                i--;
                continue;
            }

            Console.WriteLine($"-> Pase la tarjeta por el lector para asignarla a {nombreIngresado}...");

            // LeerTarjeta puede devolver vacio mientras no llega una tarjeta.
            string idUnico = string.Empty;
            while (string.IsNullOrEmpty(idUnico))
            {
                if (!lector.EstaConectado())
                {
                    Console.WriteLine("   (Se perdió la conexión con la Pico, esperando reconexión...)");
                    while (!lector.EstaConectado())
                    {
                        Thread.Sleep(300);
                    }
                    Console.WriteLine("   Pico reconectada, pase la tarjeta de nuevo.");
                }

                idUnico = lector.LeerTarjeta();

                if (string.IsNullOrEmpty(idUnico))
                {
                    Thread.Sleep(200); // evita consumir CPU de más mientras espera
                }
            }

            // El UID se conserva como string para no perder ceros iniciales.
            Jugador nuevoJugador = new Jugador(nombreIngresado, idUnico);
            listaJugadores.Add(nuevoJugador);

            Console.WriteLine($"¡Jugador creado con éxito! [{nuevoJugador.Nombre} -> ID: {nuevoJugador.IdTarjeta}]");
        }

        // Muestra los jugadores registrados para confirmar la captura.
        Console.WriteLine("\n=== JUGADORES LISTOS PARA EL MONOPOLY ===");
        foreach (Jugador j in listaJugadores)
        {
            Console.WriteLine($"Nombre: {j.Nombre} | UID Tarjeta: {j.IdTarjeta}");
        }

    }
}