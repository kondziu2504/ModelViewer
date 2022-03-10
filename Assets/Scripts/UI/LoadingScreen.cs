using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] ModelLoader modelLoader;
    [SerializeField] CanvasGroup canvasGroup;

    private void Start()
    {
        if (modelLoader.LoadingProgress == 1f)
            Hide();
        else
            modelLoader.onLoaded.AddListener(Hide);
    }

    private void Hide()
    {
        canvasGroup.LeanAlpha(0f, 0.3f)
            .setOnComplete(() => canvasGroup.blocksRaycasts = false);
    }
}
