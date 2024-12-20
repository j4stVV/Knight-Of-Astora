using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bornfire : MonoBehaviour
{
    public bool interacted;
    private void Start()
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetButtonDown("Interact"))
        {
            Debug.Log("Interacted");
            interacted = true;
        }
    }
}
