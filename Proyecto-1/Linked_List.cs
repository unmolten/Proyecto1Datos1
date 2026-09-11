using System;

// Clase LinkedList (Lista Enlazada Simple):
// Estructura de datos dinámica compuesta por nodos enlazados secuencialmente.
// En este proyecto se utiliza de dos formas principales:
// 1. Como lista circular para el TABLERO de Monopoly (donde el último nodo apunta al primero).
// 2. Como lista lineal para el INVENTARIO de propiedades de cada jugador.
public class LinkedList
{
    // Puntero al primer nodo de la lista (en Monopoly, representa la casilla de Salida/GO)
    private Node? head;

    // Cantidad total de elementos almacenados en la lista
    private int size;

    // Puntero al último nodo de la lista (en Monopoly, la casilla previa a Salida)
    private Node? tail;

    // Retorna el primer nodo (cabeza) de la lista enlazada.
    // Útil para iniciar recorridos o posicionar a los jugadores en la Salida.
    public Node? GetHead()
    {
        return this.head;
    }

    // Retorna el último nodo (cola) de la lista enlazada.
    public Node? GetTail()
    {
        return this.tail;
    }

    // Constructor: Inicializa una lista enlazada vacía.
    public LinkedList()
    {
        this.head = null;
        this.tail = null;
        this.size = 0;
    }

    // Verifica si la lista no contiene ningún nodo.
    // Retorna true si está vacía, false en caso contrario.
    public Boolean IsEmpty()
    {
        return this.head == null;
    }

    // Retorna el tamaño actual (número de nodos) de la lista.
    public int Size()
    {
        return this.size;
    }

    // Método privado auxiliar para recalcular y actualizar la referencia de 'tail'
    // recorriendo la lista desde la cabeza hasta el último elemento disponible.
    private void UpdateTail()
    {
        if (this.head == null)
        {
            this.tail = null;
            return;
        }

        Node? current = this.head;
        this.tail = this.head;

        // Avanza mientras exista un nodo siguiente
        while (current != null && current.GetNext() != null)
        {
            current = current.GetNext();
            this.tail = current;
        }
    }

    // Inserta un nuevo nodo al inicio de la lista (en la cabeza).
    // Complejidad temporal: O(1).
    public void InsertFirst(Object data)
    {
        Node newNode = new Node(data);

        // El nuevo nodo ahora apunta a la antigua cabeza
        newNode.SetNext(this.head);
        this.head = newNode;
        this.size++;

        // Si la lista estaba vacía, la cola también es este nuevo nodo
        if (this.tail == null)
        {
            this.tail = this.head;
        }

        UpdateTail();
        Console.WriteLine("Nodo insertado al inicio: " + newNode.GetData());
    }

    // Inserta un nodo inmediatamente después del primer elemento (después de head).
    public void InsertAfterHead(Object data)
    {
        if (this.head == null)
        {
            InsertFirst(data);
            return;
        }

        Node? nextAfterHead = this.head.GetNext();
        Node newNode = new Node(data);

        // Conecta el nuevo nodo con el que le seguía a head
        newNode.SetNext(nextAfterHead);

        // Conecta head con el nuevo nodo
        this.head.SetNext(newNode);
        this.size++;
        UpdateTail();
        Console.WriteLine("Nodo insertado despues del head: " + newNode.GetData());
    }

    // Inserta un nuevo nodo al final de la lista.
    // En Monopoly, se usa para ir construyendo las casillas del tablero en orden (0, 1, 2, ...),
    // y también para agregar una propiedad comprada al inventario del jugador.
    // Complejidad temporal: O(1) gracias al puntero 'tail'.
    public void InsertEnd(Object data)
    {
        Node newNode = new Node(data);

        // Caso 1: La lista está vacía
        if (this.head == null)
        {
            this.head = newNode;
            this.tail = newNode;
            this.size++;
            return;
        }

        // Caso 2: Ya existen nodos en la lista
        if (this.tail != null)
        {
            this.tail.SetNext(newNode);
        }

        this.tail = newNode;
        this.size++;
    }

    // Elimina el primer nodo (head) de la lista y lo retorna.
    // Si la lista está vacía, retorna null.
    public Node? DeleteFirst()
    {
        if (this.head != null)
        {
            Node temp = this.head;
            this.head = this.head.GetNext();
            this.size--;

            if (this.size == 0)
            {
                this.tail = null;
            }
            else
            {
                UpdateTail();
            }

            return temp;
        }
        else
        {
            return null;
        }
    }

    // Imprime en consola todos los elementos de la lista linealmente.
    // Limita la iteración al tamaño actual para prevenir bucles infinitos si la lista es circular.
    public void PrintList()
    {
        if (this.head == null)
        {
            Console.WriteLine("Lista vacia");
            return;
        }

        Node? current = this.head;
        Console.Write("Lista: ");

        for (int i = 0; i < this.size; i++)
        {
            if (current == null)
            {
                break;
            }

            Console.Write(current.GetData() + " -> ");
            current = current.GetNext();
        }

        Console.WriteLine("null");
    }

    // CONVERSIÓN A LISTA CIRCULAR (CLAVE PARA MONOPOLY):
    // Conecta el puntero 'next' del último nodo (tail) de regreso al primer nodo (head).
    // De esta manera, cuando los jugadores avanzan casillas con los dados y superan
    // la última casilla, continúan automáticamente en la casilla de Salida (GO) sin salirse del rango.
    public void MakeCircular()
    {
        if (this.head != null)
        {
            UpdateTail();
            if (this.tail != null)
            {
                // El enlace circular: Cola -> Cabeza
                this.tail.SetNext(this.head);
            }
        }
    }

    // Imprime el contenido de una lista circular dando exactamente una vuelta completa (size pasos).
    public void PrintCircular()
    {
        if (this.head == null)
        {
            Console.WriteLine("Lista vacia");
            return;
        }

        Node current = this.head;
        int visited = 0;

        do
        {
            Console.WriteLine(current.GetData());
            current = current.GetNext() ?? this.head;
            visited++;
        } while (visited < this.size);
    }

    // Obtiene el dato almacenado en un nodo en una posición específica (índice base 0).
    // Lanza excepciones si el índice está fuera de rango o si la lista está vacía.
    public Object GetDataNode(int position)
    {
        if (this.head == null)
        {
            throw new InvalidOperationException("La lista esta vacia");
        }

        if (position < 0 || position >= this.size)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        Node current = this.head;

        for (int i = 0; i < position; i++)
        {
            current = current.GetNext() ?? throw new InvalidOperationException("La lista no tiene suficientes nodos");
        }

        return current.GetData();
    }
}