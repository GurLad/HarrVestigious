using Godot;
using System;

public class Vector2Int // Incomplete (aka no =,+,...), but who cares
{
    public static Vector2Int Zero { get; } = new Vector2Int(0, 0);
    public static Vector2Int One { get; } = new Vector2Int(1, 1);

    public int x { get; set; } = 0;
    public int y { get; set; } = 0;

    public static bool operator ==(Vector2Int a, Vector2Int b)
    {
        if ((object)a == null) return (object)b == null;
        if ((object)b == null) return (object)a == null;
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2Int a, Vector2Int b)
    {
        return !(a == b);
    }

    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int(Vector2 vector2) : this(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y)) { }

    public static float Distance(Vector2Int vec1, Vector2Int vec2)
    {
        return Mathf.Sqrt(Mathf.Pow(vec1.x - vec2.x, 2) + Mathf.Pow(vec1.y - vec2.y, 2));
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
