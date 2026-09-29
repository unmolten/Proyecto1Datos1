extends Control
class_name TurnActionsHud

## Panel de acciones del turno interactivo en la interfaz grafica de Godot.
## Permite al usuario en su turno lanzar los dados fisicos o virtuales, comprar
## la propiedad donde se encuentra, construir mejoras, pagar fianza de carcel,
## consultar su estado financiero o concluir su turno.

## Ruta hacia el nodo del cliente de red en la escena
@export var network_client_path: NodePath = NodePath("../../Game")

var network_client: NetworkClient = null

## Elementos de la interfaz para control de acciones
@onready var turn_label: Label = $Background/Margin/VBox/Header/TurnLabel
@onready var status_label: Label = $Background/Margin/VBox/Header/StatusLabel
@onready var roll_btn: Button = $Background/Margin/VBox/ButtonRow1/RollBtn
@onready var buy_btn: Button = $Background/Margin/VBox/ButtonRow1/BuyBtn
@onready var build_btn: Button = $Background/Margin/VBox/ButtonRow2/BuildBtn
@onready var jail_btn: Button = $Background/Margin/VBox/ButtonRow2/JailBtn
@onready var end_turn_btn: Button = $Background/Margin/VBox/ButtonRow2/EndTurnBtn
@onready var status_btn: Button = $Background/Margin/VBox/ButtonRow3/StatusBtn
@onready var manage_btn: Button = $Background/Margin/VBox/ButtonRow3/ManageBtn

var _current_can_roll: bool = false
var _current_can_buy: bool = false
var _current_can_end: bool = false
var _current_in_jail: bool = false
var _current_can_build: bool = false

func _ready() -> void:
	if network_client_path != NodePath() and has_node(network_client_path):
		network_client = get_node(network_client_path)
	elif has_node("/root/Board/Game"):
		network_client = get_node("/root/Board/Game")

	if network_client:
		network_client.turn_actions_updated.connect(_on_turn_actions_updated)
		network_client.message_received.connect(_on_raw_message)
		network_client.dice_rolled.connect(_on_dice_rolled)

	roll_btn.pressed.connect(_on_roll_pressed)
	buy_btn.pressed.connect(_on_buy_pressed)
	build_btn.pressed.connect(_on_build_pressed)
	jail_btn.pressed.connect(_on_jail_pressed)
	end_turn_btn.pressed.connect(_on_end_turn_pressed)
	status_btn.pressed.connect(_on_status_pressed)
	manage_btn.pressed.connect(_on_manage_pressed)

	_reset_state()

## Restablece los botones a su estado deshabilitado por defecto
func _reset_state() -> void:
	roll_btn.disabled = true
	roll_btn.text = "Tirar Dados"
	buy_btn.disabled = true
	buy_btn.text = "Comprar Propiedad"
	build_btn.disabled = true
	build_btn.visible = false
	jail_btn.disabled = true
	jail_btn.visible = false
	end_turn_btn.disabled = true
	status_label.text = "Esperando turno..."

## Refleja visualmente los valores de dados obtenidos por un jugador
func _on_dice_rolled(player_id: int, d1: int, d2: int, total: int) -> void:
	status_label.text = "Jugador %d tiro: [%d] + [%d] = %d" % [player_id, d1, d2, total]
	roll_btn.text = "%d + %d = %d" % [d1, d2, total]

## Actualiza la disponibilidad de cada boton segun la fase del turno actual
func _on_turn_actions_updated(can_roll: bool, can_buy: bool, buy_price: int, buy_name: String, can_end: bool, in_jail: bool, can_build: bool = false, build_cost: int = 0, build_name: String = "") -> void:
	_current_can_roll = can_roll
	_current_can_buy = can_buy
	_current_can_end = can_end
	_current_in_jail = in_jail
	_current_can_build = can_build

	var player_id := 1
	if network_client:
		player_id = network_client.current_turn_player_id
	turn_label.text = "Turno: Jugador %d" % player_id

	roll_btn.disabled = not can_roll
	if can_roll:
		roll_btn.text = "Tirar Dados"

	buy_btn.disabled = not can_buy
	if can_buy:
		buy_btn.text = "Comprar %s ($%d)" % [buy_name, buy_price]
	else:
		buy_btn.text = "Comprar Propiedad"

	build_btn.visible = can_build
	build_btn.disabled = not can_build
	if can_build:
		build_btn.text = "Construir en %s ($%d)" % [build_name, build_cost]

	jail_btn.visible = in_jail
	jail_btn.disabled = not in_jail

	end_turn_btn.disabled = not can_end

	# Mensajes descriptivos del estado del turno
	if in_jail and can_roll:
		status_label.text = "En carcel: Paga fianza ($50) o tira dados."
	elif can_roll:
		status_label.text = "Tira los dados para moverte."
	elif can_buy:
		status_label.text = "Puedes comprar o terminar turno."
	elif can_build:
		status_label.text = "Puedes construir casas en tu propiedad."
	elif can_end:
		status_label.text = "Listo para terminar turno."
	else:
		status_label.text = "Esperando accion..."

## Interpreta mensajes directos del servidor relativos al cambio de turno
func _on_raw_message(raw: String) -> void:
	var parts := raw.split("/")
	if parts.size() >= 3 and parts[0] == "jugador" and parts[2] == "turno":
		var pid := int(parts[1])
		turn_label.text = "Turno: Jugador %d" % pid

## Envia la peticion de tirada de dados al servidor
func _on_roll_pressed() -> void:
	if network_client:
		status_label.text = "Esperando tirada de dados..."
		roll_btn.disabled = true
		network_client.send_action("tirar")

## Envia la solicitud de adquisicion de la propiedad actual
func _on_buy_pressed() -> void:
	if network_client:
		status_label.text = "Esperando tarjeta RFID para autorizar..."
		buy_btn.disabled = true
		network_client.send_action("comprar")

## Envia la solicitud de construccion de una casa
func _on_build_pressed() -> void:
	if network_client:
		status_label.text = "Solicitando construccion..."
		build_btn.disabled = true
		network_client.send_action("comprarcasa")

## Envia la solicitud de pago de fianza para liberacion de la carcel
func _on_jail_pressed() -> void:
	if network_client:
		status_label.text = "Pagando fianza..."
		jail_btn.disabled = true
		network_client.send_action("salircarcel")

## Concluye el turno del jugador activo
func _on_end_turn_pressed() -> void:
	if network_client:
		status_label.text = "Pasando turno..."
		end_turn_btn.disabled = true
		network_client.send_action("terminar")

## Solicita al servidor un resumen del estado del jugador
func _on_status_pressed() -> void:
	if network_client:
		status_label.text = "Solicitando estado..."
		network_client.send_action("5")

## Solicita al servidor el menu de gestion de propiedades
func _on_manage_pressed() -> void:
	if network_client:
		status_label.text = "Gestion de propiedades..."
		network_client.send_action("8")
