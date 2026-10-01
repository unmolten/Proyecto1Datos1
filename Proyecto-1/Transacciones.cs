using System;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic;

/**
 * @file Transacciones.cs
 * @brief Registro, procesamiento y almacenamiento de transacciones financieras.
 *
 * Cada movimiento de dinero (compras, alquileres, pagos al banco, ganancias por eventos)
 * se procesa y persiste en un archivo de texto secuencial ('Almacenamiento.txt'),
 * permitiendo generar reportes contables e historiales filtrados.
 */

/**
 * @class Transaccion
 * @brief Modela una operacion monetaria entre participantes o con la banca.
 */
class Transaccion
{
    /** @brief Ruta relativa al archivo donde se almacenan las transacciones en crudo. */
    private const string rutaAlmacenamiento = "../../../Almacenamiento.txt";

    /** @brief Ruta relativa al archivo de reporte formateado para lectura humana. */
    private const string rutaReporte = "../../../Reporte.txt";

    /** @brief Contador atomico para la generacion incremental de identificadores unicos. */
    private static int refID = 0;

    /** @brief Identificador secuencial unico de la transaccion. */
    private int transaccionID;

    /** @brief Cantidad de dinero transferida. */
    private int monto;

    /** @brief Numero de turno en el que se efectuo la operacion. */
    private int turno;

    /** @brief Estampa de tiempo de la transaccion con formato "dd/MM/yyyy-hh:mm:ss". */
    private string fechaYHora;

    /** @brief Categoria funcional de la operacion (ej: "Compra de propiedad", "Pago de alquiler"). */
    private string tipo;

    /** @brief Jugador pagador o emisor de fondos (null representa el Banco). */
    private Jugador? jugadorOrigen;

    /** @brief Jugador cobrador o receptor de fondos (null representa el Banco). */
    private Jugador? jugadorDestino;

    /** @brief Mensaje explicativo generado automaticamente para la operacion. */
    private string descripcion;

    /**
     * @brief Obtiene el ID unico de la transaccion.
     * @return Entero con el identificador.
     */
    public int GetTransaccionID()
    {
        return this.transaccionID;
    }

    /**
     * @brief Asigna el ID unico de la transaccion.
     * @param transaccionID Nuevo identificador entero.
     */
    public void SetTransaccionID(int transaccionID)
    {
        this.transaccionID = transaccionID;
    }

    /**
     * @brief Obtiene el importe monetario involucrado.
     * @return Monto en dinero.
     */
    public int GetMonto()
    {
        return this.monto;
    }

    /**
     * @brief Asigna el monto monetario involucrado.
     * @param monto Nuevo monto.
     */
    public void SetMonto(int monto)
    {
        this.monto = monto;
    }

    /**
     * @brief Obtiene el numero de turno en el que ocurrio.
     * @return Turno numerico.
     */
    public int GetTurno()
    {
        return this.turno;
    }

    /**
     * @brief Asigna el numero de turno.
     * @param turno Numero de turno.
     */
    public void SetTurno(int turno)
    {
        this.turno = turno;
    }

    /**
     * @brief Obtiene la fecha y hora de la transaccion.
     * @return Cadena con la estampa temporal.
     */
    public string GetFechaYHora()
    {
        return this.fechaYHora;
    }

    /**
     * @brief Asigna la fecha y hora de la transaccion.
     * @param fechaYHora Cadena formateada.
     */
    public void SetFechaYHora(string fechaYHora)
    {
        this.fechaYHora = fechaYHora;
    }

    /**
     * @brief Obtiene el tipo de transaccion.
     * @return Nombre del tipo.
     */
    public string GetTipo()
    {
        return this.tipo;
    }

    /**
     * @brief Asigna el tipo de transaccion.
     * @param tipo Cadena con el nuevo tipo.
     */
    public void SetTipo(string tipo)
    {
        this.tipo = tipo;
    }

    /**
     * @brief Obtiene el jugador emisor de fondos.
     * @return Jugador emisor o null si proviene del Banco.
     */
    public Jugador? GetJugadorOrigen()
    {
        return this.jugadorOrigen;
    }

    /**
     * @brief Asigna el jugador emisor de fondos.
     * @param jugadorOrigen Jugador emisor o null para el Banco.
     */
    public void SetJugadorOrigen(Jugador? jugadorOrigen)
    {
        this.jugadorOrigen = jugadorOrigen;
    }

    /**
     * @brief Obtiene el jugador receptor de fondos.
     * @return Jugador receptor o null si va al Banco.
     */
    public Jugador? GetJugadorDestino()
    {
        return this.jugadorDestino;
    }

    /**
     * @brief Asigna el jugador receptor de fondos.
     * @param jugadorDestino Jugador receptor o null para el Banco.
     */
    public void SetJugadorDestino(Jugador? jugadorDestino)
    {
        this.jugadorDestino = jugadorDestino;
    }

