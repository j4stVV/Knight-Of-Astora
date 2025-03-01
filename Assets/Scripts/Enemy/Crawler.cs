using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    [SerializeField] private float ledgeCheckX; 
    [SerializeField] private float ledgeCheckY; 
    [SerializeField] private float patrolRange; 
    [SerializeField] private LayerMask whatIsGround;
    private Vector3 startPos;
    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
        currentEnemyState = EnemyStates.Crawler_Idle;
    }
    protected override void Update()
    {
        base.Update();
    }
    protected override void UpdateEnemyState()
    {
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Crawler_Idle:
                Idle();
                break;
            case EnemyStates.Crawler_Flip:
                Flip();
                break;
            case EnemyStates.Crawler_Stunned:
                Stunned();
                break;

        }
    }
    void Idle()
    {
        Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
        Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;
        if (!Physics2D.Raycast(transform.position + ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround)
            || Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, whatIsGround))
        {
            ChangeState(EnemyStates.Crawler_Flip);
        }

        float currentPatrolPosition = transform.position.x;
        float leftBound = startPos.x - patrolRange;
        float rightBound = startPos.x + patrolRange;

        Vector2 moveDir;
        if (isFacingRight)
        {
            moveDir = Vector2.right * speed * Time.deltaTime;
            if (currentPatrolPosition >= rightBound)
            {
                ChangeState(EnemyStates.Crawler_Flip);
            }
        }
        else
        {
            moveDir = -Vector2.right * speed * Time.deltaTime;
            if (currentPatrolPosition <= leftBound)
            {
                ChangeState(EnemyStates.Crawler_Flip);
            }
        }
        transform.Translate(moveDir);
    }
    void Stunned()
    {
        anim.SetTrigger("Stunned");
        ChangeState(EnemyStates.Crawler_Idle);
    }
    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
        ChangeState(EnemyStates.Crawler_Idle);
    }
    protected override void ChangeCurrentAnimation()
    {
        anim.SetBool("Idle", (GetCurrentEnemyState == EnemyStates.Crawler_Idle));
    }
    public override void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damage, hitDirection, hitForce);
        if (health <= 0)
        {
            Destroy(gameObject, 0.5f);
        }
        else
        {
            ChangeState(EnemyStates.Crawler_Stunned);
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 ledgeCheckStart = transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
        Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + ledgeCheckStart, Vector3.down);
        Gizmos.DrawRay(transform.position, wallCheckDir);
    }
}
