extends Control
class_name ConnectionHud

## Permite a cualquier persona (en la misma PC o en otra por red local / VPN)
## ver el estado de la conexión y cambiar la IP del servidor fácilmente como espectador.

@export var network_client_path: NodePath = NodePath("../../Game")

var network_client: NetworkClient = null

@onready var status_label: Label = $Background/Margin/VBox/StatusRow/StatusLabel
@onready var toggle_btn: Button = $Background/Margin/VBox/StatusRow/ToggleBtn
@onready var input_row: HBoxContainer = $Background/Margin/VBox/InputRow
@onready var ip_edit: LineEdit = $Background/Margin/VBox/InputRow/IpEdit
@onready var port_edit: LineEdit = $Background/Margin/VBox/InputRow/PortEdit
@onready var connect_btn: Button = $Background/Margin/VBox/InputRow/ConnectBtn

func _ready() -> void:
	if network_client_path != NodePath() and has_node(network_client_path):
		network_client = get_node(network_client_path)
	elif has_node("/root/Board/Game"):
		network_client = get_node("/root/Board/Game")

	if network_client:
		network_client.connection_status_changed.connect(_on_connection_status_changed)
		ip_edit.text = network_client.host
		port_edit.text = str(network_client.port)

		if network_client.is_connected_to_server():
			_set_connected_state(true, "Conectado a %s:%d" % [network_client.host, network_client.port])
		else:
			_set_connected_state(false, "Conectando a %s:%d..." % [network_client.host, network_client.port])

	toggle_btn.pressed.connect(_on_toggle_pressed)
	connect_btn.pressed.connect(_on_connect_pressed)
	ip_edit.text_submitted.connect(func(_t): _on_connect_pressed())
	port_edit.text_submitted.connect(func(_t): _on_connect_pressed())

func _on_connection_status_changed(connected: bool, message: String) -> void:
	_set_connected_state(connected, message)

func _set_connected_state(connected: bool, message: String) -> void:
	if connected:
		status_label.text = "🟢 " + message
		input_row.visible = false
		toggle_btn.text = "⚙️"
		custom_minimum_size.y = 38
		size.y = 38
	else:
		if "Conectando" in message:
			status_label.text = "🟡 " + message
		else:
			status_label.text = "🔴 " + message
			input_row.visible = true
			toggle_btn.text = "▲"
			custom_minimum_size.y = 74
			size.y = 74

func _on_toggle_pressed() -> void:
	input_row.visible = not input_row.visible
	toggle_btn.text = "▲" if input_row.visible else "⚙️"
	var target_h := 74 if input_row.visible else 38
	custom_minimum_size.y = target_h
	size.y = target_h

func _on_connect_pressed() -> void:
	if network_client == null:
		return
	var new_ip := ip_edit.text.strip_edges()
	var new_port := int(port_edit.text.strip_edges())
	if new_port <= 0:
		new_port = 6767
	if new_ip == "":
		new_ip = "127.0.0.1"

	status_label.text = "🟡 Conectando a %s:%d..." % [new_ip, new_port]
	network_client.connect_to(new_ip, new_port)
