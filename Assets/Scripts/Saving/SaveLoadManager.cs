using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public string PlayerPrefabName;
    //public int health;
    public List<WeaponData> weapons = new List<WeaponData>();
    // SAve Slot UI Values
    public string saveTime;
    public int dungeonLevelIndex;
    public string spriteName;
}

[System.Serializable]
public class DoorData
{
    public Vector3 position;        // World or local position
    public bool isOpen;             // Current door state
    public string doorPrefabName;   // Prefab to restore the door visually
}

[System.Serializable]
public class WeaponData
{
    public string weaponDetailsName;       // WeaponDetailsSO asset name
    public int weaponListPosition;
    public int weaponClipRemainingAmmo;
    public int weaponRemainingAmmo;
    public bool isWeaponReloading;
    public bool isActiveWeapon;            // was this the equipped weapon?
}

/// <summary>
/// Serializable version of a Doorway for saving/loading.
/// </summary>
[System.Serializable]
public class DoorwayData
{
    public Vector2Int position;
    public Orientation orientation;
    public Vector2Int doorwayStartCopyPosition;
    public int doorwayCopyTileWidth;
    public int doorwayCopyTileHeight;
    public bool isConnected;
    public bool isUnavailable;
    public string doorPrefabName; // Use prefab name to restore later
}

/// <summary>
/// Serializable class for saving Room data
/// </summary>
[System.Serializable]
public class RoomData
{
    public string prefabName;
    public Vector3 position;
    public string id;
    public string templateID;
    public string roomNodeTypeID;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<DoorwayData> doorWayList; // Serialized doorways
    public List<DoorData> doors;   // Actual Doors
    public bool isPositioned;
    public bool isLit;
    public bool isClearedOfEnemies;
    public bool isPreviouslyVisited;
}

/// <summary>
/// Serializable class for the whole dungeon
/// </summary>
[System.Serializable]
public class DungeonData
{
    public List<RoomData> rooms = new List<RoomData>();
    public PlayerData player;
}

