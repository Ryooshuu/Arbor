using Arbor.Caching;
using Arbor.Elements.Systems;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Graphics.Textures;
using Arbor.Text;
using Arbor.Timing;
using GlmSharp;
using Veldrid;
using Texture = Arbor.Graphics.Textures.Texture;

namespace Arbor.Elements.Components;

public class SpriteText : IComponent, IHasSize
{
    public Entity Entity { get; set; } = null!;

    private vec2 drawSize = vec2.Zero;

    public vec2 DrawSize => drawSize;

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

    private FontInfo font = new FontInfo("Roboto");
    
    public FontInfo Font
    {
        get => font;
        set
        {
            if (value == font)
                return;

            font = value;
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
    private Texture? fontAtlas;
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

        buffer ??= Entity.Pipeline.CreateVertexBuffer<VertexUvColour>(IndexLayout.Quad);
        buffer.Clear();
        
        drawSize = vec2.Zero;
        
        foreach (var c in Text)
            addCharacterQuad(c);
        
        if (fontAtlas == null)
            return;
        
        shader ??= ShaderSetHelper.CreateTexturedShaderSet(fontAtlas.TextureView, Entity.Pipeline.Ansio4xSampler);
        bufferCache.Validate();
        Entity.Invalidate(EntityInvalidation.DrawSize);
    }
    
    private float xAdvance;

    private void addCharacterQuad(char c)
    {
        var glyph = Game.Fonts.Get(font.ToString(), c);
        if (glyph == null)
            return;

        if (fontAtlas == null)
        {
            var region = glyph.Texture as TextureRegion;
            fontAtlas = region?.Parent;
        }

        var uv = glyph.Texture.GetUvRect();
        var x = xAdvance + glyph.XOffset;
        var y = glyph.YOffset;
        
        buffer!.AddRange(new[]
        {
            new VertexUvColour(new vec2(x, y), new vec2(uv.Left, uv.Top), colour),
            new VertexUvColour(new vec2(x + glyph.Width, y), new vec2(uv.Right, uv.Top), colour),
            new VertexUvColour(new vec2(x, y + glyph.Height), new vec2(uv.Left, uv.Bottom), colour),
            new VertexUvColour(new vec2(x + glyph.Width, y + glyph.Height), new vec2(uv.Right, uv.Bottom), colour)
        });
        
        drawSize.x = Math.Max(drawSize.x, x + glyph.Width);
        drawSize.y = Math.Max(drawSize.y, y + glyph.Height);

        xAdvance += glyph.XAdvance;
    }
    
    public void Destroy()
    {
        SpriteTextSystem.Remove(this);
        
        buffer?.Dispose();
        shader?.Dispose();
    }
}
