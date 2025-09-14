using System.Collections.Generic;
using BehaviorTree;

public class MiniBossBT
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
        var buffAllies = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => _bb.nearbyAllies.Count > 0),
            new ActionNode(() =>
            {
                _controller.BuffAllies();
                return BehaviorState.Success;
            })
        });
        var attackPlayer = new SequenceNode(new BTNode[]
        {
            new ConditionNode (() => _bb.nearbyAllies.Count > 0),
            new ActionNode(() =>
            {
                _controller.Attack(_bb.targetPlayer);
                return BehaviorState.Success;
            })
        });
        var attackStructure = new ActionNode(() => _controller.Attack(_bb.targetStructure));
        var moveForward = new ActionNode(() => _controller.MoveForward());
        var engage = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => _bb.hp < _bb.hpLowThreshold),
            new ActionNode(() =>
            {
                _controller.Enrage();
                return BehaviorState.Success;
            })
        });
        var retreat = new SequenceNode(new BTNode[]
        {
            new ConditionNode(() => _bb.hp < _bb.hpLowThreshold),
            new ActionNode(() =>
            {
                _controller.Retreat();
                return BehaviorState.Success;
            })
        });
        _root = buffAllies;
    }

    public void Tick()
    {
        _root?.Evaluate();
    }
}