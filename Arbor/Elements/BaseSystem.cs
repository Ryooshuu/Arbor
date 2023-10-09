using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public class BaseSystem<T>
    where T : IComponent
{
    protected static IReadOnlyList<T> Components => components.ToList().AsReadOnly();
    private static readonly List<T> components = new List<T>();
    private static bool isDestroying;

    public static void Register(T component)
    {
        components.Add(component);
    }

    public static void Remove(T component)
    {
        if (isDestroying)
            return;

        components.Remove(component);
    }

    public static void Update(IClock clock)
    {
        foreach (var component in components)
            component.Update(clock);
    }

    public static void Draw(DrawPipeline pipeline)
    {
        foreach (var component in components)
            component.Draw(pipeline);
    }

    public static void Destroy()
    {
        isDestroying = true;
        foreach (var component in components)
            component.Destroy();
        isDestroying = false;
    }
}
