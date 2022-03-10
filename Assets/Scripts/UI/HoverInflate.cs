using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverInflate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] float scaleMultiplier = 1.1f;
    [SerializeField] float inflatingTime = 1f;
    Button button;

    float timeHovered = 0f;

    Vector3 originalScale;

    bool pointerInside = false;
    bool pointerDown = false;

    private void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (button != null && !button.interactable)
            return;

        if (pointerInside)
            timeHovered += Time.deltaTime;
        else
            timeHovered -= Time.deltaTime;

        timeHovered = Mathf.Clamp(timeHovered, 0f, inflatingTime);

        if (pointerDown)
            transform.localScale = originalScale;
        else
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleMultiplier, timeHovered / inflatingTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        timeHovered = 0f;
    }
}
