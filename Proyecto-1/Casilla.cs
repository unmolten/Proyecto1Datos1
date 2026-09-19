using System;

// Clase abstracta Casilla:
// Representa cada uno de los espacios físicos que conforman el tablero de Monopoly.
// Cada objeto 'Casilla' se almacena dentro del campo 'data' de un 'Node' en la lista enlazada circular del tablero.
// Clase base abstracta que heredan las clases Propiedad, CasillaEvento y CasillaEspecial.
public abstract class Casilla
{
    // Posición o índice numérico en el tablero (0 a 31).
    public int Posicion { get; set; }

    // Nombre visible de la casilla.
    public string Nombre { get; set; }

    // Tipo de casilla ("Salida", "Propiedad", "Carcel", "Impuesto", "ParadaLibre", "VayaALaCarcel", "Fortuna", "ArcaComunal").
    public string Tipo { get; set; }

    // Constructor base para todas las casillas.
    public Casilla(int posicion, string nombre, string tipo)
    {
        this.Posicion = posicion;
        this.Nombre = nombre;
        this.Tipo = tipo;
    }

    // POLIMORFISMO: Cada subclase implementa su comportamiento específico cuando un jugador cae en ella.
    public abstract void Accion(Jugador jugador);

    // Sobrecarga opcional para compatibilidad previa.
    public virtual void Accion() {}

    // Métodos virtuales para consulta polimórfica de propiedades.
    public virtual bool EsPropiedad() => false;
    public virtual bool TienePropietario() => false;
    public virtual int CalcularRenta() => 0;
    public virtual int Renta => CalcularRenta();

    public override string ToString()
    {
        return $"[{this.Posicion}] {this.Nombre} ({this.Tipo})";
    }
}

// Subclase Propiedad:
// Representa una propiedad que puede ser comprada, mejorada con casas y generar renta.
public class Propiedad : Casilla
{
    // Propiedades hereditarias y específicas de Propiedad
    public int PrecioCompra { get; set; }
    public int AlquilerBase { get; set; }
    public Jugador? Propietario { get; set; }
    public bool IsHipotecada { get; set; }
    public string ColorGrupo { get; set; }
    public int CantidadCasas { get; set; } // 0 a 4 casas, 5 = hotel

    public Propiedad(int posicion, string nombre, string tipo, int precioCompra, int alquilerBase, Jugador? propietario = null, string colorGrupo = "")
        : base(posicion, nombre, tipo)
    {
        this.PrecioCompra = precioCompra;
        this.AlquilerBase = alquilerBase;
        this.Propietario = propietario;
        this.IsHipotecada = false;
        this.ColorGrupo = colorGrupo;
        this.CantidadCasas = 0;
    }

    public override bool EsPropiedad() => true;

    public override bool TienePropietario() => this.Propietario != null;

    // Calcula la renta en base al alquiler base, hipoteca y cantidad de casas/hotel
    public override int CalcularRenta()
    {
        if (this.IsHipotecada) return 0;
        if (this.CantidadCasas == 0) return this.AlquilerBase;
        if (this.CantidadCasas == 5) return this.AlquilerBase * 8; // Hotel
        return this.AlquilerBase * (1 + this.CantidadCasas * 2);   // Casas 1 a 4
    }

    public int NumeroCasas() => this.CantidadCasas;

    // POLIMORFISMO: Comportamiento cuando un jugador aterriza en una Propiedad
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;

