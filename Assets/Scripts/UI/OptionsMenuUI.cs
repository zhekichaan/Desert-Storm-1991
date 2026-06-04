using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;

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

    private void OnEnable()
    {
        // Only pause time if in main game scene
        if (!isMainMenuOptions)
        {
            Time.timeScale = 0f;
        }

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
}