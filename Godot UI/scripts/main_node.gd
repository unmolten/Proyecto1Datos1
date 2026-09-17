extends Node2D
class_name GameBoard

## Este script controla visualmente el tablero,
## se coloca en el nodo raíz del tablero, que es el padre de "Visual",
## las casillas se encuentran dentro del contenedor indicado por
## casillas_container_path y deben llamarse "Casilla_0", "Casilla_1", etc.
## Cada casilla es un Marker2D y sirve como punto de referencia para colocar
## visualmente las fichas de los jugadores

# Se puede modificar desde el Inspector todos los exports
## Cantidad total de casillas que tiene el tablero
@export var casilla_count: int = 40

## Escena que se utilizará como plantilla para crear las fichas de los jugadores
@export var player_scene: PackedScene

## Ruta donde se encuentra el contenedor que tiene todas las casillas
@export var casillas_container_path: NodePath = NodePath("Visual/Casillas")

## Ruta opcional del contenedor donde se agregarán las fichas.
## Si está vacío, las fichas se agregan directamente como hijas de este nodo
@export var players_container_path: NodePath

## Array de texturas que se pueden asignar a las fichas.
## El índice 0 corresponde al jugador 1, el índice 1 al jugador 2, etc.
## (MODIFICABLE)
@export var player_sprites: Array[Texture2D] = []

## Escena que se utiliza para mostrar las casas/hotel de una propiedad
## (la escena HouseMarker.tscn)
@export var house_scene: PackedScene


## Colores que se utilizarán para las fichas si no se les asignó una textura.
## El índice 0 corresponde al jugador 1, el 1 al jugador 2, etc
const PLAYER_COLORS: Array[Color] = [Color.RED, Color.BLUE, Color.GREEN, Color.YELLOW]


# -------------- OFFSETS DE LAS FICHAS --------------


# Estos son desplazamientos respecto al centro de una casilla y se
# utilizan para que cuando varios jugadores estén en la misma casilla
# no aparezcan todos exactamente en la misma posición

# Si hay un solo jugador, se coloca en el centro
const OFFSET_1: Array[Vector2] = [Vector2.ZERO]

# Si hay dos jugadores, se colocan uno a cada lado
const OFFSET_2: Array[Vector2] = [
	Vector2(-14, 0),
	Vector2(14, 0)
]

# Si hay tres jugadores, se distribuyen en formación de triángulo
const OFFSET_3: Array[Vector2] = [
	Vector2(0, -14),
	Vector2(-14, 10),
	Vector2(14, 10)
]

# Si hay cuatro jugadores, se distribuyen en las cuatro esquinas como cuadrado
const OFFSET_4: Array[Vector2] = [
	Vector2(-14, -14),
	Vector2(14, -14),
	Vector2(-14, 14),
	Vector2(14, 14)
]


# -------------- Variables generales --------------


## Lista que contiene todas las casillas encontradas en el tablero
var casillas: Array[Node2D] = []

## Diccionario que relaciona el ID de un jugador con su objeto Player.
## Ejemplo: players[1] = objeto del jugador 1 | players[2] = objeto del jugador 2
var players: Dictionary = {}

## Diccionario que guarda en qué casilla está actualmente cada jugador.
## Ejemplo: player_casilla[1] = 15
## significa que el jugador 1 está en la casilla 15
var player_casilla: Dictionary = {}

## Diccionario que guarda qué jugadores están actualmente en cada casilla.
## Ejemplo: casilla_occupants[15] = [1, 3]
## significa que los jugadores 1 y 3 están en la casilla 15.
var casilla_occupants: Dictionary = {}

## Diccionario que guarda el HouseMarker (casas/hotel) que ya se creó
## para cada casilla. Ejemplo: house_markers[5] = nodo HouseMarker de la casilla 5
var house_markers: Dictionary = {}


# ============================================================
# INICIO DEL TABLERO
# ============================================================

# Cuando el nodo está listo dentro de la escena, se ejecuta esto.
func _ready() -> void:

	# Primero busca y guarda todas las casillas del tablero.
	_collect_casillas()

	# Después crea las fichas de los cuatro jugadores.
	_spawn_players()


