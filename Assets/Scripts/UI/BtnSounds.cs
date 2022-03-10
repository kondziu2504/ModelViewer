using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnSounds : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] AudioClip pressSound;
    [SerializeField] AudioClip hoverSound;
    [SerializeField] float volume = 1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        LeanAudio.play(pressSound, volume);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanAudio.play(hoverSound, volume).pitch = 2f;
    }
}
