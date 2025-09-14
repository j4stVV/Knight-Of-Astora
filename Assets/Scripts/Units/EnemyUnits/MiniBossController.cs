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
