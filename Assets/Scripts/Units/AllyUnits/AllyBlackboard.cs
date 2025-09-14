using UnityEngine;
using System.Collections.Generic;

public enum AllyCombatType
{
    Melee,
    Ranged,
    Support
}

public class AllyBlackboard : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float hearingRange = 10f;
    public float fieldOfView = 90f;
    
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    public float walkSpeed = 3f;
    
    [Header("Alert State")]
    public float idleAnimationInterval = 3f;
    public float lastSoundHeardTime;
    public Vector2 lastSoundPosition;
    public bool isInvestigatingSoundSource;
    
    [Header("Runtime Data")]
    public int currentPatrolIndex;
    public float lastIdleAnimationTime;
    public bool isPatrolling;
    public List<Transform> detectedEnemies = new List<Transform>();
    public bool hasHeardCombatSound;

    [Header("Warn State")]
    public bool isWarning;
    public Vector2 warnedEnemyPosition;
    public bool receivedWarnSignal;

    [Header("Engage State")]
    public AllyCombatType combatType = AllyCombatType.Ranged; // Default to Ranged for archer
    public float morale = 1.0f; // 1.0 = high, 0 = panic
    public float moraleThreshold = 0.3f;
    public bool isEngaging;
    public Transform currentTarget;
    public float engageRange = 6f; // For ranged, melee, etc.
    public float supportRange = 8f;

    public void ResetAlertState()
    {
        isInvestigatingSoundSource = false;
        hasHeardCombatSound = false;
        lastSoundPosition = Vector2.zero;
    }

    public void HandleCombatSound(Vector2 soundPosition)
    {
        lastSoundPosition = soundPosition;
        lastSoundHeardTime = Time.time;
        hasHeardCombatSound = true;
        isInvestigatingSoundSource = true;
    }

    public void ReceiveWarnSignal(Vector2 enemyPosition)
    {
        isWarning = true;
        warnedEnemyPosition = enemyPosition;
        receivedWarnSignal = true;
    }

    public void ResetWarnState()
    {
        isWarning = false;
        warnedEnemyPosition = Vector2.zero;
        receivedWarnSignal = false;
    }

    public void ResetEngageState()
    {
        isEngaging = false;
        currentTarget = null;
    }
}