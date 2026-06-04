using UnityEngine;

public enum WeaponFireMode { FullAuto, SemiAuto }

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    #region Header WEAPON BASE DETAILS
    [Space(10)]
    [Header("WEAPON BASE DETAILS")]
    #endregion

    [Tooltip("Weapon name")]
    public string weaponName;

    [Tooltip("The sprite for the weapon - the sprite should have the 'generate physics shape' option selected")]
    public Sprite weaponSprite;


    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("WEAPON CONFIGURATION")]
    #endregion

    [Tooltip("Offset position for the end of the weapon from the sprite pivot point")]
    public Vector3 weaponShootPosition;

    [Tooltip("Weapon current ammo")]
    public AmmoDetailsSO weaponCurrentAmmo;

    [Tooltip("Weapon shoot effect SO - contains particle effect parameters")]
    public WeaponShootEffectSO weaponShootEffect;


    #region Header WEAPON SOUND EFFECTS
    [Space(10)]
    [Header("WEAPON SOUND EFFECTS")]
    #endregion

    [Tooltip("Sound effect played when the weapon fires")]
    public SoundEffectSO weaponFiringSoundEffect;

    [Tooltip("Sound effect played during a standard mag-swap reload")]
    public SoundEffectSO weaponReloadSoundEffect;

    [Tooltip("Sound effect played per shell inserted during a shell-by-shell reload")]
    public SoundEffectSO weaponSlugInsertSoundEffect;

    [Tooltip("Sound effect played after firing (e.g. pump action)")]
    public SoundEffectSO weaponPumpSoundEffect;

    [Tooltip("Delay in seconds before the pump sound plays after firing")]
    public float weaponPumpSoundDelay = 0.15f;

    #region Header WEAPON RELOAD CONFIGURATION
    [Space(10)]
    [Header("WEAPON RELOAD CONFIGURATION")]
    #endregion

    [Tooltip("If true, plays one slug insert sound per shell instead of a single reload sound")]
    public bool isShellByShellReload = false;

    [Tooltip("Time in seconds to insert a single shell (shell-by-shell reload only)")]
    public float weaponShellInsertTime = 0.5f;


    [Tooltip("Reload time in seconds. For standard reloads, set this to match the length of your reload sound clip")]
    public float weaponReloadTime = 0f;


    #region Header WEAPON FIRE MODE
    [Space(10)]
    [Header("WEAPON FIRE MODE")]
    #endregion

    [Tooltip("Full Auto: fires continuously while held. Semi Auto: one shot per trigger press.")]
    public WeaponFireMode fireMode = WeaponFireMode.FullAuto;

    #region Header WEAPON OPERATING VALUES
    [Space(10)]
    [Header("WEAPON OPERATING VALUES")]
    #endregion

    [Tooltip("If true, the weapon has infinite ammo")]
    public bool hasInfiniteAmmo = false;

    [Tooltip("If true, the weapon has infinite clip capacity")]
    public bool hasInfiniteClipCapacity = false;

    [Tooltip("Shots before a reload is required")]
    public int weaponClipAmmoCapacity = 6;

    [Tooltip("Maximum number of rounds that can be held for this weapon")]
    public int weaponAmmoCapacity = 100;

    [Tooltip("Fire rate - 0.2 means 5 shots per second")]
    public float weaponFireRate = 0.2f;

    [Tooltip("Time in seconds the fire button must be held before the weapon fires")]
    public float weaponPrechargeTime = 0f;


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponCurrentAmmo), weaponCurrentAmmo);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponFireRate), weaponFireRate, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponReloadTime), weaponReloadTime, true);

        if (!hasInfiniteAmmo)
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponAmmoCapacity), weaponAmmoCapacity, false);

        if (!hasInfiniteClipCapacity)
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponClipAmmoCapacity), weaponClipAmmoCapacity, false);
    }
#endif
    #endregion
}