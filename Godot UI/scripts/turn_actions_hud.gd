extends Control
class_name TurnActionsHud

## Controla los botones de acción del turno para jugar directamente desde Godot.
## Permite tirar dados, comprar la propiedad actual, pagar fianza o terminar turno.

@export var network_client_path: NodePath = NodePath("../../Game")

var network_client: NetworkClient = null

@onready var turn_label: Label = $Background/Margin/VBox/Header/TurnLabel
@onready var status_label: Label = $Background/Margin/VBox/Header/StatusLabel
@onready var roll_btn: Button = $Background/Margin/VBox/ButtonRow1/RollBtn
@onready var buy_btn: Button = $Background/Margin/VBox/ButtonRow1/BuyBtn
@onready var jail_btn: Button = $Background/Margin/VBox/ButtonRow2/JailBtn
@onready var end_turn_btn: Button = $Background/Margin/VBox/ButtonRow2/EndTurnBtn

var _current_can_roll: bool = false
var _current_can_buy: bool = false
var _current_can_end: bool = false
var _current_in_jail: bool = false

func _ready() -> void:
	if network_client_path != NodePath() and has_node(network_client_path):
		network_client = get_node(network_client_path)
	elif has_node("/root/Board/Game"):
		network_client = get_node("/root/Board/Game")

	if network_client:
		network_client.turn_actions_updated.connect(_on_turn_actions_updated)
		network_client.message_received.connect(_on_raw_message)

	roll_btn.pressed.connect(_on_roll_pressed)
	buy_btn.pressed.connect(_on_buy_pressed)
	jail_btn.pressed.connect(_on_jail_pressed)
	end_turn_btn.pressed.connect(_on_end_turn_pressed)

	_reset_state()

func _reset_state() -> void:
	roll_btn.disabled = true
	buy_btn.disabled = true
	buy_btn.text = "🏠 Comprar Propiedad"
	jail_btn.disabled = true
	jail_btn.visible = false
	end_turn_btn.disabled = true
	status_label.text = "Esperando turno..."

func _on_turn_actions_updated(can_roll: bool, can_buy: bool, buy_price: int, buy_name: String, can_end: bool, in_jail: bool) -> void:
	_current_can_roll = can_roll
	_current_can_buy = can_buy
	_current_can_end = can_end
	_current_in_jail = in_jail

	var player_id := 1
	if network_client:
		player_id = network_client.current_turn_player_id
	turn_label.text = "🎮 Turno: Jugador %d" % player_id

	roll_btn.disabled = not can_roll

	buy_btn.disabled = not can_buy
	if can_buy:
		buy_btn.text = "🏠 Comprar %s ($%d)" % [buy_name, buy_price]
	else:
		buy_btn.text = "🏠 Comprar Propiedad"

	jail_btn.visible = in_jail
	jail_btn.disabled = not in_jail

	end_turn_btn.disabled = not can_end

	# Textos informativos
	if in_jail and can_roll:
		status_label.text = "¡En cárcel! Paga fianza ($50) o tira dados."
	elif can_roll:
		status_label.text = "Tira los dados para moverte."
	elif can_buy:
		status_label.text = "Puedes comprar o terminar turno."
	elif can_end:
		status_label.text = "Listo para terminar turno."
	else:
		status_label.text = "Esperando acción..."

func _on_raw_message(raw: String) -> void:
	var parts := raw.split("/")
	if parts.size() >= 3 and parts[0] == "jugador" and parts[2] == "turno":
		var pid := int(parts[1])
		turn_label.text = "🎮 Turno: Jugador %d" % pid

func _on_roll_pressed() -> void:
	if network_client:
		status_label.text = "🎲 Lanzando dados..."
		roll_btn.disabled = true
		network_client.send_action("tirar")

func _on_buy_pressed() -> void:
	if network_client:
		status_label.text = "💳 Solicitando compra..."
		buy_btn.disabled = true
		network_client.send_action("comprar")

func _on_jail_pressed() -> void:
	if network_client:
		status_label.text = "💳 Pagando fianza..."
		jail_btn.disabled = true
		network_client.send_action("salircarcel")

func _on_end_turn_pressed() -> void:
	if network_client:
		status_label.text = "⏭️ Pasando turno..."
		end_turn_btn.disabled = true
		network_client.send_action("terminar")
