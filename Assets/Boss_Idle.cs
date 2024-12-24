using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Idle : StateMachineBehaviour
{
    Rigidbody2D rb;
    

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponentInParent<Rigidbody2D>();
        Debug.Log("-----1-----");
        Debug.Log("Hello");

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb.velocity = Vector2.zero;
        JumpToPlayer(animator);

        if (BossScript.instance.attackCountDown <= 0)
        {
            BossScript.instance.AttackHandler();
            BossScript.instance.attackCountDown = BossScript.instance.attackTimer;
        }
        Boss_Fall.Instance.Grounded(animator);
    }
    

    void JumpToPlayer(Animator animator)
    {
        float _distance = Mathf.Abs(PlayerController.Instance.transform.position.x - rb.position.x);   
        if (_distance > BossScript.instance.attackRange)
        {
            animator.SetBool("Jump", true);
        }
        return;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BossScript.instance.moveToPosition = PlayerController.Instance.transform.position;
    }

}
