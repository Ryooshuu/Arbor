using System.Drawing;
using Arbor;
using Arbor.Debugging;
using Arbor.Elements;
using Arbor.Elements.Components;
using Arbor.Graphics;
using Arbor.Graphics.Textures;
using Arbor.IO.Stores;
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
        var largeStore = new LargeTextureStore(Pipeline, new TextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
        
        entity = CreateEntity();
        // entity.AddComponent(new SpriteText
        // {
        //     Font = new FontInfo("Roboto", italics: true),
        //     Text = "Hello, World!",
        //     Colour = RgbaFloat.CornflowerBlue
        // });
        // transform = entity.AddComponent(new Transform
        // {
        //     Position = new vec2(300, 100),
        //     Scale = new vec2(50f),
        //     Origin = Anchor.Centre
        // });

        var sprite = entity.AddComponent<Sprite>();
        transform = entity.AddComponent<Transform>();
        
        var texture = Textures.Get("Shot_01.png")!;
        sprite.Sampler = Pipeline.PointSampler;
        sprite.Texture = texture;
        sprite.TextureRect = new SixLabors.ImageSharp.RectangleF(0, 16 * 2, 16, 16);
        sprite.Colour = new RgbaFloat(1, 0f, 0.25f, 1);
        sprite.BoundingSize = new vec2(64);
        
        transform.Position = new vec2(100);
        // transform.Rotation -= 10;
        transform.Origin = Anchor.Centre;
    }

    protected override void Update(IFrameBasedClock clock)
    {
        // transform.Position = new vec2((float) (transform.Position.x + 0.1f * clock.ElapsedFrameTime), transform.Position.y);
        // transform.Rotation += (float)(0.1f * clock.ElapsedFrameTime);
    }

    protected override void Draw(DrawPipeline pipeline)
    {
        DebugDraw.Rect(new RectangleF(transform.Position.x, transform.Position.y, 2, 2), RgbaFloat.Blue);
        DebugDraw.Line(new vec2(0), new vec2(50), RgbaFloat.Red);
        DebugDraw.Rect(new vec2(20), new vec2(70, 20), new vec2(60, 60), new vec2(20, 70), RgbaFloat.Green);
        DebugDraw.Rect(new RectangleF(20, 20, 50, 50), RgbaFloat.Blue);
        DebugDraw.Shape(RgbaFloat.Yellow, new vec2(70), new vec2(80, 70), new vec2(75, 80), new vec2(70));
    }
}
