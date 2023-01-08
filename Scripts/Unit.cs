using Godot;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using static System.Collections.Specialized.BitVector32;

public class Unit : Spatial
{
    public enum SFXType { Begin, Attack, Damaged, Move, Die }
    public static readonly int STAT_COUNT = 6;
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
            if (FloorMarker?.IsWinFloor(_pos.x, _pos.y) ?? false)
            {
                QueueAnimation(new AnimDie(), new AnimDie.Args(1));
                QueueImmediateAction(() => TurnFlowController.Win());
                QueueImmediateAction(() => actionQueue.Clear());
            }
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
                anchorAnimations?.RemoveAnimation(UnitAnchorAnimations.Mode.Shake);
            }
            else
            {
                AttachAction(new UAGiveVest());
                vestObject.Visible = true;
                if (Soul <= 1)
                {
                    anchorAnimations?.AddAnimation(UnitAnchorAnimations.Mode.Shake);
                }
            }
        }
    }
    private bool _stunned;
    public bool Stunned
    {
        get
        {
            return _stunned;
        }
        set
        {
            _stunned = value;
            stunnedObject.Visible = Stunned;
        }
    }
    public List<AUnitAction> Actions = new List<AUnitAction>();
    public bool Moved = false;
    public bool Animating => currentAnimation != null || actionQueue.Count > 0;
    // External objects
    public TurnFlowController TurnFlowController;
    public FloorMarker FloorMarker;
    public PlayerUIController PlayerUIController;
    // Misc external
    public Vector2Int PatrolPoint;
    // For animations
    private AAnimation currentAnimation;
    private Queue<Action> actionQueue = new Queue<Action>();
    // Internal objects
    private UnitAnchorAnimations anchorAnimations;
    private Spatial vestObject;
    private Spatial stunnedObject;
    private Particles damageParticles;
    private AudioStreamPlayer3D audioPlayer;
    // Misc
    private Random rng = new Random();
    private Vector2Int initalPos;

    public void Init()
    {
        vestObject = GetNode<Spatial>("Anchor/Vest");
        stunnedObject = GetNode<Spatial>("Anchor/StunnedObject");
    }

    public override void _Ready()
    {
        base._Ready();
        anchorAnimations = GetNode<UnitAnchorAnimations>("Anchor");
        damageParticles = GetNode<Particles>("Anchor/MeshInstance/DamageParticles");
        audioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer");
        anchorAnimations.AddAnimation(UnitAnchorAnimations.Mode.Breath);
        initalPos = new Vector2Int(Pos.x, Pos.y);
        // All units can move, attack and wait
        AttachAction(new UAMove());
        AttachAction(new UAAttack());
        AttachAction(new UAWait());
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (currentAnimation?.Done ?? actionQueue.Count > 0)
        {
            if (currentAnimation != null)
            {
                // Run one more frame to make sure it actually ends on 1
                currentAnimation.AnimateFrame(1);
                currentAnimation.QueueFree();
                currentAnimation = null;
            }
            if (actionQueue.Count > 0)
            {
                actionQueue.Dequeue().Invoke();
            }
        }
    }

    public void BeginTurn(bool resetExhaustedActions = true)
    {
        if (resetExhaustedActions)
        {
            Actions.ForEach(a => a.Exhausted = false);
        }
        if (actionQueue.Count > 0)
        {
            QueueImmediateAction(() => BeginTurn(resetExhaustedActions));
            return;
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
            if (Stunned)
            {
                Stunned = false;
                UseAction<UAWait>();
            }
            else
            {
                AIAction();
            }
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
        Actions.Sort((a, b) => a.SortOrder > b.SortOrder ? 1 : (a.SortOrder < b.SortOrder ? -1 : 0));
    }

    public bool CanUseAction<T>() where T : AUnitAction
    {
        T target = (T)Actions.Find(a => a is T);
        if (target != null)
        {
            return !target.Exhausted;
        }
        return false;
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
        audioPlayer.Stream = ResourceLoader.Load<AudioStream>("res://SFX/" + UnitType + type + (type == SFXType.Die ? "" : rng.Next(1, 4).ToString()) + ".mp3");
        audioPlayer.Play();
    }

    public void EndTurn()
    {
        if (HasVest)
        {
            Soul--;
            TurnFlowController.UpdateUI();
            if (Soul <= 0)
            {
                Die();
            }
            else if (Soul <= 1)
            {
                anchorAnimations.AddAnimation(UnitAnchorAnimations.Mode.Shake);
            }
        }
        QueueImmediateAction(() => Moved = true);
    }

    // Stats & stuff

    public string GetStatString(int statID)
    {
        return statID switch
        {
            0 => Health.ToString(),
            1 => Soul.ToString(),
            2 => Attack.ToString(),
            3 => Defense.ToString(),
            4 => Movement.ToString(),
            5 => AttackRange.y == AttackRange.x ? AttackRange.x.ToString() : (AttackRange.x + "-" + AttackRange.y),
            _ => null
        };
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        damageParticles.Emitting = true;
        PlaySFX(SFXType.Damaged);
        TurnFlowController.UpdateUI();
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        QueueImmediateAction(() => PlaySFX(SFXType.Die));
        QueueAnimation(new AnimDie(), new AnimDie.Args(0.7f));
        QueueImmediateAction(() => TurnFlowController.RemoveUnit(this));
    }

    public void _OnMouseEntered()
    {
        AddPreview(true);
        TurnFlowController.MarkUnit(this);
    }

    public void _OnMouseLeave()
    {
        RemovePreview(true);
        TurnFlowController.UnmarkUnit(this);
    }

    public void _OnInputEvent(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shapeIdx)
    {
        FloorMarker.FloorInputEvent(Pos, camera, inputEvent, position, normal, shapeIdx);
    }

    protected virtual void AIAction()
    {
        Unit vest = TurnFlowController.GetVestUnit();
        List<Vector2Int> dangerArea = Pathfinder.GetMoveArea(Pos, Movement);

        bool TryAttackFromPlace()
        {
            if (CanAttackFrom(Pos, vest.Pos))
            {
                UseAction<UAAttack>(vest.Pos);
                return true;
            }
            return false;
        }
        bool TryMoveToAttack()
        {
            if (!CanUseAction<UAMove>())
            {
                return false;
            }
            foreach (var pos in dangerArea)
            {
                if (CanAttackFrom(pos, vest.Pos))
                {
                    UseAction<UAMove>(pos);
                    return true;
                }
            }
            return false;
        }

        switch (UnitType)
        {
            case "Orc":
                if (TryAttackFromPlace())
                {
                    return;
                }
                else
                {
                    UseAction<UAWait>();
                }
                break;
            case "Goblin":
                if (TryAttackFromPlace())
                {
                    return;
                }
                else if (TryMoveToAttack())
                {
                    return;
                }
                else if (CanUseAction<UAMove>())
                {
                    if (Pos == initalPos)
                    {
                        UseAction<UAMove>(PatrolPoint);
                    }
                    else
                    {
                        UseAction<UAMove>(initalPos);
                    }
                }
                else
                {
                    UseAction<UAWait>();
                }
                break;
            default:
                break;
        }
    }

    private bool CanAttackFrom(Vector2Int pos, Vector2Int target)
    {
        return Vector2Int.Distance(pos, target) <= 1.001f;
    }
}
