using System;

public class ConditionNode : BTNode
{
    private Func<bool> _condition;
    public ConditionNode(Func<bool> condition)
    {
        _condition = condition;
    }
    public override State Evaluate()
    {
        if (_condition())
        {
            _state = State.Success;
            return _state;
        }
        _state = State.Failure;
        return _state;
    }
}

public class CheckAllyInRangeNode : BTNode
{
    private EnemyBlackboard _blackboard;
    public CheckAllyInRangeNode(EnemyBlackboard blackboard)
    {
        _blackboard = blackboard;
    }
    public override State Evaluate()
    {
        if (_blackboard.nearbyAllies.Count > 0)
        {
            _state = State.Success;
            return _state;
        }
        _state = State.Failure;
        return _state;
    }
}