using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion Tooltip
    [SerializeField] private MovementDetailsSO movementDetails;

    private Player player;
    private int currentWeaponIndex = 0;
    private float moveSpeed;
    private bool firePreviousFrame = false;
    private bool isPlayerMovementDisabled = false;
    
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();

        inputActions = new InputSystem_Actions();
        
        string json = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(json))
            inputActions.asset.LoadBindingOverridesFromJson(json);
    
        inputActions.Enable();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.Shoot.performed += OnShootPerformed;
        inputActions.Player.Shoot.canceled += OnShootCanceled;

        inputActions.Player.Interact.performed += OnInteractPerformed;

        inputActions.Player.PreviousWeapon.performed += OnPreviousWeaponPerformed;
        inputActions.Player.NextWeapon.performed += OnNextWeaponPerformed;

        inputActions.Player.Reload.performed += OnReloadPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.Shoot.performed -= OnShootPerformed;
        inputActions.Player.Shoot.canceled -= OnShootCanceled;

        inputActions.Player.Interact.performed -= OnInteractPerformed;

        inputActions.Player.PreviousWeapon.performed -= OnPreviousWeaponPerformed;
        inputActions.Player.NextWeapon.performed -= OnNextWeaponPerformed;

        inputActions.Player.Reload.performed -= OnReloadPerformed;

        inputActions.Player.Disable();
    }

    private void Start()
    {
        SetPlayerAnimationSpeed();
    }

    public void ReloadBindings(string json)
    {
        if (!string.IsNullOrEmpty(json))
            inputActions.asset.LoadBindingOverridesFromJson(json);
    }
    
    /// <summary>
    /// Set the player starting weapon
    /// </summary>
    private void SetStartingWeapon()
    {
        int index = 0;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetWeaponByIndex(index, false);
                break;
            }
            index++;
        }
    }

    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        if (isPlayerMovementDisabled)
            return;

        MovementInput();
        AimWeaponAndFire();
    }

    #region Movement

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void MovementInput()
    {
        if (Time.timeScale == 0f) return;

        Vector2 direction = moveInput;

        // Adjust distance for diagonal movement (pythagoras approximation)
        if (direction.x != 0f && direction.y != 0f)
        {
            direction *= 0.7f;
        }

        if (direction != Vector2.zero)
        {
            if (player == null)
            {
                Debug.LogError("Player is NULL");
                return;
            }

            if (player.movementByVelocityEvent == null)
            {
                Debug.LogError("movementByVelocityEvent is NULL");
                return;
            }

            player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
        }
        else
        {
            player.idleEvent.CallIdleEvent();
        }
    }

    #endregion Movement

    #region Aim & Fire

    private bool isFiring = false;

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        isFiring = true;
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        isFiring = false;
    }

    private void AimWeaponAndFire()
    {
        if (Time.timeScale == 0f) return;

        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetShootPosition());
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if (isFiring)
        {
            player.fireWeaponEvent.CallFireWeaponEvent(true, firePreviousFrame, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
            firePreviousFrame = true;
        }
        else
        {
            firePreviousFrame = false;
        }
    }

    #endregion Aim & Fire

    #region Weapon Switching

    private void OnPreviousWeaponPerformed(InputAction.CallbackContext context)
    {
        PreviousWeapon();
    }

    private void OnNextWeaponPerformed(InputAction.CallbackContext context)
    {
        NextWeapon();
    }

    public void SetWeaponByIndex(int weaponIndex, bool playSound = true)
    {
        if (weaponIndex < player.weaponList.Count)
        {
            currentWeaponIndex = weaponIndex;
            player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[weaponIndex]);

            if (playSound)
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponSwitch);
        }
    }

    public int NextWeapon()
    {
        currentWeaponIndex++;

        if (currentWeaponIndex + 1 > player.weaponList.Count)
        {
            currentWeaponIndex = 0;
        }

        SetWeaponByIndex(currentWeaponIndex);

        return currentWeaponIndex;
    }

    public int PreviousWeapon()
    {
        currentWeaponIndex--;

        if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = player.weaponList.Count - 1;
        }

        SetWeaponByIndex(currentWeaponIndex);

        return currentWeaponIndex;
    }

    /// <summary>
    /// Set the current weapon to be first in the player weapon list
    /// </summary>
    private void SetCurrentWeaponToFirstInTheList()
    {
        List<Weapon> tempWeaponList = new List<Weapon>();

        Weapon currentWeapon = player.weaponList[currentWeaponIndex];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        int index = 2;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon == currentWeapon) continue;

            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        player.weaponList = tempWeaponList;
        currentWeaponIndex = 0;

        SetWeaponByIndex(currentWeaponIndex);
    }

    #endregion Weapon Switching

    #region Reload

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

        if (currentWeapon.isWeaponReloading) return;

        if (currentWeapon.weaponRemainingAmmo < currentWeapon.weaponDetails.weaponClipAmmoCapacity && !currentWeapon.weaponDetails.hasInfiniteAmmo) return;

        if (currentWeapon.weaponClipRemainingAmmo == currentWeapon.weaponDetails.weaponClipAmmoCapacity) return;

        player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 0);
    }

    #endregion Reload

    #region Interact

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        float useItemRadius = 2f;

        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(player.GetPlayerPosition(), useItemRadius);

        foreach (Collider2D collider2D in collider2DArray)
        {
            IUseable iUseable = collider2D.GetComponent<IUseable>();

            if (iUseable != null)
            {
                iUseable.UseItem();
            }
        }
    }

    #endregion Interact

    /// <summary>
    /// Enable the player movement
    /// </summary>
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.idleEvent.CallIdleEvent();
    }

    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }

#endif

    #endregion Validation
}