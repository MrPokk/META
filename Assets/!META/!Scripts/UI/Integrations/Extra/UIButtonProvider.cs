using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Button;


public class UIButtonProvider : MonoBehaviour, IPointerClickHandler
{
    public ButtonClickedEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
