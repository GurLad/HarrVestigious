using Godot;
using Godot.Collections;
using System;

public class Floor : Spatial
{
    private Dictionary<FloorMarker.MarkType, Spatial> markers = new Dictionary<FloorMarker.MarkType, Spatial>();

    public override void _Ready()
    {
        base._Ready();
        markers.Add(FloorMarker.MarkType.PreviewMove, GetNode<Spatial>("Markers/PreviewMove"));
        markers.Add(FloorMarker.MarkType.PreviewAttack, GetNode<Spatial>("Markers/PreviewAttack"));
        markers.Add(FloorMarker.MarkType.Move, GetNode<Spatial>("Markers/Move"));
        markers.Add(FloorMarker.MarkType.Attack, GetNode<Spatial>("Markers/Attack"));
    }

    public void AddMarker(FloorMarker.MarkType markType)
    {
        markers[markType].Visible = true;
    }

    public void RemoveMarker(FloorMarker.MarkType markType)
    {
        markers[markType].Visible = false;
    }
}
