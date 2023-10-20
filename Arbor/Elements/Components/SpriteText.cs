using Arbor.Caching;
using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Timing;
using Veldrid;

namespace Arbor.Elements.Components;

public class SpriteText : IComponent
{
    public Entity Entity { get; set; } = null!;

    #region Properties

    private string text = string.Empty;
    
    public string Text
    {
        get => text;
        set
        {
            if (value == text)
                return;

            text = value;
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
    
    public SpriteText()
    {
        SpriteTextSystem.Register(this);
    }

    public void Update(IClock clock)
    {
        if (transform == null && Entity.GetComponent<Transform>() != null)
        {
            transform = Entity.GetComponent<Transform>();
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
        
        bufferCache.Validate();
    }

    public void Destroy()
    {
        SpriteTextSystem.Remove(this);
        
        buffer?.Dispose();
        shader?.Dispose();
    }
}
