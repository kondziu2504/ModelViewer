using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ControlsPanel : MonoBehaviour
{
    [SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;
    [SerializeField] ModelPicker modelPicker;

    private void Start()
    {
        SetBtnsInteractable();
        modelPicker.onNewModels.AddListener(SetBtnsInteractable);
    }

    private void OnDestroy()
    {
        modelPicker.onNewModels.RemoveListener(SetBtnsInteractable);
    }

    private void SetBtnsInteractable()
    {
        bool interactable = ModelsNotEmpty();
        leftBtn.interactable = interactable;
        rightBtn.interactable = interactable;
    }

    private bool ModelsNotEmpty()
    {
        return modelPicker.Models != null && modelPicker.Models.Length > 0;
    }
}
