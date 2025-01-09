using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Boss_Fall : StateMachineBehaviour
{
    public static Boss_Fall Instance;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Instance = this;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Grounded(animator);
    }
    public async void Grounded(Animator animator)
    {
        if (BossScript.instance.IsOnGround())
        {
            animator.SetBool("Grounded", true);
            await Task.Delay(2000);
        }
        else
        {
            animator.SetBool("Grounded", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
