using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public class Scene : IDisposable
{
    // TODO: move this to the game class.
    internal Window Window = null!;
    
    protected DevicePipeline Pipeline => Window.Pipeline;

    internal IFrameBasedClock FramedClock = null!;

    public IClock Clock => FramedClock;
    
    #region Entity Management
    
    public IReadOnlyList<Entity> Entities => aliveEntities;

    private readonly List<Entity> aliveEntities = new List<Entity>();

    public Entity CreateEntity()
    {
        var entity = new Entity(Pipeline);
        entity.SwitchClock(Clock);
        entity.OnDispose += removeEntity;
        aliveEntities.Add(entity);
        
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
    
    internal virtual void UpdateInternal(IFrameBasedClock clock)
    {
        SpriteSystem.Update(clock);
        SpriteTextSystem.Update(clock);
        Update(clock);
    }
    
    protected virtual void Update(IFrameBasedClock clock)
    {
    }
    
    internal virtual void DrawInternal(DrawPipeline pipeline)
    {
        SpriteSystem.Draw(pipeline);
        SpriteTextSystem.Draw(pipeline);
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
        SpriteTextSystem.Destroy();
        foreach (var entity in aliveEntities)
        {
            entity.OnDispose -= removeEntity;
            entity.Dispose();
        }
        
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
