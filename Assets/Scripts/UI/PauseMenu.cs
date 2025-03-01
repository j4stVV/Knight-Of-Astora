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
    [SerializeField] private GameObject pauseSettingUI;

    void Start()
    {
        Time.timeScale = 1.0f;
        if(instance == null)
            instance = this;
    }
    void Update()
    {
        isPaused = pauseMenuUI.activeSelf || pauseSettingUI.activeSelf;
        isSetting = pauseSettingUI.activeSelf;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GamePause();
        }
        
    }
    public void GamePause()
    {
        if (!isPaused)
        {
            if (!isSetting)
                Pause();
            else
                TurnSettingMenuOff();
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
    void TurnSettingMenuOff()
    {
        isSetting = false;
        pauseMenuUI.SetActive(isSetting);
    }
    
    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
    
}
