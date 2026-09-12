using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AimWeaponEvent))]
[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Transform from the child WeaponRotationPoint gameobject")]
    #endregion
    [SerializeField] private Transform weaponRotationPointTransform;
    [SerializeField] private Transform weaponShootPositionTransform;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float wallBuffer = 0.1f;

    private AimWeaponEvent aimWeaponEvent;
    private ActiveWeapon activeWeapon;

    private void Awake()
    {
        // Load components
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        activeWeapon = GetComponent<ActiveWeapon>();
    }

    private void OnEnable()
    {
        // Subscribe to aim weapon event
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        // Unsubscribe from aim weapon event
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

    /// <summary>
    /// Aim weapon event handler
    /// </summary>
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    private void Aim(AimDirection aimDirection, float aimAngle)
    {
        // Set angle of the weapon transform
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        // Flip weapon transform based on player direction
        switch (aimDirection)
        {
            case AimDirection.Left:
            case AimDirection.UpLeft:
                weaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                break;

            case AimDirection.Up:
            case AimDirection.UpRight:
            case AimDirection.Right:
            case AimDirection.Down:
                weaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;
        }

        ClampShootPositionToObstacle(aimAngle);
    }

    private void ClampShootPositionToObstacle(float aimAngle)
    {
        Weapon currentWeapon = activeWeapon.GetCurrentWeapon();
        if (currentWeapon == null) return;

        Vector3 restingLocalPosition = currentWeapon.weaponDetails.weaponShootPosition;

        Vector3 aimDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(aimAngle);

        Vector3 restingWorldPosition = weaponShootPositionTransform.parent.TransformPoint(restingLocalPosition);
        float weaponReach = Vector3.Distance(weaponRotationPointTransform.position, restingWorldPosition);

        if (weaponReach <= 0f)
        {
            weaponShootPositionTransform.localPosition = restingLocalPosition;
            return;
        }

        Vector3 pivotWorldOrigin = weaponRotationPointTransform.position;

        RaycastHit2D hit = Physics2D.Raycast(pivotWorldOrigin, aimDirectionVector, weaponReach, obstacleLayerMask);

        if (hit.collider != null)
        {
            float allowedDistance = Mathf.Max(hit.distance - wallBuffer, 0f);
            float pullback = weaponReach - allowedDistance;

            Vector3 localPullbackDirection = restingLocalPosition.normalized;
            weaponShootPositionTransform.localPosition = restingLocalPosition - localPullbackDirection * pullback;
        }
        else
        {
            weaponShootPositionTransform.localPosition = restingLocalPosition;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }
#endif
    #endregion
}
