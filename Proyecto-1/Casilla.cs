using System;

/**
 * @file Casilla.cs
 * @brief Jerarquia polimorfica para las casillas del tablero de Monopoly.
 *
 * Cada casilla del tablero se almacena en el campo 'data' de un Node de la lista
 * enlazada circular. La clase base Casilla provee metodos virtuales y abstractos
 * para que cada tipo de casilla (Propiedad, CasillaEvento, CasillaEspecial) ejecute
 * su comportamiento adecuado cuando un jugador aterriza en ella.
 */

/**
 * @class Casilla
 * @brief Clase base abstracta que representa un espacio fisico en el tablero.
 */
public abstract class Casilla
{
    /** @brief Posicion o indice numerico en el tablero (0 a 31). */
    private int posicion;

    /** @brief Nombre identificativo de la casilla. */
    private string nombre;

    /** @brief Categoria o tipo de casilla (Propiedad, Evento, Especial, etc.). */
    private string tipo;

    /**
     * @brief Obtiene la posicion numerica en el tablero.
     * @return Indice entero de la casilla.
     */
    public int GetPosicion()
    {
        return this.posicion;
    }

    /**
     * @brief Establece la posicion en el tablero.
     * @param posicion Indice entero a asignar.
     */
    public void SetPosicion(int posicion)
    {
        this.posicion = posicion;
    }

    /**
     * @brief Obtiene el nombre de la casilla.
     * @return Cadena con el nombre de la casilla.
     */
    public string GetNombre()
    {
        return this.nombre;
    }

    /**
     * @brief Modifica el nombre de la casilla.
     * @param nombre Cadena con el nuevo nombre.
     */
    public void SetNombre(string nombre)
    {
        this.nombre = nombre;
    }

    /**
     * @brief Obtiene el tipo de la casilla.
     * @return Cadena con el tipo de la casilla.
     */
    public string GetTipo()
    {
        return this.tipo;
    }

    /**
     * @brief Modifica el tipo de la casilla.
     * @param tipo Cadena con el nuevo tipo.
     */
    public void SetTipo(string tipo)
    {
        this.tipo = tipo;
    }

    /**
     * @brief Constructor base para inicializar una casilla del tablero.
     * @param posicion Indice numerico en el recorrido circular.
     * @param nombre Nombre descriptivo.
     * @param tipo Categoria funcional de la casilla.
     */
    public Casilla(int posicion, string nombre, string tipo)
    {
        this.posicion = posicion;
        this.nombre = nombre;
        this.tipo = tipo;
    }

    /**
     * @brief Metodo polimorfico ejecutado cuando un jugador cae en la casilla.
     * @param jugador Instancia del jugador que llego a la casilla.
     */
    public abstract void Accion(Jugador jugador);

    /**
     * @brief Sobrecarga virtual sin argumentos para compatibilidad.
     */
    public virtual void Accion() {}

    /**
     * @brief Consulta si la casilla es un bien inmueble comprable.
     * @return true si es una Propiedad; false en caso contrario.
     */
    public virtual bool EsPropiedad() => false;

    /**
     * @brief Consulta si la casilla cuenta con un dueño asignado.
     * @return true si tiene dueño; false en caso contrario.
     */
    public virtual bool TienePropietario() => false;

    /**
     * @brief Calcula la renta actual a cobrar al jugador que aterrice en ella.
     * @return Monto de renta a pagar.
     */
    public virtual int CalcularRenta() => 0;

    /**
     * @brief Metodo de conveniencia para consultar la renta.
     * @return Renta calculada.
     */
    public virtual int GetRenta() => CalcularRenta();

    /**
     * @brief Representacion en cadena de texto de la casilla.
     * @return Cadena con formato [posicion] Nombre (Tipo).
     */
    public override string ToString()
    {
        return $"[{GetPosicion()}] {GetNombre()} ({GetTipo()})";
    }
}

/**
 * @class Propiedad
 * @brief Subclase de Casilla que representa bienes inmuebles comprables y edificables.
 *
 * Permite construccion de casas o un hotel, genera cobro de renta a otros jugadores,
 * y puede ser hipotecada o deshipotecada segun el reglamento del juego.
 */
public class Propiedad : Casilla
{
    /** @brief Costo base para comprar la propiedad. */
    private int precioCompra;

    /** @brief Monto de alquiler basico sin construcciones ni multiplicadores. */
    private int alquilerBase;

