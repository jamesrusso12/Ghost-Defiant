using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class SimpleMRUKManager : MonoBehaviour
{
    [Header("MRUK Configuration")]
    public bool loadSceneOnStartup = true;
    public int roomIndex = -1; // -1 for random room
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    [Header("Fallback Settings")]
    public GameObject[] fallbackRoomPrefabs;
    public TextAsset[] fallbackSceneJsons;
    
    private bool isInitialized = false;
    private MRUK mrukInstance;
    
    void Start()
    {
        StartCoroutine(InitializeMRUK());
    }
    
    private IEnumerator InitializeMRUK()
    {
        if (enableDebugLogs)
            Debug.Log("[SimpleMRUKManager] Starting MRUK initialization...");
            
        // Wait for MRUK to be available
        yield return new WaitUntil(() => MRUK.Instance != null);
        
        mrukInstance = MRUK.Instance;
        
        // Configure MRUK settings
        ConfigureMRUKSettings();
        
        // Wait for initialization
        yield return new WaitUntil(() => mrukInstance.IsInitialized);
        
        if (enableDebugLogs)
            Debug.Log("[SimpleMRUKManager] MRUK initialized successfully!");
            
        isInitialized = true;
        
        // Notify other systems that MRUK is ready
        OnMRUKInitialized();
    }
    
    private void ConfigureMRUKSettings()
    {
        if (mrukInstance == null) return;
        
        // Configure scene settings
        mrukInstance.SceneSettings.LoadSceneOnStartup = loadSceneOnStartup;
        mrukInstance.SceneSettings.RoomIndex = roomIndex;
        
        // Set up fallback rooms
        if (fallbackRoomPrefabs != null && fallbackRoomPrefabs.Length > 0)
        {
            mrukInstance.SceneSettings.RoomPrefabs = fallbackRoomPrefabs;
        }
        
        if (fallbackSceneJsons != null && fallbackSceneJsons.Length > 0)
        {
            mrukInstance.SceneSettings.SceneJsons = fallbackSceneJsons;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SimpleMRUKManager] Configured with RoomIndex: {roomIndex}");
        }
    }
    
    private void OnMRUKInitialized()
    {
        // Log room information
        if (mrukInstance.Rooms.Count > 0)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SimpleMRUKManager] Found {mrukInstance.Rooms.Count} rooms");
                foreach (var room in mrukInstance.Rooms)
                {
                    Debug.Log($"[SimpleMRUKManager] Room: {room.name}, Anchors: {room.Anchors.Count}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[SimpleMRUKManager] No rooms detected! Check your spatial mapping setup.");
        }
        
        // Notify other systems
        BroadcastMessage("OnMRUKReady", SendMessageOptions.DontRequireReceiver);
    }
    
    public bool IsMRUKReady()
    {
        return isInitialized && mrukInstance != null && mrukInstance.IsInitialized;
    }
    
    public MRUKRoom GetCurrentRoom()
    {
        if (IsMRUKReady())
        {
            return mrukInstance.GetCurrentRoom();
        }
        return null;
    }
    
    public void ReloadScene()
    {
        if (mrukInstance != null)
        {
            mrukInstance.LoadSceneFromDevice();
        }
    }
    
    // Public methods for external systems to check MRUK status
    public void WaitForMRUK(System.Action callback)
    {
        if (IsMRUKReady())
        {
            callback?.Invoke();
        }
        else
        {
            StartCoroutine(WaitForMRUKCoroutine(callback));
        }
    }
    
    private IEnumerator WaitForMRUKCoroutine(System.Action callback)
    {
        yield return new WaitUntil(() => IsMRUKReady());
        callback?.Invoke();
    }
}
