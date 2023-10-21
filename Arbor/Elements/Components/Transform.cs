using Arbor.Elements.Systems;
using Arbor.Utils;
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

    private vec2 size = new vec2(1);

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

    private vec2 originPosition;

    public vec2 OriginPosition
    {
        get => originPosition;
        set
        {
            if (value.x is < 0 or > 1)
                throw new ArgumentException("Value cannot be smaller than 0 or bigger than 1", nameof(value));
            if (value.y is < 0 or > 1)
                throw new ArgumentException("Value cannot be smaller than 0 or bigger than 1", nameof(value));

            originPosition = value;
            validateMatrix();
        }
    }

    private Anchor origin = Anchor.Centre;

    public Anchor Origin
    {
        get => origin;
        set
        {
            origin = value;
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

        if (position != vec2.Zero)
            mat *= mat4.Translate(position.x, position.y, 0);
        if (scale != vec2.Zero)
            mat *= mat4.Scale(scale.x, scale.y, 1);
        if (rotation != 0)
        {
            var degrees = rotation * MathF.PI / 180f;
            mat *= mat4.RotateZ(degrees);
        }

        var offset = calculateOriginPosition();

        if (offset != vec2.Zero)
            mat *= mat4.Translate(-offset.x, -offset.y, 0);
        
        matrix = mat;
    }

    private vec2 calculateOriginPosition()
    {
        if (Origin == Anchor.Custom)
            return new vec2(Entity.Width * OriginPosition.x, Entity.Height * OriginPosition.y);
                
        var result = vec2.Zero;

        if (Origin.HasFlagFast(Anchor.X1))
            result.x = Entity.Width / 2;
        else if (Origin.HasFlagFast(Anchor.X2))
            result.x = Entity.Width;

        if (Origin.HasFlagFast(Anchor.Y1))
            result.y = Entity.Height / 2;
        else if (Origin.HasFlagFast(Anchor.Y2))
            result.y = Entity.Height;

        return result;
    }

    public void Destroy()
    {
        TransformSystem.Remove(this);
    }
}

[Flags]
public enum Anchor
{
    TopLeft = Y0 | X0,
    TopCentre = Y0 | X1,
    TopRight = Y0 | X2,

    CentreLeft = Y1 | X0,
    Centre = Y1 | X1,
    CentreRight = Y1 | X2,

    BottomLeft = Y2 | X0,
    BottomCentre = Y2 | X1,
    BottomRight = Y2 | X2,

    /// <summary>
    /// The vertical counterpart is at "Top" position.
    /// </summary>
    Y0 = 1,

    /// <summary>
    /// The vertical counterpart is at "Centre" position.
    /// </summary>
    Y1 = 1 << 1,

    /// <summary>
    /// The vertical counterpart is at "Bottom" position.
    /// </summary>
    Y2 = 1 << 2,

    /// <summary>
    /// The horizontal counterpart is at "Left" position.
    /// </summary>
    X0 = 1 << 3,

    /// <summary>
    /// The horizontal counterpart is at "Centre" position.
    /// </summary>
    X1 = 1 << 4,

    /// <summary>
    /// The horizontal counterpart is at "Right" position.
    /// </summary>
    X2 = 1 << 5,

    /// <summary>
    /// The user is manually updating the outcome, so we shouldn't.
    /// </summary>
    Custom = 1 << 6,
}
