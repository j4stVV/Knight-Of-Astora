using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Jump : StateMachineBehaviour
{
    Rigidbody2D rb;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponentInParent<Rigidbody2D>();
        BossScript.instance.jumpDistance = Vector2.Distance(PlayerController.Instance.transform.position,
            rb.transform.position);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        TargetPlayerPosition(animator);

        if (BossScript.instance.attackCountDown <= 0)
        {
            BossScript.instance.AttackHandler();
            BossScript.instance.attackCountDown = BossScript.instance.attackTimer;
        }
    }
    void TargetPlayerPosition(Animator animator)
    {
        if (PlayerController.Instance.IsOnGround())
        {
            BossScript.instance.Flip();
            float xForce;
            float yForce;
            if (BossScript.instance.facingLeft)
            {
                xForce = -BossScript.instance.jumpDistance;
            }
            else 
            {
                xForce = BossScript.instance.jumpDistance;
            }
            yForce = BossScript.instance.jumpForce;

            rb.velocity = new Vector2(xForce, yForce);
        }
        float _distance = PlayerController.Instance.transform.position.x - rb.position.x;
        if (_distance <= BossScript.instance.attackRange)
        {
            animator.SetBool("Jump", false);
        }
    }
    //void SpaceOut(Animator animator)
    //{
    //    if (PlayerController.Instance.IsOnGround())
    //    {
    //        BossScript.instance.Flip();
    //        float xForce;
    //        float yForce;
    //        if (BossScript.instance.facingLeft)
    //        {
    //            xForce = -BossScript.instance.jumpDistance;
    //        }
    //        else
    //        {
    //            xForce = BossScript.instance.jumpDistance;
    //        }
    //        yForce = BossScript.instance.jumpForce;
            
    //        rb.velocity = new Vector2(xForce, yForce);
    //    }

    //    float _distance = PlayerController.Instance.transform.position.x - rb.position.x;
    //    if (_distance <= BossScript.instance.attackRange)
    //    {
    //        animator.SetBool("Jump", false);
    //    }
    //}
    void BarrageAttack()
    {
        if (BossScript.instance.FireBall)
        {
            BossScript.instance.Flip();
            Vector2 _newPos = Vector2.MoveTowards(rb.position, BossScript.instance.moveToPosition,
                BossScript.instance.speed * 3 * Time.deltaTime);
            rb.MovePosition(_newPos);

            float _distance = Vector2.Distance(rb.position, _newPos);
            if( _distance > 12f && _distance < 17f)
            {
                BossScript.instance.Barrage();
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
