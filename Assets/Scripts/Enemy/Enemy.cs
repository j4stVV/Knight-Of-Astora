using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Basic Stats:")]
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;
    [SerializeField] protected bool isFacingRight = false;
    [SerializeField] protected float recoilTimer;
    //[SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected AudioClip dmgSoundClip;

    protected PlayerController player;
    public float speed;
    public float damage = 1f;
    protected float lastAttackUpdateTime;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected enum EnemyStates
    {
        //Skeleton
        Ske_Idle,
        Ske_Patrol,
        Ske_Chase,
        Ske_ReturnToStart,
        Ske_Attack,
        Ske_Stunned,
        Ske_Death,

        //Crawler
        Crawler_Idle,
        Crawler_Stunned,

        //Bat
        Bat_Idle, 
        Bat_Chase,
        Bat_Stunned, 
        Bat_Death,
        Bat_ReturnToStart,

        //Dracula
        Dracula_Stage1,
        Dracula_Stage2
    } 
    protected EnemyStates currentEnemyState;

    protected virtual EnemyStates GetCurrentEnemyState
    {
        get { return currentEnemyState; }
        set
        {
            if (currentEnemyState != value)
            {
                currentEnemyState = value;

                ChangeCurrentAnimation();
            }
        }
    }
    protected virtual void Awake()
    {
        
    }
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    protected virtual void Update()
    {
        if (!player)
        {
            try
            {
                player = PlayerController.Instance;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
            return;
        }
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyState();
        }
    }

    public virtual void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage;
        
        if (!isRecoiling)
        {
            SoundFXManager.instance.PlaySoundFX(dmgSoundClip, transform, 1f);
            rb.AddForce(hitForce * recoilFactor * hitDirection);
            isRecoiling = true;
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        // if there is maximum hp can heal per time, add next line
        // health = Mathf.Min(health, maxHealth);
    }

    protected void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !PlayerController.Instance.playerState.invincible)
        {
            Attack();
        }
    }
    //protected void OnCollisionStay2D(Collision2D other)
    //{
    //    if (other.gameObject.CompareTag("Player") && !PlayerController.Instance.playerState.invincible)
    //    {
    //        if(Time.time > lastAttackUpdateTime + attackCooldown)
    //        {
    //            Attack();
    //            lastAttackUpdateTime = Time.time;
    //        }
    //    }
    //}
    protected virtual void UpdateEnemyState() { }
    protected virtual void ChangeCurrentAnimation() { }
    protected void ChangeState(EnemyStates newState)
    {
        GetCurrentEnemyState = newState;
    }
    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage , rb.transform.position);
    }
    protected virtual void DestroyObject()
    {
        Destroy(gameObject);
    }
}