    /** @brief Referencia al jugador dueño de la propiedad (null si no tiene dueño). */
    private Jugador? propietario;

    /** @brief Indica si la propiedad se encuentra actualmente hipotecada. */
    private bool isHipotecada;

    /** @brief Grupo o color al que pertenece la propiedad dentro del tablero. */
    private string colorGrupo;

    /** @brief Cantidad de mejoras: 0 a 4 corresponden a casas, 5 representa un hotel. */
    private int cantidadCasas;

    /**
     * @brief Obtiene el precio de compra del inmueble.
     * @return Monto en dinero necesario para comprar.
     */
    public int GetPrecioCompra()
    {
        return this.precioCompra;
    }

    /**
     * @brief Asigna el precio de compra del inmueble.
     * @param precioCompra Nuevo valor monetario de compra.
     */
    public void SetPrecioCompra(int precioCompra)
    {
        this.precioCompra = precioCompra;
    }

    /**
     * @brief Obtiene la tarifa de alquiler base.
     * @return Monto de renta base.
     */
    public int GetAlquilerBase()
    {
        return this.alquilerBase;
    }

    /**
     * @brief Modifica la tarifa de alquiler base.
     * @param alquilerBase Nuevo valor base de alquiler.
     */
    public void SetAlquilerBase(int alquilerBase)
    {
        this.alquilerBase = alquilerBase;
    }

    /**
     * @brief Obtiene el jugador propietario actual.
     * @return Instancia del Jugador dueño o null si esta en venta.
     */
    public Jugador? GetPropietario()
    {
        return this.propietario;
    }

    /**
     * @brief Asigna el nuevo dueño de la propiedad.
     * @param propietario Instancia del jugador que adquiere la propiedad.
     */
    public void SetPropietario(Jugador? propietario)
    {
        this.propietario = propietario;
    }

    /**
     * @brief Indica si el inmueble esta en estado de hipoteca.
     * @return true si esta hipotecada; false si opera normalmente.
     */
    public bool GetIsHipotecada()
    {
        return this.isHipotecada;
    }

    /**
     * @brief Establece el estado de hipoteca de la propiedad.
     * @param isHipotecada Estado booleano de la hipoteca.
     */
    public void SetIsHipotecada(bool isHipotecada)
    {
        this.isHipotecada = isHipotecada;
    }

    /**
     * @brief Obtiene el grupo tematico o color de la propiedad.
     * @return Nombre del grupo al que pertenece.
     */
    public string GetColorGrupo()
    {
        return this.colorGrupo;
    }

    /**
     * @brief Asigna el grupo tematico o color de la propiedad.
     * @param colorGrupo Nombre del nuevo grupo.
     */
    public void SetColorGrupo(string colorGrupo)
    {
        this.colorGrupo = colorGrupo;
    }

    /**
     * @brief Obtiene el numero de casas o nivel de hotel construido.
     * @return Entero entre 0 y 5.
     */
    public int GetCantidadCasas()
    {
        return this.cantidadCasas;
    }

    /**
     * @brief Modifica el nivel de construccion de casas u hotel.
     * @param cantidadCasas Valor entre 0 y 5.
     */
    public void SetCantidadCasas(int cantidadCasas)
    {
        this.cantidadCasas = cantidadCasas;
    }

    /**
     * @brief Constructor de la clase Propiedad.
     * @param posicion Indice numerico en el tablero.
     * @param nombre Nombre del inmueble.
     * @param tipo Categoria funcional (generalmente "Propiedad").
     * @param precioCompra Costo en dinero para adquirirla.
     * @param alquilerBase Renta minima a cobrar.
     * @param propietario Dueño inicial (opcional, null por defecto).
     * @param colorGrupo Grupo o distrito de la propiedad.
     */
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

    /**
     * @brief Sobrescritura que confirma que este objeto es una propiedad comprable.
     * @return true siempre.
     */
    public override bool EsPropiedad() => true;

    /**
     * @brief Verifica si la propiedad ya tiene un propietario registrado.
     * @return true si propietario no es null; false en caso contrario.
     */
    public override bool TienePropietario() => GetPropietario() != null;

