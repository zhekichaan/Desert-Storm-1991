using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveMenuUI : MonoBehaviour
{
    [SerializeField] public Button saveButton;
    [SerializeField] public Button loadButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button resumeButton;  // From pause menu
    [SerializeField] private Button deleteButton;

    [SerializeField] private SaveSlotUI[] saveSlots;

    [SerializeField] private SaveLoadManager loadManager;

    [Header("Menu Configuration")]
    [Tooltip("Is this options menu in the main menu scene?")]
    [SerializeField] private bool isMainMenuOptions = false;

    private SaveSlotUI selectedSlot;


    void Start()
    {
        // Only hide on start if it's in the main game scene
        if (!isMainMenuOptions)
        {
            gameObject.SetActive(false);
        }

        for (int i = 0; i < saveSlots.Length; i++)
        {
            saveSlots[i].SetIndex(i);    
            saveSlots[i].Initialize(OnSlotClicked);
        }

        saveButton.onClick.AddListener(OnSaveClicked);
        loadButton.onClick.AddListener(OnLoadClicked);

        saveButton.interactable = false;
        loadButton.interactable = false;

        if (isMainMenuOptions)
        {
            deleteButton.onClick.AddListener(OnDeleteClicked);
            deleteButton.interactable = false;
        }
    }

    private void OnEnable()
    {
        // Only pause time if in main game scene
        if (!isMainMenuOptions)
        {
            Time.timeScale = 0f;
        }

        selectedSlot = null;         // Clear previous selection
        saveButton.interactable = false;
        loadButton.interactable = false;

        // Refresh all slots when menu opens
        for (int i = 0; i < saveSlots.Length; i++)
        {
            var data = loadManager.GetSlotData(i);
            saveSlots[i].Refresh(data);
            saveSlots[i].SetSelected(false);
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnSlotClicked(SaveSlotUI clickedSlot)
    {
        // Deselect previous
        if (selectedSlot != null)
            selectedSlot.SetSelected(false);

        // Select new
        selectedSlot = clickedSlot;
        selectedSlot.SetSelected(true);

        saveButton.interactable = true;

        // Enable load only if the slot has save data
        var data = loadManager.GetSlotData(selectedSlot.SlotIndex);
        loadButton.interactable = data != null;

        if (isMainMenuOptions)
        {
            deleteButton.interactable = data != null;
        }
    }

    private void OnSaveClicked()
    {
        if (selectedSlot == null)
            return;

        int slotIndex = selectedSlot.SlotIndex;

        loadManager.Save(slotIndex);

        if (backButton != null)
            backButton.onClick.Invoke();
    }

    private void OnLoadClicked()
    {
        if (selectedSlot == null)
            return;

        int slotIndex = selectedSlot.SlotIndex;

        LoadRequest.loadFileStatic = slotIndex;

        // Load Main game Scene option
        if (isMainMenuOptions)
        {

            SceneManager.LoadScene("MainGameScene");
            return;
        }

        loadManager.OnLoadButtonPressed(slotIndex);

        gameObject.SetActive(false);

        if (backButton != null)
            backButton.onClick.Invoke();

        if (resumeButton != null)
            resumeButton.onClick.Invoke();
    }

    private void OnDeleteClicked()
    {
        if (selectedSlot == null)
            return;

        int slotIndex = selectedSlot.SlotIndex;

        // Delete the file
        loadManager.DeleteSave(slotIndex);

        // Refresh the slot UI
        var data = loadManager.GetSlotData(slotIndex);
        selectedSlot.Refresh(data);

        // Disable buttons since slot is now empty
        loadButton.interactable = false;
        deleteButton.interactable = false;
    }

}
