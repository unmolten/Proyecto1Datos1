import sys
import time
import machine
import uselect
import random
from machine import Pin
from mfrc522 import MFRC522


# ---------- Comunicación Serial ----------

poller = uselect.poll()
poller.register(sys.stdin, uselect.POLLIN)

VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789:_-"

def leer_comando():
    try:
        eventos = poller.poll(0)
        if eventos:
            linea_raw = sys.stdin.readline().strip().upper()
            linea = "".join([c for c in linea_raw if c in VALID_CHARS])
            return linea
    except Exception:
        pass
    return ""


# ---------- Lector RFID (Pins 0-4, SPI 0) ----------

sck_pin  = 2
mosi_pin = 3
miso_pin = 4
rst_pin  = 0
cs_pin   = 1

reader = None

def inicializar_rfid():
    global reader
    try:
        reader = MFRC522(sck_pin, mosi_pin, miso_pin, rst_pin, cs_pin, 500000, 0)
        print("RFID_INICIALIZADO_OK")
        return True
    except Exception as e:
        print("ERROR_RFID:", e)
        reader = None
        return False

inicializar_rfid()


fallos_consecutivos = 0

def leer_tarjeta():
    global reader, fallos_consecutivos
    if reader is None:
        inicializar_rfid()
        if reader is None:
            return

    try:
        # Intentar con REQIDL y si no responde, con REQALL (despierta tarjetas activas/halted)
        status, tag_type = reader.request(reader.REQIDL)
        if status != reader.OK:
            status, tag_type = reader.request(reader.REQALL)

        if status == reader.OK:
            try:
                status, uid = reader.anticoll(reader.PICC_ANTICOLL1)
            except (TypeError, AttributeError):
                status, uid = reader.anticoll()

            if status == reader.OK and uid:
                uid_hex = "0x" + "".join("{:02X}".format(b) for b in uid)
                print(f"UID:{uid_hex}")
                print(f"Card Detected! Hex UID: {uid_hex}")
                fallos_consecutivos = 0
                try:
                    reader.stop_crypto1()
                except Exception:
                    pass
                time.sleep_ms(600)
                try:
                    reader.init()
                except Exception:
                    pass
                return
        else:
            fallos_consecutivos += 1
            if fallos_consecutivos > 25:
                fallos_consecutivos = 0
                try:
                    reader.init()
                except Exception:
                    pass
    except OSError:
        fallos_consecutivos = 0
        try:
            reader.init()
        except Exception:
            pass
    except Exception:
        fallos_consecutivos = 0
        try:
            reader.init()
        except Exception:
            pass


# ---------- Dados (Displays 7 segmentos con Timer hardware + Botón) ----------

pin_a = Pin(5, Pin.OUT)
pin_b = Pin(6, Pin.OUT)
pin_c = Pin(7, Pin.OUT)
pin_d = Pin(8, Pin.OUT)
pin_e = Pin(9, Pin.OUT)
pin_f = Pin(10, Pin.OUT)
pin_g = Pin(11, Pin.OUT)

display1 = Pin(12, Pin.OUT)  # Activa Display 1 (Cátodo/Ánodo activo en 0)
display2 = Pin(13, Pin.OUT)  # Activa Display 2 (Cátodo/Ánodo activo en 0)

segmentos = {
    "a": pin_a,
    "b": pin_b,
    "c": pin_c,
    "d": pin_d,
    "e": pin_e,
    "f": pin_f,
    "g": pin_g,
}

# Soporte para botón en Pin 15 (Pull-Down) o Pin 14 (Pull-Up)
boton15 = Pin(15, Pin.IN, Pin.PULL_DOWN)
boton14 = Pin(14, Pin.IN, Pin.PULL_UP)

b15_idle = boton15.value()
b14_idle = boton14.value()
b15_prev = b15_idle
b14_prev = b14_idle

# Mapeo directo de segmentos: (a, b, c, d, e, f, g) -> 0 = encendido, 1 = apagado
PATRONES = {
    0: (0, 0, 0, 0, 0, 0, 1),    # abcdef
    1: (1, 0, 0, 1, 1, 1, 1),    # bc
    2: (0, 0, 1, 0, 0, 1, 0),    # abged
    3: (0, 0, 0, 0, 1, 1, 0),    # abgcd
    4: (1, 0, 0, 1, 1, 0, 0),    # fgbc
    5: (0, 1, 0, 0, 1, 0, 0),    # afgcd
    6: (0, 1, 0, 0, 0, 0, 0),    # afgedc
    7: (0, 0, 0, 1, 1, 1, 1),    # abc
    8: (0, 0, 0, 0, 0, 0, 0),    # abcdefg
    9: (0, 0, 0, 0, 1, 0, 0),    # abcdfg
    "-": (1, 1, 1, 1, 1, 1, 0),  # solo segmento g (guión)
}
PATRON_APAGADO = (1, 1, 1, 1, 1, 1, 1)

