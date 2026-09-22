using System.Collections;
using TMPro;
using UnityEngine;

public class AnnotationMarker : MonoBehaviour
{
    public Renderer sphereRenderer;
    public Material sphereMaterial;
    public Material materialA;
    public Material materialB;

    public float duration = 3f;
    public float blinkInterval = 0.2f;

    private int annotationID;

    private Coroutine blinkCoroutine;
    private bool isBlinking = false;

    public void StartBlink()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        isBlinking = true;
        float elapsed = 0f;
        bool useMaterialA = true;

        while (elapsed < duration)
        {
            sphereRenderer.material = useMaterialA ? materialA : materialB;
            useMaterialA = !useMaterialA;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        RestoreMaterial();
    }

    public void ForceStopBlink()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
            
        RestoreMaterial();
    }

    private void RestoreMaterial()
    {
        if (isBlinking)
        {
            sphereRenderer.material = sphereMaterial;
            isBlinking = false;
            blinkCoroutine = null;
        }
    }

    public void SetAnnotationID(int id, int idAnnotation)
    {
        GetComponentInChildren<TMP_Text>().text = idAnnotation.ToString();
        annotationID = id;
    }

    public int GetAnnotationID()
    {
        return annotationID;
    }
}