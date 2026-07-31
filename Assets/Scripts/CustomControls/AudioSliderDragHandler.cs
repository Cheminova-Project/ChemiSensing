using UnityEngine;
using UnityEngine.EventSystems;

public class AudioSliderDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioController audioController;

    public void OnPointerDown(PointerEventData eventData)
    {
        audioController.StartDrag();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        audioController.EndDrag();
    }
}