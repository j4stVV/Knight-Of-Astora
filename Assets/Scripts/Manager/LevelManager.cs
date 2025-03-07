using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    //[SerializeField] private GameObject pauseMenu;
    [SerializeField] private Vector2[] respawnPos;
    public void TransitionToScene(int sceneIndex)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneIndex);
        for (int i = 0; i < respawnPos.Length; i++)
        {
            if (i == (sceneIndex - 1))
            {
                PauseMenu.instance.GamePause();
                PlayerController.Instance.transform.position = respawnPos[i];
                break;
            }
        }
    }
}
