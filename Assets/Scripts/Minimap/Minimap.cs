using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class Minimap : SingletonMonobehaviour<Minimap>
{
    #region Tooltip
    [Tooltip("Populate with the child MinimapPlayer gameobject")]
    #endregion Tooltip

    [SerializeField] private GameObject miniMapPlayer;

    #region Tooltip
    [Tooltip("Populate with the minimap enemy ping prefab (red dot with MinimapPing attached)")]
    #endregion Tooltip
    [SerializeField] private GameObject miniMapEnemyPrefab;


    private Transform playerTransform;
    // Draw enemy icons on the minimap
    private readonly Dictionary<Enemy, Transform> enemyIcons = new Dictionary<Enemy, Transform>();

    private void Start()
    {
        playerTransform = GameManager.Instance.GetPlayer().transform;

        // Populate player as cinemachine camera target
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        // Set minimap player icon
        SpriteRenderer spriteRenderer = miniMapPlayer.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = GameManager.Instance.GetPlayerMiniMapIcon();
        }
    }

    private void Update()
    {
        // Move the minimap player to follow the player
        if (playerTransform != null && miniMapPlayer != null)
        {
            miniMapPlayer.transform.position = playerTransform.position;
        }

        // Move each minimap enemy icon to follow its enemy
        foreach (KeyValuePair<Enemy, Transform> pair in enemyIcons)
        {
            if (pair.Key != null && pair.Value != null)
            {
                pair.Value.position = pair.Key.transform.position;
            }
        }
    }

    /// Create a red-dot minimap icon that tracks the given enemy. Called from Enemy.EnemyInitialization.
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null || miniMapEnemyPrefab == null || enemyIcons.ContainsKey(enemy)) return;

        GameObject icon = Instantiate(miniMapEnemyPrefab, enemy.transform.position, Quaternion.identity, transform);
        enemyIcons.Add(enemy, icon.transform);
    }

    /// Remove an enemy's minimap icon. Called from Enemy.OnDestroy.
    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null || !enemyIcons.TryGetValue(enemy, out Transform icon)) return;

        if (icon != null) Destroy(icon.gameObject);
        enemyIcons.Remove(enemy);
    }

    public void Reinitialize()
    {
        playerTransform = GameManager.Instance.GetPlayer().transform;

        // Populate player as cinemachine camera target
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        // Set minimap player icon
        SpriteRenderer spriteRenderer = miniMapPlayer.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = GameManager.Instance.GetPlayerMiniMapIcon();
        }
        // Safety clean (enemies normally unregister themselves via OnDestroy in Enemy.cs)
        foreach (Transform icon in enemyIcons.Values)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        enemyIcons.Clear();
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(miniMapPlayer), miniMapPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(miniMapEnemyPrefab), miniMapEnemyPrefab);
    }

#endif

    #endregion Validation
}
