using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BtnSounds : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] AudioClip pressSound;
    [SerializeField] AudioClip hoverSound;
    [SerializeField] float volume = 1f;
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        LeanAudio.play(pressSound, volume);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        LeanAudio.play(hoverSound, volume).pitch = 2f;
    }
}
