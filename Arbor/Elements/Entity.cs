using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public class Entity : IDisposable
{
    public float Width { get; set; }
    public float Height { get; set; }
    public Action<Entity>? OnDispose { get; set; }
    public Action<EntityInvalidation>? OnInvalidated { get; set; }

    internal uint Id { get; set; }
    internal DevicePipeline Pipeline { get; private set; }
    internal IReadOnlyList<IComponent> Components => componentMap.Values.ToList().AsReadOnly();

    private readonly Dictionary<Type, IComponent> componentMap = new Dictionary<Type, IComponent>();
    
    private IClock? clock = new StopwatchClock();

    public IClock Clock => clock!;

    internal Entity(DevicePipeline pipeline)
    {
        Pipeline = pipeline;
    }

    public T AddComponent<T>()
        where T : IComponent, new()
    {
        var component = new T { Entity = this };
        component.Initialize();
        componentMap.Add(typeof(T), component);
        
        return component;
    }

    public T AddComponent<T>(T instance)
        where T : IComponent
    {
        instance.Entity = this;
        instance.Initialize();
        componentMap.Add(typeof(T), instance);

        return instance;
    }
    
    public T? GetComponent<T>()
        where T : class, IComponent
    {
        return componentMap.TryGetValue(typeof(T), out var component) ? (T)component : null;
    }
    
    public void SwitchClock(IClock clock)
    {
        this.clock = clock;
    }
    
    public void Invalidate(EntityInvalidation invalidation)
    {
        handleInvalidation(invalidation);
        OnInvalidated?.Invoke(invalidation);
    }
    
    private void handleInvalidation(EntityInvalidation invalidation)
    {
        foreach (var component in Components)
        {
            switch (invalidation)
            {
                case EntityInvalidation.DrawSize:
                    if (component is IHasSize hasSize)
                    {
                        var size = hasSize.DrawSize;
                        Width = Math.Max(Width, size.x);
                        Height = Math.Max(Height, size.y);
                    }
                    
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(invalidation), invalidation, null);
            }
            
            component.OnEntityInvalidated(invalidation);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var component in Components)
        {
            component.Destroy();
            componentMap.Remove(component.GetType());
        }
        
        OnDispose?.Invoke(this);
    }
}

public enum EntityInvalidation
{
    DrawSize
}
