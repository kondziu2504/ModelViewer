using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    CinemachineTrackedDolly cameraDolly;

    float _progress;
    float Progress // values from 0 to 1, where 1 means full zoom
    {
        get => _progress;
        set => _progress = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        cameraDolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        Progress = 0.5f;
    }

    public void Zoom(float deltaProgress)
    {
        Progress += deltaProgress;
    }

    private void Update()
    {
        UpdateCameraPos();
    }

    private void UpdateCameraPos()
    {
        CinemachineSmoothPath cameraPath = (CinemachineSmoothPath)cameraDolly.m_Path;
        float targetPosition = Progress * (cameraPath.m_Waypoints.Length - 1);
        cameraDolly.m_PathPosition = targetPosition;
    }
}
