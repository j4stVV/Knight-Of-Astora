using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class Boss_Jump : StateMachineBehaviour
{
    Rigidbody2D rb;

    bool isGrounded = false;
    bool isFalling = false;
    bool isJumping = false;
    bool isIdle = true;

    float xForce;
    float yForce;

    float lastYPos;

    float _distance;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponentInParent<Rigidbody2D>();
        _distance = Mathf.Abs(PlayerController.Instance.transform.position.x - rb.position.x);
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
            xForce = (BossScript.instance.jumpDistance - 6f) / 2;
            yForce = BossScript.instance.jumpForce;

            int direction = 0;
            if (BossScript.instance.facingLeft)
            {
                direction = -1;
            }
            else
            {
                direction = 1;
            }

            if (_distance <= BossScript.instance.jumpDistance / 2)
            {
                rb.velocity = new Vector2(xForce * direction, 0);
            }
            else
            {
                rb.velocity = new Vector2(xForce * direction, yForce);
            }
        }
        if (_distance <= BossScript.instance.attackRange)
        {
            animator.SetBool("Jump", false);
        }
    }
    
    void BarrageAttack()
    {
        if (BossScript.instance.FireBall)
        {
            BossScript.instance.Flip();
            Vector2 _newPos = Vector2.MoveTowards(rb.position, BossScript.instance.moveToPosition,
                BossScript.instance.speed * 3 * Time.deltaTime);
            rb.MovePosition(_newPos);

            float _distance = Vector2.Distance(rb.position, _newPos);
            if( _distance > 12f && _distance < 17f && !BossScript.instance.IsOnGround())
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
