using Arbor.Graphics;
using Arbor.Timing;

namespace Arbor.Elements;

public interface IComponent
{
    Entity Entity { get; set; }

    void Initialize() { }
    void Update(IClock clock) { }
    void Draw(DrawPipeline pipeline) { }
    void Destroy() { }
}
