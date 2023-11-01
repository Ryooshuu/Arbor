namespace Arbor.Graphics.Textures;

internal class TextureWithRefCount : Texture
{
    private readonly ReferenceCount count;

    public TextureWithRefCount(Texture parent, ReferenceCount count)
        : base(parent)
    {
        this.count = count;
        
        count.Increment();
    }

    ~TextureWithRefCount()
    {
        Dispose(false);
    }
    
    public bool IsDisposed { get; private set; }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        
        if (IsDisposed)
            return;

        IsDisposed = true;
        count.Decrement();
    }

    public class ReferenceCount
    {
        private readonly object lockObject;
        private readonly Action? onAllReferencesLost;

        private int referenceCount;

        public ReferenceCount(object lockObject, Action onAllReferencesLost)
        {
            this.lockObject = lockObject;
            this.onAllReferencesLost = onAllReferencesLost;
        }

        public void Increment()
        {
            lock (lockObject)
                Interlocked.Increment(ref referenceCount);
        }
        
        public void Decrement()
        {
            lock (lockObject)
            {
                if (Interlocked.Decrement(ref referenceCount) == 0)
                    onAllReferencesLost?.Invoke();
            }
        }
    }
}
