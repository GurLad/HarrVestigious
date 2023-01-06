using Godot;
using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
    public static S SafeGetKey<T, S>(this Dictionary<T, S> dictionary, T key, S defaultValue = default)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }

    public static void AddOrSet<T, S>(this Dictionary<T, S> dictionary, T key, S value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
        else
        {
            dictionary[key] = value;
        }
    }

    public static Vector3 To3D(this Vector2Int vector2Int)
    {
        return new Vector3(vector2Int.y, 0, vector2Int.x);
    }

    public static Vector2Int To2D(this Vector3 vector3)
    {
        return new Vector2Int(Mathf.RoundToInt(vector3.z), Mathf.RoundToInt(vector3.x));
    }
}
