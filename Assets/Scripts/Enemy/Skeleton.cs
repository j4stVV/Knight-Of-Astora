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
    [SerializeField] private float maxChasingDistance = 20f;
    [SerializeField] private float chasingSpeed;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;

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
        if (!isRecoiling)
        {
            
        }
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
        }
    }
    void Idle()
    {
        anim.SetBool("Idle", true); 
        rb.velocity = Vector2.zero;

        float distanceToPlayer = Vector2.Distance(transform.position,
            PlayerController.Instance.transform.position);

        if (Time.time >= (lastAttackTime + attackCooldown) && distanceToPlayer <= attackRange)
        {
            anim.SetBool("Idle", false);
            currentEnemyState = EnemyStates.Ske_Attack;
        }
        else if (distanceToPlayer > attackRange)
        {
            anim.SetBool("Idle", false);
            currentEnemyState = EnemyStates.Ske_Chase;
        }
    }
    void Patrol() // => done
    {
        CheckPlayerDetection();

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
    void CheckPlayerDetection()     // => done
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            anim.SetBool("Walk", false);
            currentEnemyState = EnemyStates.Ske_Chase;
        }
    }
    void Chase()        // => done
    {
        anim.SetBool("Walk", true);

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        float distanceToStartPos = Vector2.Distance(transform.position, originalPos);

        if (distanceToPlayer > maxChasingDistance || distanceToStartPos > maxChasingDistance)
        {
            ChangeState(EnemyStates.Ske_ReturnToStart);
            return;
        }
        if(distanceToPlayer <= attackRange)
        {
            anim.SetBool("Walk", false);
            currentEnemyState = EnemyStates.Ske_Attack;
            return;
        }
        Vector2 direction = (PlayerController.Instance.transform.position - transform.position).normalized;

        //chasing player until player gone too far from foes' initiate position
        float attackDir = isFacingRight ? 1f : -1f;
        transform.position = Vector2.MoveTowards
                (transform.position,
                new Vector2(PlayerController.Instance.transform.position.x + attackRange * attackDir, transform.position.y),
                chasingSpeed * Time.deltaTime);
        if (direction.x > 0 && !isFacingRight)
            Flip();
        else if (direction.x < 0 && isFacingRight)
            Flip();
    }
    void ReturnToStartPosition()    // => done
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
    protected override void Attack()
    {
        base.Attack();
    }
    void PerformAttack()    // => done
    {       
        float distanceToPlayer = Vector2.Distance(transform.position, 
            PlayerController.Instance.transform.position);

        //check if player is out of range
        if (distanceToPlayer > attackRange)
        {
            anim.SetBool("Attack", false);
            currentEnemyState = EnemyStates.Ske_Chase;
            return;
        }

        //Check the Ske's direction has the same ones with the player
        Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
        bool playerInFront = (isFacingRight && directionToPlayer.x > 0) || (!isFacingRight && directionToPlayer.x < 0);

        if (!playerInFront)
        {
            Flip();
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetBool("Attack", true);
            rb.velocity = Vector2.zero;
            lastAttackTime = Time.time;
            StartCoroutine(ResetAttackAnimation());
        }
        else
        {
            // In cooldown time
            currentEnemyState = EnemyStates.Ske_Idle;
        }
    }
    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("Attack", false);
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
        rb.velocity = Vector2.zero;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        float distanceToStart = Vector2.Distance(transform.position, originalPos);

        if (distanceToPlayer <= detectionRange && distanceToStart <= maxChasingDistance)
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
    }
    void Flip()     // => done
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    public override void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        anim.SetBool("Attack", false);
        anim.SetBool("Walk", false);
        anim.SetBool("Idle", false);

        anim.SetTrigger("Stunned");
        base.EnemyHit(damage, hitDirection, hitForce);
        if (health <= 0)
        {
            ChangeState(EnemyStates.Ske_Death);
            anim.SetTrigger("Death");
        }
        else
        {
            ChangeState(EnemyStates.Ske_Stunned);
        }
    }
}
