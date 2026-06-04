using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private Image borderImage; // assign border image
    [SerializeField] private GameObject playerImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI dateText;


    [SerializeField] private GameObject emptyState;
    [SerializeField] private GameObject savedState;

    [SerializeField] private SaveLoadManager saveManager;

    private Button button;

    private Color baseRed = new Color32(141, 34, 34, 255);

    public int SlotIndex { get; private set; }

    public void Initialize(System.Action<SaveSlotUI> onClicked)
    {
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(() => onClicked(this));
    }

    public void SetSelected(bool selected)
    {
        borderImage.color = selected ? Color.yellow : baseRed;
    }

    public void SetIndex(int index)
    {
        SlotIndex = index;
    }

    public void Refresh(DungeonData data)
    {
        if (data == null)
        {
            // Empty slot
            emptyState.SetActive(true);
            savedState.SetActive(false);

            levelText.text = "";
            dateText.text = "";
            return;
        }

        // Saved slot
        emptyState.SetActive(false);
        savedState.SetActive(true);
        levelText.text = $"Level {data.player.dungeonLevelIndex + 1}";
        dateText.text = data.player.saveTime;
        // Use SaveLoadManager to get the sprite
        if (saveManager != null && !string.IsNullOrEmpty(data.player.spriteName))
        {
            Sprite sprite = saveManager.GetPlayerSprite(data.player.spriteName);
            if (sprite != null)
                playerImage.GetComponent<Image>().sprite = sprite;
        }
    }
}