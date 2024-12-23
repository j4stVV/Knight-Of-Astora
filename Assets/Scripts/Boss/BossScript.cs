using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BossScript : Enemy
{

    public static BossScript instance;

    public Transform SideAttackTransform;
    public Vector2 SideAttackArea;

    public float attackRange;
    public float attackTimer;

    [HideInInspector] public bool facingLeft;

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundCheckLayer;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float groundCheckDistance;

    bool stunned, canStun;
    bool alive;

    [Header("Movement")]
    public float jumpForce;
    public float jumpDistance; 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(groundCheckPoint.position + new Vector3(-0.05f, 0, 0) +
            Vector3.down * groundCheckDistance / 2, new Vector3(boxSize.x, boxSize.y, 1));
    }
    public bool IsOnGround()
    {
        if (Physics2D.BoxCast(groundCheckPoint.position, boxSize, 0,
            Vector2.down, groundCheckDistance, groundCheckLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    //private void Grounded()
    //{
    //    if (IsOnGround())
    //    {
    //        anim.SetBool("Grounded", true);
    //        jump = false;
    //    }
    //    else
    //    {
    //        anim.SetBool("Grounded", false);
    //        jump = true;
    //    }
    //}
    public void Flip()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x
            && transform.localScale.x > 0)
        {
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, 0);
            facingLeft = true;
        }
        else
        {
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, 180);
            facingLeft = false;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }
    protected override void Start()
    {
        base.Start();
        
        alive = true;
    }
    
    protected override void Update()
    {
        base.Update();
        if (!attacking)
        {
            attackCountDown -= Time.deltaTime;
        }
        if (alive)
        {
            //Grounded();
        }
    }
    private void OnCollisionStay2D(Collision2D other)
    {
        
    }
    #region attacking
    #region variable
    [HideInInspector] public bool jump;
    [HideInInspector] public bool attacking;
    [HideInInspector] public float attackCountDown;
    [HideInInspector] public bool damagedlayer = false;

    [HideInInspector] public Vector2 moveToPosition;
    #endregion

    #region Control
    public void AttackHandler()
    {
        if(currentEnemyState == EnemyStates.Dracula_Stage1)
        {

        }
    }
    public void ResetAllAttack()
    {
        attacking = false;

        fireballAttack = false;

        StopCoroutine(Barrage());
    }
    #endregion

    #region Stage 1

    [HideInInspector] public bool fireballAttack;
    public GameObject FireBall;
    
    void BarrageBendDown() 
    {
        attacking = true;
        rb.velocity = Vector2.zero;
        fireballAttack = true;
        anim.SetTrigger("Fireball");
    }
    public IEnumerator Barrage()
    {
        rb.velocity = Vector2.zero;
        float currentAngle = 15f;
        GameObject projectile = Instantiate(FireBall, transform.position, Quaternion.identity);
        if (facingLeft)
        {
            projectile.transform.eulerAngles =new Vector3(projectile.transform.eulerAngles.x, 
                180, currentAngle);
        }
        else
        {
            projectile.transform.eulerAngles = new Vector3(projectile.transform.eulerAngles.x, 
                0, currentAngle);
        }
        yield return new WaitForSeconds(0.1f);
        ResetAllAttack();
    }
    public IEnumerator TripleBarrage()
    {
        rb.velocity = Vector2.zero;
        float currentAngle = 15f;
        for(int i=0; i< 3; i++)
        {
            GameObject projectile = Instantiate(FireBall, transform.position, Quaternion.identity);
            if (facingLeft)
            {
                projectile.transform.eulerAngles = new Vector3(projectile.transform.eulerAngles.x,
                    180, currentAngle);
            }
            else
            {
                projectile.transform.eulerAngles = new Vector3(projectile.transform.eulerAngles.x,
                    0, currentAngle);
            }
            yield return new WaitForSeconds(0.4f);
        }
        
        yield return new WaitForSeconds(0.1f);
        ResetAllAttack();
    }

    #endregion

    #endregion



}
