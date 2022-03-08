using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ModelLoader : MonoBehaviour
{
    [SerializeField] ModelPicker modelPicker;
    [SerializeField] string modelsFolder = "Input\\";
    [SerializeField] Material defaultMaterial;
    
    void Start()
    {
        string[] modelFiles = Directory.GetFiles(modelsFolder, "*.fbx");

        List<GameObject> loadedModels = new List<GameObject>();

        foreach (string modelFile in modelFiles)
        {
            GameObject loadedModel = FbxToGameObjectConverter.Convert(modelFile, defaultMaterial);
            loadedModel.name = Path.GetFileNameWithoutExtension(modelFile);
            loadedModels.Add(loadedModel);
        }

        modelPicker.SetModels(loadedModels.ToArray());
    }
}
