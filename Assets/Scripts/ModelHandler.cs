using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelHandler : MonoBehaviour
{
    [SerializeField] Transform sceneCenter;
    [SerializeField] Transform handledModel;
    public Transform HandledModel => handledModel;
    [SerializeField] bool findObjectCenter = true;

    Quaternion rotation = Quaternion.identity;

    private void Awake()
    {
        Rotate(Quaternion.identity);
    }

    public void Rotate(Quaternion deltaRotation)
    {
        rotation = deltaRotation * rotation;

        handledModel.localPosition = Vector3.zero;
        handledModel.rotation = Quaternion.identity;

        Vector3 rotationCenter = findObjectCenter ? FindRotationCenter() : sceneCenter.position;

        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);
        handledModel.RotateAround(rotationCenter, axis, angle);
    }

    private Vector3 FindRotationCenter()
    {
        MeshRenderer[] meshRenderers = handledModel.GetComponentsInChildren<MeshRenderer>();

        float bottom, top;
        bottom = top = handledModel.position.y;
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
