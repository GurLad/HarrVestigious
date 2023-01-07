using Godot;
using System;

public class FloorMarker : Node
{
    public enum MarkType { None, PreviewMove = 1, PreviewAttack = 2, Move = 4, Attack = 8 }
    private Floor[,] floors;

    public void NewLevel(Vector2Int size)
    {
        floors = new Floor[size.x, size.y];
    }

    public void AddFloor(int x, int y, Floor floor)
    {
        floors[x, y] = floor;
    }

    public void AddMarker(Vector2Int pos, MarkType markType)
    {
        AddMarker(pos.x, pos.y, markType);
    }

    public void AddMarker(int x, int y, MarkType markType)
    {
        if (floors[x, y] == null)
        {
            throw new Exception("Marking something that isn't a floor!");
        }
        floors[x, y].AddMarker(markType);
    }

    public void RemoveMarker(Vector2Int pos, MarkType markType)
    {
        RemoveMarker(pos.x, pos.y, markType);
    }

    public void RemoveMarker(int x, int y, MarkType markType)
    {
        if (floors[x, y] == null)
        {
            throw new Exception("Marking something that isn't a floor!");
        }
        floors[x, y].RemoveMarker(markType);
    }
}
