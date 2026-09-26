extends Control
class_name PaymentModal

## Modal interactivo de pago estilo Apple Pay / Google Wallet / Terminal POS NFC.
## Se abre automáticamente cuando el servidor solicita autorización de pago
## (comprar propiedades, casas, pagar renta, hipotecas, impuestos o cárcel).
## Permite autorizar con toque RFID (físico o botón simulador) o cancelar.

@export var network_client_path: NodePath = NodePath("../../Game")

var network_client: NetworkClient = null

@onready var title_label: Label = $CenterContainer/ModalPanel/Margin/VBox/TitleLabel
@onready var player_label: Label = $CenterContainer/ModalPanel/Margin/VBox/PlayerLabel
@onready var concept_label: Label = $CenterContainer/ModalPanel/Margin/VBox/ConceptLabel
@onready var amount_label: Label = $CenterContainer/ModalPanel/Margin/VBox/AmountLabel
@onready var status_label: Label = $CenterContainer/ModalPanel/Margin/VBox/StatusLabel
@onready var rfid_btn: Button = $CenterContainer/ModalPanel/Margin/VBox/BtnRow/RfidBtn
@onready var cancel_btn: Button = $CenterContainer/ModalPanel/Margin/VBox/BtnRow/CancelBtn

var _active_player_id: int = 0
var _active_amount: int = 0
var _is_closing: bool = false

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

	rfid_btn.pressed.connect(_on_rfid_pressed)
	cancel_btn.pressed.connect(_on_cancel_pressed)

func _on_payment_requested(player_id: int, amount: int, concept: String) -> void:
	_active_player_id = player_id
	_active_amount = amount
	_is_closing = false

	player_label.text = "👤 Jugador %d" % player_id
	concept_label.text = "Concepto: %s" % concept
	amount_label.text = "$%d" % amount
	status_label.text = "Acerque su tarjeta/celular RFID al sensor físico\no presione el botón para autorizar."
	status_label.set("theme_override_colors/font_color", Color(0.75, 0.78, 0.85))

	rfid_btn.disabled = false
	rfid_btn.text = "📱💳 Pagar con RFID / Teléfono"
	cancel_btn.disabled = false

	visible = true

func _on_rfid_pressed() -> void:
	if _is_closing or network_client == null:
		return
	rfid_btn.disabled = true
	cancel_btn.disabled = true
	status_label.text = "📡 Detectando RFID / Contactless..."
	status_label.set("theme_override_colors/font_color", Color(0.3, 0.8, 1.0))
	network_client.confirm_rfid_payment()

func _on_cancel_pressed() -> void:
	if _is_closing or network_client == null:
		return
	rfid_btn.disabled = true
	cancel_btn.disabled = true
	status_label.text = "Cancelando pago..."
	network_client.cancel_rfid_payment()

func _on_payment_completed(player_id: int, _amount: int) -> void:
	if not visible:
		return
	_is_closing = true
	rfid_btn.disabled = true
	cancel_btn.disabled = true
	status_label.text = "✅ ¡Pago Autorizado con Éxito!"
	status_label.set("theme_override_colors/font_color", Color(0.2, 0.95, 0.4))

	var timer := get_tree().create_timer(1.2)
	timer.timeout.connect(func():
		visible = false
		_is_closing = false
	)

func _on_payment_cancelled(_player_id: int) -> void:
	if not visible:
		return
	_is_closing = true
	rfid_btn.disabled = true
	cancel_btn.disabled = true
	status_label.text = "❌ Pago Cancelado."
	status_label.set("theme_override_colors/font_color", Color(0.95, 0.3, 0.3))

	var timer := get_tree().create_timer(1.0)
	timer.timeout.connect(func():
		visible = false
		_is_closing = false
	)
