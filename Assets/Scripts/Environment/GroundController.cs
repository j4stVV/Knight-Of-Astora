using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    [SerializeField] GameObject bossFightArea;
    [SerializeField] GameObject boss;
    [SerializeField] Vector2 spawnPos;

    private GameObject bossInstance;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bossFightArea.SetActive(true);
            bossInstance = Instantiate(boss, spawnPos, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
