using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class Skeleton : Enemy
{
    private Vector2 originalPos;
    private bool isDeath = false;

    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float maxChasingDistance = 12f;
    [SerializeField] private float maxDistanceFromStart = 20f;
    [SerializeField] private float chasingSpeed;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float ledgeCheckX = 1f;
    [SerializeField] private float ledgeCheckY = 2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration;
    private float timer;

    private Transform currentTarget;
    private enum SkeletonExtraState { CounterAttack = 1000 }

    protected override void Awake()
    {
        base.Awake();

        originalPos = transform.position;
        chasingSpeed = speed * 2f;
        currentEnemyState = EnemyStates.Ske_Patrol;
    }

    protected override void Update()
    {
        base.Update();
    }
    protected override void UpdateEnemyState()
    {
        base.UpdateEnemyState();
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Ske_Idle:
                Idle();
                break;
            case EnemyStates.Ske_Chase:
                Chase();
                break;
            case EnemyStates.Ske_Patrol:
                Patrol();
                break;
            case EnemyStates.Ske_Attack:
                PerformAttack();
                break;
            case EnemyStates.Ske_ReturnToStart:
                ReturnToStartPosition();
                break;
            case EnemyStates.Ske_Stunned:
                Stunned();
                break;
            case EnemyStates.Ske_Death:
                Death();
                break;
            case (EnemyStates)SkeletonExtraState.CounterAttack:
                CounterAttack();
                break;
        }
    }
    void Idle()
    {
        anim.SetBool("Walk", false);
        rb.velocity = Vector2.zero;

        float distanceToPlayer = Vector2.Distance(transform.position,
            player.transform.position);

        if (Time.time >= (lastAttackTime + attackCooldown) && distanceToPlayer <= attackRange)
        {
            currentEnemyState = EnemyStates.Ske_Attack;
        }
        else if (distanceToPlayer > attackRange)
        {
            currentEnemyState = EnemyStates.Ske_Chase;
        }
    }
    void Patrol()
    {
        CheckPlayerDetection();

        Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(-ledgeCheckX, 1.5f) : new Vector3(ledgeCheckX, 1.5f);
        Vector2 wallCheckDir = transform.localScale.x > 0 ? -transform.right : transform.right;
        if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround)
            || Physics2D.Raycast(transform.position + new Vector3(0, 0.5f, 0), wallCheckDir, ledgeCheckX, whatIsGround))
        {
            Flip();
        }
        anim.SetBool("Walk", true);

        float currentPatrolPosition = transform.position.x;
        float leftBound = originalPos.x - patrolRange;
        float rightBound = originalPos.x + patrolRange;

        if (isFacingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (currentPatrolPosition >= rightBound)
            {
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (currentPatrolPosition <= leftBound)
            {
                Flip();
            }
        }
        
    }
    private Transform FindNearestEnemyTarget()
{
    Transform nearest = player.transform;
    float minDist = Vector2.Distance(transform.position, player.transform.position);
    GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
    foreach (var ally in allies)
    {
        float dist = Vector2.Distance(transform.position, ally.transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            nearest = ally.transform;
        }
    }
    return nearest;
}
    void CheckPlayerDetection()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        Transform nearestAlly = null;
        float distanceToAlly = float.MaxValue;
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        foreach (var ally in allies)
        {
            float dist = Vector2.Distance(transform.position, ally.transform.position);
            if (dist < distanceToAlly)
            {
                distanceToAlly = dist;
                nearestAlly = ally.transform;
            }
        }
        if (distanceToPlayer <= detectionRange || distanceToAlly <= detectionRange)
        {
            anim.SetBool("Walk", false);
            currentEnemyState = EnemyStates.Ske_Chase;
            if (distanceToAlly < distanceToPlayer)
                currentTarget = nearestAlly;
            else
                currentTarget = player.transform;
        }
    }
    void Chase()
    {
        anim.SetBool("Walk", true);
        if (currentTarget == null) currentTarget = player.transform;
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        float distanceToStartPos = Vector2.Distance(transform.position, originalPos);

        if (distanceToTarget > maxChasingDistance || distanceToStartPos > maxDistanceFromStart)
        {
            ChangeState(EnemyStates.Ske_ReturnToStart);
            return;
        }
        if (distanceToTarget <= attackRange)
        {
            anim.SetBool("Walk", false);
            currentEnemyState = EnemyStates.Ske_Attack;
            return;
        }
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float attackDir = isFacingRight ? 1f : -1f;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(currentTarget.position.x + attackRange * attackDir, transform.position.y), chasingSpeed * Time.deltaTime);
        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }
    void ReturnToStartPosition()
    {
        float distanceToStart = Vector2.Distance(transform.position, originalPos);
        if (distanceToStart <= 0.1f)
        {
            transform.position = originalPos;
            anim.SetBool("Walk", false);
            currentEnemyState = EnemyStates.Ske_Patrol;
            return;
        }
        Vector2 direction = (originalPos - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, originalPos, chasingSpeed * Time.deltaTime);
        if (direction.x > 0 && !isFacingRight)
            Flip();
        else if (direction.x < 0 && isFacingRight)
            Flip();
    }
    void PerformAttack()    
    {
        if (currentTarget == null) currentTarget = player.transform;
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

        // Check if target is out of range
        if (distanceToTarget > attackRange)
        {
            Debug.Log("distance to target: " + distanceToTarget);
            currentEnemyState = EnemyStates.Ske_Chase;
            return;
        }

        // Check if the target is in front
        Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
        bool targetInFront = (isFacingRight && directionToTarget.x > 0) || (!isFacingRight && directionToTarget.x < 0);

        if (!targetInFront)
        {
            Flip();
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("Attack");
            rb.velocity = Vector2.zero;
            lastAttackTime = Time.time;
        }
    }
    protected override void Attack()
    {
        // Called by animation event at the correct frame
        PerformAttackTarget(currentTarget);
    }
    void Death()
    {
        if (!isDeath)
        {
            StartCoroutine(PerformDeath());
        }
    }
    IEnumerator PerformDeath()
    {
        isDeath = true;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("Death");
        Destroy(gameObject, 1f);
        yield return null;
    }
    void Stunned()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float distanceToStart = Vector2.Distance(transform.position, originalPos);
        if (distanceToPlayer <= detectionRange && distanceToStart <= maxDistanceFromStart)
        {
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(EnemyStates.Ske_Attack);
            }
            else
            {
                ChangeState(EnemyStates.Ske_Chase);
            }
        }
        else if (distanceToStart > maxChasingDistance)
        {
            ChangeState(EnemyStates.Ske_ReturnToStart);
        }
        else
        {
            ChangeState(EnemyStates.Ske_Patrol);
        }
        rb.velocity = Vector2.zero;
    }
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    public override void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        anim.SetTrigger("Stunned");
        base.EnemyHit(damage, hitDirection, hitForce);
        if (health <= 0)
        {
            ChangeState(EnemyStates.Ske_Death);
            anim.SetTrigger("Death");
        }
        else
        {
            // Switch to counter-attack state
            currentTarget = FindNearestEnemyTarget();
            ChangeState((EnemyStates)SkeletonExtraState.CounterAttack);
        }
    }
    private void CounterAttack()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            currentTarget = FindNearestEnemyTarget();
        if (currentTarget == null) {
            ChangeState(EnemyStates.Ske_Patrol);
            return;
        }
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackRange)
        {
            // Move towards target
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            float attackDir = isFacingRight ? 1f : -1f;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(currentTarget.position.x + attackRange * attackDir, transform.position.y), chasingSpeed * Time.deltaTime);
            anim.SetBool("Walk", true);
            if (direction.x > 0 && !isFacingRight) Flip();
            else if (direction.x < 0 && isFacingRight) Flip();
        }
        else
        {
            anim.SetBool("Walk", false);
            PerformAttack();
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(-ledgeCheckX, 1.5f) : new Vector3(ledgeCheckX, 1.5f);
        Vector2 wallCheckDir = transform.localScale.x > 0 ? -transform.right : transform.right;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + ledgeCheckStart, Vector3.down * ledgeCheckY);
        Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), wallCheckDir);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            ChangeState(EnemyStates.Ske_Death);
            anim.SetTrigger("Death");
        }
    }
}
