using Arbor;
using Arbor.Elements;
using Arbor.Elements.Components;
using Arbor.Timing;
using GlmSharp;
using Veldrid;
using Veldrid.ImageSharp;

namespace SampleGame;

public class SampleGame : Game
{
    private Entity? entity;
    private Transform? transform;
    
    protected override void Load()
    {
        entity = CreateEntity();
        entity.AddComponent<Sprite>();
        entity.AddComponent<Transform>();
        
        var sprite = entity.GetComponent<Sprite>()!;
        sprite.Texture = new ImageSharpTexture(@"D:\Projects\Projects\Arbor\SampleGame\Textures\10-wKGO250UVi.png");
        sprite.Colour = new RgbaFloat(1, 0.5f, 0.5f, 1);
        
        transform = entity.GetComponent<Transform>()!;
        transform.Position = new vec2(20);
        transform.Rotation -= 10;
        transform.Scale = new vec2(0.5f);
    }

    protected override void Update(IFrameBasedClock clock)
    {
        transform!.Position = new vec2((float) (transform.Position.x + 0.25f * clock.ElapsedFrameTime), transform.Position.y);
    }
}
