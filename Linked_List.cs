class LinkedList
{
    private Node? head;
    private int size;
    int nums = 1;

    public LinkedList()
    {
        this.head = null;
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

    public void InsertFirst(Object data)
    {
        Node newNode = new Node(data);
        newNode.SetNext(this.head);
        this.head = newNode;
        this.size++;
        Console.WriteLine(newNode.GetNext());
        Console.WriteLine("Nodo Insertado!!!");
    }

    public Node? DeleteFirst()
    {
        if (this.head != null)
        {
            Node temp = this.head;
            this.head = this.head.GetNext();
            this.size--;
            return temp;
        }
        else
        {
            return null;
        }
    }
    public void InsertAfter(Object data)
    {
        if (this.head == null)
        {
            InsertFirst(data);
            return;
        }
        Node newNode = new Node(data);
        newNode.SetNext(this.head.GetNext());
        this.size++;
        Console.WriteLine("Nodo ", nums, " insertado!!");
        nums++;
    }
}