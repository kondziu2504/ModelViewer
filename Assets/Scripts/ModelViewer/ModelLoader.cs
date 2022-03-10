using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.Events;

public class ModelLoader : MonoBehaviour
{
    [SerializeField] ModelPicker modelPicker;
    [SerializeField] string modelsFolder = "Input\\";
    [SerializeField] Material defaultMaterial;

    public float LoadingProgress { get; private set; } = 0f;
    public UnityEvent onLoaded = new UnityEvent();


    void Start()
    {
        StartCoroutine(LoadModelsCoroutine());
    }

    IEnumerator LoadModelsCoroutine()
    {
        //Start loading on next frame, so first frame is rendered
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (!Directory.Exists(modelsFolder))
            Directory.CreateDirectory(modelsFolder);

        string[] modelFiles = Directory.GetFiles(modelsFolder, "*.fbx", SearchOption.AllDirectories);

        List<GameObject> loadedModels = new List<GameObject>();

        foreach (string modelFile in modelFiles)
        {
            GameObject loadedModel = FbxToGameObjectConverter.Convert(modelFile, defaultMaterial, Directory.GetCurrentDirectory() + "\\" + modelsFolder);
            loadedModel.name = Path.GetFileNameWithoutExtension(modelFile);
            loadedModels.Add(loadedModel);
            LoadingProgress = (float)loadedModels.Count / modelFiles.Length;
            yield return new WaitForEndOfFrame();
        }

        modelPicker.SetModels(loadedModels.ToArray());
        LoadingProgress = 1f;
        onLoaded.Invoke();
    }
}
