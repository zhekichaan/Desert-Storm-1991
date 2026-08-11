using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyWeaponAI : MonoBehaviour
{
    [Tooltip("Select the layers that the enemy bullets will hit")]
    [SerializeField] private LayerMask layerMask;
    [Tooltip("Populate this with the WeaponShootPosition child gameobject transform")]
    [SerializeField] private Transform weaponShootPosition;
    [Tooltip("How long the enemy must have the player in their sights before firing (seconds)")]
    [SerializeField] private float aimDelay = 0.5f;

    private Enemy enemy;
    private EnemyDetailsSO enemyDetails;
    private float firingIntervalTimer;
    private float firingDurationTimer;
    private float aimTimer = 0f;
    private bool isAiming = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        enemyDetails = enemy.enemyDetails;
        firingIntervalTimer = WeaponShootInterval();
        firingDurationTimer = WeaponShootDuration();
    }

    private void Update()
    {
        firingIntervalTimer -= Time.deltaTime;

        if (firingIntervalTimer < 0f)
        {
            if (firingDurationTimer >= 0)
            {
                firingDurationTimer -= Time.deltaTime;
                FireWeapon();
            }
            else
            {
                firingIntervalTimer = WeaponShootInterval();
                firingDurationTimer = WeaponShootDuration();
                aimTimer = 0f;
                isAiming = false;
            }
        }
    }

    private float WeaponShootDuration()
    {
        return Random.Range(enemyDetails.firingDurationMin, enemyDetails.firingDurationMax);
    }

    private float WeaponShootInterval()
    {
        return Random.Range(enemyDetails.firingIntervalMin, enemyDetails.firingIntervalMax);
    }

    private void FireWeapon()
    {
        Vector3 playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;
        Vector3 weaponDirection = GameManager.Instance.GetPlayer().GetPlayerPosition() - weaponShootPosition.position;
        float weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);
        float enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);
        AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

        enemy.aimWeaponEvent.CallAimWeaponEvent(enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection);

        if (enemyDetails.enemyWeapon == null || enemy.activeWeapon == null) return;

        if (enemy.activeWeapon.weaponClipRemainingAmmo <= 0 && !enemy.activeWeapon.isWeaponReloading && !enemy.activeWeapon.weaponDetails.hasInfiniteClipCapacity)
        {
            enemy.reloadWeaponEvent.CallReloadWeaponEvent(enemy.activeWeapon, 0);
            return;
        }

        float enemyAmmoRange = enemyDetails.enemyWeapon.weaponCurrentAmmo.ammoRange;
        bool playerInRange = playerDirectionVector.magnitude <= enemyAmmoRange;
        bool hasLineOfSight = !enemyDetails.firingLineOfSightRequired || IsPlayerInLineOfSight(weaponDirection, enemyAmmoRange);

        if (playerInRange && hasLineOfSight)
        {
            if (!isAiming)
            {
                isAiming = true;
                aimTimer = 0f;
            }

            aimTimer += Time.deltaTime;

            if (aimTimer >= aimDelay)
            {
                enemy.fireWeaponEvent.CallFireWeaponEvent(true, true, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection);
            }
        }
        else
        {
            isAiming = false;
            aimTimer = 0f;
        }
    }

    private bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyAmmoRange)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition.position, (Vector2)weaponDirection, enemyAmmoRange, layerMask);
        return raycastHit2D && raycastHit2D.transform.CompareTag(Settings.playerTag);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponShootPosition), weaponShootPosition);
    }
#endif
    #endregion Validation
}