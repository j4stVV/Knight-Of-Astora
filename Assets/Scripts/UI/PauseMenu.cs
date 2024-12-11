using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [SerializeField] private bool isPaused = false;
    public bool IsPause {  get { return isPaused; } }
    [SerializeField] private GameObject pauseMenuUI;

    void Start()
    {
        if(instance == null)
            instance = this;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GamePause();
        }
        
    }
    public void GamePause()
    {
        if (!isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }
    private void Resume()
    {
        isPaused = !isPaused;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(isPaused);
    }
    private void Pause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(isPaused);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
    
}
