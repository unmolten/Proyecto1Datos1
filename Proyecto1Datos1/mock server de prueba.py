"""
Mock server para probar el cliente de Godot mientras se desarrolla
el servidor real en C#

Mayormente está hecha con Claude y comentada con ChatGPT porque no es
una parte importante del proyecto como tal, meramente de testing temporal

Con el proyecto de Godot corriendo (NetworkClient conectado),
escribir comandos en esta terminal, por ejemplo:

    jugador/1/activar
    jugador/1/mover/casilla/3
    jugador/2/activar
    jugador/2/mover/casilla/3
    jugador/1/desactivar
"""

# Usa socket para la conexion TCP
import socket

# Para manejar mas de un cliente sin bloqueo usa threading
import threading

# Informacion del IP y puerto
HOST = "127.0.0.1"
PORT = 9999

# Lista donde se guardan las conexiones de los clientes conectados
clients = []

# Lock utilizado para evitar que varios hilos modifiquen la lista de clientes al mismo tiempo y provoquen problemas
clients_lock = threading.Lock()


# Esta función se encarga de manejar a cada cliente que se conecta
def handle_client(conn: socket.socket, addr) -> None:

    # Muestra en la terminal la dirección del cliente que se conectó
    print(f"Cliente conectado: {addr}")

    # Bloquea la lista mientras la modifica
    with clients_lock:

        # Agrega la conexión del nuevo cliente a la lista
        clients.append(conn)

    try:

        # Mientras el cliente siga conectado, se mantiene este hilo funcionando
        while True:

            # Esperamos a que el cliente envíe algún dato
            data = conn.recv(1024)

            # Si no recibimos datos, significa que el cliente se desconectó
            if not data:
                break

    # Si el cliente cierra la conexión abruptamente
    except ConnectionResetError:
        pass

    finally:

        # Bloqueamos la lista antes de modificarla
        with clients_lock:

            # Si la conexión todavía está en la lista
            if conn in clients:

                # La eliminamos porque el cliente se desconectó
                clients.remove(conn)

        # Avisamos en la terminal que el cliente se desconectó
        print(f"Cliente desconectado: {addr}")


# Esta función envía un mensaje a todos los clientes conectados
def broadcast(message: str) -> None:

    # Se agrega un salto de línea al final del mensaje
    # porque NetworkClient utiliza "\n" para saber dónde termina cada mensaje
    line = message.strip() + "\n"

    # Bloqueamos la lista mientras recorremos las conexiones
    with clients_lock:

        # Se hace una copia de la lista para poder recorrerla
        # sin problemas si alguna conexión falla
        for c in list(clients):

            try:

                # Convertimos el mensaje de texto a bytes usando UTF-8
                # y lo enviamos por la conexión TCP
                c.sendall(line.encode("utf-8"))

            # Si ocurre un error con la conexión
            except OSError:

                # Se elimina ese cliente de la lista
                clients.remove(c)


# Esta función espera constantemente a que se conecten nuevos clientes
def accept_loop(server: socket.socket) -> None:

    # El servidor sigue aceptando conexiones mientras esté funcionando
    while True:

        # Espera hasta que algún cliente intente conectarse
        conn, addr = server.accept()

        # Se crea un hilo independiente para manejar ese cliente
        # de esta forma el servidor puede seguir aceptando otros clientes
        threading.Thread(
            target=handle_client,
            args=(conn, addr),
            daemon=True
        ).start()


# Función principal del Mock Server
def main() -> None:

    # Se crea un socket TCP
    # AF_INET = IPv4
    # SOCK_STREAM = TCP
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # Permite reutilizar el puerto inmediatamente después de cerrar
    # el servidor, evitando algunos errores de "puerto ocupado"
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    # Se asigna la IP y el puerto al servidor
    server.bind((HOST, PORT))

    # Se pone el socket en modo servidor para comenzar a aceptar conexiones
    server.listen()

    # Avisamos en la terminal que el servidor ya está esperando conexiones
    print(f"Mock server escuchando en {HOST}:{PORT}")

    # Mostramos los comandos que se pueden escribir para probar Godot
    print("Comandos: jugador/<id>/activar | jugador/<id>/desactivar | jugador/<id>/mover/casilla/<n>")

    # Mostramos cómo cerrar el servidor
    print("Escribe 'salir' para terminar.\n")


    # Se crea un hilo separado para aceptar conexiones
    # mientras el hilo principal queda libre para recibir comandos escritos
    threading.Thread(
        target=accept_loop,
        args=(server,),
        daemon=True
    ).start()


    # Bucle principal que espera comandos escritos en la terminal
    while True:

        try:

            # Espera a que escribamos un comando
            cmd = input("> ").strip()

        # Si se presiona Ctrl+C o se termina la entrada
        except (EOFError, KeyboardInterrupt):

            # Salimos del bucle
            break

        # Si no se escribió nada, volvemos a pedir un comando
        if not cmd:
            continue

        # Si se escribe alguno de estos comandos, terminamos el servidor
        if cmd in ("salir", "exit", "quit"):
            break

        # Envía el comando escrito a todos los clientes conectados
        broadcast(cmd)


# Esta condición hace que main() solamente se ejecute
# cuando este archivo se ejecuta directamente
if __name__ == "__main__":
    main()