using System;

namespace BehaviorTree
{
    public enum BehaviorState
    {
        Success,
        Failure,
        Running
    }

    public abstract class BTNode
    {
        protected BehaviorState _state;
        public BehaviorState state { get { return _state; } }
        public abstract BehaviorState Evaluate();
    }

    public class SelectorNode : BTNode
    {
        protected BTNode[] nodes;

        public SelectorNode(BTNode[] nodes)
        {
            this.nodes = nodes;
        }

        public override BehaviorState Evaluate()
        {
            foreach (var node in nodes)
            {
                switch (node.Evaluate())
                {
                    case BehaviorState.Success:
                        _state = BehaviorState.Success;
                        return _state;
                    case BehaviorState.Running:
                        _state = BehaviorState.Running;
                        return _state;
                    case BehaviorState.Failure:
                        continue;
                }
            }
            _state = BehaviorState.Failure;
            return _state;
        }
    }

    public class SequenceNode : BTNode
    {
        protected BTNode[] nodes;

        public SequenceNode(BTNode[] nodes)
        {
            this.nodes = nodes;
        }

        public override BehaviorState Evaluate()
        {
            bool isAnyNodeRunning = false;
            foreach (var node in nodes)
            {
                switch (node.Evaluate())
                {
                    case BehaviorState.Failure:
                        _state = BehaviorState.Failure;
                        return _state;
                    case BehaviorState.Success:
                        continue;
                    case BehaviorState.Running:
                        isAnyNodeRunning = true;
                        continue;
                    default:
                        _state = BehaviorState.Success;
                        return _state;
                }
            }
            _state = isAnyNodeRunning ? BehaviorState.Running : BehaviorState.Success;
            return _state;
        }
    }

    public class ActionNode : BTNode
    {
        private Func<BehaviorState> action;

        public ActionNode(Func<BehaviorState> action)
        {
            this.action = action;
        }

        public override BehaviorState Evaluate()
        {
            _state = action();
            return _state;
        }
    }

    public class ConditionNode : BTNode
    {
        private Func<bool> condition;

        public ConditionNode(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override BehaviorState Evaluate()
        {
            _state = condition() ? BehaviorState.Success : BehaviorState.Failure;
            return _state;
        }
    }
}