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
    [SerializeField] private float xAxis;
    [SerializeField] private float yAxis;

    Animator anim;

    // Start is called before the first frame update
    private void Awake()
    {
        
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        abilitiesState = false;
    }

    private void FixedUpdate()
    {
        if (abilitiesState)
        {
            return;
        }
        transform.position += speed * new Vector3(xAxis, yAxis, 0) * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            abilitiesState = true;
            anim.SetBool(abilitiesName, abilitiesState);
            other.GetComponent<PlayerController>().TakeDamage(damage);
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
