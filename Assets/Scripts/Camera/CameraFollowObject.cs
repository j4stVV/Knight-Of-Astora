using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    public static CameraFollowObject instance;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float flipYRotationTime = 0.5f;

    private Coroutine turnCoroutine;

    private PlayerController player;

    private bool isFacingRight;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        player = playerTransform.gameObject.GetComponent<PlayerController>();

        //isFacingRight = player.IsFacingRight;
    }
    private void FixedUpdate()
    {
        // Check if reference is lost and try to reacquire it
        if (playerTransform == null && PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }

        // Only follow if we have a valid transform
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }
    }

    public void CallTurn()
    {
        //turnCoroutine = StartCoroutine(FlipYLerp());
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEaseInOutSine(); 
    }
    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotation = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < flipYRotationTime) 
        {
            elapsedTime += Time.deltaTime;

            //Lerp the Y rotation
            yRotation = Mathf.Lerp(startRotation, endRotation, elapsedTime);
            yield return null;
        }
    }
    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;
        if (isFacingRight)
        {
            return 180f;
        }    
        else
        {
            return 0f;
        }
    }
}