    /**
     * @brief Calcula la renta a cobrar segun el estado de hipoteca y nivel de construccion.
     *
     * Si esta hipotecada, la renta es 0.
     * Si no tiene casas, es el alquiler base.
     * Si tiene hotel (nivel 5), se multiplica por 8.
     * Si tiene entre 1 y 4 casas, se multiplica proporcionalmente.
     * @return Monto calculado de alquiler.
     */
    public override int CalcularRenta()
    {
        if (GetIsHipotecada()) return 0;
        if (GetCantidadCasas() == 0) return GetAlquilerBase();
        if (GetCantidadCasas() == 5) return GetAlquilerBase() * 8; // Hotel
        return GetAlquilerBase() * (1 + GetCantidadCasas() * 2);   // Casas 1 a 4
    }

    /**
     * @brief Metodo de conveniencia para consultar el nivel de construccion.
     * @return Cantidad de casas (0-4) o 5 para hotel.
     */
    public int NumeroCasas() => GetCantidadCasas();

    /**
     * @brief Comportamiento ejecutado cuando un jugador cae en esta propiedad.
     *
     * Si no tiene dueño, notifica al jugador la opcion de compra.
     * Si pertenece a otro jugador y no esta hipotecada, exige el pago de renta
     * mediante autorizacion de tarjeta RFID y descuenta el saldo correspondiente.
     * Si cae en bancarrota, notifica la eliminacion.
     * @param jugador Jugador que aterrizo en la casilla.
     */
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;

        if (!TienePropietario())
        {
            jugador.EnviarMensaje($"[PROPIEDAD] La propiedad '{GetNombre()}' esta disponible para compra por ${GetPrecioCompra()}.");
            if (jugador.GetDinero() >= GetPrecioCompra())
            {
                jugador.EnviarMensaje("[INFO] Usa el comando 'COMPRAR' si deseas adquirirla.");
            }
            else
            {
                jugador.EnviarMensaje($"[INFO] Saldo insuficiente (${jugador.GetDinero()}) para comprarla.");
            }
        }
        else if (GetPropietario() != jugador)
        {
            if (GetIsHipotecada())
            {
                jugador.EnviarMensaje($"[INFO] '{GetNombre()}' esta hipotecada. No pagas renta.");
                return;
            }

            int renta = CalcularRenta();
            jugador.EnviarMensaje($"[ALQUILER] Caiste en '{GetNombre()}' de {GetPropietario()!.GetNombre()}. Renta a pagar: ${renta}.");

            // RFID OBLIGATORIO: el jugador debe pasar su tarjeta para confirmar el pago de renta
            juego.AutorizarPagoRFID(jugador, renta, $"Renta a {GetPropietario()!.GetNombre()} por '{GetNombre()}'");
            new Transaccion(renta, juego.GetTurnoActual(), "Pago de alquiler", jugador, GetPropietario());

            GetPropietario()!.EnviarMensaje($"[PAGO RECIBIDO] Recibiste ${renta} de renta de {jugador.GetNombre()} por '{GetNombre()}'.");
            juego.Broadcast($"[AVISO] {jugador.GetNombre()} pago ${renta} de renta a {GetPropietario()!.GetNombre()} por {GetNombre()}.", jugador);
            juego.AnunciarDinero(jugador);
            juego.AnunciarDinero(GetPropietario()!);

            // Bancarrota si no le alcanzo el saldo
            if (jugador.GetDinero() < 0)
            {
                jugador.EnviarMensaje("[BANCARROTA] No tenias suficiente dinero para pagar la renta.");
                juego.Broadcast($"[BANCARROTA] {jugador.GetNombre()} ha caido en bancarrota.", jugador);
                juego.NotificarEliminacion(jugador);
            }
        }
        else
        {
            string casasInfo = GetCantidadCasas() == 5 ? "Hotel" : $"{GetCantidadCasas()} casas";
            jugador.EnviarMensaje($"[PROPIEDAD PROPIA] Estas en tu propiedad '{GetNombre()}' ({casasInfo}).");
        }
    }

    /**
     * @brief Representacion legible en cadena de la propiedad con sus detalles.
     * @return Descripcion completa con precio, renta, dueño y nivel de construccion.
     */
    public override string ToString()
    {
        string dueño = GetPropietario() != null ? GetPropietario()!.GetNombre() : "Sin dueño";
        string nivel = GetCantidadCasas() == 5 ? "Hotel" : $"{GetCantidadCasas()} casas";
        return $"[{GetPosicion()}] {GetNombre()} (Propiedad) - Precio: ${GetPrecioCompra()} | Renta: ${CalcularRenta()} | Dueño: {dueño} | {nivel}";
    }
}

/**
 * @class CasillaEvento
 * @brief Subclase de Casilla que extrae y ejecuta cartas de Fortuna o Arca Comunal.
 */
