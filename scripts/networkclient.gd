extends Node

class_name NetworkClient

## Protocolo esperado (cada mensaje termina en "\n") [MODIFICABLE]:
##   jugador/<id>/activar
##   jugador/<id>/desactivar
##   jugador/<id>/mover/casilla/<n>
## Se usa StreamPeerTCP para establecer una conexión TCP con el servidor

## Señal de tipo string con el mensaje recibido.
## Una señal sirve para avisar a otras partes del código que algo ocurrió
signal message_received(raw: String)

# Variables exportadas que permiten modificarlas desde el editor 2D en vez de aquí
# en el código, para poner la ip del host, el puerto, el nodo del tablero
# el intervalo de reconexión
## IP del servidor
@export var host: String = "127.0.0.1"
## Puerto del servidor
@export var port: int = 9999
## Ruta al nodo del tablero
@export var game_board_path: NodePath
## Intervalo de reconexion
@export var reconnect_interval: float = 2.0

# Variables generales
## Variable para guardar el tablero (el objeto al que se llega con la ruta)
var game_board: GameBoard
## Se crea la variable que guarda una instancia de un objeto tipo StreamPeerTCP
var _socket: StreamPeerTCP = StreamPeerTCP.new()
## Variable para saber si estamos conectados al server
var _connected: bool = false
## Buffer para guardar temporalmente el mensaje recibido
var _buffer: String = ""
## cuánto tiempo ha pasado desde el último intento de conexión para volver a intentar la
## reconexion
var _reconnect_timer: float = 0.0

# Cuando el objeto está listo y cargado dentro de la escena, se ejecuta lo siguiente:
func _ready() -> void:
	
	## Si hay una ruta ya definida
	if game_board_path != NodePath():
		
		## Tomamos el nodo al que lleva la ruta, ese es el tablero
		game_board = get_node(game_board_path)
		
	## Si no habia una ruta
	else:
		
		## Se muestra un error
		push_error("NetworkClient: no se asignó game_board_path")
		
	## Cuando terminó esa parte de arriba, independientemente de si se pudo
	## conseguir un tablero o no, se intenta la conexion.
	_try_connect()

## Función para intentar conectarse al servidor mediante conexion TCP
func _try_connect() -> void:
	
	## Se intenta la conexion con el ip del host y el puerto
	var err := _socket.connect_to_host(host, port)
	
	## Si err no es igual a OK (o sea, no se inicio la conexion), se muestra el warning
	if err != OK:
		push_warning("No se pudo iniciar conexión (%s), reintentando..." % err)

# Esta función se pasa ejecutando constantemente mientras corre el juego
func _process(delta: float) -> void:
	
	## Actualiza el socket para ver que ha pasado con la conexion
	_socket.poll()
	
	## Estado actual del socket tras el .poll()
	var status := _socket.get_status()
	
	## Los diferentes casos dependiendo lo que diga status
	match status:
		
		# Si sale como conectado:
		StreamPeerTCP.STATUS_CONNECTED:
			
			# Si la variable booleana nos indica que no estaba conectado, lo cambiamos a que sí
			# se considere como conectado y hacemos un print
			if not _connected:
				_connected = true
				print("Conectado al servidor %s:%d" % [host, port])
				
			## Revisa si alguna información se envió
			_read_available()
			
		# Si sale como error o nulo:
		StreamPeerTCP.STATUS_ERROR, StreamPeerTCP.STATUS_NONE:
			
			# Si la variable booleana nos indica que está conectado, lo cambiamos a que no
			# se considere como conectado y hacemos un print
			if _connected:
				print("Desconectado del servidor")
			_connected = false
			
			## Comienza el proceso de reconexion con el timer, se le va sumando delta,
			## que representa el tiempo desde el frame anterior, o sea, vamos contando
			_reconnect_timer += delta
			
			# Si el tiempo de reconexion llega a ser mayor o igual al intervalo, se settea de vuelta
			# a 0 y se intenta la conexion
			if _reconnect_timer >= reconnect_interval:
				_reconnect_timer = 0.0
				_try_connect()
				
		# Si sale como que se está conectando, lo dejamos pasar:
		StreamPeerTCP.STATUS_CONNECTING:
			pass

