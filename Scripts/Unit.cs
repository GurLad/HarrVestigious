using Godot;
using System;
using System.Collections.Generic;

public class Unit : Spatial
{
    // Stats
    [Export]
    public int Health;
    [Export]
    public int Soul;
    [Export]
    public int Attack;
    [Export]
    public int Defense;
    [Export]
    public int Movement;
    [Export]
    public Texture Icon;
    // Data
    private Vector2Int _pos;
    public Vector2Int Pos
    {
        get
        {
            return _pos;
        }
        set
        {
            if (_pos != null)
            {
                Pathfinder.MoveObject(_pos, value);
            }
            _pos = value;
        }
    }
    public List<AUnitAction> Actions = new List<AUnitAction>();
    public bool HasVest;
    public bool Moved = false;
    // External objects
    public TurnFlowController TurnFlowController;
    public FloorMarker FloorMarker;
    public PlayerUIController PlayerUIController;
    // For animations
    private AAnimation currentAnimation;
    private Queue<Action> actionQueue = new Queue<Action>();
    // Internal objects
    private UnitAnchorAnimations anchorAnimations;
    private Spatial vestObject;

    public override void _Ready()
    {
        base._Ready();
        vestObject = GetNode<Spatial>("Anchor/Vest");
        anchorAnimations = GetNode<UnitAnchorAnimations>("Anchor");
        anchorAnimations.AddAnimation(UnitAnchorAnimations.Mode.Breath);
        if (HasVest)
        {
            anchorAnimations.AddAnimation(UnitAnchorAnimations.Mode.Vest);
            vestObject.Visible = true;
        }
        // All units can wait and move
        AttachAction(new UAMove());
        AttachAction(new UAWait());
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if ((currentAnimation?.Done ?? true) && actionQueue.Count > 0)
        {
            if (currentAnimation != null)
            {
                // Run one more frame to make sure it actually ends on 1
                currentAnimation.AnimateFrame(1);
                currentAnimation.QueueFree();
                currentAnimation = null;
            }
            actionQueue.Dequeue().Invoke();
        }
    }

    public void BeginTurn(bool resetExhaustedActions = true)
    {
        if (resetExhaustedActions)
        {
            Actions.ForEach(a => a.Exhausted = false);
        }
        if (HasVest)
        {
            PlayerUIController.ShowUI(this);
        }
        else
        {
            AIAction();
        }
    }

    public void QueueAnimation<T, S>(T animation, S animationArgs) where S : AAnimationArgs where T : AAnimation<S>
    {
        AddChild(animation);
        actionQueue.Enqueue(() =>
        {
            currentAnimation = animation.Begin(this, animationArgs);
        });
    }

    public void QueueImmediateAction(Action action)
    {
        actionQueue.Enqueue(action);
    }

    public void AttachAction(AUnitAction unitAction)
    {
        unitAction.AttachToUnit(this);
        Actions.Add(unitAction);
    }

    public void UseAction<T>(Vector2Int targetPos = null) where T : AUnitAction
    {
        T target = (T)Actions.Find(a => a is T);
        if (target != null)
        {
            target.Activate(targetPos);
        }
        else
        {
            throw new Exception("Unit can't use " + nameof(T) + "!");
        }
    }

    public void PrepareTargetedAction(AUnitAction action)
    {
        if (action.UseMoveMarkers)
        {
            List<Vector2Int> possibleMoves = Pathfinder.GetMoveArea(Pos, Movement);
            foreach (Vector2Int move in possibleMoves)
            {
                FloorMarker.AddMarker(move, FloorMarker.MarkType.Move, action);
            }
        }
    }

    public void _OnMouseEntered()
    {
        List<Vector2Int> possibleMoves = Pathfinder.GetMoveArea(Pos, Movement);
        foreach (Vector2Int move in possibleMoves)
        {
            FloorMarker.AddMarker(move, FloorMarker.MarkType.PreviewMove);
        }
    }

    public void _OnMouseLeave()
    {
        List<Vector2Int> possibleMoves = Pathfinder.GetMoveArea(Pos, Movement);
        foreach (Vector2Int move in possibleMoves)
        {
            FloorMarker.RemoveMarker(move, FloorMarker.MarkType.PreviewMove);
        }
    }

    protected virtual void AIAction()
    {
        // By default, does the wait action (aka action 0)
        UseAction<UAWait>();
    }
}
