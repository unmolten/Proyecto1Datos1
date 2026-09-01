using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;


class Server
{
    private const int puerto = 6767; // Puerto de red utilizado para la comunicacion. 67 +1000 de aura

    private static readonly object consolaLock = new object(); // FIX: para evitar que los mensajes de distintos clientes se mezclen en consola

    static async Task Main(string[] args)
    {
        //Inicia servidor TCP en la dirección IP local y puerto especificado
        TcpListener servidor = new TcpListener(IPAddress.Any, puerto);
        servidor.Start();

        //Muestra informacion del servidor
        Console.WriteLine($"Dirección IP del servidor: {((IPEndPoint)servidor.LocalEndpoint).Address}");
        Console.WriteLine($"Puerto en uso: {puerto}");

        try
        {
            while (true)
            {
                TcpClient cliente = await servidor.AcceptTcpClientAsync();
                Console.WriteLine("Cliente conectado:");
                // FIX: la IP del cliente se obtiene de RemoteEndPoint, no de servidor.LocalEndpoint (eso era 0.0.0.0)
                Console.WriteLine($"IP: {((IPEndPoint)cliente.Client.RemoteEndPoint!).Address}");

                _ = Task.Run(() => ManejoDeClienteAsync(cliente));
            }
        }
        catch (System.Exception)
        {
            
            throw;
        }
        finally
        {
            //Cerrar servidor
            servidor.Stop();
        }
    }

    private static async Task ManejoDeClienteAsync(TcpClient cliente)
    {
        using (cliente)
        using (NetworkStream stream = cliente.GetStream())
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            string? mensaje;
            while ((mensaje = await reader.ReadLineAsync()) != null)
            {
                // FIX: lock para que dos clientes escribiendo a la vez no corten el texto del otro
                lock (consolaLock)
                {
                    Console.WriteLine($"Mensaje: {mensaje}");
                }

                if (!mensaje.IsWhiteSpace())
                {
                    Metodos.mensajeshow(mensaje);
                }
            }
        }
    }
}