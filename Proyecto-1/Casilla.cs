using System;

// Clase abstracta Casilla:
// Representa cada uno de los espacios físicos que conforman el tablero de Monopoly.
// Cada objeto 'Casilla' se almacena dentro del campo 'data' de un 'Node' en la lista enlazada circular del tablero.
// Clase base abstracta que heredan las clases Propiedad, CasillaEvento y CasillaEspecial.
public abstract class Casilla
{
    // Posición o índice numérico en el tablero (0 a 31).
    private int posicion;
    private string nombre;
    private string tipo;

    public int GetPosicion()
    {
        return this.posicion;
    }

    public void SetPosicion(int posicion)
    {
        this.posicion = posicion;
    }

    public string GetNombre()
    {
        return this.nombre;
    }

    public void SetNombre(string nombre)
    {
        this.nombre = nombre;
    }

    public string GetTipo()
    {
        return this.tipo;
    }

    public void SetTipo(string tipo)
    {
        this.tipo = tipo;
    }


    // Constructor base para todas las casillas.
    public Casilla(int posicion, string nombre, string tipo)
    {
        this.posicion = posicion;
        this.nombre = nombre;
        this.tipo = tipo;
    }

    // POLIMORFISMO: Cada subclase implementa su comportamiento específico cuando un jugador cae en ella.
    public abstract void Accion(Jugador jugador);

    // Sobrecarga opcional para compatibilidad previa.
    public virtual void Accion() {}

    // Métodos virtuales para consulta polimórfica de propiedades.
    public virtual bool EsPropiedad() => false;
    public virtual bool TienePropietario() => false;
    public virtual int CalcularRenta() => 0;
    public virtual int GetRenta() => CalcularRenta();

    public override string ToString()
    {
        return $"[{GetPosicion()}] {GetNombre()} ({GetTipo()})";
    }
}

// Subclase Propiedad:
// Representa una propiedad que puede ser comprada, mejorada con casas y generar renta.
public class Propiedad : Casilla
{
    // Propiedades hereditarias y específicas de Propiedad
    private int precioCompra;
    private int alquilerBase;
    private Jugador? propietario;
    private bool isHipotecada;
    private string colorGrupo;
    private int cantidadCasas; // 0 a 4 casas, 5 = hotel

    public int GetPrecioCompra()
    {
        return this.precioCompra;
    }

    public void SetPrecioCompra(int precioCompra)
    {
        this.precioCompra = precioCompra;
    }

    public int GetAlquilerBase()
    {
        return this.alquilerBase;
    }

    public void SetAlquilerBase(int alquilerBase)
    {
        this.alquilerBase = alquilerBase;
    }

    public Jugador? GetPropietario()
    {
        return this.propietario;
    }

    public void SetPropietario(Jugador? propietario)
    {
        this.propietario = propietario;
    }

    public bool GetIsHipotecada()
    {
        return this.isHipotecada;
    }

    public void SetIsHipotecada(bool isHipotecada)
    {
        this.isHipotecada = isHipotecada;
    }

    public string GetColorGrupo()
    {
        return this.colorGrupo;
    }

    public void SetColorGrupo(string colorGrupo)
    {
        this.colorGrupo = colorGrupo;
    }

    public int GetCantidadCasas()
    {
        return this.cantidadCasas;
    }

    public void SetCantidadCasas(int cantidadCasas)
    {
        this.cantidadCasas = cantidadCasas;
    }



    public Propiedad(int posicion, string nombre, string tipo, int precioCompra, int alquilerBase, Jugador? propietario = null, string colorGrupo = "")
        : base(posicion, nombre, tipo)
    {
        this.precioCompra = precioCompra;
        this.alquilerBase = alquilerBase;
        this.propietario = propietario;
        this.isHipotecada = false;
        this.colorGrupo = colorGrupo;
        this.cantidadCasas = 0;
    }

    public override bool EsPropiedad() => true;

    public override bool TienePropietario() => GetPropietario() != null;

    // Calcula la renta en base al alquiler base, hipoteca y cantidad de casas/hotel
    public override int CalcularRenta()
    {
        if (GetIsHipotecada()) return 0;
        if (GetCantidadCasas() == 0) return GetAlquilerBase();
        if (GetCantidadCasas() == 5) return GetAlquilerBase() * 8; // Hotel
        return GetAlquilerBase() * (1 + GetCantidadCasas() * 2);   // Casas 1 a 4
    }

    public int NumeroCasas() => GetCantidadCasas();

    // POLIMORFISMO: Comportamiento cuando un jugador aterriza en una Propiedad
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;

