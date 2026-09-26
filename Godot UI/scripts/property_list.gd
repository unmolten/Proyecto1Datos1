extends Control
class_name PropertyList

## Lista de todas las propiedades del tablero, a la derecha de la pantalla.
##
## IMPORTANTE: Godot no sabe nada por su cuenta, ni siquiera qué propiedades
## existen. Cada tarjeta se crea cuando llega el mensaje de sincronización
## del servidor ("propiedad/<pos>/<nombre>/<precio>/<alquiler>/<grupo>"),
## esto pasa una vez por propiedad, apenas la instancia de Godot se conecta.
## Después, GameBoard le va avisando los cambios (dueño, casas, hipoteca)
## según van llegando los demás mensajes.

## Escena de una tarjeta individual (PropertyCard.tscn)
@export var card_scene: PackedScene

## Diccionario que relaciona el índice de la casilla con su tarjeta ya creada
var cards: Dictionary = {}


## Crea (o actualiza, si ya existía) la tarjeta de una propiedad con sus
## datos fijos. Se llama una vez por propiedad, apenas el servidor manda
## el mensaje de sincronización inicial.
func register_property(casilla_index: int, nombre: String, precio: int, alquiler: int, grupo: String) -> void:

	if card_scene == null:
		push_error("PropertyList: no se asignó card_scene")
		return

	var card: PropertyCard

	# Si por alguna razón ya existía (por ejemplo, una reconexión), se reutiliza
	if cards.has(casilla_index):
		card = cards[casilla_index]
	else:
		card = card_scene.instantiate()
		$Scroll/Cards.add_child(card)
		cards[casilla_index] = card

	card.setup(casilla_index, nombre, precio, alquiler, grupo)


## Reservado por si más adelante se quiere mostrar el nombre del dueño en la
## tarjeta (main_node.gd lo llama junto con set_casas cuando hay dueño)
func hacer_owner(casilla_index: int, owner_id: int) -> void:
	pass


## Actualiza cuántas casas/hotel tiene una propiedad
func set_casas(casilla_index: int, nivel: int) -> void:
	if cards.has(casilla_index):
		cards[casilla_index].set_casas(nivel)


## Pinta (o despinta) de blanco y negro la tarjeta de una propiedad hipotecada
func set_mortgaged(casilla_index: int, is_mortgaged: bool) -> void:
	if cards.has(casilla_index):
		cards[casilla_index].set_mortgaged(is_mortgaged)
