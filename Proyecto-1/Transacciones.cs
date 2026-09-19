/*
En este archivo se manejan las transacciones de la partida
incluyendo instanciacion de las mismas, Almacenamiento e
impresion

Esta pensado para manejar los datos que tambien se mostraran
al usuario final. O sea, ninguna clase deberia aceptar datos
como el ID del jugador sino el nombre como tal de este mismo,
o no un ID de propiedad sino su nombre real.
*/

using Microsoft.VisualBasic;
using System.IO;



class Transaccion
{
    private const string rutaAlmacenamiento = "../../../Almacenamiento.txt"; // Se suben 3 directorios para crearlo en Proyecto1Datos1/Proyecto-1. Cambiar ruta de ser requerido
    private const string rutaReporte = "../../../Reporte.txt"; // Cambiar ruta de ser requerido;
    private static int refID = 0; //ID de referencia para cada instancia
    private int transaccionID; //ID propio de cada instancia
    private int monto; //Monto transferido de jugadorOrigen a jugadorDestino
    private int turno; //Turno en el que el jugador genero la transaccion
    private string fechaYHora; //Fecha con formato "dd/MM/yyyy//hh:mm:ss"
    private string tipo; //Tipo de transaccion (predefinida)
    private Jugador? jugadorOrigen; // NOMBRE del jugador de origen. USAR NULL SI PROVIENE DEL BANCO
    private Jugador? jugadorDestino; // NOMBRE del jugador destinado. USAR NULL SI VA AL BANCO
    private string descripcion; //Descripcion autogenerada segun el tipo de transaccion

    public Transaccion(int monto, int turno, string tipo, Jugador jugadorOrigen, Jugador jugadorDestino)
    {   
        //Datos iniciales
        this.transaccionID = Interlocked.Increment(ref refID);
        this.monto = monto;
        this.turno = turno;
        this.fechaYHora = DateAndTime.Now.ToString("dd/MM/yyyy-hh:mm:ss");
        this.tipo = tipo;
        this.jugadorOrigen = jugadorOrigen;
        this.jugadorDestino = jugadorDestino;
        this.descripcion = this.GenerarDescripcion();

        // Procesar los datos
        this.ProcesarTransaccion();

        // Almacenar la transaccion tras procesarla
        this.AlmacenarTransaccion();
    }

    // Metodo para procesar la transaccion dentro del juego

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

    // Metodo para autogenerar descripciones segun el tipo
    private string GenerarDescripcion()
    {
        
        string mensajeDescripcion;
        

        //Mensajes en mayuscula deben ser reemplazados por el dato correspondiente
        switch (this.tipo)
        {
            case "Compra de propiedad":
                mensajeDescripcion = $"{jugadorOrigen} ha comprado la propiedad PROPIEDAD por {monto}";
                break;
            case "Pago de alquiler":
                mensajeDescripcion = $"{jugadorOrigen} ha pagado {monto} a {jugadorDestino} por el alquiler de la propiedad PROPIEDAD";
                break;
            case "Pago al banco":
                mensajeDescripcion = $"{jugadorOrigen} ha pagado {monto} al banco";
                break;
            case "Pago entre jugadores":
                mensajeDescripcion = $"{jugadorOrigen} ha pagado {monto} a {jugadorDestino}";
                break;
            case "Ganancia por evento":
                mensajeDescripcion = $"{jugadorOrigen} ha ganado {monto} por el evento EVENTO";
                break;
            case "Perdida por evento":
                mensajeDescripcion = $"{jugadorOrigen} ha perdido {monto} por el evento EVENTO";
                break;
            case "Premio por pasar por inicio":
                mensajeDescripcion = $"{jugadorOrigen} ha recibido {monto} por pasar por inicio";
                break;
            default:
                mensajeDescripcion = "Error";
                break;
        }

        return mensajeDescripcion;
    }

    //Método para almacenar la transacción en el archivo Almacenamiento.txt
    public void AlmacenarTransaccion()
    {
        string contenido = $"{transaccionID}.{fechaYHora}.{turno}.{tipo}.{monto}.{jugadorOrigen}.{jugadorDestino}.{descripcion}"; //La informacion se almacena con separador de punto. EVITAR NOMBRES Y DATOS QUE PUEDAN CONTENER UN PUNTO

        try
        {
            File.AppendAllText(rutaAlmacenamiento, contenido + "\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al almacenar la transacción: {ex.Message}");
        }
    }

    // -- Metodos estaticos para manejar el historial de transacciones --


    // Metodo estatico para buscar transacciones en el archivo historial.txt:
    //      atributo: Es el atributo al cual se va a realizar la busqueda (solo jugadorOrigen, jugadorDestino y tipo), si es null, imprime todas las transacciones
    //      valor: Valor el cual sera buscado en el atributo. Se ignora si atributo es null
    //      ordenar: Orden en el que se mostraran las transacciones. Puede ser:
    //          "AntiguoAReciente": Ordena de la transaccion mas antigua a la mas reciente.
    //          "RecienteAAntiguo": Ordena de la transaccion mas reciente a la mas antigua.
    //          
    //      imprimirYEsperar: Si es true, imprime la informacion en la terminal y espera a que el usuario presione enter para continuar. De lo contrario imprime toda la informacion en terminal sin esperar.
    public static void BuscarTransaccion(string atributo, string valor, string ordenar = "AntiguoAReciente", bool imprimirYEsperar = false)
    {

    }

    //Metodo para imprimir la informacion en un archivo de texto con formato para usuario final
    public static void ImprimirTransacciones()
    {
        //string encabezado = "En este archivo se encuentra la informacion";

        try
        {
            string[] lineas = File.ReadAllLines(rutaAlmacenamiento);

            foreach (var linea in lineas)
            {
                string[] datos = linea.Split("."); //Separa los datos de las lineas por un punto

                
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