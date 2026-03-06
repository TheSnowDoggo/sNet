namespace sNet.CScriptPro;

public struct Vec2 : IEquatable<Vec2>
{
    public const int Size = 2 * sizeof(double);
    
    public double X;
    public double Y;

    public Vec2(double x, double y)
    {
        X = x;
        Y = y;
    }
    
    public static Vec2 Zero => new Vec2(0, 0);
    public static Vec2 One => new Vec2(1, 1);
    public static Vec2 Up => new Vec2(0, -1);
    public static Vec2 Down => new Vec2(0, 1);
    public static Vec2 Left => new Vec2(-1, 0);
    public static Vec2 Right => new Vec2(1, 0);

    public static bool operator ==(Vec2 a, Vec2 b) => a.Equals(b);
    public static bool operator !=(Vec2 a, Vec2 b) => !a.Equals(b);
    
    public static Vec2 operator *(Vec2 a, Vec2 b) => new Vec2(a.X * b.X, a.Y * b.Y);
    public static Vec2 operator *(Vec2 a, double x) => new Vec2(a.X * x, a.Y * x);
    
    public static Vec2 operator /(Vec2 a, Vec2 b) => new Vec2(a.X / b.X, a.Y / b.Y);
    public static Vec2 operator /(Vec2 a, double x) => new Vec2(a.X / x, a.Y / x);
    
    public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);

    public readonly double LengthSquared()
    {
        return X * X + Y * Y;
    }

    public readonly double Length()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    public readonly Vec2 Normalized()
    {
        var ls = X * X + Y * Y;

        if (ls == 0)
        {
            return Zero;
        }
        
        ls = Math.Sqrt(ls);
        
        return new Vec2(X / ls, Y / ls);
    }

    public readonly Vec2 Ceiled()
    {
        return new Vec2(Math.Ceiling(X), Math.Ceiling(Y));
    }
    
    public readonly Vec2 Floored()
    {
        return new Vec2(Math.Floor(X), Math.Floor(Y));
    }

    public readonly Vec2 Truncated()
    {
        return new Vec2(Math.Truncate(X), Math.Truncate(Y));
    }
    
    public readonly Vec2 Rounded()
    {
        return new Vec2(Math.Round(X), Math.Round(Y));
    }

    public readonly double Dot(Vec2 other)
    {
        return X * other.X + Y * other.Y;
    }

    public readonly Vec2 Rotated90()
    {
        return new Vec2(-Y, X);
    }

    public readonly Vec2 Rotated180()
    {
        return new Vec2(-X, -Y);
    }

    public readonly Vec2 Rotated270()
    {
        return new Vec2(Y, -X);
    }
    
    public readonly Vec2 Rotated(double rotation)
    {
        double x = X * Math.Cos(rotation) - Y * Math.Sin(rotation);
        double y = X * Math.Sin(rotation) + Y * Math.Cos(rotation);
        
        return new Vec2(x, y);
    }

    public bool Equals(Vec2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object obj)
    {
        return obj is Vec2 vec2 && Equals(vec2);
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