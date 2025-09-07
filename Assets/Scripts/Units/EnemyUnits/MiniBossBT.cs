using System.Collections.Generic;
using UnityEditor;

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
        var buffAllies = new SequenceNode(new List<BTNode>
        {
            new ConditionNode(() => _bb.nearbyAllies.Count > 0),
            new ActionNode(() =>
            {
                _controller.BuffAllies();
                return BTNode.State.Success;
            })
        });
        var attackPlayer = new SequenceNode(new List<BTNode> 
        { 
            new ConditionNode (() => _bb.nearbyAllies.Count > 0),
            new ActionNode(() =>
            {
                _controller.Attack(_bb.targetPlayer);
                return BTNode.State.Success;
            })
        });
        var attackStructure = new ActionNode(() => _controller.Attack(_bb.targetStructure));
        var moveForward = new ActionNode(() => _controller.MoveForward());
        var engage = new SequenceNode(new List<BTNode>
        {
            new ConditionNode(() => _bb.hp < _bb.hpLowThreshold),
            new ActionNode(() =>
            {
                _controller.Enrage();
                return BTNode.State.Success;
            })
        });
        var retreat = new SequenceNode(new List<BTNode>
        {
            new ConditionNode(() => _bb.hp < _bb.hpLowThreshold),
            new ActionNode(() =>
            {
                _controller.Retreat();
                return BTNode.State.Success;
            })
        });
    }

    public void Tick()
    {
        _root.Evaluate();
    }
}