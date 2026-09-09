using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Clase TcpClientApp (Cliente de Monopoly):
// Aplicación de consola que permite a un jugador conectarse al servidor TCP de Monopoly.
// 
// Características:
// - Comunicación Bidireccional Asíncrona:
//   1. Hilo Secundario (Background Task): Escucha y muestra mensajes provenientes del servidor
//      en tiempo real (tiradas de dados, compras, cambios de turno, mensajes de chat).
//   2. Hilo Principal: Lee la entrada del usuario en la consola y la envía al servidor a través del socket.
class TcpClientApp
{
    // Puerto predeterminado donde escucha el servidor de Monopoly
    private const int Port = 6767;

    static async Task Main(string[] args)
    {
        // Solicita la dirección IP del servidor al usuario (por defecto localhost: 127.0.0.1)
        Console.Write("Ingresa la IP del servidor (ejemplo 127.0.0.1): ");
        string ipconsola = Console.ReadLine() ?? "";
        string ipAddress = string.IsNullOrWhiteSpace(ipconsola) ? "127.0.0.1" : ipconsola;

        try
        {
            // Inicializa el cliente TCP
            using TcpClient client = new TcpClient();
            Console.WriteLine($"[CLIENTE] Conectando a {ipAddress}:{Port}...");
            
            // Intenta establecer conexión con el servidor
            await client.ConnectAsync(ipAddress, Port);
            Console.WriteLine("[CLIENTE] ¡Conectado con éxito al Monopoly! Escribe 'exit' para salir.");

            // Canales de entrada y salida sobre el flujo de red (NetworkStream)
            using NetworkStream stream = client.GetStream();
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using CancellationTokenSource cts = new CancellationTokenSource();

            // TAREA EN SEGUNDO PLANO PARA RECEPCIÓN DE MENSAJES:
            // Permite que el cliente reciba eventos del servidor en cualquier momento sin quedar bloqueado
            // esperando a que el usuario presione Enter en la consola.
            var tareaRecepcion = Task.Run(async () =>
            {
                try
                {
                    string? linea;
                    while (!cts.Token.IsCancellationRequested && (linea = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine(linea);
                    }
                }
                catch
                {
                    // Manejo silencioso cuando la conexión se termina o se cancela
                }
            });

            // BUCLE PRINCIPAL (ENVÍO DE COMANDOS):
            // Lee lo que el jugador escribe y lo despacha al servidor por el socket
            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                // Envía el comando finalizado en salto de línea para que 'ReadLineAsync' del servidor lo procese
                await writer.WriteLineAsync(input);

                // Si el jugador ingresa 'exit', finalizamos la sesión local
                if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[CLIENTE] Desconectando...");
                    cts.Cancel();
                    break;
                }
            }

            // Esperar a que la tarea de recepción finalice ordenadamente
            await tareaRecepcion;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENTE] Error de conexión: {ex.Message}");
        }
    }
}