# -------------- BUSCAR LAS CASILLAS --------------


## Esta función busca todas las casillas del tablero y las guarda
## dentro del arreglo "casillas"
func _collect_casillas() -> void:

	## Primero vaciamos la lista por si ya tenía información
	casillas.clear()

	# Si no existe el path nodo contenedor de las casillas:
	if not has_node(casillas_container_path):

		# Mostramos un error y terminamos la funcións
		push_error("No se encontró el contenedor de casillas en %s" % casillas_container_path)
		return

	## Obtiene el nodo que contiene las casillas
	var container: Node = get_node(casillas_container_path)
	
	# Recorre desde 0 hasta la cantidad de casillas indicada
	for i in range(casilla_count):

		## Construimos el nombre que debería tener la casilla.
		## Por ejemplo: i = 0 es "Casilla_0" | i = 1 es "Casilla_1"
		var node_name := "Casilla_%d" % i

		# Si ya existe un nodo con esa casilla:
		if container.has_node(node_name):

			## Obtiene el nodo y lo agrega al arreglo
			casillas.append(container.get_node(node_name))

		# Si no existe la casilla esperada, muestra warning:
		else:
			push_warning("No se encontró el nodo %s dentro de %s" % [node_name, casillas_container_path])


# -------------- CREA LOS JUGADORES --------------


## Esta función spawnea las cuatro fichas de jugadores
func _spawn_players() -> void:

	## Por defecto, las fichas se agregarán como hijas de este nodo
	var container: Node = self

	# Si se especificó otro contenedor y existe:
	if players_container_path != NodePath() and has_node(players_container_path):

		# Utilizamos ese nodo como contenedor de las fichas
		container = get_node(players_container_path)

	# Recorremos los cuatro jugadores (AQUI SE LES SETTEA EL ID, HAY QUE MODIFICAR):
	for i in range(4):

		## El índice empieza en 0, pero los IDs de los jugadores empiezan en 1.
		## Por eso suma 1
		var id := i + 1

		## Creamos una nueva ficha utilizando la escena Player
		var p: Player = player_scene.instantiate()

		## Le asignamos su ID
		p.player_id = id

		## Al principio todos los jugadores están desactivados
		p.active = false

		# Si existe una textura para este jugador:
		if i < player_sprites.size() and player_sprites[i] != null:

			## Se le pone esa textura a su sprite
			p.set_sprite(player_sprites[i])

		# Si no hay textura:
		else:

			## Se usa uno de los colores placeholder
			p.set_color(PLAYER_COLORS[i])

		## Agregamos la ficha al contenedor correspondiente
		container.add_child(p)

		## Guardamos el jugador en el diccionario usando su ID
		players[id] = p

		## Inicialmente todos los jugadores empiezan en la casilla 0
		player_casilla[id] = 0

	## Inicializamos la lista de jugadores que están en la casilla 0
	casilla_occupants[0] = []


# -------------- funciones a llamar por el network client --------------


## Activa visualmente a un jugador.
## Esta función es llamada cuando NetworkClient recibe:
## "jugador/<id>/activar" (POR AHORA) (MODIFICABLE)
func activate_player(id: int) -> void:

	# Si no existe un jugador con ese ID, no hacemos nada
	if not players.has(id):
		return

	## Obtenemos la casilla donde se encuentra el jugador, si no existe información
	## usa la casilla 0
	var casilla_idx: int = player_casilla.get(id, 0)

	## Agregamos al jugador a la lista de jugadores de esa casilla
	_add_occupant(casilla_idx, id)

	## Marcamos al jugador como activo
	players[id].active = true

	## Actualiza la posición visual de los jugadores de esa casilla
	_update_casilla_arrangement(casilla_idx)


