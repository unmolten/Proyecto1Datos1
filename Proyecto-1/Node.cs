// Representa un nodo de la lista enlazada.
// Guarda un dato y una referencia al siguiente nodo.
public class Node(Object data)
{
    // El dato puede ser cualquier objeto: texto, numeros u otra clase.
    private Object data = data;

    // Puede ser null cuando el nodo es el ultimo de una lista no circular.
    private Node? next;

    // Devuelve el dato almacenado en el nodo.</summary>
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
}