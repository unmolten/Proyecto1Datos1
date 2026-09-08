class LinkedList
{
    private Node? head;
    private int size;
    private Node? tail;

    public LinkedList()
    {
        this.head = null;
        this.tail = null;
        this.size = 0;
    }

    public Boolean IsEmpty()
    {
        return this.head == null;
    }

    public int Size()
    {
        return this.size;
    }

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