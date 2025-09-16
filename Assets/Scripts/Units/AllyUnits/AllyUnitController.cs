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
    [SerializeField] private int rayCount = 8; // Number of rays to cast within field of view
    [SerializeField] private float viewDistance = 8f; // How far unit can see
    [SerializeField] private bool showVisionGizmos = true; // For debugging

    [Header("Ranged Attack")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    // Event to warn allies
    public delegate void WarnEvent(Vector2 enemyPosition);
    public static event WarnEvent OnWarn;

    private float lastShootTime = -10f;
    [SerializeField] private float shootCooldown = 1.5f;

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
        if (Vector2.Distance(transform.position, targetPoint.position) <= 2.2f)
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
        animator.SetBool("IsMoving", false);
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

        // Move to a strategic position (move close to the enemy but keep a safe distance)
        if (blackboard.isWarning && blackboard.warnedEnemyPosition != Vector2.zero)
        {
            Vector2 dir = (blackboard.warnedEnemyPosition - (Vector2)transform.position).normalized;
            float dist = Vector2.Distance(transform.position, blackboard.warnedEnemyPosition);
            float stopDist = 7.5f; // Keep a safe distance
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
                    animator.SetTrigger("Attack");
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

    public bool PursueAction()
    {
        // If not currently pursuing, set up pursue state
        if (!blackboard.isPursuing && blackboard.currentTarget != null)
        {
            blackboard.isPursuing = true;
            blackboard.lastKnownEnemyPosition = blackboard.currentTarget.position;
            blackboard.searchTimer = 0f;
        }

        // If lost target (enemy out of vision), change to search
        if (blackboard.currentTarget == null)
        {
            blackboard.searchTimer += Time.deltaTime;
            // Move to last known enemy position
            Vector2 dir = (blackboard.lastKnownEnemyPosition - (Vector2)transform.position).normalized;
            float dist = Vector2.Distance(transform.position, blackboard.lastKnownEnemyPosition);
            if (dist > 0.5f)
            {
                rb.velocity = dir * blackboard.walkSpeed;
                animator.SetBool("IsMoving", true);
            }
            else
            {
                rb.velocity = Vector2.zero;
                animator.SetBool("IsMoving", false);
            }
            // If search time exceeded, stop pursuing
            if (blackboard.searchTimer > blackboard.maxSearchTime)
            {
                blackboard.ResetPursueState();
                return true; // Transition to Alert
            }
            return false; // Still searching
        }

        // If enemy is too far from defense position, stop pursuing
        float distFromStart = Vector2.Distance(transform.position, blackboard.patrolPoints[0].position);
        if (distFromStart > blackboard.chaseRadius)
        {
            blackboard.ResetPursueState();
            return true; // Transition to Alert
        }

        // Pursue the target
        Vector2 dirToTarget = (blackboard.currentTarget.position - transform.position).normalized;
        float distToTarget = Vector2.Distance(transform.position, blackboard.currentTarget.position);
        if (dirToTarget.x > 0 && !facingRight || dirToTarget.x < 0 && facingRight)
        {
            Flip();
        }
        rb.velocity = dirToTarget * blackboard.walkSpeed;
        animator.SetBool("IsMoving", true);

        // If caught up (in engage range), stop pursuing
        if (distToTarget <= blackboard.engageRange)
        {
            blackboard.ResetPursueState();
            return true; // Transition to Engage
        }

        // Placeholder: Detect enemy by sound (not implemented)
        // TODO: Implement sound-based enemy detection for pursue
        return false;
    }

    public bool SurrenderAction()
    {
        // Trigger: HP < threshold or morale is low
        float hpRatio = 1.0f;
        if (blackboard != null && blackboard.maxHP > 0)
        {
            hpRatio = blackboard.currentHP / blackboard.maxHP;
        }
        if (!blackboard.isSurrendering && !blackboard.isLastStand)
        {
            float roll = Random.value;
            if (roll < blackboard.lastStandChance)
            {
                blackboard.isLastStand = true;
                blackboard.surrenderType = SurrenderType.LastStand;
            }
            else
            {
                blackboard.isSurrendering = true;
                blackboard.surrenderType = SurrenderType.Surrender;
            }
        }
        // Last Stand: perform special attack, special animation, then fight until death
        if (blackboard.isLastStand)
        {
            animator.SetTrigger("LastStand"); // Animation: shout, power up, etc.
            // TODO: Trigger special AoE attack here
            // After this, keep fighting until death (EngageAction only)
            // No transition out
            return false;
        }
        // Surrender: run to base, panic animation, only block if attacked
        if (blackboard.isSurrendering)
        {
            animator.SetTrigger("SurrenderPanic"); // Animation: run panic, hands up, etc.
            if (blackboard.surrenderTargetBase != null)
            {
                Vector2 dir = (blackboard.surrenderTargetBase.position - transform.position).normalized;
                float dist = Vector2.Distance(transform.position, blackboard.surrenderTargetBase.position);
                if (dir.x > 0 && !facingRight || dir.x < 0 && facingRight)
                {
                    Flip();
                }
                if (dist > 0.5f)
                {
                    rb.velocity = dir * blackboard.walkSpeed * 1.2f;
                    animator.SetBool("IsMoving", true);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    animator.SetBool("IsMoving", false);
                    // TODO: Trigger rescue event, morale recovery, etc.
                    // Remain at base, do not return to combat unless event
                }
            }
            else
            {
                // No base assigned, just panic idle
                rb.velocity = Vector2.zero;
                animator.SetBool("IsMoving", false);
            }
            // If attacked while running, only block (weak defense)
            // TODO: Implement weak block/defense logic here
            return false;
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
        if (visionOrigin == null) return;

        // Clear old detected enemies that are too far
        blackboard.detectedEnemies.RemoveAll(enemy => 
            enemy == null || Vector2.Distance(transform.position, enemy.position) > viewDistance);

        // Calculate vision arc based on facing direction
        float startAngle = facingRight ? -blackboard.fieldOfView / 2 : 180 - blackboard.fieldOfView / 2;
        float endAngle = facingRight ? blackboard.fieldOfView / 2 : 180 + blackboard.fieldOfView / 2;
        
        // Cast multiple rays within field of view
        for (int i = 0; i < rayCount; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, i / (float)(rayCount - 1));
            Vector2 direction = GetVectorFromAngle(angle);
            
            RaycastHit2D hit = Physics2D.Raycast(
                visionOrigin.position,
                direction,
                viewDistance,
                enemyLayer | obstacleLayer
            );

            if (hit.collider != null)
            {
                // Check if hit object is on enemy layer
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
                {
                    Transform enemy = hit.transform;
                    if (!blackboard.detectedEnemies.Contains(enemy))
                    {
                        blackboard.detectedEnemies.Add(enemy);
                        blackboard.isWarning = true;
                        blackboard.warnedEnemyPosition = enemy.position;
                    }
                }
            }
        }
    }

    private Vector2 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private void OnDrawGizmos()
    {
        if (!showVisionGizmos || visionOrigin == null) return;

        // Draw vision origin point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(visionOrigin.position, 0.2f);

        // Only draw cone if blackboard is assigned (avoid null when not playing)
        float fieldOfView = 90f;
        if (blackboard != null)
            fieldOfView = blackboard.fieldOfView;

        float startAngle = facingRight ? -fieldOfView / 2 : 180 - fieldOfView / 2;
        float endAngle = facingRight ? fieldOfView / 2 : 180 + fieldOfView / 2;

        Gizmos.color = new Color(1, 1, 0, 0.2f); // Semi-transparent yellow
        for (int i = 0; i < rayCount; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, i / (float)(rayCount - 1));
            Vector2 direction = GetVectorFromAngle(angle);
            Gizmos.DrawRay(visionOrigin.position, direction * viewDistance);
        }

        // Draw detected enemies if possible
        if (blackboard != null && blackboard.detectedEnemies != null)
        {
            Gizmos.color = Color.red;
            foreach (var enemy in blackboard.detectedEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(visionOrigin.position, enemy.position);
                    Gizmos.DrawWireSphere(enemy.position, 0.5f);
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
        if (arrowPrefab != null && arrowSpawnPoint != null && Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            ArrowController arrowCtrl = arrow.GetComponent<ArrowController>();
            if (arrowCtrl != null)
            {
                arrowCtrl.Init(dir, blackboard.currentTarget);
            }
        }
    }

    public void ShootArrow2()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            ArrowController arrowCtrl = arrow.GetComponent<ArrowController>();
            if (arrowCtrl != null)
            {
                arrowCtrl.Init(dir, null);
            }
        }
    }

    public void TakeDamage(float damage, Vector2 attackPos)
    {
        // Apply damage to this ally unit
        if (blackboard != null && blackboard.currentHP > 0)
        {
            blackboard.currentHP -= Mathf.RoundToInt(damage);
            if (blackboard.currentHP <= 0)
            {
                animator.SetTrigger("Death");
                return;
            }
            animator.SetTrigger("Hurt");
            // You can add hit effects here
        }
    }
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    
}
