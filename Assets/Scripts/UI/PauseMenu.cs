using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [SerializeField] private bool isPaused = false;
    [SerializeField] private bool isSetting = false;
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
        if (!isPaused && !isSetting)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }
    private void Pause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(isPaused);
    }
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
    }
    

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
    
}