## Esta función se encarga de leer los datos que el servidor ya envió
func _read_available() -> void:
	## La variable available pregunta de manera simplificada la cantidad de bytes
	## de información por leer
	var available := _socket.get_available_bytes()
	
	# Si esa cantidad es mayor a 0, hay un mensaje, se envia al buffer
	if available > 0:
		
		# Convierte los bytes a texto
		_buffer += _socket.get_utf8_string(available)
		
		# Ahora revisa si el buffer contiene mensajes completos
		_process_buffer()

## Esta función realiza la separacion de mensajes, analiza que no vengan vacios y donde terminan
func _process_buffer() -> void:
	
	# Hasta que no encuentre el \n, va a estar considerando como que el mensaje sigue
	while _buffer.find("\n") != -1:
		
		## Posición donde está el primer salto de línea.
		var idx := _buffer.find("\n")
		
		## Tomamos todo lo que está antes del \n
		## (.strip_edges elimina espacios innecesarios al principio o al final)
		var line := _buffer.substr(0, idx).strip_edges()
		
		## Quitamos del buffer el mensaje que acabamos de procesar, incluyendo el \n
		# Permite que, si había otro mensaje detrás, podamos procesarlo también
		_buffer = _buffer.substr(idx + 1)
		
		## Si el mensaje no esta vacio, lo enviamos a procesar para ver que quiere decirnos
		if line != "":
			_handle_message(line)

## Esta funcion es la que decide que es lo que significa el mensaje
func _handle_message(raw: String) -> void:
	
	## Mediante la señal, avisa que recibió un mensaje, el mensaje es esa linea que mandamos
	## desde el process_buffer, aqui se toma como un raw, sin procesar
	message_received.emit(raw)
	
	## Se separa el mensaje por partes en un array, usando "/" como separador (MODIFICABLE)
	var parts := raw.split("/")
	
	## Si la cantidad de partes del mensaje es menor a 2, ya que no hay mensajes de ese tamaño
	## o la primera de todas las partes no incluye la palabra "jugador",  se toma como un mensaje
	## no reconocido de una.
	if parts.size() < 2 or parts[0] != "jugador":
		push_warning("Mensaje no reconocido: %s" % raw)
		return
	
	## Toma el ID del jugador en la segunda parte (MODIFICABLE, POSIBLEMENTE TENGA QUE
	## CAMBIARLO PORQUE EL ID SE TOMA DEL RFID, NO DE QUE LOS JUGADORES SE LLAMEN
	## 1, 2, 3, 4)
	var player_id := int(parts[1])
	
	## Si el tamaño de las partes es mayor o igual a 3 e incluye "activar" en la tercera parte:
	if parts.size() >= 3 and parts[2] == "activar":
		
		## Se activa al jugador con el id
		game_board.activate_player(player_id)
		
	## Si el tamaño de las partes es mayor o igual a 3 e incluye "desactivar" en la tercera parte:
	elif parts.size() >= 3 and parts[2] == "desactivar":
		
		## Se desactiva al jugador con el id
		game_board.deactivate_player(player_id)
		
	## Si el tamaño de las partes es mayor o igual a 5 e incluye "mover" en la tercera parte
	## y casilla en la cuarta parte:
	elif parts.size() >= 5 and parts[2] == "mover" and parts[3] == "casilla":
		
		## Se toma el indice de la casilla en la parte 5 y se mueve al jugador al indice
		## de la casilla deseada
		var casilla_index := int(parts[4])
		game_board.move_player_to_casilla(player_id, casilla_index)
		
	## Si no se reconoció el mensaje aquí arriba, tira un warning
	else:
		push_warning("Acción no reconocida: %s" % raw)
