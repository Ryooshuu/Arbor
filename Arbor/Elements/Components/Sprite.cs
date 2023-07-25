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

    private readonly Cached bufferCache = new();
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
            transform!.Size = new vec2(Texture.Width, Texture.Height);
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

        transform!.Size = texture.Size;

        buffer = Entity.Pipeline.CreateVertexBuffer<VertexUvColour>();

        var texRect = texture.GetTextureRect();

        // TODO: Wow...
        buffer.Add(new VertexUvColour(new vec2(0, 0), new vec2(texRect.Left / texture.NativeTexture.Width, texRect.Top / texture.NativeTexture.Height), Colour));
        buffer.Add(new VertexUvColour(new vec2(texture.Width, 0), new vec2(texRect.Right / texture.NativeTexture.Width, texRect.Top / texture.NativeTexture.Height), Colour));
        buffer.Add(new VertexUvColour(new vec2(0, texture.Height), new vec2(texRect.Left / texture.NativeTexture.Width, texRect.Bottom / texture.NativeTexture.Height), Colour));
        buffer.Add(new VertexUvColour(new vec2(texture.Width, texture.Height), new vec2(texRect.Right / texture.NativeTexture.Width, texRect.Bottom / texture.NativeTexture.Height), Colour));

        shader = ShaderSetHelper.CreateTexturedShaderSet(texture.TextureView, Entity.Pipeline.GetDefaultSampler());
        bufferCache.Validate();
    }

    public void Destroy()
    {
        SpriteSystem.Remove(this);

        buffer?.Dispose();
        shader?.Dispose();
    }
}
