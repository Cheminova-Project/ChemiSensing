using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playPauseButton;
    public Sprite playIcon;
    public Sprite pauseIcon;
    public Slider timelineSlider;
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI timeLeftText;

    private AudioSource audioSource;
    private bool isDragging;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playPauseButton.onClick.AddListener(TogglePlayPause);
        timelineSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void Update()
    {
        if (audioSource.clip == null)
            return;

        if (!isDragging)
        {
            timelineSlider.value = audioSource.time / audioSource.clip.length;
        }

        UpdateTimeTexts();
        UpdatePlayPauseIcon();
    }

    void TogglePlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.Play();
        }
    }

    void OnSliderValueChanged(float value)
    {
        if (!audioSource.clip) return;

        float newTime = value * audioSource.clip.length;

        // Clamp time to prevent errors at clip end
        newTime = Mathf.Clamp(newTime, 0f, audioSource.clip.length - 0.01f);

        if (!isDragging)
        {
            audioSource.time = newTime;
        }
    }
    
    public void StartDrag() => isDragging = true;
    public void EndDrag()
    {
        isDragging = false;

        if (audioSource.clip)
        {
            float newTime = timelineSlider.value * audioSource.clip.length;
            newTime = Mathf.Clamp(newTime, 0f, audioSource.clip.length - 0.01f);
            audioSource.time = newTime;
        }
    }
    
    void UpdateTimeTexts()
    {
        float current = audioSource.time;
        float total = audioSource.clip.length;
        float remaining = total - current;

        currentTimeText.text = FormatTime(current);
        timeLeftText.text = "- " + FormatTime(remaining);
    }

    void UpdatePlayPauseIcon()
    {
        if (audioSource.isPlaying)
            playPauseButton.image.sprite = pauseIcon;
        else
            playPauseButton.image.sprite = playIcon;
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}