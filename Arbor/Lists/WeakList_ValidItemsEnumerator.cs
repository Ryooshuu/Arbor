using System.Collections;

namespace Arbor.Lists;

public partial class WeakList<T>
{
    public struct ValidItemsEnumerator : IEnumerator<T>
    {
        private readonly WeakList<T> weakList;
        private int currentItemIndex;

        internal ValidItemsEnumerator(WeakList<T> weakList)
        {
            this.weakList = weakList;

            currentItemIndex = weakList.listStart - 1;
            Current = default!;
        }

        public bool MoveNext()
        {
            while (true)
            {
                ++currentItemIndex;

                if (currentItemIndex >= weakList.listEnd)
                    return false;

                var weakReference = weakList.list[currentItemIndex].Reference;

                if (weakReference == null || !weakReference.TryGetTarget(out var obj))
                {
                    continue;
                }

                Current = obj;
                return true;
            }
        }

        public void Reset()
        {
            currentItemIndex = weakList.listStart - 1;
            Current = default!;
        }

        public T Current { get; private set; }
        readonly object IEnumerator.Current => Current;

        public void Dispose()
        {
            Current = default!;
        }
    }
}
