using System.Collections;

namespace Arbor.Lists;

public partial class WeakList<T> : IWeakList<T>, IEnumerable<T>
    where T : class
{
    private const int opportunistic_trim_threshold = 100;

    private readonly List<InvalidatableWeakReference> list = new List<InvalidatableWeakReference>();
    private int listStart;
    private int listEnd;

    private int countChangesSinceTrim;

    public void Add(T item)
        => add(new InvalidatableWeakReference(item));

    public void Add(WeakReference<T> weakReference)
        => add(new InvalidatableWeakReference(weakReference));

    private void add(in InvalidatableWeakReference item)
    {
        if (countChangesSinceTrim > opportunistic_trim_threshold)
            trim();

        if (listEnd < list.Count)
        {
            list[listEnd] = item;
            countChangesSinceTrim--;
        }
        else
        {
            list.Add(item);
            countChangesSinceTrim++;
        }

        listEnd++;
    }

    public bool Remove(T item)
    {
        var hashCode = EqualityComparer<T>.Default.GetHashCode(item);

        for (var i = listStart; i < listEnd; i++)
        {
            var reference = list[i].Reference;
            
            if (reference == null)
                continue;
            
            if (list[i].ObjectHashCode != hashCode)
                continue;
            
            if (!reference.TryGetTarget(out var target) || target != item)
                continue;

            RemoveAt(i - listStart);
            return true;
        }

        return false;
    }
    
    public bool Remove(WeakReference<T> weakReference)
    {
        for (var i = listStart; i < listEnd; i++)
        {
            if (list[i].Reference != weakReference)
                continue;
            
            RemoveAt(i - listStart);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        index += listStart;
        
        if (index < listStart || index >= listEnd)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        list[index] = default;
        
        if (index == listStart)
            listStart++;
        else if (index == listEnd - 1)
            listEnd--;
        
        countChangesSinceTrim++;
    }

    public bool Contains(T item)
    {
        var hashCode = EqualityComparer<T>.Default.GetHashCode(item);

        for (var i = listStart; i < listEnd; i++)
        {
            var reference = list[i].Reference;
            
            if (reference == null)
                continue;
            
            if (list[i].ObjectHashCode != hashCode)
                continue;
            
            if (!reference.TryGetTarget(out var target) || target != item)
                continue;

            return true;
        }

        return false;
    }
    
    public bool Contains(WeakReference<T> weakReference)
    {
        for (var i = listStart; i < listEnd; i++)
        {
            if (list[i].Reference != weakReference)
                continue;
            
            return true;
        }

        return false;
    }

    public void Clear()
    {
        listStart = listEnd = 0;
        countChangesSinceTrim = list.Count;
    }

    public ValidItemsEnumerator GetEnumerator()
    {
        trim();
        return new ValidItemsEnumerator(this);
    }
    
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    private void trim()
    {
        list.RemoveRange(listEnd, list.Count - listEnd);
        list.RemoveRange(0, listStart);

        list.RemoveAll(item => item.Reference == null || !item.Reference.TryGetTarget(out _));

        listStart = 0;
        listEnd = list.Count;
        countChangesSinceTrim = 0;
    }
    
    private readonly struct InvalidatableWeakReference
    {
        public readonly WeakReference<T>? Reference;

        public readonly int ObjectHashCode;

        public InvalidatableWeakReference(T reference)
        {
            Reference = new WeakReference<T>(reference);
            ObjectHashCode = EqualityComparer<T>.Default.GetHashCode(reference);
        }

        public InvalidatableWeakReference(WeakReference<T> reference)
        {
            Reference = reference;
            ObjectHashCode = !reference.TryGetTarget(out var target) ? 0 : EqualityComparer<T>.Default.GetHashCode(target);
        }
    }
}
