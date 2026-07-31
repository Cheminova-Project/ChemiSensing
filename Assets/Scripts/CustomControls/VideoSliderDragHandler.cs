using UnityEngine;
using UnityEngine.EventSystems;

public class VideoSliderDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public DynamicVideoDisplay videoController;

    public void OnPointerDown(PointerEventData eventData)
    {
        videoController.StartDrag();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        videoController.EndDrag();
    }
}