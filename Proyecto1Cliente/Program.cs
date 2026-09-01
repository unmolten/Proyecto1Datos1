using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TcpClientApp
{
    private const int Port = 6767;

    static async Task Main(string[] args)
    {
        Console.Write("Ingresa la IP del servidor (ejemplo 127.0.0.1): ");
        string ipconsola = Console.ReadLine() ?? "";
        string ipAddress = string.IsNullOrWhiteSpace(ipconsola) ? "127.0.0.1" : ipconsola;
        try
        {
            using TcpClient client = new TcpClient();
            Console.WriteLine($"[CLIENTE] Conectando a {ipAddress}:{Port}...");
            
            await client.ConnectAsync(ipAddress, Port);
            Console.WriteLine("[CLIENTE] ¡Conectado con éxito! Escribe mensajes para enviar (escribe 'exit' para salir):");

            using NetworkStream stream = client.GetStream();
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                // Envía el mensaje terminando con salto de línea para StreamReader.ReadLineAsync()
                await writer.WriteLineAsync(input);

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[CLIENTE] Desconectando...");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENTE] Error de conexión: {ex.Message}");
        }
    }
}