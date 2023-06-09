using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColour
{
    public Vector2 Position;
    public RgbaFloat Color;

    public VertexPositionColour(Vector2 position, RgbaFloat color)
    {
        Position = position;
        Color = color;
    }
}
