using UnityEngine;

namespace BehaviorTree
{
    // Base class for BT-based enemy using EnemyBlackboard
    public abstract class EnemyBTBase : MonoBehaviour
    {
        public EnemyBlackboard blackboard;

        protected virtual void Awake()
        {
            blackboard = new EnemyBlackboard();
        }

        // Main BT tick
        public abstract void Tick();

        // Virtual methods for main behaviors
        public virtual void Scout() { }
        public virtual void Attack() { }
        public virtual void Engage() { }
        public virtual void Frenzy() { }
        public virtual void Retreat() { }
    }
}
