using System;

// Clase Metodos:
// Encargada del enrutamiento y procesamiento de comandos que llegan desde la red.
// Actúa como el intérprete de protocolo entre los mensajes crudos recibidos por el socket
// y las operaciones del juego en 'JuegoMonopoly'.
// 
// Protocolo soportado:
// 1. Formato con delimitador de barra: "COMANDO/DATOS" (ej. "NOMBRE/Gabriel", "CHAT/Hola a todos").
// 2. Formato simple de una sola palabra o espacio: "TIRAR", "COMPRAR", "ESTADO", "TABLERO".
public class Metodos
{
    // Muestra en la consola del servidor los detalles de una instrucción recibida.
    // Separa el comando de los parámetros de forma segura sin causar excepciones de índice.
    public static void mensajeshow(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje)) return;

        // División por el carácter delimitador '/'
        string[] full = mensaje.Split('/');
        string instruccion = full[0];
        string datos = full.Length > 1 ? full[1] : "";

        Console.WriteLine($"[Comando recibido]: {instruccion}");
        if (!string.IsNullOrEmpty(datos))
        {
            Console.WriteLine($"[Datos]: {datos}");
        }
    }

    // Interpreta el comando recibido por un cliente específico y ejecuta la acción
    // correspondiente sobre el estado del juego y las estructuras de datos.
    public static void ProcesarComando(string mensaje, Jugador jugador)
    {
        if (string.IsNullOrWhiteSpace(mensaje)) return;

        // Registrar en consola del servidor para propósitos de depuración
        mensajeshow(mensaje);

        string comando;
        string datos = "";

        // Verificamos si el cliente utilizó la sintaxis "COMANDO/DATOS" o espacio simple
        if (mensaje.Contains('/'))
        {
            string[] partes = mensaje.Split(new[] { '/' }, 2);
            comando = partes[0].Trim().ToUpper();
            datos = partes.Length > 1 ? partes[1].Trim() : "";
        }
        else
        {
            string[] partes = mensaje.Trim().Split(new[] { ' ' }, 2);
            comando = partes[0].ToUpper();
            datos = partes.Length > 1 ? partes[1].Trim() : "";
        }

        var juego = JuegoMonopoly.Instancia;

        // Enrutamiento de comandos hacia la lógica de Monopoly
        switch (comando)
        {
            // Tirar los dados y avanzar en la lista circular
            case "TIRAR":
            case "ROLL":
            case "DADOS":
                juego.TirarDados(jugador);
                break;

            // Comprar la propiedad de la casilla actual e insertarla en la lista enlazada del jugador
            case "COMPRAR":
            case "BUY":
                juego.ComprarPropiedad(jugador);
                break;

            // Consultar balance, casilla actual y recorrer lista enlazada de propiedades
            case "ESTADO":
            case "STATUS":
            case "INFO":
                juego.VerEstado(jugador);
                break;

            // Recorrer la lista circular del tablero e imprimir la ubicación de todos los jugadores
            case "TABLERO":
            case "BOARD":
                juego.VerTablero(jugador);
                break;

            // Pagar fianza para liberarse de la cárcel
            case "SALIRCARCEL":
            case "FIANZA":
                juego.SalirDeCarcelConPago(jugador);
                break;

            // Cambiar el nombre del jugador
            case "NOMBRE":
            case "NAME":
                if (!string.IsNullOrWhiteSpace(datos))
                {
                    string nombreViejo = jugador.Nombre;
                    jugador.Nombre = datos;
                    jugador.EnviarMensaje($"✅ Tu nombre ahora es: {jugador.Nombre}");
                    juego.Broadcast($"[MONOPOLY] {nombreViejo} ahora se llama '{jugador.Nombre}'.", jugador);
                }
                else
                {
                    jugador.EnviarMensaje("❌ Uso: NOMBRE/<nuevo_nombre> o NOMBRE <nuevo_nombre>");
                }
                break;

            // Enviar un mensaje de chat a todos los demás jugadores conectados
            case "CHAT":
            case "MSG":
                if (!string.IsNullOrWhiteSpace(datos))
                {
                    juego.Broadcast($"💬 [{jugador.Nombre}]: {datos}");
                }
                break;

            // Mostrar la guía de comandos disponibles
            case "AYUDA":
            case "HELP":
            case "?":
                MostrarAyuda(jugador);
                break;

            default:
                jugador.EnviarMensaje($"❓ Comando no reconocido: '{comando}'. Escribe 'AYUDA' para ver los comandos disponibles.");
                break;
        }
    }

    // Envía al cliente un menú visual con todos los comandos y su descripción.
    public static void MostrarAyuda(Jugador jugador)
    {
        string ayuda =
@"╔══════════════════════════════════════════════════════════════════════════╗
║                          COMANDOS DE MONOPOLY                            ║
╠══════════════════╦═══════════════════════════════════════════════════════╣
║ TIRAR            ║ Lanza los 2 dados y avanza en la lista enlazada       ║
║ COMPRAR          ║ Compra la propiedad de la casilla actual              ║
║ ESTADO           ║ Muestra tu dinero, casilla y propiedades adquiridas   ║
║ TABLERO          ║ Muestra el estado del tablero y posiciones de todos   ║
║ SALIRCARCEL      ║ Paga fianza de $50 para salir de la cárcel            ║
║ NOMBRE/<texto>   ║ Cambia tu nombre en el juego                          ║
║ CHAT/<mensaje>   ║ Envía un mensaje a todos los jugadores conectados     ║
║ AYUDA            ║ Muestra este menú                                     ║
║ exit             ║ Sale de la partida                                    ║
╚══════════════════╩═══════════════════════════════════════════════════════╝";

        jugador.EnviarMensaje(ayuda);
    }
}