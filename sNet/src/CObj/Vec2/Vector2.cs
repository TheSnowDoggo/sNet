using SCENeo;

namespace sNet.CScriptPro;

public struct Vector2 : IEquatable<Vector2>
{
    public const int Size = 2 * sizeof(double);
    
    public double X;
    public double Y;

    public Vector2(double x, double y)
    {
        X = x;
        Y = y;
    }
    
    public static Vector2 Zero => new Vector2(0, 0);
    public static Vector2 One => new Vector2(1, 1);
    public static Vector2 Up => new Vector2(0, -1);
    public static Vector2 Down => new Vector2(0, 1);
    public static Vector2 Left => new Vector2(-1, 0);
    public static Vector2 Right => new Vector2(1, 0);
    
    public static explicit operator Vec2I(Vector2 v) => new Vec2I((int)v.X, (int)v.Y);

    public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
    public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);
    
    public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.X * b.X, a.Y * b.Y);
    public static Vector2 operator *(Vector2 a, double x) => new Vector2(a.X * x, a.Y * x);
    
    public static Vector2 operator /(Vector2 a, Vector2 b) => new Vector2(a.X / b.X, a.Y / b.Y);
    public static Vector2 operator /(Vector2 a, double x) => new Vector2(a.X / x, a.Y / x);
    
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);

    public readonly double LengthSquared()
    {
        return X * X + Y * Y;
    }

    public readonly double Length()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    public readonly Vector2 Normalized()
    {
        var ls = X * X + Y * Y;

        if (ls == 0)
        {
            return Zero;
        }
        
        ls = Math.Sqrt(ls);
        
        return new Vector2(X / ls, Y / ls);
    }

    public readonly Vector2 Ceiled()
    {
        return new Vector2(Math.Ceiling(X), Math.Ceiling(Y));
    }
    
    public readonly Vector2 Floored()
    {
        return new Vector2(Math.Floor(X), Math.Floor(Y));
    }

    public readonly Vector2 Truncated()
    {
        return new Vector2(Math.Truncate(X), Math.Truncate(Y));
    }
    
    public readonly Vector2 Rounded()
    {
        return new Vector2(Math.Round(X), Math.Round(Y));
    }

    public readonly double Dot(Vector2 other)
    {
        return X * other.X + Y * other.Y;
    }

    public readonly Vector2 Rotated90()
    {
        return new Vector2(-Y, X);
    }

    public readonly Vector2 Rotated180()
    {
        return new Vector2(-X, -Y);
    }

    public readonly Vector2 Rotated270()
    {
        return new Vector2(Y, -X);
    }
    
    public readonly Vector2 Rotated(double rotation)
    {
        double x = X * Math.Cos(rotation) - Y * Math.Sin(rotation);
        double y = X * Math.Sin(rotation) + Y * Math.Cos(rotation);
        
        return new Vector2(x, y);
    }

    public bool Equals(Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object obj)
    {
        return obj is Vector2 vec2 && Equals(vec2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"Vec2({X}, {Y})";
    }
}