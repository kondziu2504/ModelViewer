using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] CameraZoom cameraZoom;
    [SerializeField] float sensitivity = 5f;

    Input input;

    private void Awake()
    {
        input = new Input();
    }

    private void Update()
    {
        //cameraZoom.Zoom(input.ModelViewer.ZoomDelta.ReadValue<float>()); 
        float scrollY = Mouse.current.scroll.ReadValue().y;
        if (scrollY > 0)
            cameraZoom.Zoom(sensitivity * Time.deltaTime);
        else if (scrollY < 0)
            cameraZoom.Zoom(-sensitivity * Time.deltaTime);
    }

}
