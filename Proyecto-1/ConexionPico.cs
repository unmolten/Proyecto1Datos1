using System;
using System.IO.Ports;
using System.Threading;

/**
 * @file ConexionPico.cs
 * @brief Administrador de la comunicacion serial con la Raspberry Pi Pico.
 *
 * Esta clase encapsula la configuracion y lectura continua del puerto COM,
 * permitiendo recibir lecturas del sensor RFID RC522 y eventos de dados digitales
 * compartiendo un unico canal fisico con reconexion automatica en segundo plano.
 */

/**
 * @class ConexionPico
 * @brief Gestiona el ciclo de vida de la conexion serial con la Raspberry Pi Pico.
 */
public class ConexionPico
{
    /** @brief Instancia interna del puerto serial del sistema operativo. */
    private SerialPort? puerto;

    /** @brief Indicador volatil de estado de conexion activa. */
    private volatile bool conectado = false;

    /** @brief Hilo de ejecucion en segundo plano para escuchar el puerto. */
    private Thread? hiloEscucha;

    /** @brief Bandera para controlar el bucle del hilo de escucha. */
    private bool escuchando = false;

    /** @brief Nombre identificador del puerto (ej: "COM3", "/dev/ttyACM0"). */
    private string? nombrePuerto;

    /** @brief Velocidad en baudios para la transmision serial. */
    private int baudRate;

    /** @brief Objeto de sincronizacion para proteger el acceso concurrente al buffer serial. */
    private readonly object bufferLock = new object();

    /** @brief Cadena que almacena temporalmente los caracteres recibidos por el puerto. */
    private string bufferSerial = "";

    /**
     * @brief Inicia el hilo en segundo plano encargado de abrir y mantener la conexion serial.
     * @param nombrePuerto Nombre del puerto COM a conectar (por defecto "COM3").
     * @param baudRate Velocidad de comunicacion en baudios (por defecto 115200).
     */
    public void IniciarConexion(string nombrePuerto = "COM3", int baudRate = 115200)
    {
        this.nombrePuerto = nombrePuerto;
        this.baudRate = baudRate;

        escuchando = true;
        hiloEscucha = new Thread(EscucharPuerto);
        hiloEscucha.IsBackground = true;
        hiloEscucha.Start();
    }

    /**
     * @brief Metodo ejecutado por el hilo de escucha para conectar, leer y reconectar el puerto.
     */
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
                    puerto.Encoding = System.Text.Encoding.UTF8;
                    puerto.NewLine = "\n";
                    puerto.ReadTimeout = 200;
                    puerto.WriteTimeout = 500;
                    puerto.Handshake = Handshake.None;
                    puerto.DtrEnable = true;
                    puerto.Open();

                    // Enviar Ctrl-D (\x04) para reiniciar MicroPython si estaba en modo interactivo REPL
                    try
                    {
                        puerto.Write("\r\n\x04");
                        puerto.BaseStream.Flush();
                        Thread.Sleep(300);
                    }
                    catch { }

                    conectado = true;
                    Console.WriteLine($"[Pico] Conectada en {nombrePuerto}.\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Pico] No se encontro en {nombrePuerto} ({e.Message}), reintentando...");
                    conectado = false;
                    Thread.Sleep(2000);
                    continue;
                }
            }

            // Lectura continua de datos entrantes hacia el buffer en memoria
            try
            {
                var p = puerto;
                if (p != null && p.IsOpen)
                {
                    if (p.BytesToRead > 0)
                    {
                        string data = p.ReadExisting();
                        if (!string.IsNullOrEmpty(data))
                        {
                            lock (bufferLock)
                            {
                                bufferSerial += data;
                            }
                        }
                    }
                }
                else
                {
                    conectado = false;
                }
            }
            catch
            {
                conectado = false;
            }

            Thread.Sleep(25);
        }
    }

    /**
     * @brief Cierra y libera los recursos del puerto serial de forma segura.
     */
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

    /**
     * @brief Comprueba si el puerto serial se encuentra abierto y listo para operar.
     * @return true si esta conectado y abierto; false en caso contrario.
     */
    public bool EstaConectado()
    {
        return conectado && puerto != null && puerto.IsOpen;
    }

    /**
     * @brief Extrae y devuelve la primera linea completa pendiente en el buffer serial.
     *
     * Remueve los caracteres de salto de linea (\r, \n) y actualiza el buffer restante.
     * @return Cadena de texto recibida o string.Empty si no hay lineas completas.
     */
    public string LeerLinea()
    {
        var p = puerto;
        if (!conectado || p == null || !p.IsOpen) return string.Empty;

        lock (bufferLock)
        {
            try
            {
                while (true)
                {
                    int idxN = bufferSerial.IndexOf('\n');
                    int idxR = bufferSerial.IndexOf('\r');
                    if (idxN < 0 && idxR < 0)
                    {
                        break;
                    }

                    int cut = (idxN >= 0 && idxR >= 0) ? Math.Min(idxN, idxR) : Math.Max(idxN, idxR);
                    string linea = bufferSerial.Substring(0, cut).Trim();
                    int skip = cut + 1;
                    while (skip < bufferSerial.Length && (bufferSerial[skip] == '\n' || bufferSerial[skip] == '\r'))
                    {
                        skip++;
                    }
                    bufferSerial = bufferSerial.Substring(skip);

                    if (!string.IsNullOrWhiteSpace(linea))
                    {
                        return linea;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Pico] Error procesando buffer serial: " + e.Message);
                return string.Empty;
            }

            return string.Empty;
        }
    }

    /**
     * @brief Limpia el buffer en memoria y el buffer de entrada del puerto fisico.
     *
     * Previene que lecturas residuales o comandos anteriores interfieran con nuevas lecturas.
     */
    public void LimpiarEntrada()
    {
        var p = puerto;
        lock (bufferLock)
        {
            bufferSerial = "";
        }
        if (!conectado || p == null || !p.IsOpen) return;

        try
        {
            p.DiscardInBuffer();
        }
        catch (Exception e)
        {
            Console.WriteLine("[Pico] No se pudo limpiar la entrada: " + e.Message);
        }
    }

    /**
     * @brief Transmite un comando de texto a la Raspberry Pi Pico finalizado en retorno de carro y linea.
     * @param comando Texto del comando a enviar (ej: "TARJETAS", "DADOS", "STOP").
     */
    public void EnviarComando(string comando)
    {
        var p = puerto;
        if (!conectado || p == null || !p.IsOpen) return;

        try
        {
            p.Write(comando + "\r\n");
            try { p.BaseStream.Flush(); } catch { }
        }
        catch (Exception e)
        {
            Console.WriteLine("[Pico] Error enviando comando: " + e.Message);
            conectado = false;
        }
    }

    /**
     * @brief Detiene el hilo de escucha y cierra definitivamente el puerto serial.
     */
    public void CerrarConexion()
    {
        escuchando = false;
        conectado = false;
        LiberarPuerto();
    }
}