using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bornfire : MonoBehaviour
{
    public bool inRange;
    public bool interacted;

    [SerializeField] private ParticleSystem bornfireFX;
    private ParticleSystem bornfireFXInstance;

    private void Update()
    {
        if (inRange && Input.GetButtonDown("Interact"))
        {
            Debug.Log("Interacted");
            interacted = true;
            GameManager.Instance.SetCheckpoint(
            gameObject.scene.name,
            transform.position
            );
            bornfireFXInstance = Instantiate(bornfireFX, transform.position, Quaternion.identity);
            Debug.Log($"Bornfire activated in scene: {gameObject.scene.name}, Position: {transform.position}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
        }
    }
}

