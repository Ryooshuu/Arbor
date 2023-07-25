namespace Arbor.Utils;

public static class RectangleExtensions
{
    public static Rectangle Intersect(this Rectangle a, Rectangle b)
    {
        var x = Math.Max(a.X, b.X);
        var x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        var y = Math.Max(a.Y, b.Y);
        var y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);
        
        if (x2 >= x && y2 >= y)
            return new Rectangle(x, y, x2 - x, y2 - y);

        return Rectangle.Empty;
    }
}
