using Arbor.Elements.Systems;
using GlmSharp;

namespace Arbor.Elements.Components;

public class Transform : IComponent
{
    public Entity Entity { get; set; } = null!;
    
    private mat4 matrix = mat4.Identity;

    public mat4 Matrix => matrix;
    public mat4 MatrixInverse => matrix.Inverse;

    #region Properties

    private vec2 position = vec2.Zero;

    public vec2 Position
    {
        get => position;
        set
        {
            if (value == position)
                return;
            
            position = value;
            validateMatrix();
        }
    }

    private vec2 scale = vec2.Ones;

    public vec2 Scale
    {
        get => scale;
        set
        {
            if (value == scale)
                return;
            
            scale = value;
            validateMatrix();
        }
    }
    
    private float rotation = 0;

    public float Rotation
    {
        get => rotation;
        set
        {
            if (value == rotation)
                return;
            
            rotation = value;
            validateMatrix();
        }
    }

    private vec2 size = new(1);

    internal vec2 Size
    {
        get => size;
        set
        {
            if (value == size)
                return;
            
            size = value;
            validateMatrix();
        }
    }

    #endregion

    public Transform()
    {
        TransformSystem.Register(this);
    }

    private void validateMatrix()
    {
        var mat = mat4.Identity;

        var originX = size.x / 2; 
        var originY = size.y / 2; 
        
        if (position != vec2.Zero)
            mat *= mat4.Translate(position.x, position.y, 0);
        if (scale != vec2.Zero)
            mat *= mat4.Scale(scale.x, scale.y, 1);
        if (rotation != 0)
        {
            var degrees = rotation * MathF.PI / 180f;
            mat *= mat4.RotateZ(degrees);
        }
        
        if (originX != 0 && originY != 0)
            mat *= mat4.Translate(-originX, -originY, 0);
        
        matrix = mat;
    }

    public void Destroy()
    {
        TransformSystem.Remove(this);
    }
}