/// <summary>
/// Room Save/Load Manager
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    [Header("Assign all room prefabs here")]
    public List<GameObject> roomPrefabs;

    [Header("Assign Room Templates Here")]
    public List<RoomTemplateSO> roomTemplates;

    private Dictionary<string, RoomTemplateSO> roomTemplateLookup;

    [Header("Assign door prefabs here")]
    public List<GameObject> doorPrefabs;

    [Header("Assign Player prefabs here")]
    public List<GameObject> playerPrefabs;

    [Header("Assign Player Sprites Here")]
    public List<Sprite> playerSprites;

    [Header("Assign Player Details Here")]
    public List<PlayerDetailsSO> playerDetails;
    
    private Dictionary<string, Sprite> spriteLookup;
    
    [Header("Assign Weapon Details SOs here")]
    public List<WeaponDetailsSO> weaponDetailsList;
    
    private Dictionary<string, WeaponDetailsSO> weaponDetailsLookup;

    [Header("Assign GameManager transform here")]
    public Transform gameManagerTransform;

    [Header("Assign minimap here")]
    [SerializeField] private GameObject minimap;


    private Dictionary<string, GameObject> prefabLookup;
    private Dictionary<string, GameObject> doorPrefabLookup;
    private Dictionary<string, GameObject> playerPrefabLookup;

    private string savePath;
    private DungeonBuilder dungeonBuilder;

    private void Awake()
    {
        spriteLookup = new Dictionary<string, Sprite>();
        foreach (Sprite sprite in playerSprites)
        {
            if (!spriteLookup.ContainsKey(sprite.name))
                spriteLookup.Add(sprite.name, sprite);
        }

        dungeonBuilder = FindObjectOfType<DungeonBuilder>();
        savePath = Application.persistentDataPath + "/rooms.json";

        // Build prefab lookup dictionary
        prefabLookup = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in roomPrefabs)
        {
            if (!prefabLookup.ContainsKey(prefab.name))
                prefabLookup.Add(prefab.name, prefab);
        }

        // Build door prefab lookup dictionary
        doorPrefabLookup = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in doorPrefabs)
        {
            if (!doorPrefabLookup.ContainsKey(prefab.name))
                doorPrefabLookup.Add(prefab.name, prefab);
        }

        // Build player prefab lookup
        playerPrefabLookup = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in playerPrefabs)
        {
            if (!playerPrefabLookup.ContainsKey(prefab.name))
                playerPrefabLookup.Add(prefab.name, prefab);
        }
        
        // Build weapon prefab lookup
        weaponDetailsLookup = new Dictionary<string, WeaponDetailsSO>();
        foreach (WeaponDetailsSO wd in weaponDetailsList)
        {
            if (!weaponDetailsLookup.ContainsKey(wd.name))
                weaponDetailsLookup.Add(wd.name, wd);
        }

        roomTemplateLookup = new Dictionary<string, RoomTemplateSO>();
        foreach (RoomTemplateSO template in roomTemplates)
        {
            if (!roomTemplateLookup.ContainsKey(template.name))
                roomTemplateLookup.Add(template.name, template);
        }

    }

    private void Start()
    {
        // Load from main menu 
        if (LoadRequest.loadFileStatic != -1)
        {
            OnLoadButtonPressed(LoadRequest.loadFileStatic);
        }
    }

    /// <summary>
    /// Save all instantiated rooms in the scene
    /// </summary>
    /// 
    public void Save(int slotIndex)
    {
        InstantiatedRoom[] allRooms = FindObjectsOfType<InstantiatedRoom>(true);

        Door[] allDoors = FindObjectsOfType<Door>(true);

        DungeonData dungeonData = new DungeonData();

        foreach (InstantiatedRoom instRoom in allRooms)
        {
            Room room = instRoom.room;
            if (room == null)
            {
                Debug.LogWarning("InstantiatedRoom missing Room data: " + instRoom.name);
                continue;
            }

            RoomData roomData = new RoomData
            {
                prefabName = instRoom.gameObject.name.Replace("(Clone)", ""),
                position = instRoom.transform.position,
                id = room.id,
                templateID = room.templateID,
                roomNodeTypeID = room.roomNodeType != null ? room.roomNodeType.name : "",
                lowerBounds = room.lowerBounds,
                upperBounds = room.upperBounds,
                templateLowerBounds = room.templateLowerBounds,
                templateUpperBounds = room.templateUpperBounds,
                spawnPositionArray = room.spawnPositionArray,
                childRoomIDList = new List<string>(room.childRoomIDList),
                parentRoomID = room.parentRoomID,
                isPositioned = room.isPositioned,
                isLit = room.isLit,
                isClearedOfEnemies = room.isClearedOfEnemies,
                isPreviouslyVisited = room.isPreviouslyVisited,
                doorWayList = new List<DoorwayData>(),
                doors = new List<DoorData>()
            };

            // Convert actual Doorways to serializable DoorwayData
            foreach (Doorway door in room.doorWayList)
            {
                roomData.doorWayList.Add(new DoorwayData
                {
                    position = door.position,
                    orientation = door.orientation,
                    doorwayStartCopyPosition = door.doorwayStartCopyPosition,
                    doorwayCopyTileWidth = door.doorwayCopyTileWidth,
                    doorwayCopyTileHeight = door.doorwayCopyTileHeight,
                    isConnected = door.isConnected,
                    isUnavailable = door.isUnavailable,
                    doorPrefabName = door.doorPrefab != null ? door.doorPrefab.name : ""
                });
            }

            foreach (Door door in instRoom.GetComponentsInChildren<Door>())
            {
                roomData.doors.Add(new DoorData
                {
                    position = door.transform.position,
                    isOpen = door.IsOpen,
                    doorPrefabName = door.gameObject.name.Replace("(Clone)", "")
                });
            }

            dungeonData.rooms.Add(roomData);
        }

        // Save Player
        GameManager gm = FindObjectOfType<GameManager>();
        Player player = FindObjectOfType<Player>();

        if (player != null)
        {
            dungeonData.player = new PlayerData
            {
                PlayerPrefabName = player.gameObject.name.Replace("(Clone)", ""),
                position = player.transform.position,
                saveTime = System.DateTime.Now.ToString("T"),
                dungeonLevelIndex = gm.currentDungeonLevelListIndex,
                spriteName = gm.playerDetails.playerMiniMapIcon.name
            };
        }
        
        dungeonData.player.weapons = new List<WeaponData>();
        Weapon activeWeapon = player.activeWeapon.GetCurrentWeapon();

        foreach (Weapon w in player.weaponList)
        {
            dungeonData.player.weapons.Add(new WeaponData
            {
                weaponDetailsName = w.weaponDetails.name,
                weaponListPosition = w.weaponListPosition,
                weaponClipRemainingAmmo = w.weaponClipRemainingAmmo,
                weaponRemainingAmmo = w.weaponRemainingAmmo,
                isWeaponReloading = w.isWeaponReloading,
                isActiveWeapon = (w == activeWeapon)
            });
        }

        string json = JsonUtility.ToJson(dungeonData, true);

        string path = GetSavePath(slotIndex);
        File.WriteAllText(path, json);
        Debug.Log("Saved to: " + path);

    }

    /// <summary>
    /// Load rooms from JSON
    /// </summary>
    public void Load(int slotIndex)
    {

        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found at: " + savePath);
            return;
        }

        // Clear previously loaded room GameObjects
        dungeonBuilder.ClearDungeon();
        ClearLoadedRooms();
        DungeonBuilder.Instance.dungeonBuilderRoomDictionary.Clear();

        string json = File.ReadAllText(path);
        DungeonData data = JsonUtility.FromJson<DungeonData>(json);

        foreach (RoomData roomData in data.rooms)
        {
            if (!prefabLookup.ContainsKey(roomData.prefabName))
            {
                Debug.LogWarning("Prefab not found: " + roomData.prefabName);
                continue;
            }

            // Recreate Room instance
            Room room = new Room
            {
                id = roomData.id,
                templateID = roomData.templateID,
                lowerBounds = roomData.lowerBounds,
                upperBounds = roomData.upperBounds,
                templateLowerBounds = roomData.templateLowerBounds,
                templateUpperBounds = roomData.templateUpperBounds,
                spawnPositionArray = roomData.spawnPositionArray,
                childRoomIDList = new List<string>(roomData.childRoomIDList),
                parentRoomID = roomData.parentRoomID,
                isPositioned = roomData.isPositioned,
                isLit = roomData.isLit,
                isClearedOfEnemies = roomData.isClearedOfEnemies,
                isPreviouslyVisited = roomData.isPreviouslyVisited,
                doorWayList = new List<Doorway>()
            };
        
            // Assign the RoomTemplateSO from your lookup
            if (roomTemplateLookup.TryGetValue(roomData.prefabName, out RoomTemplateSO templateSO))
            {
                room.roomNodeType = templateSO.roomNodeType; // assign roomNodeType
                room.ambientMusic = templateSO.ambientMusic; // assign ambient music
                room.battleMusic = templateSO.battleMusic;   // assign battle music

                // Populate enemy spawn data
                room.enemiesByLevelList = new List<SpawnableObjectsByLevel<EnemyDetailsSO>>(templateSO.enemiesByLevelList);
                room.roomLevelEnemySpawnParametersList = new List<RoomEnemySpawnParameters>(templateSO.roomEnemySpawnParametersList);
            }
            else
            {
                Debug.LogError("RoomTemplateSO not found: " + roomData.templateID);
            }

            // Instantiate prefab
            GameObject roomGO = Instantiate(prefabLookup[roomData.prefabName], roomData.position, Quaternion.identity, dungeonBuilder.transform);
            roomGO.name = roomData.prefabName;

            InstantiatedRoom instRoom = roomGO.GetComponentInChildren<InstantiatedRoom>();
            instRoom.room = room;

            // Restore Doorways
            foreach (DoorwayData doorData in roomData.doorWayList)
            {
                Doorway door = new Doorway
                {
                    position = doorData.position,
                    orientation = doorData.orientation,
                    doorwayStartCopyPosition = doorData.doorwayStartCopyPosition,
                    doorwayCopyTileWidth = doorData.doorwayCopyTileWidth,
                    doorwayCopyTileHeight = doorData.doorwayCopyTileHeight,
                    isConnected = doorData.isConnected,
                    isUnavailable = doorData.isUnavailable,
                    doorPrefab = !string.IsNullOrEmpty(doorData.doorPrefabName)
                                    ? Resources.Load<GameObject>(doorData.doorPrefabName)
                                    : null
                };

                room.doorWayList.Add(door);
            }

            // Save reference in Room
            room.instantiatedRoom = instRoom;

            // Initialise room
            instRoom.InitialiseNoDoors(roomGO);

            DungeonBuilder.Instance.dungeonBuilderRoomDictionary.Add(room.id, room);

            // Restore Doors
            foreach (DoorData doorData in roomData.doors)
            {
                if (!doorPrefabLookup.TryGetValue(doorData.doorPrefabName, out GameObject doorPrefab))
                {
                    Debug.LogWarning("Door prefab not found: " + doorData.doorPrefabName);
                    continue;
                }

                GameObject doorGO = Instantiate(doorPrefab, doorData.position, Quaternion.identity, instRoom.transform);

                Door door = doorGO.GetComponent<Door>();

                if (door != null)
                {
                    if (doorData.isOpen)
                    {
                        door.doorCollider.enabled = false;
                        door.doorCollider.GetComponentInChildren<BoxCollider2D>().enabled = false;
                        door.doorTrigger.enabled = false;
                        door.SetIsOpen(true);
                        door.previouslyOpened = true;

                        BoxCollider2D[] allColliders = door.doorCollider.GetComponentsInChildren<BoxCollider2D>();
                        foreach (BoxCollider2D col in allColliders)
                        {
                            col.enabled = false;
                        }

                        if (doorData.doorPrefabName.Contains("NS"))
                        {
                            // Set open parameter in animator
                            door.animator.SetBool(Settings.open, true);
                            SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
                            sr.material = new Material(Shader.Find("Sprites/Default"));
                        }
                        else
                        {
                            // Set open parameter in animator
                            door.animator.SetBool(Settings.open, true);
                            //set shader for bottom of EW door
                            SpriteRenderer[] csrs = door.GetComponentsInChildren<SpriteRenderer>();

                            foreach (SpriteRenderer csr in csrs)
                            {
                                csr.material = new Material(Shader.Find("Sprites/Default"));
                            }


                        }
                    }

                }

            }

            //Control Lighting
            if (!room.isLit)
            {
                instRoom.GetComponentInChildren<RoomLightingControl>()?.InitialiseLighting();
            }
        }

        Debug.Log("Rooms loaded: " + data.rooms.Count + " rooms.");

        // Load Player. Clear Existing Player First. 
        Player existingPlayer = FindObjectOfType<Player>();
        if (existingPlayer != null)
        {
            Destroy(existingPlayer.gameObject);
        }

        // Instantiate Player
        if (!playerPrefabLookup.TryGetValue(data.player.PlayerPrefabName, out GameObject prefab))
        {
            Debug.LogError("Player prefab not found: " + data.player.PlayerPrefabName);
            return;
        }

        GameObject playerGO = Instantiate(prefab);
        playerGO.transform.position = data.player.position;

        //Set new cinemachine target
        CinemachineTargetGroup cineTarget = FindObjectOfType<CinemachineTargetGroup>();
        cineTarget.AddMember(playerGO.transform, 1f, 0f);

        // <-- Update the gamemanager -->
        GameManager gm = FindObjectOfType<GameManager>();
        gm.player = playerGO.GetComponent<Player>();
        gm.currentDungeonLevelListIndex = data.player.dungeonLevelIndex;

        PlayerDetailsSO matchingDetails = playerDetails
            .Find(p => p.playerPrefab.name.ToLower().Contains(data.player.PlayerPrefabName.ToLower()));

        if (matchingDetails != null)
        {
            gm.playerDetails = matchingDetails;
        }
        else
        {
            Debug.LogWarning("No matching PlayerDetailsSO found for " + data.player.PlayerPrefabName);
        }

        gm.player.Initialize(matchingDetails);

