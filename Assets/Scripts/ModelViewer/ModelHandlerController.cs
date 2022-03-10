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
    [SerializeField] Texture2D rotationCursor;

    Controls controls;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.ModelViewer.RotationStart.started += UpdateCursor;
        controls.ModelViewer.RotationStart.canceled += UpdateCursor;
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.ModelViewer.RotationStart.started -= UpdateCursor;
        controls.ModelViewer.RotationStart.canceled -= UpdateCursor;
    }

    void Update()
    {
        if (controls.ModelViewer.RotationEnabled.IsPressed())
            RotateModel();
    }

    void UpdateCursor(InputAction.CallbackContext obj)
    {

        if (controls.ModelViewer.RotationEnabled.IsPressed())
            Cursor.SetCursor(rotationCursor, new Vector2(rotationCursor.width / 2f, rotationCursor.height / 2f), CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void RotateModel()
    {
        if (modelHandler.HandledModel == null)
            return;

        Vector2 moveDelta = controls.ModelViewer.RotationDelta.ReadValue<Vector2>();
        
        float rotationX = moveDelta.x * sensitivity;
        float rotationY = moveDelta.y * sensitivity;

        Vector3 toTargetFlat = Vector3.Scale((modelHandler.HandledModel.position - camera.transform.position), new Vector3(1f, 0f, 1f)).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, toTargetFlat);
        Vector3 up = Vector3.Cross(toTargetFlat, right);

        modelHandler.Rotate(Quaternion.AngleAxis(-rotationX, up) * Quaternion.AngleAxis(rotationY, right));
    }
}
