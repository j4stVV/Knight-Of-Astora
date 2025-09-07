using System.Collections.Generic;

public class  SelectorNode : BTNode
{
    private List<BTNode> _children;
    public SelectorNode(List<BTNode> children)
    {
        _children = children;
    }
    public override State Evaluate()
    {
        foreach( var child in _children)
        {
            var result = child.Evaluate();
            if (result == State.Success)
            {
                _state = State.Success;
                return _state;
            }
            if (result == State.Running)
            {
                _state = State.Running;
                return _state;
            }
        }
        _state = State.Failure;
        return _state;
    }
}