using Arbor;
using Arbor.Graphics;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using GlmSharp;
using Veldrid;
using Veldrid.ImageSharp;

namespace SampleGame;

public class SampleGame : Game
{
    private VertexBuffer<VertexUvColour> buffer = null!;
    private IShaderSet shader = null!;

    protected override void Load()
    {
        buffer = Pipeline.CreateVertexBuffer<VertexUvColour>();
        var color = RgbaFloat.White;
        
        const float size = 200;
        
        buffer.Add(new VertexUvColour(new vec2(0 + 20, 0 + 20), new vec2(0, 0), color));
        buffer.Add(new VertexUvColour(new vec2(size + 20, 0 + 20), new vec2(1, 0), color));
        buffer.Add(new VertexUvColour(new vec2(0 + 20, size + 20), new vec2(0, 1), color));
        buffer.Add(new VertexUvColour(new vec2(size + 20, size + 20), new vec2(1, 1), color));
        
        var imageSharpTexture = new ImageSharpTexture(@"D:\Projects\Projects\Arbor\SampleGame\Textures\10-wKGO250UVi.png");
        shader = ShaderSetHelper.CreateTexturedShaderSet(Pipeline.CreateDeviceTextureView(imageSharpTexture), Pipeline.GetDefaultSampler());
    }

    protected override void Draw(DrawPipeline pipeline)
    {
        pipeline.BindShader(shader);
        pipeline.DrawVertexBuffer(buffer);
        pipeline.UnbindShader();
    }
}
