using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public class Scene : IDisposable
{
    internal Window Window = null!;
    
    internal DevicePipeline Pipeline => Window.Pipeline;
    
    #region Entity Management
    
    public IReadOnlyList<Entity> Entities => aliveEntities;

    private readonly List<Entity> aliveEntities = new();

    public Entity CreateEntity()
    {
        var entity = new Entity(Pipeline);
        aliveEntities.Add(entity);
        entity.OnDispose = () => aliveEntities.Remove(entity);
        
        return entity;
    }

    #endregion
    
    #region Events

    internal void LoadInternal()
    {
        Load();
    }
    
    protected virtual void Load()
    {
    }
    
    internal void UpdateInternal(IClock clock)
    {
        SpriteSystem.Update(clock);
        Update(clock);
    }
    
    protected virtual void Update(IClock clock)
    {
    }
    
    internal void DrawInternal(DrawPipeline pipeline)
    {
        SpriteSystem.Draw(pipeline);
        Draw(pipeline);
    }
    
    protected virtual void Draw(DrawPipeline pipeline)
    {
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        SpriteSystem.Destroy();
        foreach (var entity in aliveEntities)
            entity.Dispose();
        
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
