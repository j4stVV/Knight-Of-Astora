using System.Collections.Generic;
using BehaviorTree;
using UnityEngine;

public class MiniBossBT : MonoBehaviour
{
    private BTNode _root;
    private EnemyBlackboard _bb;
    private MiniBossController _controller;

    public MiniBossBT(EnemyBlackboard bb, MiniBossController controller)
    {
        _bb = bb;
        _controller = controller;
        BuildTree();
    }
    public void BuildTree()
    {
        // Retreat Sequence
        var retreat = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => RetreatCondition()),
            new ActionNode(() => _controller.MoveToBaseSpawn()),
            new ActionNode(() => _controller.HealAtSpawn()),
            new ActionNode(() => _controller.SummonUnitsUntilLimit(12))
        });

        // Frenzy Sequence
        var frenzy = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => FrenzyCondition()),
            new ActionNode(() => _controller.ReduceCooldowns()),
            new ActionNode(() => _controller.ForceAdvanceToNextArea())
        });

        // Engage Sequence (Selector for best available action)
        var engageSelector = new SelectorNode(new BTNode[]
        {
            new ActionNode(() => _controller.CastDarkBoltAtTarget()),
            new ActionNode(() => _controller.SummonSkeletonsUntilLimit(12)),
            new ActionNode(() => _controller.BuffAlliesDamageBoost())
        });
        var engage = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => EnemyInRange()),
            engageSelector
        });

        // Attack Sequence
        var attack = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => ReadyToAdvance()),
            new ActionNode(() => _controller.CommandArmyMoveToNextArea()),
            new ActionNode(() => _controller.SetFlagEnemy())
        });

        // Command Sequence
        var command = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => ArmySizeBelowLimit(12)),
            new ActionNode(() => _controller.SummonSkeletons()),
            new ActionNode(() => _controller.BuffAlliesHealNearby())
        });

        // Main Selector
        _root = new SelectorNode(new BTNode[]
        {
            retreat,
            frenzy,
            engage,
            attack,
            command
        });
    }

    public void Tick()
    {
        _root?.Evaluate();
    }

    // --- BT Condition Methods ---
    private bool RetreatCondition()
    {
        // HP < 30% OR Allies == 0
        return _bb.hp < _bb.hpMax * 0.3f || _bb.nearbyAllies.Count == 0;
    }
    private bool FrenzyCondition()
    {
        // GameTime > 10min OR ArmySize < 3
        return (Time.time > 600f) || (_bb.nearbyAllies.Count < 3);
    }
    private bool EnemyInRange()
    {
        // Check if any enemy is in range
        return _bb.detectedEnemies.Count > 0;
    }
    private bool ReadyToAdvance()
    {
        // TODO: Check if ready to advance
        return false;
    }
    private bool ArmySizeBelowLimit(int limit)
    {
        // ArmySize < limit
        return _bb.nearbyAllies.Count < limit;
    }
}