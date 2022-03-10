using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelHandler : MonoBehaviour
{
    [SerializeField] Transform sceneCenter;
    [SerializeField] ModelPicker modelPicker;
    public Transform HandledModel => modelPicker.CurrentModel == null ? null : modelPicker.CurrentModel.transform;
    [SerializeField] float maxModelSize = 10f;

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Bounds modelBounds = ModelBounds();
        Gizmos.DrawWireCube(modelBounds.center, modelBounds.size);
    }

    private void ResetRotation()
    {
        rotation = Quaternion.identity;
        Rotate(Quaternion.identity);
    }

    public void Rotate(Quaternion deltaRotation)
    {
        if (HandledModel == null)
            return;

        rotation = deltaRotation * rotation;

        HandledModel.position = sceneCenter.position;
        HandledModel.rotation = Quaternion.identity;
        HandledModel.localScale = Vector3.one;
        FitModelToMaxSize();
        HandledModel.position = sceneCenter.position;
        Bounds modelBounds = ModelBounds();
        HandledModel.position -= (modelBounds.center - sceneCenter.position);

        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);
        HandledModel.RotateAround(sceneCenter.position, axis, angle);
    }

    private Bounds ModelBounds()
    {
        if (HandledModel == null)
            return new Bounds();

        MeshRenderer[] meshRenderers = HandledModel.GetComponentsInChildren<MeshRenderer>();

        Bounds totalBounds = new Bounds(HandledModel.position, Vector3.zero);
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            totalBounds.min = new Vector3(
                Mathf.Min(totalBounds.min.x, meshRenderer.bounds.min.x),
                Mathf.Min(totalBounds.min.y, meshRenderer.bounds.min.y),
                Mathf.Min(totalBounds.min.z, meshRenderer.bounds.min.z)
            );

            totalBounds.max = new Vector3(
                Mathf.Max(totalBounds.max.x, meshRenderer.bounds.max.x),
                Mathf.Max(totalBounds.max.y, meshRenderer.bounds.max.y),
                Mathf.Max(totalBounds.max.z, meshRenderer.bounds.max.z)
            );
        }
        return totalBounds;
    }

    private void FitModelToMaxSize()
    {
        Bounds modelBounds = ModelBounds();
        float biggestDim = Mathf.Max(modelBounds.size.x, modelBounds.size.y, modelBounds.size.z);
        if (biggestDim > maxModelSize)
        {
            HandledModel.localScale *= maxModelSize / biggestDim;
        }
    }

    private Vector3 FindRotationCenter()
    {
        MeshRenderer[] meshRenderers = HandledModel.GetComponentsInChildren<MeshRenderer>();
        
        Bounds modelBounds = ModelBounds();
        
        Vector3 rotationCenter = new Vector3(
            sceneCenter.position.x,
            modelBounds.center.y,
            sceneCenter.position.z
        );

        return rotationCenter;
    }
}
