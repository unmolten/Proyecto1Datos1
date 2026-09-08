internal class Program
{
    private readonly LinkedList HeadLista = new LinkedList();

    public static void Main()
    {
        Program program = new Program();

        program.HeadLista.InsertFirst("Head");

        for (int i = 1; i <= 24; i++)
        {
            program.HeadLista.InsertAfterHead(i);
        }
        program.HeadLista.MakeCircular();
        program.HeadLista.PrintCircular();
    }
}

