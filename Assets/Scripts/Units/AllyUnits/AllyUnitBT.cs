using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class AllyUnit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class AllyUnitBT : MonoBehaviour
{
    private BTNode root;
    private AllyBlackboard blackboard;
    private AllyUnitController controller;

    void Awake()
    {
        blackboard = GetComponent<AllyBlackboard>();
        controller = GetComponent<AllyUnitController>();
        
        // Construct behavior tree
        root = new SelectorNode(new BTNode[]
        {
            // Surrender/Last Stand State
            new SequenceNode(new BTNode[]
            {
                new ConditionNode(() =>
                    (blackboard.surrenderType == SurrenderType.None &&
                        ((blackboard.maxHP > 0 && (blackboard.currentHP / blackboard.maxHP) < blackboard.hpLowThreshold)
                        || blackboard.morale < blackboard.moraleThreshold))
                    || blackboard.isSurrendering || blackboard.isLastStand),
                new ActionNode(() =>
                {
                    return controller.SurrenderAction() ? BehaviorState.Success : BehaviorState.Running;
                })
            }),

            // Pursue State
            new SequenceNode(new BTNode[]
            {
                new ConditionNode(() =>
                    (blackboard.detectedEnemies.Count > 0 && blackboard.currentTarget != null &&
                    Vector2.Distance(controller.transform.position, blackboard.currentTarget.position) > blackboard.engageRange &&
                    Vector2.Distance(controller.transform.position, blackboard.currentTarget.position) <= blackboard.chaseRadius)
                    || blackboard.isPursuing),
                new ActionNode(() =>
                {
                    return controller.PursueAction() ? BehaviorState.Success : BehaviorState.Running;
                })
            }),

            // Engage State
            new SequenceNode(new BTNode[]
            {
                new ConditionNode(() => blackboard.detectedEnemies.Count > 0 && blackboard.isWarning && blackboard.morale >= blackboard.moraleThreshold),
                new ActionNode(() =>
                {
                    return controller.EngageAction() ? BehaviorState.Success : BehaviorState.Running;
                })
            }),

            // Warn State
            new SequenceNode(new BTNode[]
            {
                new ConditionNode(() => (blackboard.detectedEnemies.Count > 0 || blackboard.isWarning)),
                new ActionNode(() =>
                {
                    return controller.WarnAction() ? BehaviorState.Success : BehaviorState.Running;
                })
            }),

            // Alert State
            new SequenceNode(new BTNode[] 
            {
                new ConditionNode(() => blackboard.detectedEnemies.Count == 0 && !blackboard.hasHeardCombatSound && !blackboard.isWarning),
                new SelectorNode(new BTNode[] 
                {
                    // Patrol behavior
                    new SequenceNode(new BTNode[] 
                    {
                        new ConditionNode(() => blackboard.patrolPoints != null && blackboard.patrolPoints.Length > 0),
                        new ActionNode(() => 
                        {
                            return controller.Patrol() ? BehaviorState.Success : BehaviorState.Running;
                        })
                    }),
                    
                    // Random idle animations
                    new ActionNode(() => 
                    {
                        if (Time.time - blackboard.lastIdleAnimationTime > blackboard.idleAnimationInterval)
                        {
                            controller.PlayIdleAnimation();
                            blackboard.lastIdleAnimationTime = Time.time;
                            return BehaviorState.Success;
                        }
                        return BehaviorState.Running;
                    })
                })
            }),

            // Investigate Sound
            new SequenceNode(new BTNode[]
            {
                new ConditionNode(() => blackboard.hasHeardCombatSound),
                new ActionNode(() =>
                {
                    return controller.InvestigateSound() ? BehaviorState.Success : BehaviorState.Running;
                })
            })
        });
    }

    void Update()
    {
        if (root != null)
        {
            root.Evaluate();
        }
    }
}
