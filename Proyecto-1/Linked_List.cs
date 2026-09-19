// Lista circular doblemente enlazada que mantiene referencias al primer nodo (head),
// al ultimo nodo (tail) y a la cantidad de elementos (size).
// Cada nodo apunta tanto a su siguiente como a su anterior de forma circular.
public class LinkedList
{
    private Node? head;
    private int size;
    private Node? tail;

    // Crea una lista circular vacia.
    public LinkedList()
    {
        this.head = null;
        this.tail = null;
        this.size = 0;
    }

    // Indica si la lista no contiene nodos.
    public Boolean IsEmpty()
    {
        return this.head == null;
    }

    // Devuelve la cantidad actual de nodos.
    public int Size()
    {
        return this.size;
    }

    // Devuelve la referencia al primer nodo (head).
    public Node? GetHead()
    {
        return this.head;
    }

    // Devuelve la referencia al ultimo nodo (tail).
    public Node? GetTail()
    {
        return this.tail;
    }

    // Actualiza tail recorriendo la lista si fuera necesario.
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

    // Inserta un nodo al principio de la lista manteniendo la circularidad doble.
    public void InsertFirst(Object data)
    {
        Node newNode = new Node(data);

        if (this.head == null)
        {
            newNode.SetNext(newNode);
            newNode.SetPrevious(newNode);
            this.head = newNode;
            this.tail = newNode;
        }
        else
        {
            newNode.SetNext(this.head);
            newNode.SetPrevious(this.tail);
            this.tail!.SetNext(newNode);
            this.head.SetPrevious(newNode);
            this.head = newNode;
        }

        this.size++;
        Console.WriteLine("Nodo insertado al inicio: " + newNode.GetData());
    }

    // Inserta un nodo inmediatamente despues de head.
    // Si la lista esta vacia, el dato se convierte en el primer nodo.
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

    // Inserta un nodo al final de la lista manteniendo la circularidad doble.
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

    // Elimina y devuelve el primer nodo. Devuelve null si la lista esta vacia.
    public Node? DeleteFirst()
    {
        if (this.head == null)
        {
            return null;
        }

        Node temp = this.head;

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

        temp.SetNext(null);
        temp.SetPrevious(null);
        return temp;
    }

    // Elimina y devuelve el ultimo nodo. Devuelve null si la lista esta vacia.
    public Node? DeleteLast()
    {
        if (this.head == null)
        {
            return null;
        }

        Node temp = this.tail!;

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

    // Elimina la primera aparicion del dato especificado en la lista circular.
    // Devuelve true si el elemento fue encontrado y eliminado; false en caso contrario.
    public bool Delete(Object data)
    {
        if (this.head == null)
        {
            return false;
        }

        // Si el dato esta en la cabeza
        if (object.Equals(this.head.GetData(), data))
        {
            DeleteFirst();
            return true;
        }

        // Si el dato esta en la cola
        if (object.Equals(this.tail!.GetData(), data))
        {
            DeleteLast();
            return true;
        }

        // Busca en los nodos intermedios
        Node current = this.head.GetNext()!;
        int visited = 1;

        while (current != this.head && visited < this.size)
        {
            if (object.Equals(current.GetData(), data))
            {
                Node prev = current.GetPrevious()!;
                Node next = current.GetNext()!;

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

    // Elimina la primera aparicion del dato especificado (alias de Delete).
    public bool Remove(Object data)
    {
        return Delete(data);
    }

    // Imprime la lista mostrando los enlaces bidireccionales hasta completar una vuelta.
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

    // Asegura que tail y head se encuentren conectados bidireccionalmente.
    public void MakeCircular()
    {
        if (this.head != null && this.tail != null)
        {
            this.tail.SetNext(this.head);
            this.head.SetPrevious(this.tail);
        }
    }

    // Imprime exactamente size nodos para recorrer el ciclo hacia adelante una vez.
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

    // Imprime exactamente size nodos en sentido contrario usando los enlaces previos.
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

    // Devuelve el dato ubicado en una posicion basada en cero.
    // Por ejemplo, la posicion 4 corresponde al quinto nodo.
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

    // Devuelve el nodo ubicado en una posicion basada en cero.
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