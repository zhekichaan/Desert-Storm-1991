using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDetailPanel : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI weaponDamageText;
    [SerializeField] private TextMeshProUGUI weaponSpeedText;
    // add whatever other fields you want to display

    public void Show(Weapon weapon)
    {
        gameObject.SetActive(true);
        weaponImage.sprite = weapon.weaponDetails.weaponSprite;
        weaponNameText.text = weapon.weaponDetails.weaponName;
        weaponDamageText.text = weapon.weaponDetails.weaponCurrentAmmo.ammoDamage.ToString();
        weaponSpeedText.text = weapon.weaponDetails.weaponFireRate.ToString("N1");
        ammoText.text = weapon.weaponDetails.hasInfiniteAmmo
            ? "INF"
            : $"{weapon.weaponRemainingAmmo:D2} / {weapon.weaponDetails.weaponAmmoCapacity:D2}";
    }
}