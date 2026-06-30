using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Button[] tabButtons;

    public Color activeTabColor;
    public Color inactiveTabColor;
    public Color activeTextColor;
    public Color inactiveTextColor;

    public GameObject[] pages;

    void Start() => ShowTab(0);

    public void ShowTab(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            bool isActive = i == index;
            pages[i].SetActive(isActive);

            var img = tabButtons[i].GetComponentInChildren<Image>();
            var txt = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (img) img.color = isActive ? activeTabColor : inactiveTabColor;
            if (txt) txt.color = isActive ? activeTextColor : inactiveTextColor;
        }
    }
}