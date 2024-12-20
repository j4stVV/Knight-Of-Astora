using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    [SerializeField] private float chaseDistance;
    [SerializeField] private float stunDuration;

    float timer;
    protected override void Start()
    {
        base.Start();
        ChangState(EnemyStates.Bat_Idle);   
    }

    protected override void UpdateEnemyState()
    {
        float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);     

        switch (GetCurrentEnemyState)
        {
            case EnemyStates.Bat_Idle:
                if (distance < chaseDistance)
                {
                    ChangState(EnemyStates.Bat_Chase);
                }
                break;
            case EnemyStates.Bat_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, PlayerController.Instance.transform.position, Time.deltaTime * speed));

                FlipBat();
                break;
            case EnemyStates.Bat_Stunned:
                timer += Time.deltaTime;
                if (timer > stunDuration)
                {
                    ChangState(EnemyStates.Bat_Idle);
                    timer = 0;
                }
                break;
            case EnemyStates.Bat_Death:
                
                break;

        }
    }

    public override void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damage, hitDirection, hitForce);
        if (health <= 0)
        {
            ChangState(EnemyStates.Bat_Death);
        }
        else
        {
            ChangState(EnemyStates.Bat_Stunned);
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        anim.SetBool("Idle", GetCurrentEnemyState == EnemyStates.Bat_Idle);
        anim.SetBool("Chase", GetCurrentEnemyState == EnemyStates.Bat_Chase);
        anim.SetBool("Stunned", GetCurrentEnemyState == EnemyStates.Bat_Stunned);

        if (GetCurrentEnemyState == EnemyStates.Bat_Death)
        {
            anim.SetTrigger("Death");
        }
    }

    void FlipBat()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }
    }
}