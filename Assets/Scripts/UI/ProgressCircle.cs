using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressCircle : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] ModelLoader modelLoader;
    [SerializeField] TextMeshProUGUI percentageTextField;

    private void Awake()
    {
        loadingBar.fillAmount = 0f;
    }

    void Update()
    {
        loadingBar.fillAmount = modelLoader.LoadingProgress;
        percentageTextField.text = string.Format("{0}%", (int)(modelLoader.LoadingProgress * 100f));
    }
}
