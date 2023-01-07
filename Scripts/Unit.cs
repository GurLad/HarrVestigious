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
    public Vector2Int Pos;
    public List<AUnitAction> Actions = new List<AUnitAction>();
    public bool HasVest;
    public bool Moved = false;
    // For animations
    private AAnimation currentAnimation;
    private Queue<Action> actionQueue = new Queue<Action>();
    private UnitAnchorAnimations anchorAnimations;
    private Spatial vestObject;

    public void Move(Vector2Int target) // Move is technically an action that everything has - TODO: move to an action
    {
        // TODO: play move animation
        actionQueue.Enqueue(() => Pos = target);
    }

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
        // All units have the wait action
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

    public void BeginTurn()
    {
        if (HasVest)
        {
            // TBA: player UI
            Moved = true;
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
            if (target.RequiresTarget && targetPos == null)
            {
                throw new Exception(target.Name + " requires a target!");
            }
            target.Activate(targetPos);
            if (!target.FreeAction)
            {
                QueueImmediateAction(() => Moved = true);
            }
        }
        else
        {
            throw new Exception("Unit can't use " + nameof(T) + "!");
        }
    }

    public void _OnMouseEntered()
    {

    }

    public void _OnMouseLeave()
    {

    }

    protected virtual void AIAction()
    {
        // By default, does the wait action (aka action 0)
        UseAction<UAWait>();
    }
}