        if (!TienePropietario())
        {
            jugador.EnviarMensaje($"🏠 La propiedad '{GetNombre()}' está disponible para compra por ${GetPrecioCompra()}.");
            if (jugador.GetDinero() >= GetPrecioCompra())
            {
                jugador.EnviarMensaje($"💡 Usa el comando 'COMPRAR' si deseas adquirirla.");
            }
            else
            {
                jugador.EnviarMensaje($"❌ Saldo insuficiente (${jugador.GetDinero()}) para comprarla.");
            }
        }
        else if (GetPropietario() != jugador)
        {
            if (GetIsHipotecada())
            {
                jugador.EnviarMensaje($"ℹ️ '{GetNombre()}' está hipotecada. No pagas renta.");
                return;
            }

            int renta = CalcularRenta();
            jugador.EnviarMensaje($"💸 Caíste en '{GetNombre()}' de {GetPropietario()!.GetNombre()}. Renta a pagar: ${renta}.");

            // Transferencia de dinero mediante el registro de Transacciones con verificación RFID
            juego.AutorizarPagoRFID(jugador, renta, $"Renta a {GetPropietario()!.GetNombre()} por '{GetNombre()}'");
            new Transaccion(renta, juego.GetTurnoActual(), "Pago de alquiler", jugador, GetPropietario());

            GetPropietario()!.EnviarMensaje($"💰 ¡Recibiste ${renta} de renta de {jugador.GetNombre()} por '{GetNombre()}'!");
            juego.Broadcast($"📢 {jugador.GetNombre()} pagó ${renta} de renta a {GetPropietario()!.GetNombre()} por {GetNombre()}.", jugador);
            juego.AnunciarDinero(jugador);
            juego.AnunciarDinero(GetPropietario()!);

            if (jugador.GetDinero() < 0)
            {
                jugador.EnviarMensaje("⚠️ ¡Estás en bancarrota! Tu saldo es negativo.");
                juego.Broadcast($"🚨 ¡{jugador.GetNombre()} ha caído en bancarrota!", jugador);
            }
        }
        else
        {
            string casasInfo = GetCantidadCasas() == 5 ? "Hotel" : $"{GetCantidadCasas()} casas";
            jugador.EnviarMensaje($"🏡 Estás en tu propia propiedad '{GetNombre()}' ({casasInfo}).");
        }
    }

    public override string ToString()
    {
        string dueño = GetPropietario() != null ? GetPropietario()!.GetNombre() : "Sin dueño";
        string nivel = GetCantidadCasas() == 5 ? "Hotel" : $"{GetCantidadCasas()} casas";
        return $"[{GetPosicion()}] {GetNombre()} (Propiedad) - Precio: ${GetPrecioCompra()} | Renta: ${CalcularRenta()} | Dueño: {dueño} | {nivel}";
    }
}

// Subclase CasillaEvento:
// Representa casillas de Fortuna o Arca Comunal que roban cartas y aplican sus efectos.
public class CasillaEvento : Casilla
{
    private string tipoEvento;

    public string GetTipoEvento()
    {
        return this.tipoEvento;
    }

    public void SetTipoEvento(string tipoEvento)
    {
        this.tipoEvento = tipoEvento;
    }



    public CasillaEvento(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
        this.tipoEvento = tipo;
    }

    // POLIMORFISMO: Acción al caer en una Casilla de Evento (Fortuna o Arca Comunal)
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;
        LinkedList mazo = (GetTipo() == "ArcaComunal" || GetNombre().Contains("Arca"))
            ? juego.GetMazoArcaComunal()
            : juego.GetMazoFortuna();

        CartaEvento carta = MazoCartas.RobarCarta(mazo);
        jugador.EnviarMensaje($"\n🎴 [CARTA DE {GetNombre().ToUpper()}]:");
        jugador.EnviarMensaje($"\"{carta.GetDescripcion()}\"\n");
        juego.Broadcast($"📢 {jugador.GetNombre()} sacó una carta de {GetNombre()}: \"{carta.GetDescripcion()}\"", jugador);

