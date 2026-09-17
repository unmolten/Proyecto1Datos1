extends Node2D
class_name Player

## Para la escena del jugador se requiere un nodo 2D, un nodo sprite y un nodo
## label (MODIFICABLE)

# Variables exportables
## ID del jugador
@export var player_id: int = 0
## Duracion del movimiento (visual)
@export var move_duration: float = 0.4

## Variable que determina si el jugador está activo.
## En este proyecto se utiliza únicamente para controlar su representación
## visual, es decir, si la ficha debe ser visible o no
var active: bool = false:
	
	# Cada vez que cambia el valor de active:
	set(value):
		
		## Guarda el nuevo valor
		active = value
		
		## Hace que la ficha sea visible si active es true
		## y que deje de ser visible si active es false
		visible = value

## Cuando el nodo está listo dentro de la escena, se ejecuta esto
func _ready() -> void:
	
	# Establece la visibilidad inicial según el valor de active
	visible = active
	
	# Actualiza el texto del Label para mostrar el ID del jugador (MODIFICABLE)
	_update_label()

## Esta función actualiza el texto que aparece sobre la ficha
func _update_label() -> void:
	
	# Comprueba si existe un nodo llamado "Label"
	if has_node("Label"):
		
		## Convierte el ID del jugador a texto y lo colocamos en el Label
		$Label.text = str(player_id)

## Tiñe el Sprite del jugador con el color indicado.
## Es útil cuando todavia no hay una textura definida
## o cuando se requiere utilizar un color como identificación visual
func set_color(color: Color) -> void:
	
	# Comprueba que exista el nodo Sprite
	if has_node("Sprite"):
		
		# Cambia el color/modulación del Sprite
		$Sprite.modulate = color

## Asigna una textura diferente al Sprite del jugador.
## Esto permite utilizar un dibujo diferente para cada jugador
func set_sprite(texture: Texture2D) -> void:
	
	# Comprueba que exista el Sprite y que la textura no sea nula
	if has_node("Sprite") and texture != null:
		
		# Asigna la textura recibida al Sprite
		$Sprite.texture = texture

## Mueve suavemente la ficha hasta una posición global.
## La posición ya fue calculada anteriormente por GameBoard
func move_to(target_position: Vector2) -> void:
	
	## Crea un Tween para realizar una transición suave
	var tween := create_tween()
	
	## Configurar el tipo de transición y la forma en que termina.
	## | - TRANS_SINE hace que el movimiento sea más suave |
	## | - EASE_OUT hace que el movimiento vaya desacelerando al llegar al destino |
	tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	
	## Le indica al Tween que cambie la posición global de la ficha
	## hasta la posición indicada durante el tiempo definido en move_duration
	tween.tween_property(self, "global_position", target_position, move_duration)
