using System.Collections;
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
    private FireWeaponEvent fireWeaponEvent;
    private Coroutine reloadWeaponCoroutine;
    private Weapon currentWeapon;

    private void Awake()
    {
        // Load components
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
    }

    private void OnEnable()
    {
        // subscribe to reload weapon event
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;

        // Subscribe to set active weapon event
        setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;

        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        // unsubscribe from reload weapon event
        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;

        // Unsubscribe from set active weapon event
        setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;

        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
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
            reloadWeaponCoroutine = StartCoroutine(ShellByShellReloadRoutine());
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

        if (!weapon.weaponDetails.hasInfiniteAmmo)
        {
            weapon.weaponRemainingAmmo -= ammoToLoad;
        }
        weapon.weaponClipRemainingAmmo += ammoToLoad;
        
        weapon.weaponReloadTimer = 0f;
        weapon.isWeaponReloading = false;
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
    }

    /// <summary>
    /// Shell-by-shell reload: plays one slug insert sound per shell being loaded,
    /// evenly spaced across the total reload time.
    /// </summary>
    private IEnumerator ShellByShellReloadRoutine()
    {
        
        if (currentWeapon.weaponDetails.weaponSlugInsertSoundEffect != null)
            SoundEffectManager.Instance.PlaySoundEffect(currentWeapon.weaponDetails.weaponSlugInsertSoundEffect);
        
        float interval = currentWeapon.weaponDetails.weaponShellInsertTime;

        float elapsed = 0f;
        while (elapsed < interval)
        {
            elapsed += Time.deltaTime;
            currentWeapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        currentWeapon.weaponClipRemainingAmmo++;
        if (!currentWeapon.weaponDetails.hasInfiniteAmmo)
            currentWeapon.weaponRemainingAmmo--;
        
        bool clipFull = currentWeapon.weaponClipRemainingAmmo >= currentWeapon.weaponDetails.weaponClipAmmoCapacity;
        bool outOfReserveAmmo = !currentWeapon.weaponDetails.hasInfiniteAmmo && currentWeapon.weaponRemainingAmmo <= 0;

        currentWeapon.weaponReloadTimer = 0f;
        currentWeapon.isWeaponReloading = false;
        weaponReloadedEvent.CallWeaponReloadedEvent(currentWeapon);
        
        if (!clipFull && !outOfReserveAmmo)
        {
            // Load the next shell
            reloadWeaponEvent.CallReloadWeaponEvent(currentWeapon, 0);
        }
    }

    public void CancelReload()
    {
        if (currentWeapon == null || !currentWeapon.isWeaponReloading) return;

        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
            reloadWeaponCoroutine = null;
        }

        currentWeapon.isWeaponReloading = false;
        currentWeapon.weaponReloadTimer = 0f;

        weaponReloadedEvent.CallWeaponReloadedEvent(currentWeapon);
    }

    /// <summary>
    /// Set active weapon event handler
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (currentWeapon != null && currentWeapon != setActiveWeaponEventArgs.weapon)
        {
            CancelReload();
        }

        currentWeapon = setActiveWeaponEventArgs.weapon;
    }

    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        if (currentWeapon == null || !currentWeapon.isWeaponReloading) return;
        if (!currentWeapon.weaponDetails.isShellByShellReload) return;
        if (currentWeapon.weaponClipRemainingAmmo <= 0) return;

        CancelReload();
    }
}