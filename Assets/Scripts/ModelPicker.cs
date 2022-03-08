using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModelPicker : MonoBehaviour
{
    [SerializeField] Transform modelsParent;
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
        foreach (GameObject model in this.models)
            model.transform.parent = modelsParent;

        ShowCurrentModel();
    }

    public void SetModels(GameObject[] models)
    {
        foreach (GameObject model in this.models)
            model.SetActive(false);

        this.models = models;
        foreach (GameObject model in this.models)
            model.transform.parent = modelsParent;
        currentModel = 0;
        onModelChanged.Invoke();
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
