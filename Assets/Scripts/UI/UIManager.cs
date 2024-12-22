using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameObject deathScreen;
    public SceneFader sceneFader;
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
        sceneFader = GetComponentInChildren<SceneFader>();
        DontDestroyOnLoad(gameObject);
    }
    public IEnumerator ActiveDeathScreen()
    {
        //yield return new WaitForSeconds(0.8f);
        //StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.In));

        yield return new WaitForSeconds(0.8f);
        deathScreen.SetActive(true);
    }

    public IEnumerator DeactiveDeathScreen()
    {
        yield return new WaitForSeconds(0.5f);
        deathScreen.SetActive(false);
    }
}
