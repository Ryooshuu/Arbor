using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public class Scene : IDisposable
{
    // TODO: move this to the game class.
    internal Window Window = null!;
    
    internal DevicePipeline Pipeline => Window.Pipeline;
    
    #region Entity Management
    
    public IReadOnlyList<Entity> Entities => aliveEntities;

    private readonly List<Entity> aliveEntities = new List<Entity>();

    public Entity CreateEntity()
    {
        var entity = new Entity(Pipeline);
        aliveEntities.Add(entity);
        entity.OnDispose += removeEntity;
        
        return entity;
    }

    private void removeEntity(Entity entity)
    {
        aliveEntities.Remove(entity);
    }

    #endregion
    
    #region Events

    internal virtual void LoadInternal()
    {
        Load();
    }
    
    protected virtual void Load()
    {
    }
    
    internal void UpdateInternal(IFrameBasedClock clock)
    {
        SpriteSystem.Update(clock);
        Update(clock);
    }
    
    protected virtual void Update(IFrameBasedClock clock)
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
        {
            entity.OnDispose -= removeEntity;
            entity.Dispose();
        }
        
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
