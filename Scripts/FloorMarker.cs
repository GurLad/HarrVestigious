using Godot;
using System;

public class FloorMarker : Node
{
    public enum MarkType { None, PreviewMove = 1, PreviewAttack = 2, Move = 4, Attack = 8 }
    public TurnFlowController TurnFlowController;
    private Floor[,] floors;
    private Vector2Int size;

    public void NewLevel(Vector2Int size, TurnFlowController turnFlowController)
    {
        TurnFlowController = turnFlowController;
        this.size = size;
        floors = new Floor[size.x, size.y];
    }

    public void AddFloor(int x, int y, Floor floor)
    {
        floors[x, y] = floor;
        floors[x, y].Init(new Vector2Int(x, y), this);
    }

    public void FloorInputEvent(Vector2Int pos, Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (floors[pos.x, pos.y] == null)
        {
            throw new Exception("Clicking on a unit that's not a floor!");
        }
        floors[pos.x, pos.y]._OnInputEvent(camera, inputEvent, position, normal, shapeIdx);
    }

    public void ClearMarkers(MarkType type = MarkType.None)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (floors[x, y] != null)
                {
                    if (type != MarkType.None)
                    {
                        floors[x, y].RemoveMarker(type);
                    }
                    else
                    {
                        floors[x, y].RemoveMarker(MarkType.PreviewMove);
                        floors[x, y].RemoveMarker(MarkType.PreviewAttack);
                        floors[x, y].RemoveMarker(MarkType.Move);
                        floors[x, y].RemoveMarker(MarkType.Attack);
                    }
                }
            }
        }
    }

    public void AddMarker(Vector2Int pos, MarkType markType, AUnitAction action = null)
    {
        AddMarker(pos.x, pos.y, markType, action);
    }

    public void AddMarker(int x, int y, MarkType markType, AUnitAction action = null)
    {
        if (floors[x, y] == null)
        {
            throw new Exception("Marking something that isn't a floor!");
        }
        floors[x, y].AddMarker(markType, action);
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
