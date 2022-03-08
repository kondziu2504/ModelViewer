using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModelPicker : MonoBehaviour
{
    [SerializeField] GameObject[] models;

    int currentModel = 0;

    public UnityEvent onModelChanged = new UnityEvent();

    public GameObject CurrentModel
    {
        get
        {
            if (models == null || models.Length == 0)
                return null;
            else 
                return models[currentModel];
        }
    }

    private void Awake()
    {
        ShowCurrentModel();
    }

    public void SetModels(GameObject[] models)
    {
        this.models = models;
        currentModel = 0;
        ShowCurrentModel();
    }

    public void NextModel()
    {
        if (models == null || models.Length == 0)
            return;
        currentModel = (currentModel + 1) % models.Length;
        onModelChanged.Invoke();
        ShowCurrentModel();
    }
    public void PrevModel()
    {
        if (models == null || models.Length == 0)
            return;
        currentModel = (currentModel - 1 + models.Length) % models.Length;
        onModelChanged.Invoke();
        ShowCurrentModel();
    }

    private void ShowCurrentModel()
    {
        int i = -1;
        foreach(GameObject model in models)
        {
            i++;
            model.SetActive(i == currentModel);
        }
    }
}
