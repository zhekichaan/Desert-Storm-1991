using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(EnemyWeaponAI))]
[RequireComponent(typeof(AimWeaponEvent))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(ReloadWeapon))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(EnemyMovementAI))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AnimateEnemy))]
[RequireComponent(typeof(MaterializeEffect))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
#endregion REQUIRE COMPONENTS

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    private HealthEvent healthEvent;
    private Health health;
    [HideInInspector] public AimWeaponEvent aimWeaponEvent;
    [HideInInspector] public ReloadWeaponEvent reloadWeaponEvent;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    private FireWeapon fireWeapon;
    private SetActiveWeaponEvent setActiveWeaponEvent;
    private EnemyMovementAI enemyMovementAI;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;
    private MaterializeEffect materializeEffect;
    private CircleCollider2D circleCollider2D;
    private PolygonCollider2D polygonCollider2D;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;
    [HideInInspector] public Animator animator;
    private Room currentRoom;
    [HideInInspector] public Weapon activeWeapon;
    
    #region Tooltip
    [Tooltip("Populate with the blood splatter prefab to instantiate when enemy is hit")]
    #endregion
    [SerializeField] private GameObject bloodSplatterPrefab;

    #region Tooltip
    [Tooltip("The chance (0-1) that a blood splatter effect is spawned when the enemy takes damage")]
    #endregion
    [SerializeField] [Range(0f, 1f)] private float bloodSplatterChance = 0.3f;
    
    #region Tooltip
    [Tooltip("How long the enemy keeps dripping smaller blood drops after being hit")]
    #endregion
    [SerializeField] private float bleedDuration = 2.5f;

    #region Tooltip
    [Tooltip("Minimum time between each blood drop while bleeding")]
    #endregion
    [SerializeField] private float bleedDropIntervalMin = 0.15f;

    #region Tooltip
    [Tooltip("Maximum time between each blood drop while bleeding")]
    #endregion
    [SerializeField] private float bleedDropIntervalMax = 0.5f;

    #region Tooltip
    [Tooltip("Scale multiplier applied to blood drops while bleeding (smaller than the main splatter)")]
    #endregion
    [SerializeField] private float bleedDropScale = 0.5f;
    
    #region Tooltip
    [Tooltip("How long a blood splatter/drop stays fully visible before it starts fading")]
    #endregion
    [SerializeField] private float bloodVisibleDuration = 3f;

    #region Tooltip
    [Tooltip("How long the fade-out takes once it starts")]
    #endregion
    [SerializeField] private float bloodFadeDuration = 1.5f;

    private Coroutine bleedCoroutine;
    
    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();
        materializeEffect = GetComponent<MaterializeEffect>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        //subscribe to health event
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
    }

    private void OnDisable()
    {
        //subscribe to health event
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
        
        if (bleedCoroutine != null)
        {
            StopCoroutine(bleedCoroutine);
            bleedCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        Minimap.Instance?.UnregisterEnemy(this);
    }

    /// <summary>
    /// Handle health lost event
    /// </summary>
    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        // Only spawn blood if actual damage was dealt (not on init, heal, etc.)
        if (healthEventArgs.damageAmount > 0)
        {
            SpawnBloodSplatter();

            if (Random.value < bloodSplatterChance)
            {
                StartBleeding();
            };
        }
        
        if (healthEventArgs.healthAmount <= 0)
        {
            EnemyDestroyed();
        }
    }
    
    /// <summary>
    /// Starts (or restarts/extends) the bleeding trail effect after being hit
    /// </summary>
    private void StartBleeding()
    {
        // If already bleeding, stop the old coroutine and start fresh so the timer extends
        if (bleedCoroutine != null)
        {
            StopCoroutine(bleedCoroutine);
        }

        bleedCoroutine = StartCoroutine(BleedRoutine());
    }

    private IEnumerator BleedRoutine()
    {
        float elapsed = 0f;

        while (elapsed < bleedDuration)
        {
            SpawnBloodDrop();

            float interval = Random.Range(bleedDropIntervalMin, bleedDropIntervalMax);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        bleedCoroutine = null;
    }

    /// <summary>
    /// Spawns a small blood drop at the enemy's current position (used for the bleed trail)
    /// </summary>
    private void SpawnBloodDrop()
    {
        if (bloodSplatterPrefab == null) return;

        GameObject bloodDrop = Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);
        bloodDrop.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
        bloodDrop.transform.localScale *= bleedDropScale;

        if (currentRoom != null && currentRoom.instantiatedRoom != null)
        {
            bloodDrop.transform.SetParent(currentRoom.instantiatedRoom.transform);
        }
        
        SortingGroup dropSortingGroup = bloodDrop.GetComponentInChildren<SortingGroup>();
        if (dropSortingGroup != null)
        {
            SortingGroup enemySortingGroup = GetComponent<SortingGroup>();
            dropSortingGroup.sortingLayerName = enemySortingGroup.sortingLayerName;
            dropSortingGroup.sortingOrder = enemySortingGroup.sortingOrder - 1;
        }
    }

    private void SpawnBloodSplatter()
    {
        if (bloodSplatterPrefab == null) return;
        
        if (Random.value > bloodSplatterChance) return;
        
        GameObject bloodSplatter = Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);
        bloodSplatter.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
        
        if (currentRoom != null && currentRoom.instantiatedRoom != null)
        {
            bloodSplatter.transform.SetParent(currentRoom.instantiatedRoom.transform);
        }

        SortingGroup splatterSortingGroup = bloodSplatter.GetComponentInChildren<SortingGroup>();
        if (splatterSortingGroup != null)
        {
            SortingGroup enemySortingGroup = GetComponent<SortingGroup>();
            splatterSortingGroup.sortingLayerName = enemySortingGroup.sortingLayerName;
            splatterSortingGroup.sortingOrder = enemySortingGroup.sortingOrder - 1;
        }
    }

    /// <summary>
    /// Enemy destroyed
    /// </summary>
    private void EnemyDestroyed()
    {
        DestroyedEvent destroyedEvent = GetComponent<DestroyedEvent>();
        destroyedEvent.CallDestroyedEvent(false, health.GetStartingHealth());
    }

    /// <summary>
    /// Initialise the enemy
    /// </summary>
    public void EnemyInitialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel, Room room)
    {
        this.enemyDetails = enemyDetails;
        this.currentRoom = room;

        SetEnemyMovementUpdateFrame(enemySpawnNumber);
        SetEnemyStartingHealth(dungeonLevel);
        SetEnemyStartingWeapon();
        SetEnemyAnimationSpeed();

        Minimap.Instance?.RegisterEnemy(this);

        // Materialise enemy
        StartCoroutine(MaterializeEnemy());
    }

    /// <summary>
    /// Set enemy movement update frame
    /// </summary>
    private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    {
        // Set frame number that enemy should process it's updates
        enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathfindingOver);
    }

    /// <summary>
    /// Set the starting health for the enemy
    /// </summary>
    private void SetEnemyStartingHealth(DungeonLevelSO dungeonLevel)
    {
        // Get the enemy health for the dungeon level
        foreach (EnemyHealthDetails enemyHealthDetails in enemyDetails.enemyHealthDetailsArray)
        {
            if (enemyHealthDetails.dungeonLevel == dungeonLevel)
            {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }
        health.SetStartingHealth(Settings.defaultEnemyHealth);
    }


    /// <summary>
    /// Set enemy starting weapon as per the weapon details SO
    /// </summary>
    private void SetEnemyStartingWeapon()
    {
        // Process if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            Weapon weapon = new Weapon() { weaponDetails = enemyDetails.enemyWeapon, weaponReloadTimer = 0f, weaponClipRemainingAmmo = enemyDetails.enemyWeapon.weaponClipAmmoCapacity, weaponRemainingAmmo = enemyDetails.enemyWeapon.weaponAmmoCapacity, isWeaponReloading = false };

            activeWeapon = weapon;

            // Set weapon for enemy
            setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);

        }
    }

    /// <summary>
    /// Set enemy animator speed to match movement speed
    /// </summary>
    private void SetEnemyAnimationSpeed()
    {
        // Set animator speed to match movement speed
        animator.speed = enemyMovementAI.moveSpeed / Settings.baseSpeedForEnemyAnimations;
    }

    private IEnumerator MaterializeEnemy()
    {
        // Disable collider, Movement AI and Weapon AI
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(enemyDetails.enemyMaterializeShader, enemyDetails.enemyMaterializeColor, enemyDetails.enemyMaterializeTime, spriteRendererArray, enemyDetails.enemyStandardMaterial));

        // Enable collider, Movement AI and Weapon AI
        EnemyEnable(true);

    }

    private void EnemyEnable(bool isEnabled)
    {
        // Enable/Disable colliders
        circleCollider2D.enabled = isEnabled;
        polygonCollider2D.enabled = isEnabled;

        // Enable/Disable movement AI
        enemyMovementAI.enabled = isEnabled;

        // Enable / Disable Fire Weapon
        fireWeapon.enabled = isEnabled;

    }
}