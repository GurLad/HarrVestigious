using Godot;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

public class Unit : Spatial
{
    public enum SFXType { Begin, Attack, Damaged }
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
    public Vector2 AttackRange = Vector2.One;
    [Export]
    public Texture Icon;
    [Export]
    public string UnitType;
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
    private bool _hasVest;
    public bool HasVest
    {
        get
        {
            return _hasVest;
        }
        set
        {
            _hasVest = value;
            if (!HasVest)
            {
                RemoveAction<UAGiveVest>();
                vestObject.Visible = false;
            }
            else
            {
                AttachAction(new UAGiveVest());
                vestObject.Visible = true;
            }
        }
    }
    public List<AUnitAction> Actions = new List<AUnitAction>();
    public bool Moved = false;
    public bool Stunned;
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
    private Particles damageParticles;
    private AudioStreamPlayer3D audioPlayer;
    // Misc
    private Random rng = new Random();

    public void Init()
    {
        vestObject = GetNode<Spatial>("Anchor/Vest");
    }

    public override void _Ready()
    {
        base._Ready();
        anchorAnimations = GetNode<UnitAnchorAnimations>("Anchor");
        damageParticles = GetNode<Particles>("Anchor/MeshInstance/DamageParticles");
        audioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer");
        anchorAnimations.AddAnimation(UnitAnchorAnimations.Mode.Breath);
        // All units can move, attack and wait
        AttachAction(new UAMove());
        AttachAction(new UAAttack());
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
            if (resetExhaustedActions)
            {
                PlaySFX(SFXType.Begin);
            }
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

    public void RemoveAction<T>() where T : AUnitAction
    {
        T target = (T)Actions.Find(a => a is T);
        if (target != null)
        {
            Actions.Remove(target);
        }
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
        else
        {
            List<Vector2Int> possibleMoves = Pathfinder.GetAttackArea(Pos, action.Range);
            foreach (Vector2Int move in possibleMoves)
            {
                FloorMarker.AddMarker(move, FloorMarker.MarkType.Attack, action);
            }
        }
    }

    public void AddPreview(bool previewMove, Vector2Int range = null)
    {
        if (previewMove)
        {
            List<Vector2Int> possibleMoves = Pathfinder.GetMoveArea(Pos, Movement);
            foreach (Vector2Int move in possibleMoves)
            {
                FloorMarker.AddMarker(move, FloorMarker.MarkType.PreviewMove);
            }
        }
        else
        {
            List<Vector2Int> possibleMoves = Pathfinder.GetAttackArea(Pos, range);
            foreach (Vector2Int move in possibleMoves)
            {
                FloorMarker.AddMarker(move, FloorMarker.MarkType.PreviewAttack);
            }
        }
    }

    public void RemovePreview(bool previewMove)
    {
        if (previewMove)
        {
            FloorMarker.ClearMarkers(FloorMarker.MarkType.PreviewMove);
        }
        else
        {
            FloorMarker.ClearMarkers(FloorMarker.MarkType.PreviewAttack);
        }
    }

    public void PlaySFX(SFXType type)
    {
        audioPlayer.Stream = ResourceLoader.Load<AudioStream>("res://SFX/" + UnitType + type + rng.Next(1, 4) + ".mp3");
        audioPlayer.Play();
    }

    // Attacks & stuff

    public void TakeDamage(int amount)
    {
        Health -= amount;
        damageParticles.Emitting = true;
        PlaySFX(SFXType.Damaged);
        if (Health <= 0)
        {
            // TBA
        }
    }

    public void _OnMouseEntered()
    {
        AddPreview(true);
    }

    public void _OnMouseLeave()
    {
        RemovePreview(true);
    }

    public void _OnInputEvent(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shapeIdx)
    {
        FloorMarker.FloorInputEvent(Pos, camera, inputEvent, position, normal, shapeIdx);
    }

    protected virtual void AIAction()
    {
        // By default, does the wait action (aka action 0)
        UseAction<UAWait>();
    }
}
