using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnSounds : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] AudioClip sound;
    [SerializeField] float volume = 1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        LeanAudio.play(sound, volume);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanAudio.play(sound, volume).pitch = 2f;
    }
}
