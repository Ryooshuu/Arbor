using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColour
{
    public vec2 Position;
    public RgbaFloat Color;

    public VertexPositionColour(vec2 position, RgbaFloat color)
    {
        Position = position;
        Color = color;
    }
}
