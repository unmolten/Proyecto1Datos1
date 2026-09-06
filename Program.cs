internal class Program
{
    private readonly LinkedList HeadLista = new LinkedList();

    public static void Main()
    {
        Program program = new Program();
        program.HeadLista.InsertFirst("Nodo1");
        int numNodes = 0;
        int num = 1;
        while (numNodes != 24)
        {
            program.HeadLista.InsertAfter(num);
            num++;
            numNodes++;
        }
    }
}

