using System.Runtime.InteropServices;
using Arbor.Graphics.Shaders;
using Arbor.Graphics.Shaders.Vertices;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using Shader = Veldrid.Shader;

namespace Arbor.Graphics;

public class DevicePipeline : IDisposable
{
    private readonly GraphicsDevice device;
    internal readonly DrawPipeline DrawPipeline;

    internal ResourceFactory Factory => device.ResourceFactory;

    public DevicePipeline(GraphicsDevice device)
    {
        this.device = device;
        DrawPipeline = new DrawPipeline(this);
    }

    internal void Submit(CommandList commandList)
    {
        device.SubmitCommands(commandList);
        device.SwapBuffers();
    }

    internal Pipeline CreatePipeline(GraphicsPipelineDescription description)
        => Factory.CreateGraphicsPipeline(description);

    public Framebuffer GetSwapchainFramebuffer()
        => device.SwapchainFramebuffer;

    public Sampler GetDefaultSampler()
        => device.Aniso4xSampler;

    public VertexBuffer<T> CreateVertexBuffer<T>()
        where T : unmanaged
        => new(this);

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
            device.UpdateBuffer(buffer, 0, values);

        return buffer;
    }
    
    public void UpdateBuffer<T>(DeviceBuffer buffer, T[] values, uint offset = 0)
        where T : unmanaged
    {
        device.UpdateBuffer(buffer, offset, values);
    }

    public Texture CreateTexture(ImageSharpTexture texture)
        => texture.CreateDeviceTexture(device, Factory);
    

    public TextureView CreateDeviceTextureView(ImageSharpTexture texture)
    {
        var text = texture.CreateDeviceTexture(device, Factory);
        return Factory.CreateTextureView(text);
    }

    public TextureView CreateDeviceTextureView(Texture texture)
        => Factory.CreateTextureView(texture);

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

    public void Dispose()
    {
        DrawPipeline.Dispose();
        device.Dispose();
    }
}
