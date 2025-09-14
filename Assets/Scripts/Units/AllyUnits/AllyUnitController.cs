using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnitController : MonoBehaviour
{
    private AllyBlackboard blackboard;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    [Header("Vision")]
    [SerializeField] private Transform visionOrigin;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Ranged Attack")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    // Event to warn allies
    public delegate void WarnEvent(Vector2 enemyPosition);
    public static event WarnEvent OnWarn;

    void Start()
    {
        blackboard = GetComponent<AllyBlackboard>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool Patrol()
    {
        if (blackboard.patrolPoints == null || blackboard.patrolPoints.Length == 0)
            return true;

        Transform targetPoint = blackboard.patrolPoints[blackboard.currentPatrolIndex];
        Vector2 directionToTarget = (targetPoint.position - transform.position).normalized;
        
        // Check if we need to flip the sprite
        if (directionToTarget.x > 0 && !facingRight || directionToTarget.x < 0 && facingRight)
        {
            Flip();
        }

        // Move towards patrol point
        rb.velocity = directionToTarget * blackboard.walkSpeed;
        animator.SetBool("IsMoving", true);

        // Check if we've reached the current patrol point
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            blackboard.currentPatrolIndex = (blackboard.currentPatrolIndex + 1) % blackboard.patrolPoints.Length;
            return true;
        }

        CheckVision();
        return false;
    }

    public void PlayIdleAnimation()
    {
        animator.SetTrigger("IdleAction");
    }

    public bool InvestigateSound()
    {
        if (!blackboard.isInvestigatingSoundSource)
            return true;

        Vector2 directionToSound = (blackboard.lastSoundPosition - (Vector2)transform.position).normalized;
        
        // Turn towards the sound
        if (directionToSound.x > 0 && !facingRight || directionToSound.x < 0 && facingRight)
        {
            Flip();
        }

        // Move towards the sound source
        if (Vector2.Distance(transform.position, blackboard.lastSoundPosition) > 0.5f)
        {
            rb.velocity = directionToSound * blackboard.walkSpeed;
            animator.SetBool("IsMoving", true);
            return false;
        }
        
        // Reached the sound source
        rb.velocity = Vector2.zero;
        animator.SetBool("IsMoving", false);
        blackboard.ResetAlertState();
        return true;
    }

    public bool WarnAction()
    {
        // Warning animation
        animator.SetTrigger("Warn"); // Requires a trigger for warning animation

        // Send warning signal to allies (send the position of the first detected enemy)
        if (blackboard.detectedEnemies.Count > 0)
        {
            Vector2 enemyPos = blackboard.detectedEnemies[0].position;
            OnWarn?.Invoke(enemyPos);
            blackboard.isWarning = true;
            blackboard.warnedEnemyPosition = enemyPos;
        }

        // Move to a strategic position (here: move close to the enemy but keep a safe distance)
        if (blackboard.isWarning && blackboard.warnedEnemyPosition != Vector2.zero)
        {
            Vector2 dir = (blackboard.warnedEnemyPosition - (Vector2)transform.position).normalized;
            float dist = Vector2.Distance(transform.position, blackboard.warnedEnemyPosition);
            float stopDist = 2.5f; // Keep a safe distance
            if (dist > stopDist)
            {
                rb.velocity = dir * blackboard.walkSpeed;
                animator.SetBool("IsMoving", true);
                return false;
            }
            else
            {
                rb.velocity = Vector2.zero;
                animator.SetBool("IsMoving", false);
                return true;
            }
        }
        return false;
    }

    public bool EngageAction()
    {
        // If morale is too low, panic (could add random movement or idle)
        if (blackboard.morale < blackboard.moraleThreshold)
        {
            animator.SetTrigger("Panic"); // Requires a panic animation
            rb.velocity = Vector2.zero;
            return true; // End engage, could transition to surrender or flee
        }

        // Find the closest enemy in range
        Transform target = null;
        float minDist = float.MaxValue;
        foreach (var enemy in blackboard.detectedEnemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.position);
            if (dist < minDist && dist <= blackboard.engageRange)
            {
                minDist = dist;
                target = enemy;
            }
        }
        blackboard.currentTarget = target;
        if (target == null)
        {
            // No enemy in range, disengage
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            blackboard.ResetEngageState();
            return true;
        }

        Vector2 dirToTarget = (target.position - transform.position).normalized;
        float distToTarget = Vector2.Distance(transform.position, target.position);

        // Flip to face target
        if (dirToTarget.x > 0 && !facingRight || dirToTarget.x < 0 && facingRight)
        {
            Flip();
        }

        switch (blackboard.combatType)
        {
            case AllyCombatType.Melee:
                // Move to enemy and attack if close
                if (distToTarget > 1.2f)
                {
                    rb.velocity = dirToTarget * blackboard.walkSpeed;
                    animator.SetBool("IsMoving", true);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    animator.SetBool("IsMoving", false);
                    animator.SetTrigger("Attack"); // Requires attack animation
                    // Add block/parry randomly
                    if (Random.value < 0.2f)
                        animator.SetTrigger("Block");
                }
                break;
            case AllyCombatType.Ranged:
                // Stay behind melee allies if possible
                float safeDist = 2.5f;
                if (distToTarget < safeDist)
                {
                    // Move back to keep distance
                    rb.velocity = -dirToTarget * blackboard.walkSpeed;
                    animator.SetBool("IsMoving", true);
                }
                else if (distToTarget <= blackboard.engageRange)
                {
                    rb.velocity = Vector2.zero;
                    animator.SetBool("IsMoving", false);
                    animator.SetTrigger("Attack"); // Requires shoot animation
                    ShootArrow();
                }
                break;
            case AllyCombatType.Support:
                // Find ally to buff/heal
                Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, blackboard.supportRange);
                foreach (var col in allies)
                {
                    if (col.CompareTag("Ally") && col.gameObject != this.gameObject)
                    {
                        // Example: trigger heal/buff animation
                        animator.SetTrigger("Buff");
                        break;
                    }
                }
                rb.velocity = Vector2.zero;
                animator.SetBool("IsMoving", false);
                break;
        }
        return false;
    }

    private void OnEnable()
    {
        OnWarn += HandleWarnSignal;
    }
    private void OnDisable()
    {
        OnWarn -= HandleWarnSignal;
    }
    private void HandleWarnSignal(Vector2 enemyPosition)
    {
        if (Vector2.Distance(transform.position, enemyPosition) <= blackboard.detectionRange * 2)
        {
            blackboard.ReceiveWarnSignal(enemyPosition);
        }
    }

    private void CheckVision()
    {
        // Simple line of sight check
        if (visionOrigin != null)
        {
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(visionOrigin.position, direction, blackboard.detectionRange, enemyLayer | obstacleLayer);
            
            if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
            {
                if (!blackboard.detectedEnemies.Contains(hit.transform))
                {
                    blackboard.detectedEnemies.Add(hit.transform);
                    // This will trigger transition to Alert state in future implementation
                    // Switch to warning state
                    blackboard.isWarning = true;
                    blackboard.warnedEnemyPosition = hit.transform.position;
                }
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // This method would be called by other units or the game system when combat sounds occur nearby
    public void OnCombatSoundHeard(Vector2 soundPosition)
    {
        if (Vector2.Distance(transform.position, soundPosition) <= blackboard.hearingRange)
        {
            blackboard.HandleCombatSound(soundPosition);
        }
    }

    public void ShootArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null && blackboard.currentTarget != null)
        {
            Vector2 dir = (blackboard.currentTarget.position - arrowSpawnPoint.position).normalized;
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            ArrowController arrowCtrl = arrow.GetComponent<ArrowController>();
            if (arrowCtrl != null)
            {
                arrowCtrl.Init(dir, blackboard.currentTarget);
            }
        }
    }
}
