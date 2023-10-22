using Arbor;
using Arbor.Elements;
using Arbor.Elements.Components;
using Arbor.IO.Stores;
using Arbor.Text;
using Arbor.Timing;
using GlmSharp;
using Veldrid;

namespace SampleGame;

public class SampleGame : Game
{
    private Entity entity = null!;
    private Transform transform = null!;
    
    protected override void Load()
    {
        Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(SampleGame).Assembly), @"Resources"));
        
        entity = CreateEntity();
        entity.AddComponent(new SpriteText
        {
            Font = new FontInfo("Roboto", italics: true),
            Text = "Hello, World!",
            Colour = RgbaFloat.CornflowerBlue
        });
        transform = entity.AddComponent(new Transform
        {
            Position = new vec2(100, 100),
            Scale = new vec2(50f),
            Origin = Anchor.Centre
        });

        // var sprite = entity.AddComponent<Sprite>();
        // transform = entity.AddComponent<Transform>();
        //
        // sprite.Texture = Textures.Get("10-wKGO250UVi.png")!;
        // sprite.Colour = new RgbaFloat(1, 0.5f, 0.5f, 1);
        //
        // transform.Position = new vec2(100, 100);
        // // transform.Rotation -= 10;
        // transform.Scale = new vec2(1f);
        // transform.Origin = Anchor.Centre;
    }

    protected override void Update(IFrameBasedClock clock)
    {
        transform.Position = new vec2((float) (transform.Position.x + 0.1f * clock.ElapsedFrameTime), transform.Position.y);
        transform.Rotation += (float)(0.1f * clock.ElapsedFrameTime);
    }
}
