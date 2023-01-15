using System.Collections;

namespace Org.NProlog.Core.Predicate;

public interface ICheckedEnumerator<T> : IEnumerator<T>
{
    bool CanMoveNext { get; }
    T Remove();
}

public class ListCheckedEnumerator<T> : ICheckedEnumerator<T>
{
    public static ListCheckedEnumerator<T> Of(List<T> list) => new(list);
    private readonly List<T> list;
    private int pos = -1;
    public ListCheckedEnumerator(List<T> list)
    {
        this.list = list;
    }

    public bool CanMoveNext => this.pos < this.list.Count - 1;

    public T Current => this.pos < 0 ? throw new InvalidOperationException("Call MoveNext() first") :
        this.pos >= list.Count ? throw new IndexOutOfRangeException(nameof(pos)) :
        this.list[this.pos];

    object IEnumerator.Current => this.Current;


    public void Dispose()
    {
        //
    }

    public bool MoveNext()
    {
        if (this.pos < this.list.Count - 1)
        {
            this.pos++;
            return true;
        }
        return false;
    }

    public void Reset() => this.pos = -1;

    public T Remove()
    {
        var value = this.list[this.pos];
        this.list.RemoveAt(this.pos);
        return value;
    }
}

public class ArrayCheckedEnumerator<T> : ICheckedEnumerator<T>
{
    public static ArrayCheckedEnumerator<T> Of(T[] list) => new(list);
    private readonly T[] array;
    private int pos = -1;
    public ArrayCheckedEnumerator(T[] array)
    {
        this.array = array;
    }

    public bool CanMoveNext => this.pos < this.array.Length - 1;

    public T Current => this.pos < 0 ? throw new InvalidOperationException("Call MoveNext() first") :
        this.pos >= array.Length ? throw new IndexOutOfRangeException(nameof(pos)) :
        this.array[this.pos];

    object IEnumerator.Current => this.Current;


    public void Dispose()
    {
        //
    }

    public bool MoveNext()
    {
        if (this.pos < this.array.Length - 1)
        {
            this.pos++;
            return true;
        }
        return false;
    }
    public void Reset() => this.pos = -1;

    public T Remove()
    {
        throw new InvalidOperationException("can not remove from array");
    }
}
