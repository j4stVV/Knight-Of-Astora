using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] allVirtualCameras;

    [Header("Control for lerping the Y Damping during jump/fall")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCoroutine;

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normYPanAmount;

    private Vector2 startingTrackedObjectOffset;

    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        for (int i = 0; i < allVirtualCameras.Length; i++)
        {
            if (allVirtualCameras[i].enabled)
            {
                //set the current active camera 
                currentCamera = allVirtualCameras[i];
                
                //set the framing transposer 
                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        //set the YDamping amount so it's based on the inspector value
        normYPanAmount = framingTransposer.m_YDamping;

        //set the starting position of the tracked object offset
        startingTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
    }

    #region Lerp the Y Damping

    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }
    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        //grab the starting damping amount
        float startFampAmount = framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        //determine the end damping amount
        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            LerpedFromPlayerFalling = true;
        }

        else
        {
            endDampAmount = normYPanAmount;
        }

        //lerp the pan amount
        float elapsedTime = 0f;
        while(elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startFampAmount, endDampAmount, (elapsedTime / fallYPanTime));
            framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }

    #endregion

    #region Pan Camera

    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartPos)
    {
        panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartPos));
    }
    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = Vector2.zero;

        //handle pan from trigger
        if(!panToStartPos)
        {
            //set the direction and distance
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.right;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.left;
                    break;
                default:
                    break;
            }
            endPos *= panDistance;
            startPos = startingTrackedObjectOffset;
            endPos += startPos;
        }
        //handle the pan back to starting position
        else
        {
            startPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjectOffset;
        }

        //handle actual panning of the camera
        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;
            Vector3 panLerp = Vector3.Lerp(startPos, endPos, (elapsedTime / panTime));
            framingTransposer.m_TrackedObjectOffset = panLerp;
            yield return null;
        }

    }

    #endregion

    #region Swap Camera

    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft,CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        //if the camera is on the left and our trigger exit direction was on the right 
        if(currentCamera == cameraFromLeft && triggerExitDirection.x >= 0f)
        {
            //activate new camera
            cameraFromRight.enabled = true;

            //deactivate old camera
            cameraFromLeft.enabled = false;

            //set the new camera as the current camera
            currentCamera = cameraFromRight;

            //update our composer variable
            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        }
        //if the camera is on the right and our trigger exit direction was on the left 
        if(currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            //activate new camera
            cameraFromLeft.enabled = true;

            //deactivate old camera
            cameraFromRight.enabled = false;

            //set the new camera as the current camera
            currentCamera = cameraFromLeft;

            //update our composer variable
            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        }

    }

    #endregion
}
