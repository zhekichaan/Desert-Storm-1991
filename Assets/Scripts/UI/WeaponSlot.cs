using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    
    public void Setup(Sprite weaponIcon, string weaponName, int ammo, bool hasInfiniteAmmo)
    {
        icon.sprite = weaponIcon;
        nameText.text = weaponName;
        
        if (hasInfiniteAmmo)
        {
            ammoText.text = "INF";
        }
        else
        {
            ammoText.text = ammo.ToString("D2");
        }
    }
}
