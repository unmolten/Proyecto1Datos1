using System;

// Clase Casilla:
// Representa cada uno de los espacios físicos que conforman el tablero de Monopoly.
// Cada objeto 'Casilla' se almacena dentro del campo 'data' de un 'Node' en la lista enlazada del tablero.
// Clase abstracta que hereda a las clases Propiedad, CasillaEvento y CasillaEspecial.
public abstract class Casilla
{
    // Posición o índice numérico en el tablero (de 0 a TotalCasillas - 1).
    public int Posicion { get; set; }

    // Nombre visible de la casilla (ej. "Salida (GO)", "Avenida Mediterráneo", "Cárcel").
    public string Nombre { get; set; }

    // Tipo de casilla que define su comportamiento dentro del juego:
    // - "Salida": Otorga bono de dinero al caer o pasar.
    // - "Propiedad": Puede ser comprada por un jugador y cobrar renta a los rivales.
    // - "Carcel": Zona de espera o visita.
    // - "Impuesto": Descuenta una tarifa fija al jugador.
    // - "ParadaLibre": Casilla neutral donde no ocurre penalización ni cobro.
    // - "VayaALaCarcel": Arresta al jugador y lo teletransporta al nodo de la Cárcel.
    // - "Fortuna": Otorga eventos aleatorios (premios, multas, saltos).
    // - "ArcaComunal": Otorga eventos aleatorios (premios, multas, saltos).
    public string Tipo { get; set; }

    // Constructor para inicializar una casilla con sus atributos básicos.
    public Casilla(int posicion, string nombre, string tipo)
    {
        this.Posicion = posicion;
        this.Nombre = nombre;
        this.Tipo = tipo;
    }

    // NOTA: la logica real de que pasa al caer en cada casilla vive centralizada
    // en JuegoMonopoly.ProcesarCasilla(), no aqui. La razon es que esa logica
    // necesita cosas que una Casilla sola no tiene a mano: la lista de TODOS
    // los jugadores (para cobrar renta a favor de otro jugador), los mazos de
    // CartaEvento, de que turno vamos, etc. Este metodo se deja disponible
    // por si mas adelante se prefiere mover logica especifica aqui.
    public virtual void Accion() { }
}

public class Propiedad : Casilla
{
    // Precio de compra de la casilla.
    public int PrecioCompra { get; set; }

    // Monto BASE de renta que debe pagar un jugador si cae en esta casilla y
    // pertenece a otro jugador (sin contar casas/hotel construidos encima).
    // NOTA: se conserva por compatibilidad, pero el calculo real ahora usa
    // la tabla de multiplicadores por color (ObtenerMultiplicador) o la
    // escala propia de ferrocarriles.
    public int AlquilerBase { get; set; }

    // Referencia al jugador que es dueño de esta propiedad.
    // Si es null, significa que la propiedad está disponible para ser comprada.
    public Jugador? Propietario { get; set; }

    // Verifica si la propiedad se encuentra hipotecada.
    public bool IsHipotecada { get; set; }

    // Grupo de color al que pertenece (para saber cuando un jugador tiene el
    // monopolio completo de un color, mas adelante).
    // Valor especial: "Ferrocarril" para las 4 estaciones.
    public string ColorGrupo { get; set; }

    // Cantidad de casas construidas: 0 a 4 = esa cantidad de casas, 5 = hotel
    // (reemplaza las 4 casas, igual que en el Monopoly de mesa).
    public int CantidadCasas { get; set; }

    public Propiedad(int posicion, string nombre, string tipo, int precioCompra, int alquilerBase, Jugador? propietario = null, string colorGrupo = "Gris")
    : base(posicion, nombre, tipo)
    {
        this.PrecioCompra = precioCompra;
        this.AlquilerBase = alquilerBase;
        this.Propietario = propietario;
        this.IsHipotecada = false;
        this.ColorGrupo = colorGrupo;
        this.CantidadCasas = 0;
    }

    // Verifica si la propiedad ya tiene un propietario asignado.
    public bool TienePropietario()
    {
        return this.Propietario != null;
    }

    // Devuelve true si esta casilla es un ferrocarril.
    // Los ferrocarriles se identifican por su grupo de color especial.
    public bool EsFerrocarril()
    {
        return this.ColorGrupo == "Ferrocarril";
    }

