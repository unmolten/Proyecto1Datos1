using System;

/**
 * @file Linked_List.cs
 * @brief Implementacion de una lista circular doblemente enlazada.
 *
 * Esta estructura de datos es el pilar fundamental del proyecto. Se utiliza para:
 * 1. Modelar el tablero de Monopoly donde la ultima casilla se enlaza con la primera.
 * 2. Gestionar la rotacion de los mazos de cartas (Fortuna y Arca Comunal).
 * 3. Mantener el registro dinamico de jugadores y el inventario de propiedades de cada uno.
 */

/**
 * @class LinkedList
 * @brief Estructura de lista circular doblemente enlazada.
 *
 * Mantiene referencias al primer elemento (head), al ultimo elemento (tail)
 * y un contador entero con la cantidad de nodos (size). En una configuracion circular,
 * head.GetPrevious() apunta a tail y tail.GetNext() apunta a head.
 */
public class LinkedList
{
    /** @brief Referencia al primer nodo de la lista. */
    private Node? head;

    /** @brief Cantidad total de elementos almacenados en la lista. */
    private int size;

    /** @brief Referencia al ultimo nodo de la lista. */
    private Node? tail;

    /**
     * @brief Constructor por defecto. Inicializa una lista vacia.
     */
    public LinkedList()
    {
        this.head = null;
        this.tail = null;
        this.size = 0;
    }

    /**
     * @brief Determina si la lista carece de elementos.
     * @return true si la lista esta vacia (head es null); de lo contrario, false.
     */
    public Boolean IsEmpty()
    {
        return this.head == null;
    }

    /**
     * @brief Devuelve la cantidad de nodos presentes en la lista.
     * @return Entero con el tamaño actual de la lista.
     */
    public int Size()
    {
        return this.size;
    }

    /**
     * @brief Obtiene el primer nodo de la lista (cabeza).
     * @return Nodo inicial o null si la lista esta vacia.
     */
    public Node? GetHead()
    {
        return this.head;
    }

    /**
     * @brief Obtiene el ultimo nodo de la lista (cola).
     * @return Nodo final o null si la lista esta vacia.
     */
    public Node? GetTail()
    {
        return this.tail;
    }

    /**
     * @brief Recorre la lista para actualizar la referencia del nodo cola (tail).
     *
     * Utilizado para recalcular el final de la lista si los enlaces sufrieron modificaciones.
     */
    private void UpdateTail()
    {
        if (this.head == null)
        {
            this.tail = null;
            return;
        }

        Node current = this.head;
        int visited = 0;

        while (current.GetNext() != null && current.GetNext() != this.head && visited < this.size)
        {
            current = current.GetNext()!;
            visited++;
        }

        this.tail = current;
    }

    /**
     * @brief Inserta un nuevo nodo al inicio de la lista.
     *
     * Ajusta los enlaces anterior y siguiente para mantener la circularidad doble.
     * El nuevo nodo pasa a ser el nuevo head.
     * @param data Informacion u objeto a almacenar.
     */
    public void InsertFirst(Object data)
    {
        Node newNode = new Node(data);

        // Si la lista estaba vacia, el nodo se apunta a si mismo
        if (this.head == null)
        {
            newNode.SetNext(newNode);
            newNode.SetPrevious(newNode);
            this.head = newNode;
            this.tail = newNode;
        }
        else
        {
            // El nuevo nodo se intercala entre el tail y el antiguo head
            newNode.SetNext(this.head);
            newNode.SetPrevious(this.tail);
            this.tail!.SetNext(newNode);
            this.head.SetPrevious(newNode);
            this.head = newNode;
        }

        this.size++;
        Console.WriteLine("Nodo insertado al inicio: " + newNode.GetData());
    }

    /**
     * @brief Inserta un nuevo elemento inmediatamente despues de la cabeza (head).
     *
     * Si la lista esta vacia, el elemento se inserta como el primer nodo.
     * @param data Informacion u objeto a almacenar.
     */
    public void InsertAfterHead(Object data)
    {
        if (this.head == null)
        {
            InsertFirst(data);
            return;
        }

        Node nextAfterHead = this.head.GetNext()!;
        Node newNode = new Node(data);

        newNode.SetNext(nextAfterHead);
        newNode.SetPrevious(this.head);
        this.head.SetNext(newNode);
        nextAfterHead.SetPrevious(newNode);

        if (this.head == this.tail)
        {
            this.tail = newNode;
        }

        this.size++;
        Console.WriteLine("Nodo insertado despues del head: " + newNode.GetData());
    }

    /**
     * @brief Inserta un nuevo nodo al final de la lista.
     *
     * Mantiene los enlaces bidireccionales y circulares. El nuevo nodo
     * se convierte en el nuevo tail y apunta de vuelta al head.
     * @param data Informacion u objeto a almacenar.
     */
    public void InsertEnd(Object data)
    {
        Node newNode = new Node(data);

        if (this.head == null)
        {
            newNode.SetNext(newNode);
            newNode.SetPrevious(newNode);
            this.head = newNode;
            this.tail = newNode;
            this.size++;
            return;
        }

        newNode.SetNext(this.head);
        newNode.SetPrevious(this.tail);
        this.tail!.SetNext(newNode);
        this.head.SetPrevious(newNode);
        this.tail = newNode;
        this.size++;
    }

