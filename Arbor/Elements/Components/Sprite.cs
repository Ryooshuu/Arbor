using Arbor.Caching;
using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Timing;
using GlmSharp;
using Veldrid;
using Veldrid.ImageSharp;

namespace Arbor.Elements.Components;

public class Sprite : IComponent
{
    public Entity Entity { get; set; } = null!;

    #region Properties

    private ImageSharpTexture texture;

    public ImageSharpTexture Texture
    {
        get => texture;
        set
        {
            if (value == texture)
                return;

            texture = value;
            bufferCache.Invalidate();
        }
    }
    
    private RgbaFloat colour = RgbaFloat.White;
    
    public RgbaFloat Colour
    {
        get => colour;
        set
        {
            if (value == colour)
                return;

            colour = value;
            bufferCache.Invalidate();
        }
    }

    #endregion
    
    private readonly Cached bufferCache = new();
    private VertexBuffer<VertexUvColour> buffer = null!;
    private IShaderSet shader = null!;
    private Transform? transform;

    public Sprite()
    {
        SpriteSystem.Register(this);
    }

    public void Update(IClock clock)
    {
        if (transform == null && Entity.GetComponent<Transform>() != null)
            transform = Entity.GetComponent<Transform>();
        
        if (!bufferCache.IsValid)
            validateBuffer();
    }

    public void Draw(DrawPipeline pipeline)
    {
        if (transform == null)
            return;

        pipeline.PushMatrix(transform.Matrix);
        pipeline.BindShader(shader);
        pipeline.DrawVertexBuffer(buffer);
        pipeline.UnbindShader();
        pipeline.PopMatrix();
    }

    private void validateBuffer()
    {
        if (bufferCache.IsValid)
            return;
        
        buffer?.Dispose();
        shader?.Dispose();

        buffer = Entity.Pipeline.CreateVertexBuffer<VertexUvColour>();

        buffer.Add(new VertexUvColour(new vec2(0, 0), new vec2(0, 0), Colour));
        buffer.Add(new VertexUvColour(new vec2(texture.Width, 0), new vec2(1, 0), Colour));
        buffer.Add(new VertexUvColour(new vec2(0, texture.Height), new vec2(0, 1), Colour));
        buffer.Add(new VertexUvColour(new vec2(texture.Width, texture.Height), new vec2(1, 1), Colour));

        shader = ShaderSetHelper.CreateTexturedShaderSet(Entity.Pipeline.CreateDeviceTextureView(texture), Entity.Pipeline.GetDefaultSampler());
        bufferCache.Validate();
    }

    public void Destroy()
    {
        SpriteSystem.Remove(this);

        buffer.Dispose();
        shader.Dispose();
    }
}