## Desactiva visualmente a un jugador.
## Esta función es llamada cuando NetworkClient recibe:
## "jugador/<id>/desactivar" (POR AHORA) (MODIFICABLE)
func deactivate_player(id: int) -> void:

	# Si el jugador no existe, no hace nada
	if not players.has(id):
		return

	## Obtiene la casilla actual del jugador
	var casilla_idx: int = player_casilla.get(id, 0)

	## Quita al jugador de la lista de ocupantes de esa casilla
	_remove_occupant(casilla_idx, id)

	## Marca al jugador como inactivo
	players[id].active = false

	## Actualizamos el acomodo de los jugadores que quedaron en la casilla
	_update_casilla_arrangement(casilla_idx)


## Mueve visualmente a un jugador hacia una casilla determinada.
## Esta función es llamada cuando NetworkClient recibe:
## "jugador/<id>/mover/casilla/<n>" (POR AHORA) (MODIFICABLE)
func move_player_to_casilla(id: int, casilla_index: int) -> void:

	# Comprobar que exista el jugador:
	if not players.has(id):

		# Si no existe, tira warning
		push_warning("Jugador %d no existe" % id)
		return

	# Comprobar que el índice de la casilla esté dentro del tablero:
	if casilla_index < 0 or casilla_index >= casillas.size():

		# Si está fuera del rango, tira warning
		push_warning("Casilla %d fuera de rango" % casilla_index)
		return

	## Guarda el índice de la casilla donde estaba anteriormente
	var old_idx: int = player_casilla.get(id, 0)

	## Quita al jugador de la lista de ocupantes de esa casilla
	_remove_occupant(old_idx, id)

	## Actualiza la casilla actual del jugador
	player_casilla[id] = casilla_index

	## Agrega al jugador a la lista de ocupantes de la nueva casilla
	_add_occupant(casilla_index, id)

	## Actualiza la distribución visual de la antigua casilla
	_update_casilla_arrangement(old_idx)

	## Actualiza la distribución visual de la nueva casilla
	_update_casilla_arrangement(casilla_index)


# -------------- funciones internas --------------


## Esta funcion agrega un jugador a la lista de jugadores que ocupan una casilla
func _add_occupant(casilla_index: int, id: int) -> void:

	## Obtenemos la lista actual de jugadores de esa casilla.
	## Si todavía no existe, usa una lista vacía
	var list: Array = casilla_occupants.get(casilla_index, [])

	# El jugador todavia no está en la lista?:
	if not list.has(id):

		## Si no está, lo agrega
		list.append(id)

	## Guarda nuevamente la lista en el diccionario
	casilla_occupants[casilla_index] = list


## Esta función quita un jugador de la lista de jugadores de una casilla
func _remove_occupant(casilla_index: int, id: int) -> void:

	## Obtiene la lista actual de jugadores de esa casilla
	var list: Array = casilla_occupants.get(casilla_index, [])

	## Elimina el ID del jugador de la lista
	list.erase(id)

	## Guarda nuevamente la lista actualizada
	casilla_occupants[casilla_index] = list


## Esta funcion actualiza la posición visual de todos los jugadores de una casilla
func _update_casilla_arrangement(casilla_index: int) -> void:

	# Comprueba que la casilla exista:
	if casilla_index < 0 or casilla_index >= casillas.size():
		return

	## Obtiene el nodo de la casilla correspondiente
	var casilla: Node2D = casillas[casilla_index]

	## Obtiene los jugadores que actualmente están en esa casilla
	var occupants: Array = casilla_occupants.get(casilla_index, [])

	## Elimina de la lista cualquier jugador que no exista,
	## o que esté desactivado
	occupants = occupants.filter(func(id): return players.has(id) and players[id].active)

	# Si no hay jugadores activos en la casilla, no hay nada que acomodar:
	if occupants.is_empty():
		return

	## Variable donde guarda los offsets que utilizará
	var offsets: Array[Vector2]

	## Dependiendo de cuántos jugadores haya, elije una distribución diferente
	match occupants.size():

		# Uno:
		1:
			offsets = OFFSET_1

		# Dos:
		2:
			offsets = OFFSET_2

		# Tres:
		3:
			offsets = OFFSET_3

		# Cuatro:
		4:
			offsets = OFFSET_4

	# Recorre todos los jugadores que están en la casilla
	for i in range(occupants.size()):

		## Obtiene el ID del jugador actual
		var id: int = occupants[i]

		## Calcula la posición global de la ficha.
		## El offset se calcula tomando como referencia la casilla
		var target: Vector2 = casilla.to_global(offsets[i])

		## Mueve visualemente al jugador a la posicion
		players[id].move_to(target)


