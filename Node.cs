public class Node(Object data)
{
    private Object data = data;
    private Node? next;
    public Object GetData()
    {
        return this.data;
    }
    public void SetData(object data)
    {
        this.data = data;
    }
    public Node? GetNext()
    {
        return this.next;
    }
    public void SetNext(Node? node)
    {
        this.next = node;
    }
}