        AplicarEfectoCarta(carta, jugador, juego);
    }

    private void AplicarEfectoCarta(CartaEvento carta, Jugador jugador, JuegoMonopoly juego)
    {
        switch (carta.GetTipo())
        {
            case TipoEfectoCarta.GanarDinero:
                new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Ganancia por evento", jugador, null);
                jugador.EnviarMensaje($"💵 Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PerderDinero:
                new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"💸 Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PagarACadaJugador:
                Node? actualP = juego.GetJugadores().GetHead();
                for (int i = 0; i < juego.GetJugadores().Size(); i++)
                {
                    if (actualP?.GetData() is Jugador otro && otro.GetId() != jugador.GetId())
                    {
                        new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Pago entre jugadores", jugador, otro);
                        otro.EnviarMensaje($"💰 {jugador.GetNombre()} te pagó ${carta.GetMonto()} por evento.");
                    }
                    actualP = actualP?.GetNext();
                }
                jugador.EnviarMensaje($"Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.CobrarDeCadaJugador:
                Node? actualC = juego.GetJugadores().GetHead();
                for (int i = 0; i < juego.GetJugadores().Size(); i++)
                {
                    if (actualC?.GetData() is Jugador otro && otro.GetId() != jugador.GetId())
                    {
                        new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Pago entre jugadores", otro, jugador);
                        otro.EnviarMensaje($"💸 Pagaste ${carta.GetMonto()} a {jugador.GetNombre()} por evento.");
                    }
                    actualC = actualC?.GetNext();
                }
                jugador.EnviarMensaje($"Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PerderDineroPorPropiedad:
                int costoProp = carta.GetMonto() * jugador.GetPropiedades().Size();
                new Transaccion(costoProp, juego.GetTurnoActual(), "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"💸 Pagaste ${costoProp} (${carta.GetMonto()} x {jugador.GetPropiedades().Size()} propiedades). Saldo: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PerderDineroPorConstruccion:
                int totalCasas = 0;
                int totalHoteles = 0;
                Node? propNodo = jugador.GetPropiedades().GetHead();
                for (int i = 0; i < jugador.GetPropiedades().Size(); i++)
                {
                    if (propNodo?.GetData() is Propiedad prop)
                    {
                        if (prop.NumeroCasas() == 5) totalHoteles++;
                        else totalCasas += prop.NumeroCasas();
                    }
                    propNodo = propNodo?.GetNext();
                }
                int costoConst = (totalCasas * carta.GetMontoPorCasa()) + (totalHoteles * carta.GetMontoPorHotel());
                new Transaccion(costoConst, juego.GetTurnoActual(), "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"💸 Reparaciones: ${costoConst} ({totalCasas} casas x ${carta.GetMontoPorCasa()}, {totalHoteles} hoteles x ${carta.GetMontoPorHotel()}). Saldo: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.MoverACasilla:
                juego.MoverJugadorACasilla(jugador, carta.GetCasillaDestino());
                break;

            case TipoEfectoCarta.MoverCasillas:
                juego.MoverJugadorCasillas(jugador, carta.GetCantidadCasillas());
                break;

            case TipoEfectoCarta.IrACarcel:
                juego.EnviarACarcel(jugador);
                break;

            case TipoEfectoCarta.SalirDeCarcelGratis:
                jugador.SetCartasSalirDeCarcel(jugador.GetCartasSalirDeCarcel() + 1);
                jugador.EnviarMensaje($"🎟️ ¡Guardas una carta para salir gratis de la cárcel! (Total cartas: {jugador.GetCartasSalirDeCarcel()})");
                break;

            case TipoEfectoCarta.TomarOtraCarta:
                LinkedList mazoExtra = (carta.GetMazo() == 1) ? juego.GetMazoFortuna() : juego.GetMazoArcaComunal();
                CartaEvento cartaExtra = MazoCartas.RobarCarta(mazoExtra);
                jugador.EnviarMensaje($"🎴 Carta adicional: \"{cartaExtra.GetDescripcion()}\"");
                AplicarEfectoCarta(cartaExtra, jugador, juego);
                break;
        }

        if (carta.GetPierdeTurno())
        {
            jugador.SetPierdeSiguienteTurno(true);
            jugador.EnviarMensaje("⏳ Pierdes tu siguiente turno.");
        }
    }
}

// Subclase CasillaEspecial:
// Representa casillas no comprables con reglas fijas (Salida, Cárcel, Impuesto, Parada Libre, Vaya a la Cárcel).
public class CasillaEspecial : Casilla
{
    public CasillaEspecial(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
    }

    // POLIMORFISMO: Acción al caer en una Casilla Especial
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;

        switch (GetTipo())
        {
            case "Salida":
                new Transaccion(200, juego.GetTurnoActual(), "Premio por pasar por inicio", jugador, null);
                jugador.EnviarMensaje("💵 ¡Aterrizaste en Salida! Cobraste $200 de bono.");
                juego.Broadcast($"📢 {jugador.GetNombre()} cayó en Salida y cobró $200.", jugador);
                break;

            case "Impuesto":
                int montoImpuesto = 100;
                juego.AutorizarPagoRFID(jugador, montoImpuesto, "Impuesto sobre la renta");
                new Transaccion(montoImpuesto, juego.GetTurnoActual(), "Pago al banco", jugador, null);
                jugador.EnviarMensaje($"🧾 Impuesto sobre la renta: Pagaste ${montoImpuesto} al banco. Saldo: ${jugador.GetDinero()}");
                juego.Broadcast($"📢 {jugador.GetNombre()} pagó ${montoImpuesto} de impuestos.", jugador);
                juego.AnunciarDinero(jugador);
                break;

            case "Carcel":
                if (jugador.GetEnCarcel())
                {
                    jugador.EnviarMensaje($"🔒 Estás cumpliendo condena en la Cárcel (Turnos en espera: {jugador.GetTurnosEnCarcel()}/3).");
                }
                else
                {
                    jugador.EnviarMensaje("👮 Estás en la Cárcel sólo de visita. No hay penalización.");
                }
                break;

            case "VayaALaCarcel":
                jugador.EnviarMensaje("🚨 ¡Cometiste una infracción! ¡Vas directo a la Cárcel sin cobrar Salida!");
                juego.EnviarACarcel(jugador);
                break;

            case "ParadaLibre":
                jugador.EnviarMensaje("☕ Parada Libre: Tómate un respiro, no hay cobros ni multas.");
                break;

            default:
                jugador.EnviarMensaje($"📍 Te encuentras en {GetNombre()}.");
                break;
        }
    }
}
