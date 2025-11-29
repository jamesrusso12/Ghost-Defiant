using Meta.XR.BuildingBlocks;
using UnityEngine;

/// <summary>
/// Diagnostic tool to check passthrough configuration and status.
/// Run this in Play mode to see what's wrong with passthrough.
/// </summary>
public class PassthroughDiagnostics : MonoBehaviour
{
    [Header("Auto-Run Diagnostics")]
    [Tooltip("Run diagnostics automatically when entering Play mode")]
    public bool runOnStart = true;

    [ContextMenu("Run Diagnostics Now")]
    public void RunDiagnostics()
    {
        Debug.Log("=== PASSTHROUGH DIAGNOSTICS ===");
        
        CheckSceneMeshBuildingBlock();
        CheckPassthroughLayer();
        CheckRoomMeshVisualizerDisabler();
        CheckCameraSetup();
        
        Debug.Log("=== END DIAGNOSTICS ===");
    }

    private void Start()
    {
        if (runOnStart)
        {
            RunDiagnostics();
        }
    }

    private void CheckSceneMeshBuildingBlock()
    {
        Debug.Log("--- Checking Scene Mesh Building Block ---");
        
        // Find Scene Mesh Building Block
        var buildingBlocks = FindObjectsByType<BuildingBlock>(FindObjectsSortMode.None);
        BuildingBlock sceneMeshBlock = null;
        
        foreach (var block in buildingBlocks)
        {
            if (block.name.Contains("Scene Mesh") || block.name.Contains("Room Mesh"))
            {
                sceneMeshBlock = block;
                break;
            }
        }

        if (sceneMeshBlock == null)
        {
            Debug.LogError("❌ Scene Mesh Building Block NOT FOUND in scene!");
            Debug.LogError("   Passthrough requires the Scene Mesh Building Block to be present.");
            return;
        }

        Debug.Log($"✅ Scene Mesh Building Block found: {sceneMeshBlock.name}");
        Debug.Log($"   Active: {sceneMeshBlock.gameObject.activeInHierarchy} (should be TRUE)");
        
        if (!sceneMeshBlock.gameObject.activeInHierarchy)
        {
            Debug.LogError("   ❌ BUILDING BLOCK IS DISABLED! This will break passthrough!");
            Debug.LogError("   → Enable the GameObject immediately!");
        }

        // Check RoomMeshController
        var roomMeshController = sceneMeshBlock.GetComponent<RoomMeshController>();
        if (roomMeshController == null)
        {
            Debug.LogWarning("⚠️ RoomMeshController component not found on Building Block");
        }
        else
        {
            Debug.Log($"✅ RoomMeshController found and enabled: {roomMeshController.enabled}");
        }

        // Check RoomMeshEvent
        var roomMeshEvent = sceneMeshBlock.GetComponent<RoomMeshEvent>();
        if (roomMeshEvent == null)
        {
            Debug.LogWarning("⚠️ RoomMeshEvent component not found on Building Block");
        }
        else
        {
            Debug.Log($"✅ RoomMeshEvent found");
        }
    }

    private void CheckPassthroughLayer()
    {
        Debug.Log("--- Checking Passthrough Layer ---");
        
        // Find OVRPassthroughLayer components
        var passthroughLayers = FindObjectsByType<OVRPassthroughLayer>(FindObjectsSortMode.None);
        
        if (passthroughLayers.Length == 0)
        {
            Debug.LogError("❌ No OVRPassthroughLayer components found!");
            Debug.LogError("   Passthrough requires at least one OVRPassthroughLayer component.");
            return;
        }

        Debug.Log($"✅ Found {passthroughLayers.Length} OVRPassthroughLayer(s):");
        
        foreach (var layer in passthroughLayers)
        {
            var go = layer.gameObject;
            Debug.Log($"   - {go.name}");
            Debug.Log($"     Active: {go.activeInHierarchy}");
            Debug.Log($"     Component Enabled: {layer.enabled}");
            
            if (!go.activeInHierarchy || !layer.enabled)
            {
                Debug.LogWarning($"   ⚠️ This passthrough layer is disabled!");
            }

            // Check if it's a background layer (Underlay = background, Overlay = foreground)
            Debug.Log($"     Overlay Type: {layer.overlayType}");
            if (layer.overlayType.ToString() == "Underlay")
            {
                Debug.Log($"   ✅ This is a background passthrough layer (Underlay)");
            }
            else if (layer.overlayType.ToString() == "Overlay")
            {
                Debug.Log($"   ⚠️ This is a foreground passthrough layer (Overlay) - might cover content");
            }
        }
    }

    private void CheckRoomMeshVisualizerDisabler()
    {
        Debug.Log("--- Checking RoomMeshVisualizerDisabler ---");
        
        var disabler = FindAnyObjectByType<RoomMeshVisualizerDisabler>();
        
        if (disabler == null)
        {
            Debug.LogWarning("⚠️ RoomMeshVisualizerDisabler not found (this is okay if you don't need it)");
            return;
        }

        Debug.Log($"✅ RoomMeshVisualizerDisabler found: {disabler.gameObject.name}");
        Debug.Log($"   Active: {disabler.gameObject.activeInHierarchy}");
        Debug.Log($"   Component Enabled: {disabler.enabled}");
        Debug.Log($"   Note: If this script breaks passthrough, disable it and manually hide the mesh renderer instead.");

        // Try to get the roomMeshEvent reference using reflection to check if it's set
        var field = typeof(RoomMeshVisualizerDisabler).GetField("roomMeshEvent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var roomMeshEvent = field.GetValue(disabler) as RoomMeshEvent;
            if (roomMeshEvent == null)
            {
                Debug.LogWarning("   ⚠️ RoomMeshEvent reference is not assigned");
            }
            else
            {
                Debug.Log($"   ✅ RoomMeshEvent reference is assigned: {roomMeshEvent.gameObject.name}");
            }
        }
    }

    private void CheckCameraSetup()
    {
        Debug.Log("--- Checking Camera Setup ---");
        
        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"Found {cameras.Length} Camera(s):");
        
        foreach (var cam in cameras)
        {
            if (cam.name.Contains("CenterEye") || cam.name.Contains("Main"))
            {
                Debug.Log($"   - {cam.name}");
                Debug.Log($"     Active: {cam.gameObject.activeInHierarchy}");
                Debug.Log($"     Enabled: {cam.enabled}");
                Debug.Log($"     Culling Mask: {cam.cullingMask}");
                Debug.Log($"     Clear Flags: {cam.clearFlags}");
                
                if (cam.clearFlags == CameraClearFlags.Skybox)
                {
                    Debug.LogWarning($"     ⚠️ Camera is clearing to Skybox - this might hide passthrough!");
                }
            }
        }
    }
}

