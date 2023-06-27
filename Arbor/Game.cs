using Arbor.Elements;
using Arbor.Graphics;

namespace Arbor;

public abstract class Game : Scene
{
    internal Window Window = null!;
    
    public DevicePipeline Pipeline => Window.Pipeline;
}
