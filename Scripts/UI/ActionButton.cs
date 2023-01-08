using Godot;
using System;

public class ActionButton : Button
{
    public AUnitAction Action;
    private PlayerUIController uiController;
    private Unit player;

    public void Init(AUnitAction action, PlayerUIController playerUIController, Unit unit)
    {
        player = unit;
        uiController = playerUIController;
        Action = action;
        Text = action.Name;
    }

    public void _OnMouseEntered()
    {
        uiController.HelpLabel.Text = Action.Description;
        uiController.HelpLabel.GetParent<Control>().Visible = true;
        if (Action.RequiresTarget)
        {
            if (Action.UseMoveMarkers)
            {
                player.AddPreview(true);
            }
            else
            {
                player.AddPreview(false, Action.Range);
            }
        }
    }

    public void _OnMouseLeave()
    {
        uiController.HelpLabel.GetParent<Control>().Visible = false;
        if (Action.RequiresTarget)
        {
            if (Action.UseMoveMarkers)
            {
                player.RemovePreview(true);
            }
            else
            {
                player.RemovePreview(false);
            }
        }
    }

    public void _OnPressed()
    {
        base._Pressed();
        uiController.HideUI();
        if (Action.RequiresTarget)
        {
            player.PrepareTargetedAction(Action);
        }
        else
        {
            Action.Activate();
        }
    }
}
