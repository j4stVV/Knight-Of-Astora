using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SubCameraController : MonoBehaviour
{
    public static SubCameraController Instance;
    private CinemachineConfiner2D confiner;
    private float timeGap = 3f;
    private float lastUpdateTime;
    private void Update()
    {
        if(Time.time > lastUpdateTime + timeGap)
        {
            FindCamBoundary();
        }
    }
    void FindCamBoundary()
    {
        lastUpdateTime = Time.time; 
        confiner = GetComponent<CinemachineConfiner2D>();
        GameObject boundaryObject = GameObject.FindGameObjectWithTag("CamBoundary");
        if (boundaryObject != null)
        {
            Collider2D boundaryCollider = boundaryObject.GetComponent<Collider2D>();
            if (boundaryCollider != null)
            {
                confiner.m_BoundingShape2D = boundaryCollider;
            }
            else
            {
                Debug.LogError("Not found component Collider2D on GameObject with tag \"CamBoundary\"");
            }
        }
    }

}
