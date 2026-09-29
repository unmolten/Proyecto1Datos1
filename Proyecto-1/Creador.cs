using System;
using System.Threading;

/**
 * @file Creador.cs
 * @brief Modulo auxiliar para la creacion y registro de jugadores interactivos.
 *
 * Facilita el asistente por consola para solicitar nombres y vincularlos
 * con tarjetas fisicas leidas por el lector RFID RC522 en la Raspberry Pi Pico.
 */

/**
 * @class Creador
 * @brief Provee metodos estaticos para capturar y registrar jugadores.
 */
class Creador
{
    /**
     * @brief Coordina la captura interactiva de participantes y sus tarjetas RFID.
     *
     * Espera a que la Raspberry Pi Pico este conectada, solicita la cantidad de
     * participantes y, para cada uno, pide su nombre y la presentacion de una tarjeta
     * fisica en el lector RFID, garantizando que ningun UID quede duplicado.
     * @param conexion Instancia activa de ConexionPico para comunicarse con el hardware.
     * @return Arreglo de instancias de Jugador creadas y configuradas con su UID correspondiente.
     */
    public static Jugador[] CrearJugador(ConexionPico conexion)
    {
        // Se comparte la conexion serial con el resto de componentes del sistema
        ObtenerID lector = new ObtenerID(conexion);

        Console.WriteLine("Esperando a que la Pico se conecte...");
        while (!lector.EstaConectado())
        {
            Thread.Sleep(300);
        }
        Console.WriteLine("Lector listo.\n");
        lector.IniciarLecturaTarjetas();

        Console.Write("¿Cuantos jugadores van a jugar?: ");
        if (!int.TryParse(Console.ReadLine(), out int cantidad) || cantidad <= 0)
        {
            Console.WriteLine("Cantidad invalida.");
            return Array.Empty<Jugador>();
        }

        Jugador[] listaJugadores = new Jugador[cantidad];
        int contador = 0;

        // Registro iterativo de cada participante
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
            conexion.LimpiarEntrada();

            string idUnico = string.Empty;
            while (string.IsNullOrEmpty(idUnico))
            {
                if (!lector.EstaConectado())
                {
                    Console.WriteLine("   (Se perdio la conexion con la Pico, esperando reconexion...)");
                    while (!lector.EstaConectado())
                    {
                        Thread.Sleep(300);
                    }
                    Console.WriteLine("   Pico reconectada, pase la tarjeta de nuevo.");
                }

                idUnico = lector.LeerTarjeta();

                if (!string.IsNullOrEmpty(idUnico))
                {
                    // Comprobar que la tarjeta no pertenezca a un jugador registrado previamente
                    bool yaAsignada = false;
                    for (int j = 0; j < contador; j++)
                    {
                        if (listaJugadores[j].GetIdTarjeta().Equals(idUnico, StringComparison.OrdinalIgnoreCase))
                        {
                            yaAsignada = true;
                            break;
                        }
                    }
                    if (yaAsignada)
                    {
                        Console.WriteLine($"   [AVISO] La tarjeta {idUnico} ya esta asignada a otro jugador. Pase una tarjeta diferente.");
                        idUnico = string.Empty;
                        Thread.Sleep(500);
                        continue;
                    }
                }
                else
                {
                    Thread.Sleep(150); // Pausa breve para evitar consumo innecesario de procesador
                }
            }

            // El UID se almacena como texto hexadecimal completo
            Jugador nuevoJugador = new Jugador(nombreIngresado, idUnico, 1500);
            listaJugadores[contador] = nuevoJugador;
            contador++;

            Console.WriteLine($"Jugador creado con exito: [{nuevoJugador.GetNombre()} -> ID: {nuevoJugador.GetIdTarjeta()}]");
            conexion.LimpiarEntrada();
            Thread.Sleep(300);
        }

        // Muestra el resumen de participantes registrados
        Console.WriteLine("\n=== JUGADORES LISTOS PARA EL MONOPOLY ===");
        for (int i = 0; i < contador; i++)
        {
            Jugador j = listaJugadores[i];
            Console.WriteLine($"Nombre: {j.GetNombre()} | UID Tarjeta: {j.GetIdTarjeta()}");
        }

        return listaJugadores;
    }
}