using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public class Scene : IDisposable
{
    internal void LoadInternal()
    {
        Load();
    }
    
    protected virtual void Load()
    {
    }
    
    internal void UpdateInternal(IClock clock)
    {
        Update(clock);
    }
    
    protected virtual void Update(IClock clock)
    {
    }
    
    internal void DrawInternal(DrawPipeline pipeline)
    {
        Draw(pipeline);
    }
    
    protected virtual void Draw(DrawPipeline pipeline)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
