using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    //respawn variables
    private int defaultSceneIndex = 1;
    private Vector2 platformingRespawnPoint = new Vector2(-37, -11);
    private string checkpointSceneName; 
    private Vector2 checkpointPosition;

    //scene transition variables
    [SerializeField] private Animator transitionAnim;
    public string transitionedFromScene;
    private Vector2 transitionPos;
    private Vector2 transitionDir;
    private float delay = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void FadeIn()
    {
        Debug.Log("Fade in called");
        transitionAnim.SetTrigger("Start");
    }
    public void FadeOut()
    {
        Debug.Log("Fade out called");
        transitionAnim.SetTrigger("End");
    }
    public void SetCheckpoint(string sceneName, Vector2 position)
    {
        checkpointSceneName = sceneName;
        checkpointPosition = position;

        Debug.Log($"Checkpoint set! Scene: {sceneName}, Position: {position}");
    }
    public void setTransitionPoint(Vector2 position, Vector2 direction)
    {
        transitionPos = position;
        transitionDir = direction;
    }
    public void RespawnPlayer()
    {
        if (!string.IsNullOrEmpty(checkpointSceneName))
        {
            if (SceneManager.GetActiveScene().name != checkpointSceneName)
            {
                SceneManager.LoadSceneAsync(checkpointSceneName);
                StartCoroutine(RespawnAfterSceneLoad());
            }
            else
            {
                RespawnAtCheckpoint();
            }
        }
        else
        {
            RespawnAtDefault();
        }
    }
    private IEnumerator RespawnAfterSceneLoad()
    {
        FadeIn();
        yield return new WaitForSeconds(1f); // Đợi scene load xong
        RespawnAtCheckpoint();
    }
    private void RespawnAtCheckpoint()
    {
        PlayerController.Instance.transform.position = checkpointPosition;
        StartCoroutine(UIManager.Instance.DeactiveDeathScreen());
        PlayerController.Instance.Respawn();
    }
    private void RespawnAtDefault()
    {
        SceneManager.LoadSceneAsync(defaultSceneIndex);
        PlayerController.Instance.transform.position = platformingRespawnPoint;
        StartCoroutine(UIManager.Instance.DeactiveDeathScreen());
        PlayerController.Instance.Respawn();
    }
    public IEnumerator FadeAndLoadScene(string sceneName)
    {
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync(sceneName);
        transitionAnim.SetTrigger("Start");
        StartCoroutine(PlayerController.Instance.WalkIntoNewScene(transitionPos, transitionDir, delay));
    }
}
