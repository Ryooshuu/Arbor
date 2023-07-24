using Arbor.Graphics;

namespace Arbor.Elements;

public class Entity : IDisposable
{
    public Action? OnDispose { get; set; }
    
    internal uint Id { get; set; }
    internal DevicePipeline Pipeline { get; private set; }
    internal IReadOnlyList<IComponent> Components => componentMap.Values.ToList().AsReadOnly();

    private readonly Dictionary<Type, IComponent> componentMap = new();

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
    
    public T GetComponent<T>()
        where T : IComponent
    {
        return (T)componentMap[typeof(T)];
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var component in Components)
        {
            component.Destroy();
            componentMap.Remove(component.GetType());
        }
        
        OnDispose?.Invoke();
    }
}
