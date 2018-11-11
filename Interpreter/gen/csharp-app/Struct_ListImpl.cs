public class ListImpl
{
    public int[] type;
    public int size;
    public int capacity;
    public Value[] array;

    public ListImpl(int[] type, int size, int capacity, Value[] array)
    {
        this.type = type;
        this.size = size;
        this.capacity = capacity;
        this.array = array;
    }
}
