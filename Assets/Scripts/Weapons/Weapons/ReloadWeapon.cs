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
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }

        reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(reloadWeaponEventArgs.weapon, reloadWeaponEventArgs.topUpAmmoPercent));
    }

    /// <summary>
    /// Reload weapon coroutine
    /// </summary>
    private IEnumerator ReloadWeaponRoutine(Weapon weapon, int topUpAmmoPercent)
    {
        weapon.isWeaponReloading = true;

        if (weapon.weaponDetails.isShellByShellReload)
        {
            yield return StartCoroutine(ShellByShellReloadRoutine(weapon));
        }
        else
        {
            yield return StartCoroutine(StandardReloadRoutine(weapon));
        }

        ApplyReloadAmmo(weapon, topUpAmmoPercent);

        weapon.weaponReloadTimer = 0f;
        weapon.isWeaponReloading = false;
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
    }

    /// <summary>
    /// Standard mag-swap reload — plays a single reload sound whose length drives the reload time.
    /// </summary>
    private IEnumerator StandardReloadRoutine(Weapon weapon)
    {
        if (weapon.weaponDetails.weaponReloadSoundEffect != null)
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadSoundEffect);

        float reloadTime = weapon.weaponDetails.weaponReloadTime;

        while (weapon.weaponReloadTimer < reloadTime)
        {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Shell-by-shell reload: plays one slug insert sound per shell being loaded,
    /// evenly spaced across the total reload time.
    /// </summary>
    private IEnumerator ShellByShellReloadRoutine(Weapon weapon)
    {
        int shellsToLoad = weapon.weaponDetails.weaponClipAmmoCapacity - weapon.weaponClipRemainingAmmo;

        // Nothing to load, still wait out the animation time
        if (shellsToLoad <= 0)
        {
            yield return new WaitForSeconds(weapon.weaponDetails.weaponReloadTime);
            yield break;
        }

        float interval = weapon.weaponDetails.weaponShellInsertTime;

        for (int i = 0; i < shellsToLoad; i++)
        {
            if (weapon.weaponDetails.weaponSlugInsertSoundEffect != null)
                SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponSlugInsertSoundEffect);

            float elapsed = 0f;
            while (elapsed < interval)
            {
                elapsed += Time.deltaTime;
                weapon.weaponReloadTimer += Time.deltaTime;
                yield return null;
            }
        }

    }

    /// <summary>
    /// Applies topUpAmmo and refills the clip after any reload style.
    /// </summary>
    private void ApplyReloadAmmo(Weapon weapon, int topUpAmmoPercent)
    {
        if (topUpAmmoPercent != 0)
        {
            int ammoIncrease = Mathf.RoundToInt(
                (weapon.weaponDetails.weaponAmmoCapacity * topUpAmmoPercent) / 100f);

            weapon.weaponRemainingAmmo = Mathf.Min(
                weapon.weaponRemainingAmmo + ammoIncrease,
                weapon.weaponDetails.weaponAmmoCapacity);
        }

        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        else if (weapon.weaponRemainingAmmo >= weapon.weaponDetails.weaponClipAmmoCapacity)
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        else
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponRemainingAmmo;
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

            reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(setActiveWeaponEventArgs.weapon, 0));
        }
    }
}