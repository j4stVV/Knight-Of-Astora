using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(groundCheckPoint.position + new Vector3(-0.05f, 0, 0) +
            Vector3.down * groundCheckDistance / 2, new Vector3(boxSize.x, boxSize.y, 1));
    }
    private bool IsOnGround()
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
    public void Flip()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x
            && transform.localScale.x > 0)
        {
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, 180);
            facingLeft = true;
        }
        else
        {
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, 0);
            facingLeft = false;
        }
    }
    public void CastSpell()
    {

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
        //anim = GetComponent<Animator>();

        alive = true;
    }
    
    protected override void Update()
    {
        base.Update();
        if (!isRecoiling)
        {
            transform.position = Vector2.MoveTowards
                (transform.position,
                new Vector2(PlayerController.Instance.transform.position.x, transform.position.y),
                speed * Time.deltaTime);
        }
    }
    private void OnCollisionStay2D(Collision2D other)
    {
        
    }
    #region attacking
    #region variable
    [HideInInspector] public bool attacking;
    [HideInInspector] public float attackCountDown;

    #endregion

    #region Control
    public void AttackHandler()
    {
        
    }
    public void ResetAllAttack()
    {
        fireballAttack = false;
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
