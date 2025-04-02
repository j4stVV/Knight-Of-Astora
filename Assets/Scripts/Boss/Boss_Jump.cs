using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class Boss_Jump : StateMachineBehaviour
{
    Rigidbody2D rb;

    float distanceX;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponentInParent<Rigidbody2D>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //get range between player and boss minus half of attack range
        //so that boss can get closer to player
        distanceX = PlayerController.Instance.transform.position.x - rb.position.x 
            - BossScript.instance.attackRange / 2;

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
            rb.AddForce(new Vector2(distanceX, BossScript.instance.jumpForce), 
                ForceMode2D.Impulse);
        }
        if (distanceX <= BossScript.instance.attackRange)
        {
            animator.SetBool("Jump", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
