using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    [SerializeField] private GameObject selector;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Button button;
    
    public event Action OnClicked;

    private void Awake()
    {
        button.onClick.AddListener(() => OnClicked?.Invoke());
    }
    
    public void Setup(Sprite weaponIcon, string weaponName, int ammo, bool hasInfiniteAmmo)
    {
        nameText.text = weaponName;
        ammoText.text = hasInfiniteAmmo ? "INF" : ammo.ToString("D2");
        selector.SetActive(false);
    }

    public void Select(bool selected)
    {
        selector.SetActive(selected);
    }
}