        if (!TienePropietario())
        {
            jugador.EnviarMensaje($"🏠 La propiedad '{this.Nombre}' está disponible para compra por ${this.PrecioCompra}.");
            if (jugador.Dinero >= this.PrecioCompra)
            {
                jugador.EnviarMensaje($"💡 Usa el comando 'COMPRAR' si deseas adquirirla.");
            }
            else
            {
                jugador.EnviarMensaje($"❌ Saldo insuficiente (${jugador.Dinero}) para comprarla.");
            }
        }
        else if (this.Propietario != jugador)
        {
            if (this.IsHipotecada)
            {
                jugador.EnviarMensaje($"ℹ️ '{this.Nombre}' está hipotecada. No pagas renta.");
                return;
            }

            int renta = CalcularRenta();
            jugador.EnviarMensaje($"💸 Caíste en '{this.Nombre}' de {this.Propietario!.Nombre}. Renta a pagar: ${renta}.");

            // Transferencia de dinero mediante el registro de Transacciones
            new Transaccion(renta, juego.TurnoActual, "Pago de alquiler", jugador, this.Propietario);

            this.Propietario.EnviarMensaje($"💰 ¡Recibiste ${renta} de renta de {jugador.Nombre} por '{this.Nombre}'!");
            juego.Broadcast($"📢 {jugador.Nombre} pagó ${renta} de renta a {this.Propietario.Nombre} por {this.Nombre}.", jugador);

            if (jugador.Dinero < 0)
            {
                jugador.EnviarMensaje("⚠️ ¡Estás en bancarrota! Tu saldo es negativo.");
                juego.Broadcast($"🚨 ¡{jugador.Nombre} ha caído en bancarrota!", jugador);
            }
        }
        else
        {
            string casasInfo = this.CantidadCasas == 5 ? "Hotel" : $"{this.CantidadCasas} casas";
            jugador.EnviarMensaje($"🏡 Estás en tu propia propiedad '{this.Nombre}' ({casasInfo}).");
        }
    }

    public override string ToString()
    {
        string dueño = this.Propietario != null ? this.Propietario.Nombre : "Sin dueño";
        string nivel = this.CantidadCasas == 5 ? "Hotel" : $"{this.CantidadCasas} casas";
        return $"[{this.Posicion}] {this.Nombre} (Propiedad) - Precio: ${this.PrecioCompra} | Renta: ${CalcularRenta()} | Dueño: {dueño} | {nivel}";
    }
}

// Subclase CasillaEvento:
// Representa casillas de Fortuna o Arca Comunal que roban cartas y aplican sus efectos.
public class CasillaEvento : Casilla
{
    public string TipoEvento { get; set; }

    public CasillaEvento(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
        this.TipoEvento = tipo;
    }

    // POLIMORFISMO: Acción al caer en una Casilla de Evento (Fortuna o Arca Comunal)
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;
        LinkedList mazo = (this.Tipo == "ArcaComunal" || this.Nombre.Contains("Arca"))
            ? juego.MazoArcaComunal
            : juego.MazoFortuna;

        CartaEvento carta = MazoCartas.RobarCarta(mazo);
        jugador.EnviarMensaje($"\n🎴 [CARTA DE {this.Nombre.ToUpper()}]:");
        jugador.EnviarMensaje($"\"{carta.Descripcion}\"\n");
        juego.Broadcast($"📢 {jugador.Nombre} sacó una carta de {this.Nombre}: \"{carta.Descripcion}\"", jugador);

