using System.Runtime.InteropServices;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Arbor.Graphics.Textures;
using Arbor.Lists;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Veldrid.Shader;
using Texture = Arbor.Graphics.Textures.Texture;

namespace Arbor.Graphics;

public class DevicePipeline : IDisposable
{
    private readonly LockedWeakList<Texture> allTextures = new LockedWeakList<Texture>();
    
    internal readonly GraphicsDevice Device;
    internal readonly DrawPipeline DrawPipeline;

    internal ResourceFactory Factory => Device.ResourceFactory;

    public DevicePipeline(GraphicsDevice device)
    {
        Device = device;
        DrawPipeline = new DrawPipeline(this);
    }

    internal void Submit(CommandList commandList)
    {
        Device.SubmitCommands(commandList);
        Device.SwapBuffers();
    }

    internal Pipeline CreatePipeline(GraphicsPipelineDescription description)
        => Factory.CreateGraphicsPipeline(description);

    #region Buffers

    public VertexBuffer<T> CreateVertexBuffer<T>(IndexLayout layout = IndexLayout.Linear)
        where T : unmanaged => new VertexBuffer<T>(this, layout);

    public DeviceBuffer CreateBuffer(BufferUsage usage, uint size)
        => Factory.CreateBuffer(new BufferDescription(size, usage));

    public DeviceBuffer CreateBuffer<T>(T[] values, BufferUsage usage, uint? size = null)
        where T : unmanaged
    {
        var bufferSize = (uint) (values.Length * Marshal.SizeOf<T>());

        if (size.HasValue)
            bufferSize = size.Value;

        var buffer = Factory.CreateBuffer(new BufferDescription(bufferSize, usage));
        if (values.Length > 0)
            Device.UpdateBuffer(buffer, 0, values);

        return buffer;
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, T[] values, uint offset = 0)
        where T : unmanaged
    {
        Device.UpdateBuffer(buffer, offset, values);
    }

    #endregion

    #region Textures

    public Framebuffer GetSwapchainFramebuffer()
        => Device.SwapchainFramebuffer;

    public Sampler GetDefaultSampler()
        => Device.Aniso4xSampler;
    
    internal Texture[] GetAllTextures()
        => allTextures.ToArray();

    public Texture CreateTexture(TextureUpload texture)
    {
        var description = TextureDescription.Texture2D(texture.Width, texture.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled | TextureUsage.RenderTarget);
        var tex = Factory.CreateTexture(ref description);

        return registerTexture(new Texture(tex, this));
    }

    public Texture? CreateTexture(int width, int height, RgbaFloat initialisationColour)
    {
        var image = new Image<Rgba32>(width, height, new Rgba32(initialisationColour.ToVector4()));
        var upload = new TextureUpload(image);

        var texture = CreateTexture(upload);
        UpdateTexture(texture.NativeTexture, upload);

        return texture;
    }

    public TextureView CreateDeviceTextureView(Veldrid.Texture texture)
        => Factory.CreateTextureView(texture);

    public void UpdateTexture(Veldrid.Texture original, ITextureUpload upload)
    {
        UpdateTexture(original, (uint) upload.Bounds.X, (uint) upload.Bounds.Y, (uint) upload.Bounds.Width, (uint) upload.Bounds.Height, upload.Data);
    }

    public void UpdateTexture<T>(Veldrid.Texture original, uint x, uint y, uint width, uint height, ReadOnlySpan<T> data)
        where T : unmanaged
    {
        Device.UpdateTexture(original, data, x, y, 0, width, height, 1, 0, 0);
    }

    internal event Action<Texture>? TextureCreated;

    private Texture registerTexture(Texture texture)
    {
        allTextures.Add(texture);
        TextureCreated?.Invoke(texture);
        return texture;
    }

    #endregion

    #region Shaders

    public ResourceLayout CreateResourceLayout(ResourceLayoutElementDescription[] descriptions)
        => Factory.CreateResourceLayout(new ResourceLayoutDescription(descriptions));

    public ResourceLayout CreateResourceLayout(ResourceLayoutElementDescription descriptions)
        => Factory.CreateResourceLayout(new ResourceLayoutDescription(descriptions));

    public ResourceSet CreateResourceSet(ResourceLayout layout, params BindableResource[] resources)
        => Factory.CreateResourceSet(new ResourceSetDescription(layout, resources));

    public ResourceSet CreateResourceSet(ResourceLayoutElementDescription[] descriptions, BindableResource[] resources)
        => CreateResourceSet(CreateResourceLayout(descriptions), resources);

    public IEnumerable<Shader> CompileShaders(IVertexShader vertex, IBindableShader fragment)
        => Factory.CreateFromSpirv(vertex.CreateShaderDescriptionInternal(), fragment.CreateShaderDescriptionInternal());

    public IEnumerable<Shader> CompileShaders(IShader compute)
    {
        return new[] { Factory.CreateFromSpirv(compute.CreateShaderDescriptionInternal()) };
    }

    #endregion

    public void Dispose()
    {
        DrawPipeline.Dispose();
        Device.Dispose();
    }
}
