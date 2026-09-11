using System;

// Clase Node (Nodo):
// Representa el elemento fundamental de una lista enlazada (Linked List).
// Cada nodo contiene un dato genérico (en nuestro caso, puede ser una Casilla de Monopoly)
// y una referencia o puntero hacia el siguiente nodo ('next') en la secuencia.
public class Node(Object data)
{
    // Dato o contenido almacenado dentro del nodo (ej. objeto Casilla)
    private Object data = data;

    // Referencia al siguiente nodo de la lista. Si es null, indica el final de una lista lineal.
    // En una lista circular, el último nodo apunta nuevamente a la cabeza (head).
    private Node? next;

    // Retorna el dato contenido en este nodo.
    public Object GetData()
    {
        return this.data;
    }

    // Permite modificar o actualizar el dato contenido en este nodo.
    public void SetData(object data)
    {
        this.data = data;
    }

    // Obtiene la referencia al siguiente nodo conectado.
    public Node? GetNext()
    {
        return this.next;
    }

    // Establece el enlace hacia el siguiente nodo.
    public void SetNext(Node? node)
    {
        this.next = node;
    }
}