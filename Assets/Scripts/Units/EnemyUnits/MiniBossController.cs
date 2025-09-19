using UnityEngine;
using BehaviorTree;

public class MiniBossController : MonoBehaviour
{
    public EnemyBlackboard blackboard;

    private MiniBossBT _bt;

    private void Awake()
    {
        blackboard = new EnemyBlackboard();
        blackboard.hpMax = 200;
        blackboard.hp = 200;
        blackboard.hpLowThreshold = 50;
        blackboard.waveActive = true;
    }

    private void Start()
    {
        _bt = new MiniBossBT(blackboard, this);
    }

    private void Update()
    {
        _bt.Tick();
    }

    public void Spawn()
    {
        Debug.Log("MiniBoss spawned.");
    }

    public BehaviorState MoveForward()
    {
        Debug.Log("MiniBoss moving forward.");
        transform.Translate(Vector3.right * Time.deltaTime);
        return BehaviorState.Running;
    }

    public BehaviorState Attack(Transform target)
    {
        if (target == null) return BehaviorState.Failure;
        Debug.Log($"MiniBoss attacking {target.name}");
        return BehaviorState.Success;
    }

    public BehaviorState MoveToBaseSpawn()
    {
        Debug.Log("MiniBoss moving to base spawn.");
        // TODO: Move to spawn/base position
        return BehaviorState.Running;
    }

    public BehaviorState HealAtSpawn()
    {
        Debug.Log("MiniBoss healing at spawn.");
        // TODO: Heal logic
        return BehaviorState.Success;
    }

    public BehaviorState SummonUnitsUntilLimit(int limit)
    {
        Debug.Log($"MiniBoss summoning units until limit {limit}.");
        // TODO: Summon logic
        return BehaviorState.Success;
    }

    public BehaviorState ReduceCooldowns()
    {
        Debug.Log("MiniBoss reducing cooldowns.");
        // TODO: Reduce cooldowns logic
        return BehaviorState.Success;
    }

    public BehaviorState ForceAdvanceToNextArea()
    {
        Debug.Log("MiniBoss force advancing to next area.");
        // TODO: Advance logic
        return BehaviorState.Success;
    }

    public BehaviorState CastDarkBoltAtTarget()
    {
        Debug.Log("MiniBoss casting Dark Bolt at target.");
        // TODO: Cast Dark Bolt logic
        return BehaviorState.Success;
    }

    public BehaviorState SummonSkeletonsUntilLimit(int limit)
    {
        Debug.Log($"MiniBoss summoning skeletons until limit {limit}.");
        // TODO: Summon skeletons logic
        return BehaviorState.Success;
    }

    public BehaviorState BuffAlliesDamageBoost()
    {
        Debug.Log("MiniBoss buffing allies with damage boost.");
        // TODO: Buff damage logic
        return BehaviorState.Success;
    }

    public BehaviorState CommandArmyMoveToNextArea()
    {
        Debug.Log("MiniBoss commanding army to move to next area.");
        // TODO: Command army logic
        return BehaviorState.Success;
    }

    public BehaviorState SetFlagEnemy()
    {
        Debug.Log("MiniBoss setting area flag to enemy.");
        // TODO: Set flag logic
        return BehaviorState.Success;
    }

    public BehaviorState SummonSkeletons()
    {
        Debug.Log("MiniBoss summoning skeletons.");
        // TODO: Summon skeletons logic
        return BehaviorState.Success;
    }

    public BehaviorState BuffAlliesHealNearby()
    {
        Debug.Log("MiniBoss buffing allies with heal.");
        // TODO: Heal nearby allies logic
        return BehaviorState.Success;
    }

    public void BuffAllies()
    {
        float buffRadius = 5f;  // Buff range, can modify in Inspector
        float healAmount = 20f; // Amount of health restored for each small monster 
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, buffRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.gameObject != this.gameObject)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Heal(healAmount);
                    Debug.Log($"Buffed {hit.name} for {healAmount} HP");
                }
            }
        }
        Debug.Log("MiniBoss buffed nearby allies.");
    }

    public void Enrage()
    {
        if (!blackboard.isEnraged)
        {
            Debug.Log("MiniBoss enraged! Increased power.");
            blackboard.isEnraged = true;
        }
    }

    public void Retreat()
    {
        Debug.Log("MiniBoss retreating.");
    }
}
