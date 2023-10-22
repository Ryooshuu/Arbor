using Arbor;
using Arbor.Elements;
using Arbor.Elements.Components;
using Arbor.IO.Stores;
using Arbor.Text;
using Arbor.Timing;
using GlmSharp;

namespace SampleGame;

public class SampleGame : Game
{
    private Entity entity = null!;
    private Transform transform = null!;
    
    protected override void Load()
    {
        Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(SampleGame).Assembly), @"Resources"));
        
        entity = CreateEntity();
        var text = entity.AddComponent<SpriteText>();
        transform = entity.AddComponent<Transform>();

        text.Font = new FontInfo("Roboto", italics: true);
        text.Text = "Hello, World!";
        
        transform.Position = new vec2(100, 100);
        transform.Scale = new vec2(50f);

        // var sprite = entity.AddComponent<Sprite>();
        // transform = entity.AddComponent<Transform>();
        //
        // sprite.Texture = Textures.Get("10-wKGO250UVi.png")!;
        // sprite.Colour = new RgbaFloat(1, 0.5f, 0.5f, 1);
        //
        // transform.Position = new vec2(100, 100);
        // // transform.Rotation -= 10;
        // transform.Scale = new vec2(1f);
    }

    protected override void Update(IFrameBasedClock clock)
    {
        // transform.Position = new vec2((float) (transform.Position.x + 0.1f * clock.ElapsedFrameTime), transform.Position.y);
        // transform.Rotation += (float)(0.1f * clock.ElapsedFrameTime);
    }
}