# -------------- CASAS / HOTELES --------------


## Esta función calcula, para una casilla dada, dónde debe ir su grupo de
## casas/hotel y si deben acomodarse en horizontal o en vertical.
## Devuelve un diccionario vacío si la casilla no pertenece a ninguna
## de las cuatro filas de propiedades.
##
## Offsets (siempre hacia el centro del tablero) y orientación:
##   Casillas 1 a 7   -> +90 en X   -> se acomodan en VERTICAL
##   Casillas 9 a 15  -> +95.93 en Y -> se acomodan en HORIZONTAL
##   Casillas 17 a 23 -> -90 en X   -> se acomodan en VERTICAL
##   Casillas 25 a 31 -> -84 en Y   -> se acomodan en HORIZONTAL
func _get_house_placement(casilla_index: int) -> Dictionary:

	# Verifica que la casilla exista dentro del arreglo
	if casilla_index < 0 or casilla_index >= casillas.size():
		return {}

	## Posición local de la casilla (dentro del contenedor "Casillas")
	var base_pos: Vector2 = casillas[casilla_index].position

	var anchor: Vector2
	var horizontal: bool

	if casilla_index >= 1 and casilla_index <= 7:
		anchor = base_pos + Vector2(90, 0)
		horizontal = false

	elif casilla_index >= 9 and casilla_index <= 15:
		anchor = base_pos + Vector2(0, 95.93)
		horizontal = true

	elif casilla_index >= 17 and casilla_index <= 23:
		anchor = base_pos + Vector2(-90, 0)
		horizontal = false

	elif casilla_index >= 25 and casilla_index <= 31:
		anchor = base_pos + Vector2(0, -84)
		horizontal = true

	# Si la casilla no cae en ninguna de las filas de propiedades
	# (por ejemplo, una esquina, un ferrocarril, arca o fortuna)
	else:
		return {}

	return {"position": anchor, "horizontal": horizontal}


## Esta función es llamada cuando NetworkClient recibe:
## "jugador/<id>/comprarcasa/<1 a 5>/casilla/<n>" (POR AHORA, SIN VALIDAR NADA,
## simplemente muestra la cantidad de casas u hotel indicado en esa casilla)
##
## level: 1, 2, 3 o 4 = esa cantidad de casas | 5 = hotel (reemplaza las casas)
func set_house_level(casilla_index: int, level: int) -> void:

	## Calcula dónde y cómo debe ir el marcador de casas para esta casilla
	var placement: Dictionary = _get_house_placement(casilla_index)

	# Si la casilla no admite casas (esquina, ferrocarril, arca, fortuna, etc.)
	if placement.is_empty():
		push_warning("La casilla %d no admite casas/hotel" % casilla_index)
		return

	# Si no se asignó la escena de casas en el Inspector, no se puede continuar
	if house_scene == null:
		push_error("No se asignó house_scene en el Inspector de Board")
		return

	## Contenedor donde viven las casillas (ahí mismo se agregan las casas)
	var container: Node = get_node(casillas_container_path)

	## Nombre único que tendrá el nodo de casas de esta casilla
	var node_name := "Casas_%d" % casilla_index

	var marker: HouseMarker

	# Si ya existe un marcador de casas para esta casilla, lo reutiliza
	if house_markers.has(casilla_index):
		marker = house_markers[casilla_index]

	# Si no existe todavía, lo crea e instancia por primera vez
	else:
		marker = house_scene.instantiate()
		marker.name = node_name
		container.add_child(marker)
		house_markers[casilla_index] = marker

	## Ubica el marcador en la posición calculada
	marker.position = placement["position"]

	## Le dice cuántas casas/hotel mostrar y en qué dirección acomodarse
	marker.set_level(level, placement["horizontal"])
