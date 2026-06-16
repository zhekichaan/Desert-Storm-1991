using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject weaponSlotPrefab;
    [SerializeField] private Transform content;

    public void OnEnable()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        List<Weapon> weapons = GameManager.Instance.GetPlayer().weaponList;
        
        foreach (var weapon in weapons)
        {
            var slot = Instantiate(weaponSlotPrefab, content);
            slot.GetComponent<WeaponSlot>().Setup(weapon.weaponDetails.weaponSprite, weapon.weaponDetails.weaponName, weapon.weaponRemainingAmmo, weapon.weaponDetails.hasInfiniteAmmo);
        }
    }
}