    /**
     * @brief Obtiene el texto descriptivo de la operacion.
     * @return Descripcion textual.
     */
    public string GetDescripcion()
    {
        return this.descripcion;
    }

    /**
     * @brief Asigna el texto descriptivo de la operacion.
     * @param descripcion Nueva descripcion textual.
     */
    public void SetDescripcion(string descripcion)
    {
        this.descripcion = descripcion;
    }

    /**
     * @brief Constructor que inicializa, procesa y persiste una transaccion.
     * @param monto Cantidad de dinero transferido.
     * @param turno Turno de juego en el que ocurre.
     * @param tipo Categoria descriptiva de la operacion.
     * @param jugadorOrigen Participante que paga (null si paga el Banco).
     * @param jugadorDestino Participante que cobra (null si cobra el Banco).
     */
    public Transaccion(int monto, int turno, string tipo, Jugador? jugadorOrigen, Jugador? jugadorDestino)
    {   
        this.transaccionID = Interlocked.Increment(ref refID);
        this.monto = monto;
        this.turno = turno;
        this.fechaYHora = DateAndTime.Now.ToString("dd/MM/yyyy-hh:mm:ss");
        this.tipo = tipo;
        this.jugadorOrigen = jugadorOrigen;
        this.jugadorDestino = jugadorDestino;
        this.descripcion = this.GenerarDescripcion();

        // Actualizar saldos de los participantes involucrados
        this.ProcesarTransaccion();

        // Registrar la operacion en el archivo persistente
        this.AlmacenarTransaccion();
    }

    /**
     * @brief Modifica los balances financieros de origen y destino segun el tipo de operacion.
     */
    private void ProcesarTransaccion()
    {
        switch (this.tipo)
        {
            case "Compra de propiedad":
                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de comprar propiedad cuando jugadorOrigen es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() - monto);
                break;

            case "Pago de alquiler":
                if (jugadorOrigen == null || jugadorDestino == null)
                {
                    Console.WriteLine("ERROR: Intento de pagar alquiler cuando jugadorOrigen o jugadorDestino es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() - monto);
                jugadorDestino.SetBalance(jugadorDestino.GetBalance() + monto);
                break;

            case "Pago al banco":
                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de pagar al banco cuando jugadorOrigen es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() - monto);
                break;

            case "Pago entre jugadores":
                if (jugadorOrigen == null || jugadorDestino == null)
                {
                    Console.WriteLine("ERROR: Intento de pago entre jugadores cuando jugadorOrigen o jugadorDestino es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() - monto);
                jugadorDestino.SetBalance(jugadorDestino.GetBalance() + monto);
                break;

            case "Ganancia por evento":
                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de ganancia por evento cuando jugadorOrigen es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() + monto);
                break;

            case "Perdida por evento":
                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de perdida por evento cuando jugadorOrigen es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() - monto);
                break;

            case "Premio por pasar por inicio":
                if (jugadorOrigen == null)
                {
                    Console.WriteLine("ERROR: Intento de premio por pasar por inicio cuando jugadorOrigen es null");
                    return;
                }
                jugadorOrigen.SetBalance(jugadorOrigen.GetBalance() + monto);
                break;

            default:
                Console.WriteLine("ERROR: Tipo de transaccion no reconocido");
                break;
        }
    }

    /**
     * @brief Genera un texto explicativo legible en lenguaje natural sobre la transaccion.
     * @return Cadena que resume quien pago a quien y por que concepto.
     */
    private string GenerarDescripcion()
    {
        string mensajeDescripcion;

        string origen = this.jugadorOrigen?.GetNombre() ?? "El Banco";
        string destino = this.jugadorDestino?.GetNombre() ?? "el Banco";

        switch (this.tipo)
        {
            case "Compra de propiedad":
                mensajeDescripcion = $"{origen} ha comprado una propiedad por ${monto}";
                break;
            case "Pago de alquiler":
                mensajeDescripcion = $"{origen} ha pagado ${monto} a {destino} por alquiler";
                break;
            case "Pago al banco":
                mensajeDescripcion = $"{origen} ha pagado ${monto} al banco";
                break;
            case "Pago entre jugadores":
                mensajeDescripcion = $"{origen} ha pagado ${monto} a {destino}";
                break;
            case "Ganancia por evento":
                mensajeDescripcion = $"{origen} ha ganado ${monto} por evento";
                break;
            case "Perdida por evento":
                mensajeDescripcion = $"{origen} ha perdido ${monto} por evento";
                break;
            case "Premio por pasar por inicio":
                mensajeDescripcion = $"{origen} ha recibido ${monto} por pasar por inicio";
                break;
            default:
                mensajeDescripcion = $"{origen} realizo {tipo} por ${monto}";
                break;
        }

        return mensajeDescripcion;
    }

