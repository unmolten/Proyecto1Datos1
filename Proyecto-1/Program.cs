using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// Clase Server:
// Servidor TCP asíncrono y multihilo para el juego de Monopoly.
// 
// Funcionamiento:
// 1. Escucha peticiones entrantes en el puerto 6767 usando TcpListener.
// 2. Por cada cliente que se conecta, lanza una tarea en segundo plano ('Task.Run') para atenderlo de forma concurrente.
// 3. Crea e inicializa el tablero circular de Monopoly ('JuegoMonopoly').
// 4. Asocia cada socket conectado con un objeto 'Jugador', gestionando su ciclo de vida (conexión, comandos, desconexión).
class Server
{
    // Puerto de red local utilizado para la comunicación por sockets
    private const int puerto = 6767;

    // Objeto de bloqueo para sincronizar la escritura en la consola del servidor
    // y evitar que mensajes de distintos clientes se entrecrucen o desordenen
    private static readonly object consolaLock = new object();

    static async Task Main(string[] args)
    {
        // 1. Inicializamos la instancia global del juego Monopoly con su lista circular de casillas
        var juego = JuegoMonopoly.Instancia;

        // 2. Inicia el socket TCP de escucha en todas las interfaces de red locales (IPAddress.Any)
        TcpListener servidor = new TcpListener(IPAddress.Any, puerto);
        servidor.Start();

        // Muestra la información de bienvenida y diagnóstico en la consola del servidor
        Console.WriteLine("==================================================");
        Console.WriteLine("     SERVIDOR DE MONOPOLY (LISTAS ENLAZADAS)     ");
        Console.WriteLine("==================================================");
        Console.WriteLine($"Dirección IP del servidor: {((IPEndPoint)servidor.LocalEndpoint).Address}");
        Console.WriteLine($"Puerto en uso: {puerto}");
        Console.WriteLine($"Tablero cargado con {juego.Tablero.Size()} casillas en lista circular.");
        Console.WriteLine("Esperando conexiones de jugadores...");
        Console.WriteLine("==================================================");

        try
        {
            // Bucle principal del servidor: Acepta nuevos clientes indefinidamente
            while (true)
            {
                // Espera asíncrona no bloqueante hasta que un cliente intente conectarse
                TcpClient cliente = await servidor.AcceptTcpClientAsync();
                var remoteIp = ((IPEndPoint)cliente.Client.RemoteEndPoint!).Address;

                lock (consolaLock)
                {
                    Console.WriteLine($"[CONEXIÓN] Cliente conectado desde IP: {remoteIp}");
                }

                // Despacha la atención del nuevo cliente a un hilo del ThreadPool mediante Task.Run
                // para que el servidor continúe aceptando otras conexiones sin detenerse
                _ = Task.Run(() => ManejoDeClienteAsync(cliente));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR SERVIDOR] {ex.Message}");
            throw;
        }
        finally
        {
            // Cierre seguro del socket del servidor al finalizar la aplicación
            servidor.Stop();
        }
    }

    // Maneja el ciclo de vida completo de la sesión de un cliente conectado:
    // - Configuración de canales de entrada (StreamReader) y salida (StreamWriter).
    // - Registro del jugador en la partida.
    // - Lectura de comandos en bucle y delegación a 'Metodos.ProcesarComando'.
    // - Limpieza y desconexión segura al salir.
    private static async Task ManejoDeClienteAsync(TcpClient cliente)
    {
        Jugador? jugador = null;
        var juego = JuegoMonopoly.Instancia;

        try
        {
            using (cliente)
            using (NetworkStream stream = cliente.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
            {
                // Registramos al cliente como un nuevo jugador en el juego (se le asigna el nodo Salida)
                jugador = juego.RegistrarJugador(writer);

                // Mensaje de bienvenida inicial enviado directamente al terminal del cliente
                jugador.EnviarMensaje($"\n🎲 ¡Bienvenido al Monopoly TCP, {jugador.Nombre}!");
                jugador.EnviarMensaje($"Comienzas con $1500 en la casilla 'Salida'.");
                jugador.EnviarMensaje("Escribe 'TIRAR' para lanzar los dados o 'AYUDA' para ver los comandos.\n");

                // Muestra el menú de ayuda inicial en el cliente
                Metodos.MostrarAyuda(jugador);

                string? mensaje;
                // Bucle de lectura: Lee líneas completas enviadas por el cliente hasta que se cierre la conexión
                while ((mensaje = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(mensaje)) continue;

                    lock (consolaLock)
                    {
                        Console.WriteLine($"[{jugador.Nombre}]: {mensaje}");
                    }

                    // Comando para desconexión voluntaria
                    if (mensaje.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        jugador.EnviarMensaje("¡Hasta la próxima!");
                        break;
                    }

                    // Procesa el comando del juego conectando la entrada de red con las listas enlazadas
                    Metodos.ProcesarComando(mensaje, jugador);
                }
            }
        }
        catch (Exception ex)
        {
            lock (consolaLock)
            {
                Console.WriteLine($"[CLIENTE DESCONECTADO] {(jugador?.Nombre ?? "Cliente")}: {ex.Message}");
            }
        }
        finally
        {
            // Si el jugador estaba registrado, lo retiramos de la lista de jugadores activos
            if (jugador != null)
            {
                juego.DesconectarJugador(jugador);
            }
        }
    }
}