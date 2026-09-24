extends Control
class_name TurnHud

## Esta escena es el indicador fijo de la esquina superior derecha:
## el avatar del jugador que tiene el turno actual, junto con cuantas
## esmeraldas tiene. Vive en un CanvasLayer aparte, por eso no se mueve
## ni se hace zoom junto con la Camera2D del tablero


## Cambia la imagen del avatar por la del jugador que tiene el turno
func set_turn_player(texture: Texture2D) -> void:
	if has_node("Avatar") and texture != null:
		$Avatar.texture = texture


## Actualiza el numero de esmeraldas que se muestra
func set_money(monto: int) -> void:
	if has_node("MoneyLabel"):
		$MoneyLabel.text = str(monto)