    /**
     * @brief Escribe una linea con formato delimitado por puntos en 'Almacenamiento.txt'.
     */
    public void AlmacenarTransaccion()
    {
        string nomOrigen = this.jugadorOrigen?.GetNombre() ?? "Banco";
        string nomDestino = this.jugadorDestino?.GetNombre() ?? "Banco";
        string contenido = $"{transaccionID}.{fechaYHora}.{turno}.{tipo}.{monto}.{nomOrigen}.{nomDestino}.{descripcion}";

        try
        {
            File.AppendAllText(rutaAlmacenamiento, contenido + "\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al almacenar la transaccion: {ex.Message}");
        }
    }

    /**
     * @brief Imprime los campos de una transaccion en la terminal con formato ordenado.
     * @param datos Arreglo de strings con los campos desglosados.
     */
    private static void ImprimirTransaccionEnTerminal(string[] datos)
    {
        Console.WriteLine("\n--------------------------\n");
        Console.WriteLine("ID: " + datos[0]);
        Console.WriteLine("Fecha: " + datos[1]);
        Console.WriteLine("Turno: " + datos[2]);
        Console.WriteLine("Tipo: " + datos[3]);
        Console.WriteLine("Monto: " + datos[4]);
        Console.WriteLine("Origen: " + datos[5]);
        Console.WriteLine("Destino: " + datos[6]);
        Console.WriteLine("Descripcion: " + datos[7]);
        Console.WriteLine("\n--------------------------\n");
    }

    /**
     * @brief Busca y filtra transacciones en el archivo persistente segun un atributo y valor.
     * @param atributo Campo por el cual filtrar ("jugadorOrigen", "jugadorDestino", "tipo" o null para todos).
     * @param valor Valor esperado en dicho atributo.
     * @param ordenar Criterio de ordenacion ("AntiguoAReciente" o "RecienteAAntiguo").
     * @param imprimirYEsperar Si es true, pausa tras cada registro solicitando confirmacion al usuario.
     */
    public static void BuscarTransaccion(string? atributo, string valor, string ordenar = "AntiguoAReciente", bool imprimirYEsperar = false)
    {
        try
        {
            string[] lineas = File.ReadAllLines(rutaAlmacenamiento);

            if (ordenar == "RecienteAAntiguo")
            {
                Array.Reverse(lineas);
            }

            foreach (var linea in lineas)
            {
                string[] datos = linea.Split('.');

                switch (atributo)
                {
                    case "jugadorOrigen":
                        if (datos[5] == valor)
                        {
                            ImprimirTransaccionEnTerminal(datos);
                        } 
                        break;
                    case "jugadorDestino":
                        if (datos[6] == valor)
                        {
                            ImprimirTransaccionEnTerminal(datos);
                        }
                        break;
                    case "tipo":
                        if (datos[3] == valor)
                        {
                            ImprimirTransaccionEnTerminal(datos);
                        }
                        break;
                    default:
                        if (atributo == null)
                        {
                            ImprimirTransaccionEnTerminal(datos);
                        }
                        break;
                }
                if (imprimirYEsperar)
                {   
                    Console.WriteLine("Presiona Enter para continuar...");
                    Console.WriteLine("Digita '-' para cancelar.");
                    string? opt = Console.ReadLine();

                    if (opt == "-")
                    {   
                        Console.WriteLine("Se ha cancelado la impresion en terminal...");
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar transacciones: {ex.Message}");
        }
    }

    /**
     * @brief Vuelca todas las transacciones almacenadas a un archivo de reporte formateado ('Reporte.txt').
     */
    public static void ImprimirTransacciones()
    {
        try
        {
            string[] lineas = File.ReadAllLines(rutaAlmacenamiento);

            foreach (var linea in lineas)
            {
                string[] datos = linea.Split(".");
                
                File.AppendAllText(rutaReporte, "\n--------------------------\n");
                File.AppendAllText(rutaReporte, "ID: " + datos[0] + '\n');
                File.AppendAllText(rutaReporte, "Fecha: " + datos[1] + '\n');
                File.AppendAllText(rutaReporte, "Turno: " + datos[2] + '\n');
                File.AppendAllText(rutaReporte, "Tipo: " + datos[3] + '\n');
                File.AppendAllText(rutaReporte, "Monto: " + datos[4] + '\n');
                File.AppendAllText(rutaReporte, "Origen: " + datos[5] + '\n');
                File.AppendAllText(rutaReporte, "Destino: " + datos[6] + '\n');
                File.AppendAllText(rutaReporte, "Descripcion: " + datos[7] + '\n');
                File.AppendAllText(rutaReporte, "\n--------------------------\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer las transacciones: {ex.Message}");
        }
    }
}