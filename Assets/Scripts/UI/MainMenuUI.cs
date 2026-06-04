using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with the enter the dungeon play button gameobject")]
    #endregion Tooltip
    [SerializeField] private GameObject playButton;
    #region Tooltip
    [Tooltip("Populate with the quit button gameobject")]
    #endregion
    [SerializeField] private GameObject quitButton;
    #region Tooltip
    [Tooltip("Populate with the high scores button gameobject")]
    #endregion
    [SerializeField] private GameObject highScoresButton;
    #region Tooltip
    [Tooltip("Populate with the instructions button gameobject")]
    #endregion
    [SerializeField] private GameObject instructionsButton;
    #region Tooltip
    [Tooltip("Populate with the return to main menu button gameobject")]
    #endregion
    [SerializeField] private GameObject returnToMainMenuButton;
    #region Tooltip
    [Tooltip("Populate with the new game button gameobject")]
    #endregion
    [SerializeField] private GameObject newGameButton;
    #region Tooltip
    [Tooltip("Populate with the load game button gameobject")]
    #endregion
    [SerializeField] private GameObject loadGameButton;
    #region Tooltip
    [Tooltip("Populate with the options button gameobject")]
    #endregion
    [SerializeField] private GameObject optionsButton;
    #region Tooltip
    [Tooltip("Populate with the game title text gameobject")]
    #endregion
    [SerializeField] private GameObject gameTitleText;

    private bool isCharacterSelectionSceneLoaded = false;
    private bool isInstructionSceneLoaded = false;
    private bool isHighScoresSceneLoaded = false;
    private bool isLoadGameSceneLoaded = false;
    private bool isOptionsSceneLoaded = false;

    private void Start()
    {
        // Play Music
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        playButton.SetActive(false);
        returnToMainMenuButton.SetActive(false);
        gameTitleText.SetActive(true); // Show title on start
    }

    /// <summary>
    /// Called from the High Scores Button
    /// </summary>
    public void LoadHighScores()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);
        optionsButton.SetActive(false);
        gameTitleText.SetActive(false);
        isHighScoresSceneLoaded = true;

        if (isCharacterSelectionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("CharacterSelectorScene");
        }

        returnToMainMenuButton.SetActive(true);

        // Load High Score scene additively
        SceneManager.LoadScene("HighScoreScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Play Game / Enter The Dungeon Button
    /// </summary>
    public void PlayGame()
    {
        LoadRequest.loadFileStatic = -1;
        SceneManager.LoadScene("MainGameScene");
    }

    /// <summary>
    /// Called from the Load Game Button
    /// </summary>
    public void LoadGame()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);
        optionsButton.SetActive(false);
        gameTitleText.SetActive(false);
        isLoadGameSceneLoaded = true;

        if (isCharacterSelectionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("CharacterSelectorScene");
        }

        returnToMainMenuButton.SetActive(true);

        LoadRequest.loadFileStatic = -1;
        // Load Load Game scene additively
        SceneManager.LoadScene("LoadGameScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Options Button
    /// </summary>
    public void LoadOptions()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);
        optionsButton.SetActive(false);
        gameTitleText.SetActive(false);
        isOptionsSceneLoaded = true;

        if (isCharacterSelectionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("CharacterSelectorScene");
        }

        returnToMainMenuButton.SetActive(true);

        // Load Options scene additively
        SceneManager.LoadScene("OptionsScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Return To Main Menu Button
    /// </summary>
    public void LoadCharacterSelector()
    {
        returnToMainMenuButton.SetActive(true);

        if (isHighScoresSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("HighScoreScene");
            isHighScoresSceneLoaded = false;
        }
        else if (isInstructionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("InstructionsScene");
            isInstructionSceneLoaded = false;
        }
        else if (isLoadGameSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("LoadGameScene");
            isLoadGameSceneLoaded = false;
        }
        else if (isOptionsSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("OptionsScene");
            isOptionsSceneLoaded = false;
        }

        playButton.SetActive(true);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);
        optionsButton.SetActive(false);
        gameTitleText.SetActive(false);
        isCharacterSelectionSceneLoaded = true;

        // Load character selector scene additively
        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Instructions Button
    /// </summary>
    public void LoadInstructions()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        newGameButton.SetActive(false);
        loadGameButton.SetActive(false);
        optionsButton.SetActive(false);
        gameTitleText.SetActive(false); // Hide title
        isInstructionSceneLoaded = true;

        if (isCharacterSelectionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("CharacterSelectorScene");
        }

        returnToMainMenuButton.SetActive(true);

        // Load instructions scene additively
        SceneManager.LoadScene("InstructionsScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called to return to the main menu and close all additively loaded scenes
    /// </summary>
    public void ReturnToMainMenu()
    {
        returnToMainMenuButton.SetActive(false);

        // Unload all possible additively loaded scenes
        if (isHighScoresSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("HighScoreScene");
            isHighScoresSceneLoaded = false;
        }

        if (isInstructionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("InstructionsScene");
            isInstructionSceneLoaded = false;
        }

        if (isLoadGameSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("LoadGameScene");
            isLoadGameSceneLoaded = false;
        }

        if (isOptionsSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("OptionsScene");
            isOptionsSceneLoaded = false;
        }

        if (isCharacterSelectionSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("CharacterSelectorScene");
            isCharacterSelectionSceneLoaded = false;
        }

        // Show main menu buttons
        playButton.SetActive(false);
        quitButton.SetActive(true);
        highScoresButton.SetActive(true);
        instructionsButton.SetActive(true);
        newGameButton.SetActive(true);
        loadGameButton.SetActive(true);
        optionsButton.SetActive(true);
        gameTitleText.SetActive(true); // Show title
    }

    /// <summary>
    /// Quit the game - this method is called from the onClick event set in the inspector
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    #region Validation
#if UNITY_EDITOR
    // Validate the scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playButton), playButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(quitButton), quitButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(highScoresButton), highScoresButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(instructionsButton), instructionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(returnToMainMenuButton), returnToMainMenuButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(newGameButton), newGameButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(loadGameButton), loadGameButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(optionsButton), optionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(gameTitleText), gameTitleText);
    }
#endif
    #endregion
}