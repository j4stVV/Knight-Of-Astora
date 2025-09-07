using System;

public class ActionNode : BTNode
{
    private Func<State> _action;
    public ActionNode(Func<State> action)
    {
        _action = action;
    }
    public override State Evaluate()
    {
        _state = _action();
        return _state;
    }
}

public class  BuffAllyActionNode
{
    private MiniBossController controller;
    private EnemyBlackboard blackboard;

}