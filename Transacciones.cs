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
    private static int refID = 0; //ID de referencia para cada instancia
    private int transaccionID; //ID propio de cada instancia
    private int monto; //Monto transferido de jugadorOrigen a jugadorDestino
    private int turno; //Turno en el que el jugador genero la transaccion
    private string fechaYHora; //Fecha con formato "dd/MM/yyyy//hh:mm:ss"
    private string tipo; //Tipo de transaccion (predefinida)
    private string jugadorOrigen; // NOMBRE del jugador de origen. 
    private string jugadorDestino; // NOMBRE del jugador destinado
    private string descripcion; //Descripcion autogenerada segun el tipo de transaccion

    public Transaccion(int monto, int turno, string tipo, string jugadorOrigen, string jugadorDestino)
    {
        this.transaccionID = Interlocked.Increment(ref refID);
        this.monto = monto;
        this.turno = turno;
        this.fechaYHora = DateAndTime.Now.ToString("dd/MM/yyyy-hh:mm:ss");
        this.tipo = tipo;
        this.jugadorOrigen = jugadorOrigen;
        this.jugadorDestino = jugadorDestino;
        this.descripcion = this.GenerarDescripcion();

        AlmacenarTransaccion();
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
        string rutaArchivo = "../../../Almacenamiento.txt"; //Se suben 3 directorios para crearlo en la ubicacion de este archivo. Cambiar ruta de ser requerido
        string contenido = $"{transaccionID}.{fechaYHora}.{turno}.{tipo}.{monto}.{jugadorOrigen}.{jugadorDestino}.{descripcion}"; //La informacion se almacena con separador de punto. EVITAR NOMBRES Y DATOS QUE PUEDAN CONTENER UN PUNTO

        try
        {
            File.AppendAllText(rutaArchivo, contenido + "\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al almacenar la transacción: {ex.Message}");
        }
    }

    //Metodo para imprimir la informacion en un archivo de texto con formato para usuario final
    public static void ImprimirTransacciones()
    {
        string rutaAlmacenamiento = "../../../Almacenamiento.txt";
        string rutaReporte = "../../../Reporte.txt";
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