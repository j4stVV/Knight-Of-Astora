using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    //Input
    private float horizontalInput;

    private bool isOnGround;

    private bool isBlocking;

    //Rolling
    private bool isRolling;
    private float rollDuration = 0.5f;
    private float rollCurrentTime;

    //Dashing
    private bool canDash = true;
    private bool dashed;
    private bool isDashing = false;
    private float gravity;

    //Attacking
    private int currentAttack = 0;
    private float timeSinceAttack;
    [SerializeField] private float damage = 1f;
    [SerializeField] Transform SideAttackTransform;
    [SerializeField] Vector2 SideAttackArea;
    [SerializeField] LayerMask attackableLayer;

    //Double Jump
    public bool isJumping = false;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJump;

    //Recoil
    public bool recoilingX = false;
    public bool lookingRight;
    [SerializeField] int recoilXStep = 5;
    [SerializeField] float recoilXSpeed = 100;
    int stepsXRecoiled;

    //Health
    public bool isHealing;
    private float healTimer;
    [SerializeField] float timeToHeal;
    public HealController healController;

    //Mana
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    public ManaController manaController;

    //Player attributes
    [SerializeField] private float jumpForce;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float rollForce;
    public int health;
    public int maxHealth;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    public bool invincible = false;

    private Rigidbody2D playerRb;
    private Animator playerAnimation;
    private BoxCollider2D playerBC;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimation = GetComponent<Animator>();
        playerBC = GetComponent<BoxCollider2D>();

        gravity = playerRb.gravityScale;

        Health = maxHealth;
        healController.SetMaxHealth(Health);

        Mana = mana;
        manaController.SetMaxMana(Mana);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        // Set speed when player in the air
        playerAnimation.SetFloat("AirSpeedY", playerRb.velocity.y);

        // Increase timer that controls attack combo
        timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if (isRolling)
            rollCurrentTime += Time.deltaTime;
        // Disable rolling if timer extends duration
        if (rollCurrentTime > rollDuration)
        {
            isRolling = false;
            rollCurrentTime = 0f;
        }


        //Check if player land on ground
        Grounded();

        //Handle Input Movement
        horizontalInput = Input.GetAxis("Horizontal");
        //Flip
        Flip();
        // Move
        Move();
        // Jump
        Jump();
        // Attack
        Attack();
        // Roll
        Roll();
        // Block
        //Block();
        //Dash
        StartDash();

        Heal();
    }

    private void FixedUpdate()
    {
        Recoil();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
    }

    private void Grounded()
    {
        if (isOnGround)
        {
            playerAnimation.SetBool("Grounded", true);
            airJumpCounter = 0;
            dashed = false;
            isJumping = false;
        }
        else
        {
            playerAnimation.SetBool("Grounded", false);
        }
    }

    private void Flip()
    {
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y); 
            lookingRight = true;
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            lookingRight = false;
        }
    }

    private void Move()
    {
        if (!isRolling && !isBlocking && !isDashing && !isHealing)
        {
            playerRb.velocity = new Vector2(horizontalInput * playerSpeed, playerRb.velocity.y);
            //transform.Translate(new Vector2(horizontalInput * playerSpeed * Time.deltaTime, 0));
        }

        if (horizontalInput != 0)
        {
            playerAnimation.SetFloat("Speed", 1);
        }
        else
        {
            playerAnimation.SetFloat("Speed", 0);

        }
    }
    private void Jump()
    {
        if (Input.GetKeyUp(KeyCode.Space) && playerRb.velocity.y > 0)
        {
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isOnGround && !isRolling && !isHealing)
        {
            isJumping = true;
            isOnGround = false;
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
        }
        else if (!isOnGround && airJumpCounter < maxAirJump && Input.GetKeyDown(KeyCode.Space))
        {
            airJumpCounter++;
            playerAnimation.SetTrigger("Jump");
            playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
        }
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && timeSinceAttack > 0.5f && !isRolling && !isHealing)
        {
            currentAttack++;
            if (currentAttack > 3)
                currentAttack = 1;

            if (timeSinceAttack > 1f)
                currentAttack = 1;

            playerAnimation.SetTrigger("Attack" + currentAttack);
            timeSinceAttack = 0f;

            Hit(SideAttackTransform, SideAttackArea, ref recoilingX, recoilXSpeed);
        }
    }

    void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackableLayer);
        if (objectsToHit.Length > 0)
        {
            recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, 
                    (-transform.position + objectsToHit[i].transform.position).normalized, 
                    recoilStrength);
                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                    manaController.SetMana(Mana);
                }
            } 
            
        }
    }

    void Recoil()
    {
        if (recoilingX)
        {
            if (lookingRight)
            {
                playerRb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else playerRb.velocity = new Vector2(recoilXSpeed, 0);
        }

        //Stop Recoil
        if (recoilingX && stepsXRecoiled < recoilXStep)
            stepsXRecoiled++;
        else StopRecoilX();
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        recoilingX = false;
    }

    public void TakeDamage(float damage)
    {
        Health -= Mathf.RoundToInt(damage);
        healController.SetHealth(Health);
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        invincible = true;
        playerAnimation.SetTrigger("Hurt");
        yield return new WaitForSeconds(1f);
        invincible = false;
    }
    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);
            }
        }
    }
    IEnumerator IFrame()
    {
        invincible = true;
        yield return new WaitForSeconds(rollDuration);
        invincible = false;
    }
    void Heal()
    {
        if (Input.GetMouseButton(1) && Health < maxHealth && !isRolling && Mana > 0 && !isDashing && !isJumping)
        {
            playerRb.velocity = Vector3.zero;
            isHealing = true;
            healTimer += Time.deltaTime;
            playerAnimation.SetBool("Heal", true);
            
            if (healTimer >= timeToHeal)
            {
                Health++;
                healController.SetHealth(Health);
                healTimer = 0;
            }

            Mana -= Time.deltaTime * manaDrainSpeed;
            manaController.SetMana(Mana);
        }
        else
        {
            isHealing = false;
            playerAnimation.SetBool("Heal", false);
            healTimer = 0;
        }
    }
    float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
            }
        }
    }
    private void Roll()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mana >= 0.3f && isOnGround && !isRolling && !isDashing && !isHealing)
        {
            isRolling = true;
            playerAnimation.SetTrigger("Roll");
            Mana -= 0.3f;
            manaController.SetMana(Mana);
            StartCoroutine(IFrame());
            playerRb.velocity = new Vector2(rollForce * transform.localScale.x, 0);
        }
    }

    /*private void Block()
    {
        if (Input.GetMouseButtonDown(1) && isOnGround && !isRolling)
        {
            isBlocking = true;
            playerRb.velocity = Vector2.zero;
            playerAnimation.SetTrigger("Block");
            playerAnimation.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isBlocking = false;
            playerAnimation.SetBool("IdleBlock", false);
        }
    }*/

    private void StartDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && canDash && !dashed && !isBlocking && !isRolling && !isHealing)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        playerAnimation.SetTrigger("Dash");
        playerRb.gravityScale = 0;
        playerRb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        playerRb.gravityScale = gravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }
}
