using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string transitionedFromScene;

    //[SerializeField]private GameObject bossFightZone;

    public Vector2 platformingRespawnPoint;
    private string checkpointSceneName; // Tên scene của checkpoint
    private Vector2 checkpointPosition; // Vị trí checkpoint
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

    public void SetCheckpoint(string sceneName, Vector2 position)
    {
        checkpointSceneName = sceneName;
        checkpointPosition = position;

        Debug.Log($"Checkpoint set! Scene: {sceneName}, Position: {position}");
    }

    public void RespawnPlayer()
    {
        if (!string.IsNullOrEmpty(checkpointSceneName))
        {
            if (SceneManager.GetActiveScene().name != checkpointSceneName)
            {
                // Chuyển về scene chứa checkpoint
                SceneManager.LoadScene(checkpointSceneName);
                StartCoroutine(RespawnAfterSceneLoad());
            }
            else
            {
                // Hồi sinh tại checkpoint trong scene hiện tại
                RespawnAtCheckpoint();
            }
        }
        else
        {
            // Hồi sinh tại vị trí mặc định
            RespawnAtDefault();
        }
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    bossFightZone.SetActive(true);
    //}

    private IEnumerator RespawnAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.1f); // Đợi scene load xong
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
        PlayerController.Instance.transform.position = platformingRespawnPoint;
        StartCoroutine(UIManager.Instance.DeactiveDeathScreen());
        PlayerController.Instance.Respawn();
    }
}
