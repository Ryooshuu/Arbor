using Arbor;
using Arbor.Elements;
using Arbor.Elements.Components;
using Arbor.Timing;

namespace SampleGame;

public class SampleGame : Game
{
    private Entity? spriteEntity;
    
    protected override void Load()
    {
        spriteEntity = CreateEntity();
        spriteEntity.AddComponent<Sprite>();
    }
    
    protected override void Update(IClock clock)
    {
        // if (!(clock.CurrentTime > 5000))
        //     return;
        //
        // if (spriteEntity == null)
        //     return;
        //
        // spriteEntity.Dispose();
        // spriteEntity = null;
    }
}
