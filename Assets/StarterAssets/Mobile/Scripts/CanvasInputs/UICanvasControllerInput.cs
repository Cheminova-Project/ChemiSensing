using UnityEngine;

public class UICanvasControllerInput : MonoBehaviour
{
    [Header("Output")]
    public MobileFirstPersonController mobileFirstPersonController;

    public void VirtualMoveInput(Vector2 virtualMoveDirection)
    {
        mobileFirstPersonController.InputMove(virtualMoveDirection);
    }
}