using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using GlmSharp;
using Veldrid;
using Veldrid.ImageSharp;

namespace Arbor.Elements.Components;

public class Sprite : IComponent
{
    public Entity Entity { get; set; } = null!;
    public ImageSharpTexture? Texture { get; set; } = new(@"D:\Projects\Projects\Arbor\SampleGame\Textures\10-wKGO250UVi.png");
    public RgbaFloat Colour { get; set; } = RgbaFloat.White;

    private VertexBuffer<VertexUvColour> buffer = null!;
    private IShaderSet shader = null!;

    public Sprite()
    {
        SpriteSystem.Register(this);
    }

    public void Initialize()
    {
        buffer = Entity.Pipeline.CreateVertexBuffer<VertexUvColour>();

        const float size = 200; // TODO: get this from the texture instead.

        buffer.Add(new VertexUvColour(new vec2(0 + 20, 0 + 20), new vec2(0, 0), Colour));
        buffer.Add(new VertexUvColour(new vec2(size + 20, 0 + 20), new vec2(1, 0), Colour));
        buffer.Add(new VertexUvColour(new vec2(0 + 20, size + 20), new vec2(0, 1), Colour));
        buffer.Add(new VertexUvColour(new vec2(size + 20, size + 20), new vec2(1, 1), Colour));

        shader = ShaderSetHelper.CreateTexturedShaderSet(Entity.Pipeline.CreateDeviceTextureView(Texture!), Entity.Pipeline.GetDefaultSampler());
    }

    public void Draw(DrawPipeline pipeline)
    {
        pipeline.BindShader(shader);
        pipeline.DrawVertexBuffer(buffer);
        pipeline.UnbindShader();
    }

    public void Destroy()
    {
        SpriteSystem.Remove(this);
        
        buffer.Dispose();
        shader.Dispose();
    }
}
