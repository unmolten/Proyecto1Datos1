/* Lista enlazada que mantiene referencias al primer nodo (head) y al ultimo
nodo (tail). Tambien puede convertirse en una lista circular. */
class LinkedList
{
    private Node? head;
    private int size;
    private Node? tail;

    // Crea una lista vacia.
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

    /* Recalcula tail recorriendo la lista. Se usa cuando se modifica el enlace
    interno y la lista todavia no esta circular. */
    private void UpdateTail()
    {
        if (this.head == null)
        {
            this.tail = null;
            return;
        }

        Node? current = this.head;
        this.tail = this.head;

        while (current != null && current.GetNext() != null)
        {
            current = current.GetNext();
            this.tail = current;
        }
    }

    // Inserta un nodo al principio de la lista.
    public void InsertFirst(Object data)
    {
        Node newNode = new Node(data);
        newNode.SetNext(this.head);
        this.head = newNode;
        this.size++;

        if (this.tail == null)
        {
            this.tail = this.head;
        }

        UpdateTail();
        Console.WriteLine("Nodo insertado al inicio: " + newNode.GetData());
    }

    /* Inserta un nodo inmediatamente despues de head.
     Si la lista esta vacia, el dato se convierte en el primer nodo. */
    public void InsertAfterHead(Object data)
    {
        if (this.head == null)
        {
            InsertFirst(data);
            return;
        }

        Node? nextAfterHead = this.head.GetNext();
        Node newNode = new Node(data);
        newNode.SetNext(nextAfterHead);
        this.head.SetNext(newNode);
        this.size++;
        UpdateTail();
        Console.WriteLine("Nodo insertado despues del head: " + newNode.GetData());
    }

    // Inserta un nodo al final de la lista.
    public void InsertEnd(Object data)
    {
        Node newNode = new Node(data);

        if (this.head == null)
        {
            this.head = newNode;
            this.tail = newNode;
            this.size++;
            return;
        }

        if (this.tail != null)
        {
            this.tail.SetNext(newNode);
        }

        this.tail = newNode;
        this.size++;
    }

    /// <summary>
    /// Elimina y devuelve el primer nodo. Devuelve null si la lista esta vacia.
    /// </summary>
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

    /* Imprime una lista lineal. El recorrido usa size para evitar depender de
    /// null cuando la lista fue convertida en circular.*/
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

    // Hace que tail apunte nuevamente a head, formando una lista circular.
    public void MakeCircular()
    {
        if (this.head != null)
        {
            UpdateTail();
            if (this.tail != null)
            {
                this.tail.SetNext(this.head);
            }
        }
    }

    // Imprime exactamente size nodos para evitar un ciclo infinito.
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

    /* Devuelve el dato ubicado en una posicion basada en cero.
    Por ejemplo, la posicion 4 corresponde al quinto nodo.*/
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