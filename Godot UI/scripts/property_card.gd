extends Control
class_name PropertyCard

## Representa UNA tarjeta de la lista de propiedades de la derecha.
## Tiene una cara de "frente" (nombre, precio, renta actual, casas) y una
## de "atrás" (el precio para deshipotecar), se voltea con el botón 🔄.
## Cuando la propiedad está hipotecada, TODA la tarjeta se pinta en blanco
## y negro con el mismo shader que usa el avatar sobre el tablero.

var casilla_index: int = -1
var precio_compra: int = 0
var alquiler_base: int = 0
var color_grupo: String = ""

var casas: int = 0
var is_mortgaged: bool = false
var flipped: bool = false

## Mismo material de blanco y negro que usa el avatar sobre el tablero
const GRAYSCALE_MATERIAL: ShaderMaterial = preload("res://scenes/grayscale_material.tres")


## Llena la tarjeta con los datos fijos de la propiedad (esto no cambia en toda la partida)
func setup(pos: int, nombre: String, precio: int, alquiler: int, grupo: String) -> void:
	casilla_index = pos
	precio_compra = precio
	alquiler_base = alquiler
	color_grupo = grupo

	$Front/NameLabel.text = nombre
	$Front/PriceLabel.text = "Precio: $%d" % precio

	_refresh()


## Actualiza cuántas casas/hotel tiene (0 a 4 = casas, 5 = hotel)
func set_casas(nivel: int) -> void:
	casas = nivel
	_refresh()


## Pinta o despinta la tarjeta completa de blanco y negro
func set_mortgaged(mortgaged: bool) -> void:
	is_mortgaged = mortgaged
	material = GRAYSCALE_MATERIAL if mortgaged else null
	_refresh()


## Voltea la tarjeta al presionar el botón
func _on_flip_pressed() -> void:
	flipped = not flipped
	$Front.visible = not flipped
	$Back.visible = flipped


## Misma fórmula de renta que usa JuegoMonopoly.cs (CalcularRenta), solo
## para MOSTRAR el número, la que de verdad manda es siempre la del servidor
func _calcular_renta() -> int:
	if is_mortgaged:
		return 0
	if casas <= 0:
		return alquiler_base
	if casas >= 5:
		return alquiler_base * 8
	return alquiler_base * (1 + casas * 2)


func _refresh() -> void:
	$Front/RentLabel.text = "Renta: $%d" % _calcular_renta()

	if casas >= 5:
		$Front/HousesLabel.text = "🏨 Hotel"
	elif casas > 0:
		$Front/HousesLabel.text = "🏠 %d casa(s)" % casas
	else:
		$Front/HousesLabel.text = "Sin casas"

	var valor_hipoteca := precio_compra / 2
	var costo_deshipotecar := int(valor_hipoteca * 1.1)
	$Back/MortgageLabel.text = "Te dieron: $%d\nPara deshipotecar: $%d" % [valor_hipoteca, costo_deshipotecar]
