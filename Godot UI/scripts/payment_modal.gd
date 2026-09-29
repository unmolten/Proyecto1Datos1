extends Control
class_name PaymentModal

## Modal visual para autorizacion de pagos mediante tarjeta RFID fisica.
## Se despliega automaticamente cuando el servidor central de Monopoly requiere
## validar una transaccion monetaria (adquisicion de propiedades, compra de casas,
## pago de alquiler a otro jugador, impuestos o fianza de carcel).
## El jugador debe aproximar su tarjeta RFID al lector conectado a la Raspberry Pi Pico.

## Ruta hacia el nodo del cliente de red en la escena
@export var network_client_path: NodePath = NodePath("../../Game")

## Instancia del cliente de red para escuchar eventos del servidor
var network_client: NetworkClient = null

## Elementos de texto e interactividad del cuadro de dialogo
@onready var title_label: Label = $CenterContainer/ModalPanel/Margin/VBox/TitleLabel
@onready var player_label: Label = $CenterContainer/ModalPanel/Margin/VBox/PlayerLabel
@onready var concept_label: Label = $CenterContainer/ModalPanel/Margin/VBox/ConceptLabel
@onready var amount_label: Label = $CenterContainer/ModalPanel/Margin/VBox/AmountLabel
@onready var status_label: Label = $CenterContainer/ModalPanel/Margin/VBox/StatusLabel
@onready var btn_row: Control = $CenterContainer/ModalPanel/Margin/VBox/BtnRow

var _active_player_id: int = 0
var _active_amount: int = 0
var _is_closing: bool = false

## Configuracion inicial del modal y vinculacion de señales
func _ready() -> void:
	visible = false

	if network_client_path != NodePath() and has_node(network_client_path):
		network_client = get_node(network_client_path)
	elif has_node("/root/Board/Game"):
		network_client = get_node("/root/Board/Game")

	if network_client:
		network_client.payment_requested.connect(_on_payment_requested)
		network_client.payment_completed.connect(_on_payment_completed)
		network_client.payment_cancelled.connect(_on_payment_cancelled)
		network_client.payment_card_wrong.connect(_on_payment_card_wrong)

	# El pago se efectua unicamente mediante lectura fisica RFID
	if btn_row:
		btn_row.visible = false

## Muestra el modal informando al jugador sobre el monto y concepto pendiente
func _on_payment_requested(player_id: int, amount: int, concept: String) -> void:
	_active_player_id = player_id
	_active_amount = amount
	_is_closing = false

	title_label.text = "AUTORIZACION RFID REQUERIDA"
	player_label.text = "Jugador %d" % player_id
	concept_label.text = "Concepto: %s" % concept
	amount_label.text = "$%d" % amount
	status_label.text = "Acerque la tarjeta RFID del Jugador %d al sensor fisico..." % player_id
	status_label.set("theme_override_colors/font_color", Color(0.85, 0.9, 1.0))

	if btn_row:
		btn_row.visible = false

	visible = true

## Notifica visualmente cuando se escaneo una tarjeta no correspondiente al turno
func _on_payment_card_wrong(player_id: int) -> void:
	if not visible:
		return
	status_label.text = "[ERROR] Tarjeta incorrecta. Se necesita la tarjeta del Jugador %d." % player_id
	status_label.set("theme_override_colors/font_color", Color(1.0, 0.3, 0.3))

## Confirma el cobro tras la lectura de la tarjeta correcta
func _on_payment_completed(player_id: int, _amount: int) -> void:
	if not visible:
		return
	_is_closing = true
	status_label.text = "[OK] Tarjeta aprobada. Pago realizado con exito."
	status_label.set("theme_override_colors/font_color", Color(0.2, 0.95, 0.4))

	var timer := get_tree().create_timer(1.2)
	timer.timeout.connect(func():
		visible = false
		_is_closing = false
	)

## Cancela la solicitud de pago y cierra la ventana
func _on_payment_cancelled(_player_id: int) -> void:
	if not visible:
		return
	_is_closing = true
	status_label.text = "[CANCELADO] Pago cancelado."
	status_label.set("theme_override_colors/font_color", Color(0.95, 0.3, 0.3))

	var timer := get_tree().create_timer(1.0)
	timer.timeout.connect(func():
		visible = false
		_is_closing = false
	)
