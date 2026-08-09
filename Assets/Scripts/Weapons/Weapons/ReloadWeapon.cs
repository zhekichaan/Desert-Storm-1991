using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(SetActiveWeaponEvent))]

[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponReloadedEvent weaponReloadedEvent;
    private SetActiveWeaponEvent setActiveWeaponEvent;
    private Coroutine reloadWeaponCoroutine;

    private void Awake()
    {
        // Load components
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        // subscribe to reload weapon event
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;

        // Subscribe to set active weapon event
        setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;
    }

    private void OnDisable()
    {
        // unsubscribe from reload weapon event
        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;

        // Unsubscribe from set active weapon event
        setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;
    }

    /// <summary>
    /// Handle reload weapon event
    /// </summary>
    private void ReloadWeaponEvent_OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        StartReloadWeapon(reloadWeaponEventArgs);
    }

    /// <summary>
    /// Start reloading the weapon
    /// </summary>
    private void StartReloadWeapon(ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        Weapon weapon = reloadWeaponEventArgs.weapon;

        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
        

        if (weapon.weaponDetails.isShellByShellReload)
        {
            weapon.isWeaponReloading = true; 
            reloadWeaponCoroutine = StartCoroutine(ShellByShellReloadRoutine(weapon));
        }
        else
        {
            int ammoToLoad = weapon.weaponDetails.weaponClipAmmoCapacity - weapon.weaponClipRemainingAmmo;

            if (ammoToLoad > weapon.weaponRemainingAmmo)
                ammoToLoad = weapon.weaponRemainingAmmo;

            weapon.isWeaponReloading = true;

            reloadWeaponCoroutine = StartCoroutine(StandardReloadRoutine(weapon, ammoToLoad));
        }
    }

    /// <summary>
    /// Standard mag-swap reload � plays a single reload sound whose length drives the reload time.
    /// </summary>
    private IEnumerator StandardReloadRoutine(Weapon weapon, int ammoToLoad)
    {
        if (weapon.weaponDetails.weaponReloadSoundEffect != null)
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadSoundEffect);

        float reloadTime = weapon.weaponDetails.weaponReloadTime;

        while (weapon.weaponReloadTimer < reloadTime)
        {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        weapon.weaponRemainingAmmo -= ammoToLoad;
        weapon.weaponClipRemainingAmmo += ammoToLoad;
        
        weapon.weaponReloadTimer = 0f;
        weapon.isWeaponReloading = false;
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
    }

    /// <summary>
    /// Shell-by-shell reload: plays one slug insert sound per shell being loaded,
    /// evenly spaced across the total reload time.
    /// </summary>
    private IEnumerator ShellByShellReloadRoutine(Weapon weapon)
    {
        
        if (weapon.weaponDetails.weaponSlugInsertSoundEffect != null)
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponSlugInsertSoundEffect);
        
        float interval = weapon.weaponDetails.weaponShellInsertTime;

        float elapsed = 0f;
        while (elapsed < interval)
        {
            elapsed += Time.deltaTime;
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        weapon.weaponClipRemainingAmmo++;
        if (!weapon.weaponDetails.hasInfiniteAmmo)
            weapon.weaponRemainingAmmo--;
        
        bool clipFull = weapon.weaponClipRemainingAmmo >= weapon.weaponDetails.weaponClipAmmoCapacity;
        bool outOfReserveAmmo = !weapon.weaponDetails.hasInfiniteAmmo && weapon.weaponRemainingAmmo <= 0;

        weapon.weaponReloadTimer = 0f;
        weapon.isWeaponReloading = false;
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
        
        if (!clipFull && !outOfReserveAmmo)
        {
            // Load the next shell
            reloadWeaponEvent.CallReloadWeaponEvent(weapon, 0);
        }
    }

    /// <summary>
    /// Set active weapon event handler
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (setActiveWeaponEventArgs.weapon.isWeaponReloading)
        {
            if (reloadWeaponCoroutine != null)
            {
                StopCoroutine(reloadWeaponCoroutine);
            }
        }
    }
}