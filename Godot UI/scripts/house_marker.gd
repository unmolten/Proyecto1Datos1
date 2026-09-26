extends Node2D
class_name HouseMarker

## Esta escena representa las casas (o el hotel) de UNA casilla de propiedad.
## No dibuja nada por sí sola: espera a que GameBoard le diga qué nivel
## mostrar (1 a 4 casas, o 5 = hotel) y en qué dirección acomodarse
## (horizontal o vertical), y ella se dibuja sola con draw_rect,
## sin necesidad de ninguna imagen/sprite.

## Nivel actual: 0 = nada, 1 a 4 = esa cantidad de casas, 5 = hotel
var level: int = 0

## true = las casas/hotel se acomodan en fila horizontal (eje X)
## false = se acomodan en columna vertical (eje Y)
var horizontal: bool = true

# -------------- Tamaños y separaciones (ajustables a ojo) --------------

## Tamaño (ancho x alto) de cada casa individual
const HOUSE_SIZE: Vector2 = Vector2(14, 14)

## Separación entre el centro de una casa y la siguiente
const HOUSE_GAP: float = 15.0

## Color de las casas
const HOUSE_COLOR: Color = Color(0.15, 0.55, 0.2)

## Tamaño del hotel: lado largo (en la dirección del acomodo)
const HOTEL_LONG: float = 40.0
## Tamaño del hotel: lado corto (en la dirección del acomodo)
const HOTEL_SHORT: float = 22.0

## Color del hotel
const HOTEL_COLOR: Color = Color(0.90, 0.1, 0.1)


## Llamar a esta función para actualizar lo que se muestra.
## level: 0 = quita todo | 1-4 = esa cantidad de casas | 5 = hotel
## is_horizontal: true = fila horizontal | false = columna vertical
func set_level(new_level: int, is_horizontal: bool) -> void:
	level = clampi(new_level, 0, 5)
	horizontal = is_horizontal

	## Pide que se vuelva a dibujar (dispara _draw())
	queue_redraw()

	## NOTA: el icono del dueño YA NO se esconde aquí. El dueño se ve desde
	## que se compra la propiedad, tenga o no casas todavía (eso lo maneja
	## set_owner_texture por separado, GameBoard lo llama apenas se compra)


## Pone (o quita) el sprite pequeño del dueño de la propiedad, un poco
## arriba de donde van las casas. Se llama por separado de set_level porque
## GameBoard ya sabe qué textura le toca a cada jugador. Se usa tanto al
## comprar la propiedad como al construir casas en ella
func set_owner_texture(texture: Texture2D) -> void:
	if not has_node("OwnerIcon"):
		return

	if texture == null:
		$OwnerIcon.visible = false
		return

	$OwnerIcon.texture = texture
	$OwnerIcon.visible = true


## Pinta (o despinta) de blanco y negro el avatar del dueño, segun si la
## propiedad esta hipotecada o no. GameBoard llama esto cuando llega
## "jugador/<id>/hipotecar/casilla/<n>" o "deshipotecar"
func set_mortgaged(is_mortgaged: bool) -> void:
	if not has_node("OwnerIcon"):
		return

	if is_mortgaged:
		$OwnerIcon.material = GRAYSCALE_MATERIAL
	else:
		$OwnerIcon.material = null


## Material de blanco y negro, se carga una sola vez y se reutiliza
const GRAYSCALE_MATERIAL: ShaderMaterial = preload("res://scenes/grayscale_material.tres")


func _draw() -> void:

	# Si el nivel es 0, no hay nada que dibujar
	if level <= 0:
		return

	# Nivel 5 = hotel (reemplaza a las 4 casas)
	if level >= 5:
		var size: Vector2
		if horizontal:
			size = Vector2(HOTEL_LONG, HOTEL_SHORT)
		else:
			size = Vector2(HOTEL_SHORT, HOTEL_LONG)

		# Se dibuja centrado en el origen del nodo (0, 0)
		draw_rect(Rect2(-size / 2.0, size), HOTEL_COLOR)
		return

	# Niveles 1 a 4 = esa cantidad de casas, repartidas y centradas
	var count := level
	var total_span := HOUSE_GAP * (count - 1)

	for i in range(count):

		## Desplazamiento de esta casa respecto al centro
		var offset: float = -total_span / 2.0 + HOUSE_GAP * i

		var pos: Vector2
		if horizontal:
			pos = Vector2(offset, 0)
		else:
			pos = Vector2(0, offset)

		draw_rect(Rect2(pos - HOUSE_SIZE / 2.0, HOUSE_SIZE), HOUSE_COLOR)