// Clear default starting weapons created by Initialize
        gm.player.weaponList.Clear();

        Weapon weaponToActivate = null;

        foreach (WeaponData wd in data.player.weapons)
        {
            if (!weaponDetailsLookup.TryGetValue(wd.weaponDetailsName, out WeaponDetailsSO details))
            {
                Debug.LogWarning("WeaponDetailsSO not found: " + wd.weaponDetailsName);
                continue;
            }

            Weapon weapon = new Weapon()
            {
                weaponDetails = details,
                weaponListPosition = wd.weaponListPosition,
                weaponClipRemainingAmmo = wd.weaponClipRemainingAmmo,
                weaponRemainingAmmo = wd.weaponRemainingAmmo,
                isWeaponReloading = false
            };

            gm.player.weaponList.Add(weapon);

            if (wd.isActiveWeapon)
                weaponToActivate = weapon;
        }

        if (weaponToActivate != null)
            gm.player.setActiveWeaponEvent.CallSetActiveWeaponEvent(weaponToActivate);
        else if (gm.player.weaponList.Count > 0)
            gm.player.setActiveWeaponEvent.CallSetActiveWeaponEvent(gm.player.weaponList[0]);

        //Reinstantiate Minimap
        minimap.GetComponent<Minimap>().Reinitialize();
        FindObjectOfType<WeaponStatusUI>()?.Reinitialize();
    }

    private void ClearLoadedRooms()
    {
        Transform dungeonTransform = dungeonBuilder.transform;

        //Debug.Log("DungeonBuilder children BEFORE clear: " + dungeonTransform.childCount);

        for (int i = dungeonTransform.childCount - 1; i >= 0; i--)
        {
            //Debug.Log("Destroying room: " + dungeonTransform.GetChild(i).name);
            Destroy(dungeonTransform.GetChild(i).gameObject);
        }

        //Debug.Log("DungeonBuilder children AFTER clear: " + dungeonTransform.childCount);
    }

    public IEnumerator LoadWithFade(int slotIndex)
    {
        ScreenFader.Instance.FadeOut();
        yield return new WaitForSeconds(0.6f); // must be >= fade duration

        Load(slotIndex); 
        yield return new WaitForSeconds(0.2f);
        ScreenFader.Instance.FadeIn();
    }

    public void OnLoadButtonPressed(int slotIndex)
    {
        StartCoroutine(LoadWithFade(slotIndex));
    }

    private string GetSavePath(int slotIndex)
    {
        return Application.persistentDataPath + $"/save_slot_{slotIndex}.json";
    }

    public DungeonData GetSlotData(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<DungeonData>(json);
    }

    // Helper method to get sprite by name
    public Sprite GetPlayerSprite(string spriteName)
    {
        try
        {
            if (spriteLookup.TryGetValue(spriteName, out Sprite sprite))
            return sprite;
        }
        catch (NullReferenceException)
        {
            // silently ignore
        }
        return null; // fallback if not found
    }

    public void DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save file at: " + path);
        }
        else
        {
            Debug.LogWarning("No save file found to delete.");
        }
    }
}
