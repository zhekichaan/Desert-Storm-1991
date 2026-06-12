using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    private void Start()
    {
        // Initially hide the pause menu
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    // Quit and load main menu - linked to from pause menu UI button
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
