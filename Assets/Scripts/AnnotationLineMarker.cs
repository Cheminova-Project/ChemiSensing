using System.Collections;
using UnityEngine;

public class AnnotationLineMarker : MonoBehaviour
{
    private LineRenderer lr;
    private Color originalStartColor;
    private Color originalEndColor;
    
    public float duration = 3f;
    public float blinkInterval = 0.2f;
    
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void StartBlink()
    {
        if (lr == null) return;

        if (!isBlinking)
        {
            originalStartColor = lr.startColor;
            originalEndColor = lr.endColor;
        }

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        isBlinking = true;
        float elapsed = 0f;
        bool isOtherColor = true;

        while (elapsed < duration)
        {
            if (isOtherColor)
            {
                lr.startColor = Color.white;
                lr.endColor = Color.white;
            }
            else
            {
                lr.startColor = originalStartColor;
                lr.endColor = originalEndColor;
            }
            
            isOtherColor = !isOtherColor;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        RestoreColor();
    }

    public void ForceStopBlink()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        RestoreColor();
    }

    private void RestoreColor()
    {
        if (isBlinking)
        {
            lr.startColor = originalStartColor;
            lr.endColor = originalEndColor;
            isBlinking = false;
            blinkCoroutine = null;
        }
    }
}