using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер для воспроизведения видео-заставки.
/// Поддерживает пропуск видео и автоматический переход в игру.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoIntroController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoClip introVideo;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";
    
    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private float skipDelay = 1f;

    private VideoPlayer videoPlayer;
    private float videoStartTime;
    private bool hasStarted = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer компонент не найден!");
            return;
        }

        SetupVideoPlayer();
    }

    private void SetupVideoPlayer()
    {
        if (introVideo != null)
            videoPlayer.clip = introVideo;

        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = false;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        videoPlayer.Play();
        videoStartTime = Time.time;
        hasStarted = true;
    }

    private void Update()
    {
        if (!hasStarted || !allowSkip)
            return;

        if (Time.time - videoStartTime < skipDelay)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || 
            Input.GetKeyDown(KeyCode.Space) || 
            Input.anyKeyDown)
        {
            SkipVideo();
        }
    }

    private void SkipVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            LoadGameScene();
        }
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneTransitionManager.Instance.LoadScene(gameSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
