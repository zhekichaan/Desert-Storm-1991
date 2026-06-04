using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSplashLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;       // assign in inspector
    public string mainMenuScene = "MainMenu"; // exact name

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Ensure loop is OFF
        videoPlayer.isLooping = false;

        // Subscribe to event
        videoPlayer.loopPointReached += OnVideoFinished;

        // Start playback if not already
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Load main menu after video finishes
        SceneManager.LoadScene(mainMenuScene);
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
