using Godot;
using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
    public static Vector3 To3D(this Vector2Int vector2Int)
    {
        return new Vector3(vector2Int.y, 0, vector2Int.x) * LevelGenerator.PHYSICAL_SIZE;
    }

    public static Vector2Int To2D(this Vector3 vector3)
    {
        return new Vector2Int(Mathf.RoundToInt(vector3.z / LevelGenerator.PHYSICAL_SIZE), Mathf.RoundToInt(vector3.x / LevelGenerator.PHYSICAL_SIZE));
    }

    public static float NextFloat(this Random random, Vector2 range)
    {
        return random.NextFloat(range.x, range.y);
    }

    public static float NextFloat(this Random random, float minValue, float maxValue)
    {
        return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
    }

    public static float Percent(this Timer timer)
    {
        return 1 - timer.TimeLeft / timer.WaitTime;
    }
}
