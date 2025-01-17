using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilitiesScript : MonoBehaviour
{
    [Header("Abilities Stats")]
    [SerializeField] private string abilitiesName;
    [SerializeField] private bool abilitiesState;
    [SerializeField] private float damage;
    [SerializeField] private int speed;
    public float xAxis;
    public float yAxis;

    private Animator anim;
    private int direction;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3f);
        anim = GetComponent<Animator>();
        abilitiesState = false;
        direction = BossScript.instance.facingLeft ? 1 : -1;
    }

    private void FixedUpdate()
    {
        if (abilitiesState)
        {
            return;
        }
        transform.position += speed * new Vector3(xAxis * direction, yAxis, 0) * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (!PlayerController.Instance.playerState.invincible)
            {
                abilitiesState = true;
                anim.SetBool(abilitiesName, abilitiesState);
                other.GetComponent<PlayerController>().TakeDamage(damage);
            }
        }   
        if (other.tag == "Ground" || other.tag == "Wall")
        {
            abilitiesState = true;
            anim.SetBool(abilitiesName, abilitiesState);
        }
    }
    public void DestroyObj()
    {
        Destroy(gameObject);
    }
    
}
