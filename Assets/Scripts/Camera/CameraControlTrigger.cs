using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraControlTrigger : MonoBehaviour
{
    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(customInspectorObjects.panCameraOnContact)
            {
                //pan the camera
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime, 
                    customInspectorObjects.panDirection, false);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 exitDirection = (collision.transform.position - coll.bounds.center).normalized;

            if(customInspectorObjects.swapCamera && customInspectorObjects.cameraOnLeft != null && customInspectorObjects.cameraOnRight != null)
            {
                CameraManager.instance.SwapCamera(customInspectorObjects.cameraOnLeft, customInspectorObjects.cameraOnRight, exitDirection);
            }

            if (customInspectorObjects.panCameraOnContact)
            {
                //pan the camera
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime,
                    customInspectorObjects.panDirection, true);
            }
        }
    }

    public CustomInspectorObjects customInspectorObjects;
}

[System.Serializable] 
public class CustomInspectorObjects
{
    public bool swapCamera = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up, 
    Down,
    Left,
    Right
}

[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;
    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.swapCamera)
        {
            cameraControlTrigger.customInspectorObjects.cameraOnLeft = EditorGUILayout.ObjectField("Camera On Left", cameraControlTrigger.customInspectorObjects.cameraOnLeft,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
            cameraControlTrigger.customInspectorObjects.cameraOnRight = EditorGUILayout.ObjectField("Camera On Right", cameraControlTrigger.customInspectorObjects.cameraOnRight,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }
        if (cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspectorObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                cameraControlTrigger.customInspectorObjects.panDirection);
            cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan Distance",
                cameraControlTrigger.customInspectorObjects.panDistance);
            cameraControlTrigger.customInspectorObjects.panTime= EditorGUILayout.FloatField("Pan Time",
                cameraControlTrigger.customInspectorObjects.panTime);
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}