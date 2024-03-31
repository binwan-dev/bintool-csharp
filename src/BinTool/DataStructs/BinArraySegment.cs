namespace BinTool.DataStructs;

public struct BinArraySegment<T>
{
    private T[] _array;

    public BinArraySegment(T[] array):this(array,0,array.Length)
    {
    }

    public BinArraySegment(T[] array, int index, int count)
    {
        _array = array;
        Offest = index;
        Count = count;
    }
    
    public int Offest { get; set; } = 0;

    public int Count { get; set; } = 0;

    public T[] Array => _array;

    public T this[int index]
    {
        get
        {
            var realIndex = Offest + index;
            return _array[realIndex];
        }
        set
        {
            var realIndex = Offest + index;
            _array[realIndex] = value;
        }
    }

    public BinArraySegment<T> Slice(int index)
    {
        return Slice(index, _array.Length);
    }

    public BinArraySegment<T> Slice(int index, int count)
    {
        Offest += index;
        Count = count + Offest > _array.Length ? _array.Length - Offest : Offest + count;
        return new BinArraySegment<T>(_array, Offest, Count);
    }

    public T[] ToArray()
    {
        return _array.Skip(Offest).Take(Count).ToArray();
    }
}