using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.IO;

public class SimpleVideoTutorial : MonoBehaviour
{
    [Header("Components")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [Header("WebGL relative URLs (for Render)")]
    [Tooltip("Path relative to the site root, e.g. 'videos/game1_tutorial.mp4'")]
    public string game1WebPath = "videos/game1_tutorial.mp4";

    [Tooltip("Path relative to the site root, e.g. 'videos/game2_tutorial.mp4'")]
    public string game2WebPath = "videos/game2_tutorial.mp4";

    [Header("Local file names (for Editor / Standalone)")]
    [Tooltip("File name inside StreamingAssets/videos, e.g. 'game1_tutorial.mp4'")]
    public string game1FileName = "game1_tutorial.mp4";

    [Tooltip("File name inside StreamingAssets/videos, e.g. 'game2_tutorial.mp4'")]
    public string game2FileName = "game2_tutorial.mp4";

    [Header("Buttons")]
    public Button playPauseButton;
    public Button stopButton;
    public Button muteButton;
    public Button game1Button;
    public Button game2Button;
    public Button backButton;

    [Header("Sliders")]
    public Slider progressSlider;
    public Slider volumeSlider;

    [Header("Optional Labels")]
    public Text playPauseText;
    public Text muteText;

    private string currentUrl;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer reference missing.");
            return;
        }

        // --- Configure VideoPlayer & AudioSource ---
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.Stop();

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        videoPlayer.prepareCompleted += OnVideoPrepared;

        // --- Hook up UI events (fixed) ---
        if (playPauseButton != null) playPauseButton.onClick.AddListener(TogglePlayPause);
        if (stopButton != null) stopButton.onClick.AddListener(StopVideo);
        if (muteButton != null) muteButton.onClick.AddListener(ToggleMute);

        if (game1Button != null) game1Button.onClick.AddListener(PlayGame1Tutorial);
        if (game2Button != null) game2Button.onClick.AddListener(PlayGame2Tutorial);

        if (backButton != null) backButton.onClick.AddListener(BackToMainMenu);

        // --- Volume slider ---
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(SetVolume);

            if (audioSource != null)
                volumeSlider.value = audioSource.volume;
        }

        // --- Progress slider ---
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.onValueChanged.AddListener(Seek);
        }

        UpdateLabels();

        // Optional: auto-load Game 1 (but do NOT auto-play)
        // PlayGame1Tutorial();

        videoPlayer.errorReceived += OnVideoError;

    }
    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[VideoPlayer] Error: {message} | URL: {vp.url}");
    }
    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared. Length (approx): " + vp.length + " seconds");
        // Show the first frame as a preview
        vp.Play();
        vp.Pause();  // now the RenderTexture / RawImage updates to this video's first frame

        // Reset the progress slider to the start
        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(0f);

        UpdateLabels();
    }

    void Update()
    {
        // Update progress slider while playing
        if (videoPlayer.isPrepared && videoPlayer.length > 0 && progressSlider != null && videoPlayer.isPlaying)
        {
            float t = (float)(videoPlayer.time / videoPlayer.length);
            progressSlider.SetValueWithoutNotify(t);
        }
    }

    // ------------ URL resolving ------------

    private string GetUrlFor(string webPath, string fileName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
     string url = Application.streamingAssetsPath + "/videos/" + fileName;
    url = url.Replace("\\", "/");
    Debug.Log($"[Video] WebGL URL: {url}");
    return url;
#else
        // Editor / Standalone: local file in StreamingAssets/videos
        string folder = Path.Combine(Application.streamingAssetsPath, "videos");
        string fullPath = Path.Combine(folder, fileName);

        bool exists = System.IO.File.Exists(fullPath);
        Debug.Log($"[Video] Trying to load file: {fullPath} | Exists: {exists}");

        string url = "file:///" + fullPath.Replace("\\", "/");
        Debug.Log($"[Video] Final URL: {url}");
        return url;
#endif
    }


    private void LoadUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("LoadUrl called with empty URL.");
            return;
        }

        currentUrl = url;
        videoPlayer.url = currentUrl;

        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(0f);

        videoPlayer.Prepare();
        UpdateLabels();
    }

    public void PlayGame1Tutorial()
    {
        LoadUrl(GetUrlFor(game1WebPath, game1FileName));
    }

    public void PlayGame2Tutorial()
    {
        LoadUrl(GetUrlFor(game2WebPath, game2FileName));
    }

    // ------------ Button handlers ------------

    public void TogglePlayPause()
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.Log("TogglePlayPause: video not prepared yet.");
            return;
        }

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();

        UpdateLabels();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();

        if (progressSlider != null)
            progressSlider.SetValueWithoutNotify(0f);

        UpdateLabels();
    }

    public void ToggleMute()
    {
        if (audioSource == null) return;

        audioSource.mute = !audioSource.mute;
        UpdateLabels();
    }

    // ------------ Slider handlers ------------

    public void SetVolume(float value)
    {
        if (audioSource == null) return;
        audioSource.volume = value;
    }

    public void Seek(float sliderValue)
    {
        if (!videoPlayer.isPrepared || videoPlayer.length <= 0)
            return;

        double targetTime = sliderValue * videoPlayer.length;
        videoPlayer.time = targetTime;
    }

    // ------------ Back to menu ------------

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ------------ UI labels ------------

    private void UpdateLabels()
    {
        if (playPauseText != null)
            playPauseText.text = (videoPlayer != null && videoPlayer.isPlaying) ? "Pause" : "Play";

        if (muteText != null && audioSource != null)
            muteText.text = audioSource.mute ? "Unmute" : "Mute";
    }
}
