using System.Collections.Generic;

public class SequenceNode : BTNode
{
    private List<BTNode> _children;
    public SequenceNode(List<BTNode> children)
    {
        _children = children;
    }
    public override State Evaluate()
    {
        bool anyRunning = false;
        foreach( var child in _children)
        {
            switch(child.Evaluate())
            {
                case State.Failure:
                    _state = State.Failure;
                    return _state;
                case State.Running:
                    anyRunning = true;
                    break;
                case State.Success:
                    continue;
            }
        }
        _state = anyRunning ? State.Running : State.Success;
        return _state;
    }
}