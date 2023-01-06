using Godot;
using System;

public class Vector2Int // Incomplete (aka no =,+,...), but who cares
{
    public static Vector2Int Zero { get; } = new Vector2Int(0, 0);
    public static Vector2Int One { get; } = new Vector2Int(1, 1);

    public int x { get; set; } = 0;
    public int y { get; set; } = 0;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
