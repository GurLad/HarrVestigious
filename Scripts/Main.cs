using Godot;
using System;

public class Main : Node
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.F11)
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
            }
        }
    }
}
