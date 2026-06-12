using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;

    #region Tooltip
    [Tooltip("Populate with the music volume level")]
    #endregion Tooltip
    [SerializeField] private TextMeshProUGUI musicLevelText;
    #region Tooltip
    [Tooltip("Populate with the sounds volume level")]
    #endregion Tooltip
    [SerializeField] private TextMeshProUGUI soundsLevelText;
    
    [Header("Menu Configuration")]
    [Tooltip("Is this options menu in the main menu scene?")]
    [SerializeField] private bool isMainMenuOptions = false;

    private void Start()
    {
        // Only hide on start if it's in the main game scene
        if (!isMainMenuOptions)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // If in main menu, show audio panel by default
            ShowAudioPanel();
        }
    }

    /// <summary>
    /// Initialize the UI text
    /// </summary>
    private IEnumerator InitializeUI()
    {
        // Wait a frame to ensure the previous music and sound levels have been set
        yield return null;

        // Initialise UI text
        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }
    
    private void OnEnable()
    {
        // Only pause time if in main game scene
        if (!isMainMenuOptions)
        {
            Time.timeScale = 0f;
        }
        
        // Initialise UI text
        StartCoroutine(InitializeUI());

        // Show audio panel by default when options menu opens
        ShowAudioPanel();
    }

    private void OnDisable()
    {
        // Only unpause if in main game scene
        if (!isMainMenuOptions)
        {
            Time.timeScale = 1f;
        }
    }

    public void ShowAudioPanel()
    {
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControlsPanel()
    {
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        if (isMainMenuOptions)
        {
            // In main menu, just close the options scene
            gameObject.SetActive(false);
        }
        else
        {
            // In game, return to pause menu
            GameManager.Instance.CloseOptionsMenu();
        }
    }
    
    /// <summary>
    /// Increase music volume - linked to from music volume increase button in UI
    /// </summary>
    public void IncreaseMusicVolume()
    {
        MusicManager.Instance.IncreaseMusicVolume();
        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    /// <summary>
    /// Decrease music volume - linked to from music volume decrease button in UI
    /// </summary>
    public void DecreaseMusicVolume()
    {
        MusicManager.Instance.DecreaseMusicVolume();
        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    /// <summary>
    /// Increase sounds volume - linked to from sounds volume increase button in UI
    /// </summary>
    public void IncreaseSoundsVolume()
    {
        SoundEffectManager.Instance.IncreaseSoundsVolume();
        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
    }

    /// <summary>
    /// Decrease sounds volume - linked to from sounds volume decrease button in UI
    /// </summary>
    public void DecreaseSoundsVolume()
    {
        SoundEffectManager.Instance.DecreaseSoundsVolume();
        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
    }
}