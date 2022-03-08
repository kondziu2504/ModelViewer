using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelHandler : MonoBehaviour
{
    [SerializeField] Transform sceneCenter;
    [SerializeField] ModelPicker modelPicker;
    public Transform HandledModel => modelPicker.CurrentModel.transform;
    [SerializeField] bool findObjectCenter = true;

    Quaternion rotation = Quaternion.identity;

    private void Awake()
    {
        ResetRotation();
        modelPicker.onModelChanged.AddListener(ResetRotation);
    }

    private void OnDestroy()
    {
        modelPicker.onModelChanged.RemoveListener(ResetRotation);
    }

    private void ResetRotation()
    {
        rotation = Quaternion.identity;
        Rotate(Quaternion.identity);
    }

    public void Rotate(Quaternion deltaRotation)
    {
        rotation = deltaRotation * rotation;

        HandledModel.localPosition = Vector3.zero;
        HandledModel.rotation = Quaternion.identity;

        Vector3 rotationCenter = findObjectCenter ? FindRotationCenter() : sceneCenter.position;

        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);
        HandledModel.RotateAround(rotationCenter, axis, angle);
    }

    private Vector3 FindRotationCenter()
    {
        MeshRenderer[] meshRenderers = HandledModel.GetComponentsInChildren<MeshRenderer>();

        float bottom, top;
        bottom = top = HandledModel.position.y;
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            bottom = Mathf.Min(bottom, meshRenderer.bounds.min.y);
            top = Mathf.Max(top, meshRenderer.bounds.max.y);
        }

        Vector3 rotationCenter = new Vector3(
            sceneCenter.position.x,
            (bottom + top) / 2f,
            sceneCenter.position.z
        );

        return rotationCenter;
    }
}