def aplicar_patron(p):
    pin_a.value(p[0])
    pin_b.value(p[1])
    pin_c.value(p[2])
    pin_d.value(p[3])
    pin_e.value(p[4])
    pin_f.value(p[5])
    pin_g.value(p[6])

def apagar_todo():
    display1.value(1)
    display2.value(1)
    aplicar_patron(PATRON_APAGADO)


dado1 = 0
dado2 = 0
puede_tirar = True
ultimo_tiro_ms = 0
display_actual = 0
modo = "DADOS"

# Rutina de interrupción periódica del Timer de hardware: refresca displays a ~166 Hz sin parpadeo
def refrescar_timer_callback(timer_obj):
    global display_actual

    # Apagar displays momentáneamente antes de cambiar segmentos para evitar efecto fantasma (ghosting)
    display1.value(1)
    display2.value(1)

    if modo == "ESPERA":
        aplicar_patron(PATRON_APAGADO)
        return

    # Determinar qué mostrar
    if dado1 > 0 and dado2 > 0:
        val1 = dado1
        val2 = dado2
    elif puede_tirar:
        val1 = "-"
        val2 = "-"
    else:
        aplicar_patron(PATRON_APAGADO)
        return

    if display_actual == 0:
        patron = PATRONES.get(val1, PATRON_APAGADO)
        aplicar_patron(patron)
        display1.value(0)
        display2.value(1)
        display_actual = 1
    else:
        patron = PATRONES.get(val2, PATRON_APAGADO)
        aplicar_patron(patron)
        display1.value(1)
        display2.value(0)
        display_actual = 0

# Iniciar timer hardware periódico a 3 ms por display (~166 Hz total, completamente estable)
try:
    timer_display = machine.Timer(-1)
except Exception:
    timer_display = machine.Timer()

timer_display.init(period=3, mode=machine.Timer.PERIODIC, callback=refrescar_timer_callback)


def actualizar_dados():
    global dado1, dado2, b15_prev, b14_prev, ultimo_tiro_ms, puede_tirar

    b15_act = boton15.value()
    b14_act = boton14.value()
    ahora = time.ticks_ms()

    # Si ya se tiró en este turno, no aceptar más pulsaciones para evitar tiros residuales
    if not puede_tirar:
        b15_prev = b15_act
        b14_prev = b14_act
        return

    presionado = False

    # Disparar si Pin 15 cambió de reposo hacia presionado (Pull-Down: activo en 1)
    if b15_act != b15_idle and b15_prev == b15_idle:
        if time.ticks_diff(ahora, ultimo_tiro_ms) > 300:
            presionado = True
            ultimo_tiro_ms = ahora

    # Disparar si Pin 14 cambió de reposo hacia presionado (Pull-Up: activo en 0)
    if b14_act != b14_idle and b14_prev == b14_idle:
        if time.ticks_diff(ahora, ultimo_tiro_ms) > 300:
            presionado = True
            ultimo_tiro_ms = ahora

    b15_prev = b15_act
    b14_prev = b14_act

    if presionado:
        dado1 = random.randint(1, 6)
        dado2 = random.randint(1, 6)
        casillas = dado1 + dado2
        puede_tirar = False
        print(f"DADOS:{dado1},{dado2},{casillas}")
        print(f"CASILLAS:{casillas}")


# ---------- Programa Principal ----------

apagar_todo()
print("PICO_LISTA")

while True:
    cmd = leer_comando()

    if cmd == "NUEVO_TURNO" or cmd == "DADOS":
        modo = "DADOS"
        dado1 = 0
        dado2 = 0
        puede_tirar = True
        print("MODO:DADOS_LISTO")
    elif cmd.startswith("MOSTRAR:"):
        partes = cmd.split(":")
        if len(partes) >= 2:
            sub = partes[1].split(",")
            try:
                d1 = int(sub[0])
                d2 = int(sub[1]) if len(sub) > 1 else 0
                if 1 <= d1 <= 6 and 1 <= d2 <= 6:
                    dado1 = d1
                    dado2 = d2
                    puede_tirar = False
                    print(f"MOSTRANDO:{dado1},{dado2}")
            except Exception:
                pass
    elif cmd == "TARJETAS":
        modo = "TARJETAS"
        print("MODO:TARJETAS")
    elif cmd == "STOP":
        modo = "ESPERA"
        dado1 = 0
        dado2 = 0
        puede_tirar = False
        apagar_todo()
        print("MODO:ESPERA")

    # Monitorear el botón del dado
    actualizar_dados()

    # Monitorear lector RFID solo si se está en modo TARJETAS o TODOS
    if modo == "TARJETAS" or modo == "TODOS":
        leer_tarjeta()

    time.sleep_ms(10)
