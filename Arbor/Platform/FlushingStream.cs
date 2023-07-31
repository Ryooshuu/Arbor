namespace Arbor.Platform;

public class FlushingStream : FileStream
{
    public FlushingStream(string path, FileMode mode, FileAccess access)
        : base(path, mode, access)
    {
    }

    private bool finalFlushRun;

    protected override void Dispose(bool disposing)
    {
        if (!finalFlushRun)
        {
            finalFlushRun = true;

            try
            {
                Flush(true);
            }
            catch
            {
                // ignored
            }
        }
        
        base.Dispose(disposing);
    }
}
