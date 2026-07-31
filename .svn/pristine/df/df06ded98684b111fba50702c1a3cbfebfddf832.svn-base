using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRFade : MonoBehaviour
{
    [Tooltip("CanvasGroup used for fade (can be on parent Canvas or directly on the Image).")]
    public CanvasGroup canvasGroup;
    public float defaultDuration = 0.5f;
    public bool startBlack = false;
    public bool autoFadeIn = true;

    public static XRFade Instance { get; private set; }
    
    void Awake()
    {
        if (startBlack)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (autoFadeIn)
        {
            FadeIn();
        }

        if(Instance != null && Instance != this && Instance.isActiveAndEnabled)
        {
            Destroy(this.gameObject);
            Debug.LogError("Multiple XRFade instances detected! Destroying duplicate.");
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void OnDisable()
    {
        Instance = null;
    }

    public void FadeIn(float duration = -1f) => StartCoroutine(Fade(0f, duration < 0 ? defaultDuration : duration));

    public void FadeOut(UnityAction onComplete, float duration = -1f) => StartCoroutine(Fade(1f, duration < 0 ? defaultDuration : duration, onComplete));

    public void FadeOut(float duration = -1f) => StartCoroutine(Fade(1f, duration < 0 ? defaultDuration : duration, null));

    IEnumerator Fade(float targetAlpha, float duration, UnityAction onComplete = null)
    {
        float start = canvasGroup.alpha;
        float time = 0f;
        
        if (targetAlpha > 0) 
        {
            canvasGroup.blocksRaycasts = true; // bloquear input durante fundido a negro
        }
        
        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // usar unscaled para que no dependa del timeScale
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, time / duration);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0;
        
        onComplete?.Invoke();
    }
}
