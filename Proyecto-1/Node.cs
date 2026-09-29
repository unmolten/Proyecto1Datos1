using System;

/**
 * @file Node.cs
 * @brief Implementacion del nodo para la lista circular doblemente enlazada.
 *
 * Cada nodo almacena un objeto generico de datos y dos punteros o referencias:
 * uno hacia el nodo siguiente y otro hacia el nodo anterior en la secuencia circular.
 */

/**
 * @class Node
 * @brief Clase que representa un elemento o vertice dentro de una estructura enlazada.
 *
 * Facilita la navegacion bidireccional requerida para el recorrido del tablero
 * y la gestion de listas dinamicas como las cartas o el inventario del jugador.
 */
public class Node(Object data)
{
    /** @brief Objeto o valor contenido dentro del nodo. */
    private Object data = data;

    /** @brief Referencia al siguiente nodo en la lista. */
    private Node? next;

    /** @brief Referencia al nodo previo en la lista. */
    private Node? previous;

    /**
     * @brief Obtiene el dato almacenado en el nodo.
     * @return El objeto guardado.
     */
    public Object GetData()
    {
        return this.data;
    }

    /**
     * @brief Asigna o reemplaza el dato almacenado en el nodo.
     * @param data Nuevo objeto a almacenar.
     */
    public void SetData(object data)
    {
        this.data = data;
    }

    /**
     * @brief Obtiene el nodo siguiente en la lista.
     * @return El nodo siguiente, o null si no esta definido.
     */
    public Node? GetNext()
    {
        return this.next;
    }

    /**
     * @brief Establece la referencia al siguiente nodo.
     * @param node Nodo que sucedera al actual.
     */
    public void SetNext(Node? node)
    {
        this.next = node;
    }

    /**
     * @brief Obtiene el nodo anterior en la lista.
     * @return El nodo previo, o null si no esta definido.
     */
    public Node? GetPrevious()
    {
        return this.previous;
    }

    /**
     * @brief Establece la referencia al nodo anterior.
     * @param node Nodo que antecedera al actual.
     */
    public void SetPrevious(Node? node)
    {
        this.previous = node;
    }

    /**
     * @brief Metodo alternativo (alias) para obtener el nodo anterior.
     * @return El nodo previo.
     */
    public Node? GetPrev()
    {
        return this.previous;
    }

    /**
     * @brief Metodo alternativo (alias) para establecer el nodo anterior.
     * @param node Nodo previo.
     */
    public void SetPrev(Node? node)
    {
        this.previous = node;
    }
}