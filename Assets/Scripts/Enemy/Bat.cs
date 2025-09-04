using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Bat : Enemy
{
    [Header("A Star Algorithms")]
    [SerializeField] private float pathUpdateInterval = 0.5f; // Route update frequency
    [SerializeField] private float nextWaypointDistance = 3f; // Distance to move to next waypoint

    [Header("Chase Settings")]
    [SerializeField] private float detectedPlayerRange = 8f;
    [SerializeField] private float maxChasingDistance = 10f;
    [SerializeField] private float maxDistanceFromStart = 20f;

    [Header("Stunned Settings")]
    [SerializeField] private float stunDuration = 1f;
    private float timer;

    // A* Pathfinding components
    private Path currentPath;
    private Seeker seeker;
    private int currentWaypoint = 0;
    //private bool reachedEndOfPath = false;
    private bool isReturningToStart = false;

    private Vector3 startPosition;
    private float lastPathUpdateTime = 0f;

    protected override void Start()
    {
        base.Start();

        seeker = GetComponent<Seeker>();
        startPosition = transform.position;
        ChangeState(EnemyStates.Bat_Idle);
    }

    protected override void Update()
    {
        //trong base.update da co san UpdateEnemyState
        base.Update();
    }
    protected override void UpdateEnemyState()
    {
        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Bat_Idle:
                Idle();
                break;
            case EnemyStates.Bat_Chase:
                Chase();
                break;
            case EnemyStates.Bat_ReturnToStart:
                ReturnToStart();
                break;
            case EnemyStates.Bat_Stunned:
                Stunned();
                break;
            case EnemyStates.Bat_Death:
                rb.gravityScale = 12f;
                Destroy(gameObject, 1f);
                break;
        }
    }
    #region Process create path and make the bat follow the path
    void CreateChasePath()
    {
        if (seeker.IsDone())
        {
            Vector3 targetPosition = PlayerController.Instance.transform.position + new Vector3(0, 1f, 0);
            seeker.StartPath(transform.position, targetPosition, OnPathComplete);
            lastPathUpdateTime = Time.time;
        }
    }
    void CreateReturnPath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, startPosition, OnPathComplete);
            lastPathUpdateTime = Time.time;
        }
    }
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;
        }
    }
    void FollowPath(float speedMultiplier = 1f)
    {
        if (currentPath == null || currentWaypoint >= currentPath.vectorPath.Count)
        {
            //reachedEndOfPath = true;
            return;
        }
        //reachedEndOfPath = false;

        float currentSpeed = speed * speedMultiplier;

        //testing
        Vector2 originDir = (Vector2)currentPath.vectorPath[currentWaypoint] - (Vector2)transform.position;

        Vector2 direction = ((Vector2)currentPath.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * currentSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);

        float distanceToWaypoint = Vector2.Distance(transform.position, currentPath.vectorPath[currentWaypoint]);
        if (distanceToWaypoint < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        FlipBat(direction.x);
    }
    #endregion
    void Idle()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        isReturningToStart = false;
        if (distanceToPlayer < detectedPlayerRange)
        {
            ChangeState(EnemyStates.Bat_Chase);
            CreateChasePath();
        }
    }
    void Chase()
    {
        if (currentPath == null)
            return;
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if ((distanceFromStart > maxDistanceFromStart || distanceToPlayer > maxChasingDistance)  && !isReturningToStart)
        {
            currentPath = null;
            ChangeState(EnemyStates.Bat_ReturnToStart);
            return;
        }

        float chaseMultiSpeed = 2f;

        if (Time.time > lastPathUpdateTime + pathUpdateInterval)
        {
            if (!isReturningToStart)
            {
                CreateChasePath();
            }
        }
        FollowPath(chaseMultiSpeed);
    }
    void ReturnToStart()
    {
        isReturningToStart = true;
        float returnMultiSpeed = 2.5f;
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceFromStart < maxDistanceFromStart)
        {
            if (distanceToPlayer < detectedPlayerRange)
            {
                isReturningToStart = false;
                currentPath = null;
                ChangeState(EnemyStates.Bat_Chase);
                CreateChasePath();
                return;
            }
        }

        if (Time.time > lastPathUpdateTime + pathUpdateInterval)
        {
            CreateReturnPath();
        }
        if (distanceFromStart <= 1f)
        {
            currentPath = null;
            transform.position = startPosition;
            ChangeState(EnemyStates.Bat_Idle);
            return;
        }

        FollowPath(returnMultiSpeed);
    }
    void Stunned()
    {
        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        timer += Time.deltaTime;
        if (timer > stunDuration)
        {
            if (distanceFromStart > maxDistanceFromStart || distanceToPlayer > maxChasingDistance)
            {
                ChangeState(EnemyStates.Bat_ReturnToStart);
            }
            else
            {
                ChangeState(EnemyStates.Bat_Idle);
            }
            rb.linearVelocity = Vector2.zero;
            timer = 0;
        }
    }
    public override void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damage, hitDirection, hitForce);
        if (health <= 0)
        {
            ChangeState(EnemyStates.Bat_Death);
        }
        else
        {
            ChangeState(EnemyStates.Bat_Stunned);
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        anim.SetBool("Idle", GetCurrentEnemyState == EnemyStates.Bat_Idle);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Bat_Chase);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Bat_ReturnToStart);
        if (GetCurrentEnemyState == EnemyStates.Bat_Stunned)
        {
            anim.SetTrigger("Stunned");
        }
        if (GetCurrentEnemyState == EnemyStates.Bat_Death)
        {
            anim.SetTrigger("Death");
        }
    }

    //Adjust FlipBat to recieve new direction
    void FlipBat(float directionX = 0)
    {
        sr.flipX = directionX < 0;
    }

    private void OnDrawGizmosSelected()
    {
        //draw chase range in Scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(startPosition, maxDistanceFromStart);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxChasingDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectedPlayerRange);

        // draw path
        if (currentPath != null)
        {
            Gizmos.color = Color.blue;
            for (int i = currentWaypoint; i < currentPath.vectorPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath.vectorPath[i], currentPath.vectorPath[i + 1]);
            }
        }
    }
}
