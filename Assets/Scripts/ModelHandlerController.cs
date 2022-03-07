using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModelHandlerController : MonoBehaviour
{
    [SerializeField] ModelHandler modelHandler;
    [SerializeField] new Camera camera;
    [Range(0.05f, 2f)]
    [SerializeField] float sensitivity = 1f;

    Input input;

    private void Awake()
    {
        input = new Input();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        if (input.ModelViewer.RotationEnabled.IsPressed())
            RotateModel();
    }

    private void RotateModel()
    {
        Vector2 moveDelta = input.ModelViewer.RotationDelta.ReadValue<Vector2>();
        
        float rotationX = moveDelta.x * sensitivity;
        float rotationY = moveDelta.y * sensitivity;

        Vector3 toTargetFlat = Vector3.Scale((modelHandler.HandledModel.position - camera.transform.position), new Vector3(1f, 0f, 1f)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, toTargetFlat);
        Vector3 up = Vector3.Cross(toTargetFlat, right);

        modelHandler.Rotate(Quaternion.AngleAxis(-rotationX, up) * Quaternion.AngleAxis(rotationY, right));
    }
}
