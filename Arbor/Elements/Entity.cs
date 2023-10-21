using Arbor.Graphics;

namespace Arbor.Elements;

public class Entity : IDisposable
{
    public Action<Entity>? OnDispose { get; set; }
    
    public float Width { get; set; }
    public float Height { get; set; }
    
    internal uint Id { get; set; }
    internal DevicePipeline Pipeline { get; private set; }
    internal IReadOnlyList<IComponent> Components => componentMap.Values.ToList().AsReadOnly();

    private readonly Dictionary<Type, IComponent> componentMap = new Dictionary<Type, IComponent>();

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
    
    public T? GetComponent<T>()
        where T : class, IComponent
    {
        return componentMap.TryGetValue(typeof(T), out var component) ? (T)component : null;
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
