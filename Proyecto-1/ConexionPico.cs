using System;
using System.IO.Ports;
using System.Threading;

/* Administra la conexion serial con la Raspberry Pi Pico.
La misma instancia se comparte entre el lector RFID y los dados. */
public class ConexionPico
{
    private SerialPort? puerto;
    private volatile bool conectado = false;
    private Thread? hiloEscucha;
    private bool escuchando = false;
    private string? nombrePuerto;
    private int baudRate;
    // Inicia el hilo que intenta abrir y mantener disponible el puerto serial.
    public void IniciarConexion(string nombrePuerto = "COM3", int baudRate = 115200)
    {
        this.nombrePuerto = nombrePuerto;
        this.baudRate = baudRate;

        escuchando = true;
        hiloEscucha = new Thread(EscucharPuerto);
        hiloEscucha.IsBackground = true;
        hiloEscucha.Start();
    }

    // Reintenta abrir el puerto mientras la conexion permanezca activa.
    private void EscucharPuerto()
    {
        while (escuchando)
        {
            if (!conectado)
            {
                LiberarPuerto();
                try
                {
                    puerto = new SerialPort(nombrePuerto, baudRate);
                    puerto.NewLine = "\n";
                    puerto.ReadTimeout = 2000;
                    puerto.Open();
                    puerto.DtrEnable = true;
                    puerto.RtsEnable = true;

                    conectado = true;
                    Console.WriteLine($"[Pico] ¡Conectada en {nombrePuerto}!\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Pico] No se encontró en {nombrePuerto} ({e.Message}), reintentando...");
                    conectado = false;
                    Thread.Sleep(2000);
                    continue;
                }
            }

            Thread.Sleep(500);
            if (puerto == null || !puerto.IsOpen)
            {
                conectado = false;
            }
        }
    }

    // Cierra y libera el puerto actual antes de intentar reconectarlo.
    private void LiberarPuerto()
    {
        try
        {
            if (puerto != null)
            {
                if (puerto.IsOpen) puerto.Close();
                puerto.Dispose();
            }
        }
        catch { }
        finally
        {
            puerto = null;
        }
    }

    // Indica si el puerto esta abierto y listo para comunicarse.
    public bool EstaConectado()
    {
        return conectado && puerto != null && puerto.IsOpen;
    }
    /* Lee una linea cruda del puerto. Puede contener un UID, CASILLAS:7,
    mensajes de estado u otra respuesta de la Pico. */
    public string LeerLinea()
    {
        if (!EstaConectado()) return string.Empty;

        try
        {
            return puerto.ReadLine().Trim();
        }
        catch (TimeoutException)
        {
            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine("[Pico] Se perdió la conexión: " + e.Message);
            conectado = false;
            return string.Empty;
        }
    }
    /* Descarta datos pendientes antes de cambiar de modo de trabajo.
    No debe llamarse mientras se espera una tarjeta o una tirada. */
    public void LimpiarEntrada()
    {
        if (!EstaConectado()) return;

        try
        {
            puerto.DiscardInBuffer();
        }
        catch (Exception e)
        {
            Console.WriteLine("[Pico] No se pudo limpiar la entrada: " + e.Message);
        }
    }
    // Envia un comando de texto a la Pico, por ejemplo TARJETAS, DADOS o STOP.
    public void EnviarComando(string comando)
    {
        if (!EstaConectado()) return;

        try
        {
            puerto.WriteLine(comando);
        }
        catch (Exception e)
        {
            Console.WriteLine("[Pico] Error enviando comando: " + e.Message);
            conectado = false;
        }
    }

    // Detiene el hilo y cierra el puerto serial.
    public void CerrarConexion()
    {
        escuchando = false;
        conectado = false;
        LiberarPuerto();
    }
}