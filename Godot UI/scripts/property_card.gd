extends Control
class_name PropertyCard

## Representa la tarjeta individual de una propiedad en la lista lateral derecha.
## Posee una cara frontal (nombre, precio de compra, renta vigente segun mejoras,
## cantidad de casas/hotel y botones de construir o vender mejoras) y una cara posterior
## (valor de hipoteca y costo para deshipotecar).
## Si la propiedad es hipotecada, se aplica un sombreado en escala de grises.

## Datos identificadores y economicos de la casilla
var casilla_index: int = -1
var precio_compra: int = 0
var alquiler_base: int = 0
var color_grupo: String = ""
var owner_id: int = 0

## Estado actual de construcciones e hipoteca
var casas: int = 0
var is_mortgaged: bool = false
var flipped: bool = false

var network_client: NetworkClient = null

## Material de escala de grises compartido para representar hipoteca
const GRAYSCALE_MATERIAL: ShaderMaterial = preload("res://scenes/grayscale_material.tres")

## Nodos hijos de la interfaz
@onready var front_panel: Control = $Front
@onready var back_panel: Control = $Back
@onready var name_label: Label = $Front/NameLabel
@onready var price_label: Label = $Front/PriceLabel
@onready var rent_label: Label = $Front/RentLabel
@onready var houses_label: Label = $Front/HousesLabel
@onready var buy_house_btn: Button = $Front/BuyHouseBtn
@onready var sell_house_btn: Button = $Front/SellHouseBtn
@onready var mortgage_label: Label = $Back/MortgageLabel
@onready var mortgage_btn: Button = $Back/MortgageBtn

func _ready() -> void:
	if buy_house_btn:
		buy_house_btn.pressed.connect(_on_buy_house_pressed)
	if sell_house_btn:
		sell_house_btn.pressed.connect(_on_sell_house_pressed)
	if mortgage_btn:
		mortgage_btn.pressed.connect(_on_mortgage_pressed)

## Obtiene la referencia activa al cliente de comunicacion de red
func _get_network_client() -> NetworkClient:
	if network_client != null:
		return network_client
	if has_node("/root/Board/Game"):
		network_client = get_node("/root/Board/Game")
	return network_client

## Inicializa la tarjeta con los atributos inmutables de la propiedad
func setup(pos: int, nombre: String, precio: int, alquiler: int, grupo: String) -> void:
	casilla_index = pos
	precio_compra = precio
	alquiler_base = alquiler
	color_grupo = grupo

	if name_label:
		name_label.text = nombre
	if price_label:
		price_label.text = "Precio: $%d" % precio

	_refresh()

## Asigna el identificador del jugador propietario
func set_property_owner_id(id: int) -> void:
	owner_id = id
	_refresh()

## Actualiza el conteo de mejoras (0 a 4 = casas, 5 = hotel)
func set_casas(nivel: int) -> void:
	casas = nivel
	_refresh()

## Cambia el estado visual entre activo y en escala de grises segun la hipoteca
func set_mortgaged(mortgaged: bool) -> void:
	is_mortgaged = mortgaged
	material = GRAYSCALE_MATERIAL if mortgaged else null
	_refresh()

## Alterna entre la vista frontal y la vista trasera de la tarjeta
func _on_flip_pressed() -> void:
	flipped = not flipped
	front_panel.visible = not flipped
	back_panel.visible = flipped

## Envia la solicitud de adquisicion de una casa al servidor
func _on_buy_house_pressed() -> void:
	var client := _get_network_client()
	if client:
		client.send_action("comprarcasa", casilla_index)

## Envia la solicitud de venta de una mejora al servidor
func _on_sell_house_pressed() -> void:
	var client := _get_network_client()
	if client:
		client.send_action("vendercasa", casilla_index)

## Alterna el estado de hipoteca comunicandose con el servidor
func _on_mortgage_pressed() -> void:
	var client := _get_network_client()
	if client:
		if is_mortgaged:
			client.send_action("deshipotecar", casilla_index)
		else:
			client.send_action("hipotecar", casilla_index)

## Calcula el valor actual de renta segun las reglas del juego
func _calcular_renta() -> int:
	if is_mortgaged:
		return 0
	if casas <= 0:
		return alquiler_base
	if casas >= 5:
		return alquiler_base * 8
	return alquiler_base * (1 + casas * 2)

## Refresca las etiquetas de texto de la tarjeta con los datos vigentes
func _refresh() -> void:
	if not is_inside_tree() or rent_label == null:
		return

	rent_label.text = "Renta: $%d" % _calcular_renta()

	if casas >= 5:
		houses_label.text = "Hotel"
	elif casas > 0:
		houses_label.text = "%d casa(s)" % casas
	else:
		houses_label.text = "Sin casas"

	var valor_hipoteca := precio_compra / 2
	var costo_deshipotecar := int(valor_hipoteca * 1.1)
	mortgage_label.text = "Hipoteca: $%d\nDeshipotecar: $%d" % [valor_hipoteca, costo_deshipotecar]

	if is_mortgaged:
		mortgage_btn.text = "Deshipotecar"
	else:
		mortgage_btn.text = "Hipotecar"
