namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// ScriptableObject que representa un evento de ARRaycastHit.
    /// Permite crear eventos personalizados basados en ARRaycastHit.
    /// </summary>
    [CreateAssetMenu(menuName = "XR/AR Foundation/Events/AR Raycast Hit Event Asset", fileName = "AR Raycast Hit Event")]
    public class ARRaycastHitEventAsset : EventAsset<ARRaycastHit>
    {
    }
}