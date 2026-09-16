// Representa un nodo de una lista circular doblemente enlazada.
// Guarda un dato, una referencia al siguiente nodo y una referencia al nodo anterior.
public class Node(Object data)
{
    // El dato puede ser cualquier objeto: texto, numeros u otra clase.
    private Object data = data;

    // Referencia al siguiente nodo en la lista circular.
    private Node? next;

    // Referencia al nodo anterior en la lista circular.
    private Node? previous;

    // Devuelve el dato almacenado en el nodo.
    public Object GetData()
    {
        return this.data;
    }

    // Reemplaza el dato almacenado en el nodo.
    public void SetData(object data)
    {
        this.data = data;
    }

    // Devuelve el siguiente nodo o null si no existe.
    public Node? GetNext()
    {
        return this.next;
    }

    // Define la referencia al siguiente nodo.
    public void SetNext(Node? node)
    {
        this.next = node;
    }

    // Devuelve el nodo anterior o null si no existe.
    public Node? GetPrevious()
    {
        return this.previous;
    }

    // Define la referencia al nodo anterior.
    public void SetPrevious(Node? node)
    {
        this.previous = node;
    }

    // Devuelve el nodo anterior (alias de GetPrevious).
    public Node? GetPrev()
    {
        return this.previous;
    }

    // Define la referencia al nodo anterior (alias de SetPrevious).
    public void SetPrev(Node? node)
    {
        this.previous = node;
    }
}