public class CasillaEvento : Casilla
{
    /** @brief Identificador del mazo correspondiente a la casilla. */
    private string tipoEvento;

    /**
     * @brief Obtiene la categoria del evento.
     * @return Cadena con el tipo de evento.
     */
    public string GetTipoEvento()
    {
        return this.tipoEvento;
    }

    /**
     * @brief Asigna la categoria del evento.
     * @param tipoEvento Nuevo tipo de evento.
     */
    public void SetTipoEvento(string tipoEvento)
    {
        this.tipoEvento = tipoEvento;
    }

    /**
     * @brief Constructor para casillas de evento.
     * @param posicion Indice numerico en el tablero.
     * @param nombre Nombre de la casilla (ej: "Arca Comunal" o "Fortuna").
     * @param tipo Categoria asignada.
     */
    public CasillaEvento(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
        this.tipoEvento = tipo;
    }

    /**
     * @brief Extrae una carta del mazo correspondiente, la muestra y aplica su efecto.
     * @param jugador Jugador que aterrizo en la casilla de evento.
     */
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;
        LinkedList mazo = (GetTipo() == "ArcaComunal" || GetNombre().Contains("Arca"))
            ? juego.GetMazoArcaComunal()
            : juego.GetMazoFortuna();

        CartaEvento carta = MazoCartas.RobarCarta(mazo);
        jugador.EnviarMensaje($"\n[CARTA DE {GetNombre().ToUpper()}]:");
        jugador.EnviarMensaje($"\"{carta.GetDescripcion()}\"\n");
        juego.Broadcast($"[AVISO] {jugador.GetNombre()} saco una carta de {GetNombre()}: \"{carta.GetDescripcion()}\"", jugador);

