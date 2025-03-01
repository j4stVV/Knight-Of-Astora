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
    //[SerializeField] private float targetReachedDistance = 0.3f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float maxChasingDistance = 7f;
    [SerializeField] private float maxDistanceFromStart = 15f;

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
        if (!PlayerController.Instance.playerState.alive)
        {
            ChangeState(EnemyStates.Bat_Idle);
            return;
        }
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
    #region Process create path and make bat follow the path
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
        float distance = Vector2.Distance(transform.position, player.transform.position);
        isReturningToStart = false;
        if (distance < chaseDistance)
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

        float disToStart = Vector3.Distance(rb.position, startPosition);

        if (Time.time > lastPathUpdateTime + pathUpdateInterval)
        {
            CreateReturnPath();
        }
        if (disToStart < 0.2f)
        {
            currentPath = null;
            transform.position = startPosition;
            ChangeState(EnemyStates.Bat_Idle);
        }
        FollowPath(returnMultiSpeed);
    }
    void Stunned()
    {
        timer += Time.deltaTime;
        if (timer > stunDuration)
        {
            ChangeState(EnemyStates.Bat_Idle);
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
            isReturningToStart = false;
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
        // draw chase range in Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPosition, maxDistanceFromStart);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxChasingDistance);

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
