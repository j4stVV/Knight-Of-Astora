using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected float recoilTimer;


    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    public float damage = 1;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected enum EnemyStates
    {
        Crawler_Idle,
        Crawler_Flip,

        Bat_Idle, 
        Bat_Chase,
        Bat_Stunned, 
        Bat_Death,
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
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Awake()
    {
        player = PlayerController.Instance;
        anim = GetComponent<Animator>();
    }
    protected virtual void Update()
    {
        UpdateEnemyState();
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
    }

    public virtual void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage;
        
        if (!isRecoiling)
        {
            rb.AddForce(hitForce * recoilFactor * hitDirection);
            isRecoiling = true;
        }
    }

    protected void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !PlayerController.Instance.playerState.invincible)
        {
            Attack();
        }
    }
    protected virtual void UpdateEnemyState() { }
    protected virtual void ChangeCurrentAnimation() { }
    protected void ChangState(EnemyStates newState)
    {
        GetCurrentEnemyState = newState;
    }
    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}
