using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject weaponSlotPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private WeaponDetailPanel detailPanel; // assign in inspector

    private List<WeaponSlot> slots = new List<WeaponSlot>();
    private int selectedIndex = -1;

    public void OnEnable()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
        slots.Clear();

        List<Weapon> weapons = GameManager.Instance.GetPlayer().weaponList;

        int currentWeaponIndex = GameManager.Instance.GetPlayer().playerControl.GetCurrentWeaponIndex();

        for (int i = 0; i < weapons.Count; i++)
        {
            int index = i;
            var weapon = weapons[i];
            var slot = Instantiate(weaponSlotPrefab, content);
            var weaponSlot = slot.GetComponent<WeaponSlot>();
            weaponSlot.Setup(
                weapon.weaponDetails.weaponSprite,
                weapon.weaponDetails.weaponName,
                weapon.weaponRemainingAmmo,
                weapon.weaponDetails.hasInfiniteAmmo
            );
            weaponSlot.OnClicked += () => SelectSlot(index);
            slots.Add(weaponSlot);
        }

        // auto-select first slot
        if (slots.Count > 0)
            SelectSlot(currentWeaponIndex);
    }

    private void Update()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if (selectedIndex >= 0 && selectedIndex < slots.Count)
                slots[selectedIndex].Select(false);
            
            selectedIndex = GameManager.Instance.GetPlayer().playerControl.PreviousWeapon();
            
            slots[selectedIndex].Select(true);

            var weapon = GameManager.Instance.GetPlayer().weaponList[selectedIndex];
            detailPanel.Show(weapon);
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (selectedIndex >= 0 && selectedIndex < slots.Count)
                slots[selectedIndex].Select(false);
            
            selectedIndex = GameManager.Instance.GetPlayer().playerControl.NextWeapon();
            
            slots[selectedIndex].Select(true);

            var weapon = GameManager.Instance.GetPlayer().weaponList[selectedIndex];
            detailPanel.Show(weapon);
        }
    }

    private void SelectSlot(int index)
    {
        if (selectedIndex >= 0 && selectedIndex < slots.Count)
            slots[selectedIndex].Select(false);

        selectedIndex = index;
        slots[selectedIndex].Select(true);

        var weapon = GameManager.Instance.GetPlayer().weaponList[index];
        detailPanel.Show(weapon);
        
        GameManager.Instance.GetPlayer().playerControl.SetWeaponByIndex(index);
    }
}
