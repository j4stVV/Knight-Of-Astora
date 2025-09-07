public abstract class BTNode
{
    public enum State { Running, Success, Failure }
    protected State _state;
    public State NodeState => _state;
    public abstract State Evaluate();
}