        AplicarEfectoCarta(carta, jugador, juego);
    }

    private void AplicarEfectoCarta(CartaEvento carta, Jugador jugador, JuegoMonopoly juego)
    {
        switch (carta.Tipo)
        {
            case TipoEfectoCarta.GanarDinero:
                new Transaccion(carta.Monto, juego.TurnoActual, "Ganancia por evento", jugador, null);
                jugador.EnviarMensaje($"💵 Saldo actual: ${jugador.Dinero}");
                break;

            case TipoEfectoCarta.PerderDinero:
                new Transaccion(carta.Monto, juego.TurnoActual, "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"💸 Saldo actual: ${jugador.Dinero}");
                break;

            case TipoEfectoCarta.PagarACadaJugador:
                Node? actualP = juego.Jugadores.GetHead();
                for (int i = 0; i < juego.Jugadores.Size(); i++)
                {
                    if (actualP?.GetData() is Jugador otro && otro.Id != jugador.Id)
                    {
                        new Transaccion(carta.Monto, juego.TurnoActual, "Pago entre jugadores", jugador, otro);
                        otro.EnviarMensaje($"💰 {jugador.Nombre} te pagó ${carta.Monto} por evento.");
                    }
                    actualP = actualP?.GetNext();
                }
                jugador.EnviarMensaje($"Saldo actual: ${jugador.Dinero}");
                break;

            case TipoEfectoCarta.CobrarDeCadaJugador:
                Node? actualC = juego.Jugadores.GetHead();
                for (int i = 0; i < juego.Jugadores.Size(); i++)
                {
                    if (actualC?.GetData() is Jugador otro && otro.Id != jugador.Id)
                    {
                        new Transaccion(carta.Monto, juego.TurnoActual, "Pago entre jugadores", otro, jugador);
                        otro.EnviarMensaje($"💸 Pagaste ${carta.Monto} a {jugador.Nombre} por evento.");
                    }
                    actualC = actualC?.GetNext();
                }
                jugador.EnviarMensaje($"Saldo actual: ${jugador.Dinero}");
                break;

            case TipoEfectoCarta.PerderDineroPorPropiedad:
                int costoProp = carta.Monto * jugador.Propiedades.Size();
                new Transaccion(costoProp, juego.TurnoActual, "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"💸 Pagaste ${costoProp} (${carta.Monto} x {jugador.Propiedades.Size()} propiedades). Saldo: ${jugador.Dinero}");
                break;

            case TipoEfectoCarta.PerderDineroPorConstruccion:
                int totalCasas = 0;
                int totalHoteles = 0;
                Node? propNodo = jugador.Propiedades.GetHead();
                for (int i = 0; i < jugador.Propiedades.Size(); i++)
                {
                    if (propNodo?.GetData() is Propiedad prop)
                    {
                        if (prop.CantidadCasas == 5) totalHoteles++;
                        else totalCasas += prop.CantidadCasas;
                    }
                    propNodo = propNodo?.GetNext();
                }
                int costoConst = (totalCasas * carta.MontoPorCasa) + (totalHoteles * carta.MontoPorHotel);
                new Transaccion(costoConst, juego.TurnoActual, "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"💸 Reparaciones: ${costoConst} ({totalCasas} casas x ${carta.MontoPorCasa}, {totalHoteles} hoteles x ${carta.MontoPorHotel}). Saldo: ${jugador.Dinero}");
                break;

            case TipoEfectoCarta.MoverACasilla:
                juego.MoverJugadorACasilla(jugador, carta.CasillaDestino);
                break;

            case TipoEfectoCarta.MoverCasillas:
                juego.MoverJugadorCasillas(jugador, carta.CantidadCasillas);
                break;

            case TipoEfectoCarta.IrACarcel:
                juego.EnviarACarcel(jugador);
                break;

            case TipoEfectoCarta.SalirDeCarcelGratis:
                jugador.CartasSalirDeCarcel++;
                jugador.EnviarMensaje($"🎟️ ¡Guardas una carta para salir gratis de la cárcel! (Total cartas: {jugador.CartasSalirDeCarcel})");
                break;

            case TipoEfectoCarta.TomarOtraCarta:
                LinkedList mazoExtra = (carta.Mazo == 1) ? juego.MazoFortuna : juego.MazoArcaComunal;
                CartaEvento cartaExtra = MazoCartas.RobarCarta(mazoExtra);
                jugador.EnviarMensaje($"🎴 Carta adicional: \"{cartaExtra.Descripcion}\"");
                AplicarEfectoCarta(cartaExtra, jugador, juego);
                break;
        }

        if (carta.PierdeTurno)
        {
            jugador.PierdeSiguienteTurno = true;
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

        switch (this.Tipo)
        {
            case "Salida":
                new Transaccion(200, juego.TurnoActual, "Premio por pasar por inicio", jugador, null);
                jugador.EnviarMensaje("💵 ¡Aterrizaste en Salida! Cobraste $200 de bono.");
                juego.Broadcast($"📢 {jugador.Nombre} cayó en Salida y cobró $200.", jugador);
                break;

            case "Impuesto":
                int montoImpuesto = 100;
                new Transaccion(montoImpuesto, juego.TurnoActual, "Pago al banco", jugador, null);
                jugador.EnviarMensaje($"🧾 Impuesto sobre la renta: Pagaste ${montoImpuesto} al banco. Saldo: ${jugador.Dinero}");
                juego.Broadcast($"📢 {jugador.Nombre} pagó ${montoImpuesto} de impuestos.", jugador);
                break;

            case "Carcel":
                if (jugador.EnCarcel)
                {
                    jugador.EnviarMensaje($"🔒 Estás cumpliendo condena en la Cárcel (Turnos en espera: {jugador.TurnosEnCarcel}/3).");
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
                jugador.EnviarMensaje($"📍 Te encuentras en {this.Nombre}.");
                break;
        }
    }
}
