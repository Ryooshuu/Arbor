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

public class Sprite : IComponent, IHasSize
{
    public Entity Entity { get; set; } = null!;
    
    private vec2 drawSize = vec2.Zero;
    vec2 IHasSize.DrawSize => drawSize;


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
    
    private RectangleF? textureRect =  null;
    
    public RectangleF? TextureRect
    {
        get => textureRect;
        set
        {
            if (value == textureRect)
                return;

            textureRect = value;
            bufferCache.Invalidate();
        }
    }

    private Sampler? sampler;

    public Sampler Sampler
    {
        get
        {
            if (sampler == null)
                sampler = Entity.Pipeline.Ansio4xSampler;

            return sampler;
        }
        set
        {
            if (value == sampler)
                return;
            
            sampler = value;
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
    
    private vec2 boundingSize = vec2.Zero;
    
    public vec2 BoundingSize
    {
        get => boundingSize;
        set
        {
            if (value == boundingSize)
                return;

            boundingSize = value;
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

        var displaySize = new vec2(texture.DisplayWidth, texture.DisplayHeight);
        
        if (boundingSize != vec2.Zero)
            displaySize = boundingSize;
        
        // TODO: conform to the size of the transform instead of the texture if it is set.
        drawSize = displaySize;
        transform!.Size = displaySize;
        
        buffer = Entity.Pipeline.CreateVertexBuffer<VertexUvColour>(IndexLayout.Quad);

        var uv = texture.GetUvRect(TextureRect);
        
        buffer.AddRange(new[]
        {
            new VertexUvColour(new vec2(0, 0), new vec2(uv.Left, uv.Top), Colour),
            new VertexUvColour(new vec2(displaySize.x, 0), new vec2(uv.Right, uv.Top), Colour),
            new VertexUvColour(new vec2(0, displaySize.y), new vec2(uv.Left, uv.Bottom), Colour),
            new VertexUvColour(new vec2(displaySize.x, displaySize.y), new vec2(uv.Right, uv.Bottom), Colour)
        });
        
        shader ??= ShaderSetHelper.CreateTexturedShaderSet(texture.TextureView, Sampler);
        bufferCache.Validate();
        Entity.Invalidate(EntityInvalidation.DrawSize);
    }

    public void Destroy()
    {
        SpriteSystem.Remove(this);

        buffer?.Dispose();
        shader?.Dispose();
    }
}
