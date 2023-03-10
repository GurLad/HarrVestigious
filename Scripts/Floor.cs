using Godot;
using Godot.Collections;
using System;

public class Floor : Spatial
{
    public Vector2Int Pos;
    public bool Win;
    private Dictionary<FloorMarker.MarkType, Spatial> markers = new Dictionary<FloorMarker.MarkType, Spatial>();
    private AUnitAction clickAction;
    private FloorMarker floorMarker;
    private FloorMarker.MarkType currentMarks;

    public override void _Ready()
    {
        base._Ready();
        markers.Add(FloorMarker.MarkType.PreviewMove, GetNode<Spatial>("Markers/PreviewMove"));
        markers.Add(FloorMarker.MarkType.PreviewAttack, GetNode<Spatial>("Markers/PreviewAttack"));
        markers.Add(FloorMarker.MarkType.Move, GetNode<Spatial>("Markers/Move"));
        markers.Add(FloorMarker.MarkType.Attack, GetNode<Spatial>("Markers/Attack"));
    }

    public void Init(Vector2Int pos, FloorMarker floorMarker)
    {
        Pos = pos;
        this.floorMarker = floorMarker;
    }

    public void AddMarker(FloorMarker.MarkType markType, AUnitAction action = null)
    {
        currentMarks |= markType;
        markers[markType].Visible = true;
        if (action != null)
        {
            clickAction = action;
        }
    }

    public void RemoveMarker(FloorMarker.MarkType markType)
    {
        currentMarks &= ~markType;
        markers[markType].Visible = false;
        if (markType == FloorMarker.MarkType.Move || markType == FloorMarker.MarkType.Attack)
        {
            clickAction = null;
        }
    }

    public void _OnInputEvent(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shapeIdx)
    {
        if (inputEvent is InputEventMouseButton && clickAction != null)
        {
            Unit atPos = floorMarker.TurnFlowController.GetUnitAtPos(Pos);
            if ((currentMarks & FloorMarker.MarkType.Attack) != FloorMarker.MarkType.None &&
                (atPos == null || !clickAction.ValidTarget(atPos)))
            {
                return;
            }
            clickAction.Activate(Pos);
            floorMarker.ClearMarkers();
        }
    }
}