        AplicarEfectoCarta(carta, jugador, juego);
    }

    /**
     * @brief Interpreta y ejecuta las consecuencias especificadas en la carta robada.
     * @param carta Carta robada del mazo.
     * @param jugador Jugador que debe recibir las consecuencias.
     * @param juego Instancia global del controlador de la partida.
     */
    private void AplicarEfectoCarta(CartaEvento carta, Jugador jugador, JuegoMonopoly juego)
    {
        switch (carta.GetTipo())
        {
            case TipoEfectoCarta.GanarDinero:
                new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Ganancia por evento", jugador, null);
                jugador.EnviarMensaje($"[SALDO] Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PerderDinero:
                new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"[SALDO] Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PagarACadaJugador:
                Node? actualP = juego.GetJugadores().GetHead();
                for (int i = 0; i < juego.GetJugadores().Size(); i++)
                {
                    if (actualP?.GetData() is Jugador otro && otro.GetId() != jugador.GetId())
                    {
                        new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Pago entre jugadores", jugador, otro);
                        otro.EnviarMensaje($"[PAGO] {jugador.GetNombre()} te pago ${carta.GetMonto()} por evento.");
                    }
                    actualP = actualP?.GetNext();
                }
                jugador.EnviarMensaje($"[SALDO] Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.CobrarDeCadaJugador:
                Node? actualC = juego.GetJugadores().GetHead();
                for (int i = 0; i < juego.GetJugadores().Size(); i++)
                {
                    if (actualC?.GetData() is Jugador otro && otro.GetId() != jugador.GetId())
                    {
                        new Transaccion(carta.GetMonto(), juego.GetTurnoActual(), "Pago entre jugadores", otro, jugador);
                        otro.EnviarMensaje($"[PAGO] Pagaste ${carta.GetMonto()} a {jugador.GetNombre()} por evento.");
                    }
                    actualC = actualC?.GetNext();
                }
                jugador.EnviarMensaje($"[SALDO] Saldo actual: ${jugador.GetDinero()}");
                break;

            case TipoEfectoCarta.PerderDineroPorPropiedad:
                int costoProp = carta.GetMonto() * jugador.GetPropiedades().Size();
                new Transaccion(costoProp, juego.GetTurnoActual(), "Perdida por evento", jugador, null);
                jugador.EnviarMensaje($"[REPARACIONES] Pagaste ${costoProp} (${carta.GetMonto()} x {jugador.GetPropiedades().Size()} propiedades). Saldo: ${jugador.GetDinero()}");
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
                jugador.EnviarMensaje($"[REPARACIONES] Costo: ${costoConst} ({totalCasas} casas x ${carta.GetMontoPorCasa()}, {totalHoteles} hoteles x ${carta.GetMontoPorHotel()}). Saldo: ${jugador.GetDinero()}");
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
                jugador.EnviarMensaje($"[CARCEL] Guardas una carta para salir gratis de la carcel. Total cartas: {jugador.GetCartasSalirDeCarcel()}.");
                break;

            case TipoEfectoCarta.TomarOtraCarta:
                LinkedList mazoExtra = (carta.GetMazo() == 1) ? juego.GetMazoFortuna() : juego.GetMazoArcaComunal();
                CartaEvento cartaExtra = MazoCartas.RobarCarta(mazoExtra);
                jugador.EnviarMensaje($"[CARTA ADICIONAL] \"{cartaExtra.GetDescripcion()}\"");
                AplicarEfectoCarta(cartaExtra, jugador, juego);
                break;
        }

        if (carta.GetPierdeTurno())
        {
            jugador.SetPierdeSiguienteTurno(true);
            jugador.EnviarMensaje("[PENALIZACION] Pierdes tu siguiente turno.");
        }
    }
}

/**
 * @class CasillaEspecial
 * @brief Subclase de Casilla para espacios con reglas fijas (Salida, Impuesto, Carcel, etc.).
 */
public class CasillaEspecial : Casilla
{
    /**
     * @brief Constructor para casillas especiales no comprables.
     * @param posicion Indice numerico en el tablero.
     * @param nombre Nombre del espacio en el tablero.
     * @param tipo Regla aplicable ("Salida", "Impuesto", "Carcel", "VayaALaCarcel", "ParadaLibre").
     */
    public CasillaEspecial(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
    }

    /**
     * @brief Ejecuta la regla propia de la casilla especial segun su tipo.
     * @param jugador Jugador que aterrizo en la casilla especial.
     */
    public override void Accion(Jugador jugador)
    {
        var juego = JuegoMonopoly.Instancia;

        switch (GetTipo())
        {
            case "Salida":
                new Transaccion(200, juego.GetTurnoActual(), "Premio por pasar por inicio", jugador, null);
                jugador.EnviarMensaje("[SALIDA] Aterrizaste en Salida. Cobraste $200 de bono.");
                juego.Broadcast($"[AVISO] {jugador.GetNombre()} cayo en Salida y cobro $200.", jugador);
                break;

            case "Impuesto":
                int montoImpuesto = 100;
                // RFID OBLIGATORIO: el jugador debe pasar su tarjeta para pagar impuestos
                juego.AutorizarPagoRFID(jugador, montoImpuesto, "Impuesto sobre la renta");
                new Transaccion(montoImpuesto, juego.GetTurnoActual(), "Pago al banco", jugador, null);
                jugador.EnviarMensaje($"[IMPUESTO] Impuesto sobre la renta: Pagaste ${montoImpuesto} al banco. Saldo: ${jugador.GetDinero()}");
                juego.Broadcast($"[AVISO] {jugador.GetNombre()} pago ${montoImpuesto} de impuestos.", jugador);
                juego.AnunciarDinero(jugador);
                // Bancarrota si no le alcanzo el saldo
                if (jugador.GetDinero() < 0)
                {
                    jugador.EnviarMensaje("[BANCARROTA] No tenias suficiente dinero para el impuesto.");
                    juego.Broadcast($"[BANCARROTA] {jugador.GetNombre()} ha caido en bancarrota.", jugador);
                    juego.NotificarEliminacion(jugador);
                }
                break;

            case "Carcel":
                if (jugador.GetEnCarcel())
                {
                    jugador.EnviarMensaje($"[CARCEL] Estas cumpliendo condena en la Carcel (Turnos en espera: {jugador.GetTurnosEnCarcel()}/3).");
                }
                else
                {
                    jugador.EnviarMensaje("[CARCEL] Estas en la Carcel solo de visita. No hay penalizacion.");
                }
                break;

            case "VayaALaCarcel":
                jugador.EnviarMensaje("[CARCEL] Cometiste una infraccion. Vas directo a la Carcel sin cobrar Salida.");
                juego.EnviarACarcel(jugador);
                break;

            case "ParadaLibre":
                jugador.EnviarMensaje("[PARADA LIBRE] Descanso en Parada Libre. No hay cobros ni penalizaciones.");
                break;

            default:
                jugador.EnviarMensaje($"[CASILLA] Te encuentras en {GetNombre()}.");
                break;
        }
    }
}