    // Devuelve el multiplicador segun el grupo de color y la "etapa":
    // 0 = sin monopolio, 1 = con monopolio, 2 = 1 casa, 3 = 2 casas,
    // 4 = 3 casas, 5 = 4 casas, 6 = hotel.
    // Tabla tomada del Excel (hoja "Multiplicadores").
    // NOTA: los ferrocarriles NO pasan por aqui; tienen su propia escala.
    private static double ObtenerMultiplicador(string colorGrupo, int etapa)
    {
        return (colorGrupo, etapa) switch
        {
            ("Marrón", 0) => 1,
            ("Marrón", 1) => 2,
            ("Marrón", 2) => 5,
            ("Marrón", 3) => 15,
            ("Marrón", 4) => 45,
            ("Marrón", 5) => 55,
            ("Marrón", 6) => 70,

            ("Celeste", 0) => 1,
            ("Celeste", 1) => 2,
            ("Celeste", 2) => 5,
            ("Celeste", 3) => 15,
            ("Celeste", 4) => 45,
            ("Celeste", 5) => 55,
            ("Celeste", 6) => 65,

            ("Morado", 0) => 1,
            ("Morado", 1) => 2,
            ("Morado", 2) => 5,
            ("Morado", 3) => 15,
            ("Morado", 4) => 45,
            ("Morado", 5) => 55,
            ("Morado", 6) => 62.5,

            ("Naranja", 0) => 1,
            ("Naranja", 1) => 2,
            ("Naranja", 2) => 5,
            ("Naranja", 3) => 14.2,
            ("Naranja", 4) => 39.2,
            ("Naranja", 5) => 53.5,
            ("Naranja", 6) => 67.8,

            ("Rojo", 0) => 1,
            ("Rojo", 1) => 2,
            ("Rojo", 2) => 5,
            ("Rojo", 3) => 15,
            ("Rojo", 4) => 37.5,
            ("Rojo", 5) => 46.2,
            ("Rojo", 6) => 55,

            ("Amarillo", 0) => 1,
            ("Amarillo", 1) => 2,
            ("Amarillo", 2) => 5,
            ("Amarillo", 3) => 13.6,
            ("Amarillo", 4) => 36.3,
            ("Amarillo", 5) => 45.4,
            ("Amarillo", 6) => 54.5,

            ("Verde", 0) => 1,
            ("Verde", 1) => 2,
            ("Verde", 2) => 5,
            ("Verde", 3) => 11.5,
            ("Verde", 4) => 34.6,
            ("Verde", 5) => 42.3,
            ("Verde", 6) => 50,

            ("Azul Oscuro", 0) => 1,
            ("Azul Oscuro", 1) => 2,
            ("Azul Oscuro", 2) => 4,
            ("Azul Oscuro", 3) => 12,
            ("Azul Oscuro", 4) => 28,
            ("Azul Oscuro", 5) => 34,
            ("Azul Oscuro", 6) => 40,

            _ => 1
        };
    }

    // Devuelve la renta de un ferrocarril segun cuantos ferrocarriles
    // tenga el mismo dueño. Escala clasica del Monopoly.
    // Se toma del Excel (hoja "Ferrocarriles").
    private static int CalcularRentaFerrocarril(int ferrocarrilesDelDueño)
    {
        return ferrocarrilesDelDueño switch
        {
            1 => 25,
            2 => 50,
            3 => 100,
            4 => 200,
            _ => 25
        };
    }

    // Calcula la renta actual a pagar.
    //
    // Caso 1 - Ferrocarril:
    //   La renta depende de cuantos ferrocarriles tenga el mismo dueño
    //   (25 / 50 / 100 / 200). El parametro 'ferrocarrilesDelDueño' indica
    //   ese conteo. 'tieneMonopolio' se ignora para ferrocarriles.
    //
    // Caso 2 - Propiedad de color:
    //   Se usa la tabla de multiplicadores segun el color y la etapa:
    //     0 = sin monopolio, 1 = con monopolio, 2..6 = 1..4 casas + hotel.
    //   Formula (igual al Excel):
    //     UnidadRenta = PrecioCompra / 10
    //     Renta = ROUND( (UnidadRenta * multiplicador) / 5 ) * 5
    public int CalcularRenta(bool tieneMonopolio = false, int ferrocarrilesDelDueño = 1)
    {
        // --- Caso especial: ferrocarril ---
        if (this.EsFerrocarril())
        {
            return CalcularRentaFerrocarril(ferrocarrilesDelDueño);
        }

        // --- Propiedades de color ---
        int etapa;
        if (this.CantidadCasas <= 0)
            etapa = tieneMonopolio ? 1 : 0;
        else
            etapa = 1 + this.CantidadCasas; // 1 casa -> 2 ... hotel (5) -> 6

        double factor = ObtenerMultiplicador(this.ColorGrupo, etapa);

        double unidadRenta = this.PrecioCompra / 10.0;
        double renta = Math.Round((unidadRenta * factor) / 5.0) * 5.0;

        return (int)renta;
    }

    // Representación en texto formateado de la casilla para mostrar en la consola.
    public override string ToString()
    {
        string dueño = this.Propietario != null ? this.Propietario.Nombre : "Sin dueño";

        if (this.EsFerrocarril())
        {
            // Para el ToString mostramos la renta asumiendo 1 ferrocarril.
            // Si quieres el valor real, calculalo desde JuegoMonopoly con
            // CalcularRenta(ferrocarrilesDelDueño: N).
            return $"[{this.Posicion}] {this.Nombre} (Ferrocarril) - Precio: ${this.PrecioCompra} | Renta (1 FC): ${this.CalcularRenta()} | Dueño: {dueño}";
        }

        return $"[{this.Posicion}] {this.Nombre} (Propiedad, {this.ColorGrupo}) - Precio: ${this.PrecioCompra} | Renta: ${this.CalcularRenta()} | Dueño: {dueño}";
    }
}

public class CasillaEvento : Casilla
{
    // El Tipo heredado ("Fortuna" o "ArcaComunal") ya indica de cual mazo
    // se debe robar, no hace falta un campo aparte.
    public CasillaEvento(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
    }

    public override string ToString()
    {
        return $"[{this.Posicion}] {this.Nombre} ({this.Tipo})";
    }
}

public class CasillaEspecial : Casilla
{
    // Monto fijo asociado a la casilla, solo tiene sentido cuando Tipo es
    // "Impuesto" (cuanto paga el jugador al caer aqui). 0 si no aplica.
    public int Monto { get; set; }

    public CasillaEspecial(int posicion, string nombre, string tipo) : base(posicion, nombre, tipo)
    {
        this.Monto = 0;
    }

    public override string ToString()
    {
        return $"[{this.Posicion}] {this.Nombre} ({this.Tipo})";
    }
}