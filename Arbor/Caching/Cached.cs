namespace Arbor.Caching;

public class Cached<T>
{
    private T value = default!;

    public T Value
    {
        get
        {
            if (!IsValid)
                throw new InvalidOperationException($"Cannot query {nameof(value)} of an invalid {nameof(Cached<T>)}");

            return value;
        }
        set
        {
            this.value = value;
            IsValid = true;
        }
    }

    public bool IsValid { get; private set; }

    public bool Invalidate()
    {
        if (!IsValid)
            return false;

        IsValid = false;
        return true;
    }
}

public class Cached
{
    public bool IsValid { get; private set; }

    public bool Invalidate()
    {
        if (!IsValid)
            return false;

        IsValid = false;
        return true;
    }

    public void Validate()
    {
        IsValid = true;
    }
}