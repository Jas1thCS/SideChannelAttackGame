using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;

public class TutorialOverlayVideo : MonoBehaviour
{
    [Header("Overlay Root")]
    public GameObject panelRoot;
    public bool pauseGameWhileOpen = true;

    private float storedTimeScale = 1f;

    [Header("Components")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [Header("Buttons")]
    public Button playPauseButton;
    public Button muteButton;
    public Button closeButton;

    [Header("Sliders")]
    public Slider progressSlider;
    public Slider volumeSlider;

    [Header("Optional Labels (TMP)")]
    public TMP_Text playPauseText;
    public TMP_Text muteText;

    [Header("Slider Value Labels (TMP)")]
    public TMP_Text progressValueText;   // e.g. "00:12 / 01:45"
    public TMP_Text volumeValueText;     // e.g. "80%"

    [Header("Video Paths")]
    [Tooltip("WebGL: path relative to site root, e.g. 'videos/game1_tutorial.mp4'")]
    public string webPath = "videos/game1_tutorial.mp4";

    [Tooltip("Editor/Standalone: file name inside StreamingAssets/videos")]
    public string fileName = "game1_tutorial.mp4";

    private string currentUrl;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (videoPlayer == null)
        {
            Debug.LogError("TutorialOverlayVideo: VideoPlayer reference missing.");
            return;
        }

        // Configure VideoPlayer & Audio
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.Stop();

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        // Hook up buttons
        if (playPauseButton != null) playPauseButton.onClick.AddListener(TogglePlayPause);
        if (muteButton != null) muteButton.onClick.AddListener(ToggleMute);
        if (closeButton != null) closeButton.onClick.AddListener(CloseOverlay);

        // Volume slider
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(SetVolume);

            if (audioSource != null)
                volumeSlider.value = audioSource.volume;
        }

        // Progress slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.onValueChanged.AddListener(Seek);
        }

        UpdateLabels();
        UpdateSliderValueLabels();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[VideoPlayer] Error: {message} | URL: {vp.url}");
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    // Called by your external "Tutorial" button
    public void OpenOverlay()
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(true);

        if (pauseGameWhileOpen)
        {
            storedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        LoadUrl(GetUrl());
    }

    private void CloseOverlay()
    {
        if (panelRoot == null) return;

        if (videoPlayer != null)
            videoPlayer.Stop();

        panelRoot.SetActive(false);

        if (pauseGameWhileOpen)
            Time.timeScale = storedTimeScale;

        UpdateLabels();
        UpdateSliderValueLabels();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Overlay video prepared. Length (approx): " + vp.length + " seconds");

        // Show first frame as preview
        vp.Play();
        vp.Pause();

        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(0f);

        UpdateLabels();
        UpdateSliderValueLabels();
    }

    void Update()
    {
        // Keep progress slider synced (only while playing)
        if (videoPlayer != null && videoPlayer.isPrepared && videoPlayer.length > 0 && progressSlider != null)
        {
            if (videoPlayer.isPlaying)
            {
                float t = (float)(videoPlayer.time / videoPlayer.length);
                progressSlider.SetValueWithoutNotify(t);
            }

            // Update label even when paused
            UpdateProgressLabel();
        }

        // Volume label can be updated anytime
        UpdateVolumeLabel();
    }

    // -------- URL helpers --------

    private string GetUrl()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.streamingAssetsPath + "/videos/" + fileName;
        url = url.Replace("\\", "/");
        Debug.Log($"[Overlay] WebGL URL: {url}");
        return url;
#else
        string folder = Path.Combine(Application.streamingAssetsPath, "videos");
        string fullPath = Path.Combine(folder, fileName);
        string url = "file:///" + fullPath.Replace("\\", "/");
        Debug.Log($"[Overlay] Editor URL: {url}");
        return url;
#endif
    }

    private void LoadUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("TutorialOverlayVideo: LoadUrl called with empty URL.");
            return;
        }

        currentUrl = url;
        videoPlayer.url = currentUrl;

        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(0f);

        videoPlayer.Prepare();
        UpdateLabels();
        UpdateSliderValueLabels();
    }

    // -------- Controls --------

    public void TogglePlayPause()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared) return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();

        UpdateLabels();
        UpdateSliderValueLabels();
    }

    public void ToggleMute()
    {
        if (audioSource == null) return;

        audioSource.mute = !audioSource.mute;
        UpdateLabels();
        UpdateVolumeLabel();
    }

    public void SetVolume(float value)
    {
        if (audioSource == null) return;

        audioSource.volume = value;
        UpdateVolumeLabel();
    }

    public void Seek(float sliderValue)
    {
        if (videoPlayer == null || !videoPlayer.isPrepared || videoPlayer.length <= 0)
            return;

        double targetTime = sliderValue * videoPlayer.length;
        videoPlayer.time = targetTime;

        UpdateProgressLabel();
    }

    private void UpdateLabels()
    {
        if (playPauseText != null && videoPlayer != null)
            playPauseText.text = videoPlayer.isPlaying ? "Pause" : "Play";

        if (muteText != null && audioSource != null)
            muteText.text = audioSource.mute ? "Unmute" : "Mute";
    }

    // -------- Slider value labels --------

    private void UpdateSliderValueLabels()
    {
        UpdateProgressLabel();
        UpdateVolumeLabel();
    }

    private void UpdateProgressLabel()
    {
        if (progressValueText == null) return;

        if (videoPlayer == null || !videoPlayer.isPrepared || videoPlayer.length <= 0)
        {
            progressValueText.text = "00:00 / 00:00";
            return;
        }

        double cur = videoPlayer.time;
        double total = videoPlayer.length;

        progressValueText.text = $"{FormatTime(cur)} / {FormatTime(total)}";
    }

    private void UpdateVolumeLabel()
    {
        if (volumeValueText == null) return;

        float vol = (audioSource != null) ? audioSource.volume : (volumeSlider != null ? volumeSlider.value : 0f);
        int percent = Mathf.RoundToInt(vol * 100f);
        volumeValueText.text = $"{percent}%";
    }

    private string FormatTime(double seconds)
    {
        if (seconds < 0) seconds = 0;
        int s = Mathf.FloorToInt((float)seconds);
        int mm = s / 60;
        int ss = s % 60;
        return $"{mm:00}:{ss:00}";
    }
}
