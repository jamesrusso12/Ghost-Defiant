using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class MRUKSetupManager : MonoBehaviour
{
    [Header("MRUK Configuration")]
    public bool loadSceneOnStartup = true;
    public int roomIndex = -1; // -1 for random room
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showRoomBounds = true;
    
    [Tooltip("RoomVisualizer component to use for visualization (optional)")]
    public RoomVisualizer roomVisualizer;
    
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
            Debug.Log("[MRUKSetupManager] Starting MRUK initialization...");
            
        // Wait for MRUK to be available
        yield return new WaitUntil(() => MRUK.Instance != null);
        
        mrukInstance = MRUK.Instance;
        
        // Configure MRUK settings
        ConfigureMRUKSettings();
        
        // Wait for initialization
        yield return new WaitUntil(() => mrukInstance.IsInitialized);
        
        if (enableDebugLogs)
            Debug.Log("[MRUKSetupManager] MRUK initialized successfully!");
            
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
            Debug.Log($"[MRUKSetupManager] Configured with RoomIndex: {roomIndex}");
        }
    }
    
    private void OnMRUKInitialized()
    {
        // Log room information
        if (mrukInstance.Rooms.Count > 0)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[MRUKSetupManager] Found {mrukInstance.Rooms.Count} rooms");
                foreach (var room in mrukInstance.Rooms)
                {
                    Debug.Log($"[MRUKSetupManager] Room: {room.name}, Anchors: {room.Anchors.Count}");
                }
            }
            
            // Show room bounds if enabled
            if (showRoomBounds)
            {
                ShowRoomBounds();
            }
            
            // Enable room visualizer if available
            if (roomVisualizer != null)
            {
                roomVisualizer.showVisualization = true;
                roomVisualizer.UpdateVisualization();
            }
            else
            {
                // Try to find existing visualizer
                roomVisualizer = FindFirstObjectByType<RoomVisualizer>();
                if (roomVisualizer != null && showRoomBounds)
                {
                    roomVisualizer.showVisualization = true;
                    roomVisualizer.UpdateVisualization();
                }
            }
        }
        else
        {
            Debug.LogWarning("[MRUKSetupManager] No rooms detected! Check your spatial mapping setup.");
        }
        
        // Notify other systems
        BroadcastMessage("OnMRUKReady", SendMessageOptions.DontRequireReceiver);
    }
    
    private void ShowRoomBounds()
    {
        var currentRoom = mrukInstance.GetCurrentRoom();
        if (currentRoom != null)
        {
            // Create visual representation of room bounds
            CreateRoomBoundsVisualization(currentRoom);
        }
    }
    
    private void CreateRoomBoundsVisualization(MRUKRoom room)
    {
        // This would create visual bounds for debugging
        // Implementation depends on your visualization needs
        if (enableDebugLogs)
        {
            Debug.Log($"[MRUKSetupManager] Room: {room.name} with {room.Anchors.Count} anchors");
            // Log anchor positions for debugging
            foreach (var anchor in room.Anchors)
            {
                Debug.Log($"[MRUKSetupManager] Anchor: {anchor.name} at {anchor.transform.position}");
            }
        }
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