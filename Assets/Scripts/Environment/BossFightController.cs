using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossFightController : MonoBehaviour
{
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject bossTriggerArea;
    [SerializeField] private GameObject bossFightArea;
    private BossScript bossScript;
    private void Awake()
    {
        try
        {
            bossScript = BossScript.instance;
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    private void FixedUpdate()
    {
        AppearEndScreen();
    }

    public void AppearEndScreen()
    {
        if (!BossScript.instance.alive)
        {
            endScreen.SetActive(true);
            bossTriggerArea.SetActive(false);
            bossFightArea.SetActive(false);
        }
    }
}