    /**
     * @brief Elimina y extrae el primer nodo (head) de la lista.
     * @return El nodo extraido, o null si la lista se encuentra vacia.
     */
    public Node? DeleteFirst()
    {
        if (this.head == null)
        {
            return null;
        }

        Node temp = this.head;

        // Si solo habia un nodo en la lista
        if (this.head == this.tail)
        {
            this.head = null;
            this.tail = null;
            this.size = 0;
        }
        else
        {
            this.head = this.head.GetNext();
            this.head!.SetPrevious(this.tail);
            this.tail!.SetNext(this.head);
            this.size--;
        }

        // Se limpian los punteros del nodo extraido
        temp.SetNext(null);
        temp.SetPrevious(null);
        return temp;
    }

    /**
     * @brief Elimina y extrae el ultimo nodo (tail) de la lista.
     * @return El nodo extraido, o null si la lista se encuentra vacia.
     */
    public Node? DeleteLast()
    {
        if (this.head == null)
        {
            return null;
        }

        Node temp = this.tail!;

        // Caso con un unico elemento
        if (this.head == this.tail)
        {
            this.head = null;
            this.tail = null;
            this.size = 0;
        }
        else
        {
            this.tail = this.tail!.GetPrevious();
            this.tail!.SetNext(this.head);
            this.head.SetPrevious(this.tail);
            this.size--;
        }

        temp.SetNext(null);
        temp.SetPrevious(null);
        return temp;
    }

    /**
     * @brief Busca y elimina la primera aparicion de un objeto en la lista.
     * @param data Objeto a buscar y remover.
     * @return true si el nodo fue encontrado y eliminado con exito; false si no existe.
     */
    public bool Delete(Object data)
    {
        if (this.head == null)
        {
            return false;
        }

        // Si el elemento coincide con la cabeza
        if (object.Equals(this.head.GetData(), data))
        {
            DeleteFirst();
            return true;
        }

        // Si el elemento coincide con la cola
        if (object.Equals(this.tail!.GetData(), data))
        {
            DeleteLast();
            return true;
        }

        // Recorrido por los nodos intermedios
        Node current = this.head.GetNext()!;
        int visited = 1;

        while (current != this.head && visited < this.size)
        {
            if (object.Equals(current.GetData(), data))
            {
                Node prev = current.GetPrevious()!;
                Node next = current.GetNext()!;

                // Re-enlazar nodos vecinos salteando el actual
                prev.SetNext(next);
                next.SetPrevious(prev);

                current.SetNext(null);
                current.SetPrevious(null);

                this.size--;
                return true;
            }

            current = current.GetNext()!;
            visited++;
        }

        return false;
    }

    /**
     * @brief Metodo de conveniencia (alias) equivalente a Delete.
     * @param data Objeto a remover.
     * @return true si se removio correctamente; false en caso contrario.
     */
    public bool Remove(Object data)
    {
        return Delete(data);
    }

    /**
     * @brief Imprime en la consola los elementos de la lista enlazada hasta completar un ciclo.
     */
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

            Console.Write(current.GetData() + " <-> ");
            current = current.GetNext();
        }

        Console.WriteLine("(circular: head)");
    }

    /**
     * @brief Conecta explicitamente el nodo cola con el nodo cabeza de forma bidireccional.
     *
     * Asegura la propiedad de circularidad tras operaciones manuales o de barajado.
     */
    public void MakeCircular()
    {
        if (this.head != null && this.tail != null)
        {
            this.tail.SetNext(this.head);
            this.head.SetPrevious(this.tail);
        }
    }

    /**
     * @brief Recorre la lista circular hacia adelante e imprime exactamente 'size' elementos.
     */
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

    /**
     * @brief Recorre la lista circular en sentido inverso (hacia atras) usando GetPrevious().
     */
    public void PrintCircularReverse()
    {
        if (this.tail == null)
        {
            Console.WriteLine("Lista vacia");
            return;
        }

        Node current = this.tail;
        int visited = 0;

        do
        {
            Console.WriteLine(current.GetData());
            current = current.GetPrevious() ?? this.tail;
            visited++;
        } while (visited < this.size);
    }

    /**
     * @brief Obtiene el dato almacenado en un indice de posicion (indexado desde 0).
     * @param position Indice entero del nodo deseado (0 <= position < size).
     * @return El objeto almacenado en la posicion solicitada.
     * @throws InvalidOperationException Si la lista esta vacia.
     * @throws ArgumentOutOfRangeException Si el indice esta fuera del rango valido.
     */
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

    /**
     * @brief Obtiene la referencia al nodo ubicado en una posicion especifica.
     * @param position Indice basado en cero.
     * @return El nodo ubicado en esa posicion.
     * @throws InvalidOperationException Si la lista esta vacia o corrupta.
     * @throws ArgumentOutOfRangeException Si el indice es negativo o mayor al tamaño.
     */
    public Node GetNodeAt(int position)
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
            current = current.GetNext()
                ?? throw new InvalidOperationException("La lista no tiene suficientes nodos");
        }

        return current;
    }
}