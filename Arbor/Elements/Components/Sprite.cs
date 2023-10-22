using Arbor.Caching;
using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Timing;
using GlmSharp;
using Veldrid;
using Texture = Arbor.Graphics.Textures.Texture;

namespace Arbor.Elements.Components;

public class Sprite : IComponent
{
    public Entity Entity { get; set; } = null!;

    #region Properties

    private Texture texture = null!;

    public Texture Texture
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

    private readonly Cached bufferCache = new Cached();
    private VertexBuffer<VertexUvColour>? buffer;
    private IShaderSet? shader;
    private Transform? transform;

    public Sprite()
    {
        SpriteSystem.Register(this);
    }

    public void Update(IClock clock)
    {
        if (transform == null && Entity.GetComponent<Transform>() != null)
        {
            transform = Entity.GetComponent<Transform>();
            transform!.Size = new vec2(Texture.DisplayWidth, Texture.DisplayHeight);
        }

        if (!bufferCache.IsValid)
            validateBuffer();
    }

    public void Draw(DrawPipeline pipeline)
    {
        if (transform == null)
            return;

        pipeline.PushMatrix(transform.Matrix);
        pipeline.BindShader(shader!);
        pipeline.DrawVertexBuffer(buffer!);
        pipeline.UnbindShader();
        pipeline.PopMatrix();
    }

    private void validateBuffer()
    {
        if (bufferCache.IsValid)
            return;

        buffer?.Dispose();
        shader?.Dispose();

        transform!.Size = new vec2(Texture.DisplayWidth, Texture.DisplayHeight);
        
        // TODO: This is incorrect, we'd want the entity to calculate the size based on the combined size of all components.
        Entity.Width = Texture.DisplayWidth;
        Entity.Height = Texture.DisplayHeight;

        buffer = Entity.Pipeline.CreateVertexBuffer<VertexUvColour>(IndexLayout.Quad);

        var uv = texture.GetUvRect();
        
        buffer.AddRange(new[]
        {
            new VertexUvColour(new vec2(0, 0), new vec2(uv.Left, uv.Top), Colour),
            new VertexUvColour(new vec2(texture.DisplayWidth, 0), new vec2(uv.Right, uv.Top), Colour),
            new VertexUvColour(new vec2(0, texture.DisplayHeight), new vec2(uv.Left, uv.Bottom), Colour),
            new VertexUvColour(new vec2(texture.DisplayWidth, texture.DisplayHeight), new vec2(uv.Right, uv.Bottom), Colour)
        });
        
        shader ??= ShaderSetHelper.CreateTexturedShaderSet(texture.TextureView, Entity.Pipeline.GetDefaultSampler());
        bufferCache.Validate();
    }
    
    public void Destroy()
    {
        SpriteSystem.Remove(this);

        buffer?.Dispose();
        shader?.Dispose();
    }
}
