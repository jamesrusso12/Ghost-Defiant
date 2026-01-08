using Meta.XR.BuildingBlocks;
using UnityEngine;
using UnityEngine.Events;

// Suppress obsolete warnings for RoomMeshEvent - still functional but deprecated
// TODO: Migrate to MRUK Effect Mesh building block in future update
#pragma warning disable CS0618

/// <summary>
/// Listens to the RoomMeshController load event and immediately hides/destroys
/// the visualized scene mesh so it does not cover the passthrough view.
/// NOTE: RoomMeshEvent is deprecated - consider migrating to MRUK Effect Mesh building block
/// </summary>
[DefaultExecutionOrder(5)]
public class RoomMeshVisualizerDisabler : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("Auto-assigned if empty. The RoomMeshEvent component from the Scene Mesh Building Block.")]
    private RoomMeshEvent roomMeshEvent;

    [Header("Behaviour")]
    [Tooltip("When enabled we destroy the generated mesh GameObject. Otherwise we simply disable its renderer.")]
    [SerializeField]
    public bool destroyRoomMeshObject = false; // Set to false to keep passthrough working - only hide the visual mesh
    
    [Tooltip("⚠️ WARNING: This script may break passthrough. If passthrough stops working, disable this script component and manually hide the mesh renderer in Play Mode instead.")]
    [SerializeField]
    public bool enableMeshHiding = false; // Disabled by default - disable script entirely if it breaks passthrough

    private UnityAction<MeshFilter> _onMeshLoadedHandler;

#if UNITY_EDITOR
    /// <summary>
    /// Auto-assigns the RoomMeshEvent reference in the Editor when the component is added or modified.
    /// Wrapped in try-catch to prevent crashes during Unity import/compilation.
    /// </summary>
    private void OnValidate()
    {
        // Safety check: Don't run during import/compilation or if Unity isn't ready
        if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isCompiling)
        {
            try
            {
                if (roomMeshEvent == null)
                {
                    roomMeshEvent = FindAnyObjectByType<RoomMeshEvent>();
                    if (roomMeshEvent != null)
                    {
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Silently fail during import/compilation to prevent crashes
                // This is expected behavior when Unity is importing assets
                if (UnityEditor.EditorApplication.isCompiling || UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
                {
                    // Expected during import - ignore
                    return;
                }
                // Only log if it's an unexpected error
                Debug.LogWarning($"[RoomMeshVisualizerDisabler] OnValidate failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Context menu option to manually find and assign the RoomMeshEvent reference.
    /// </summary>
    [ContextMenu("Auto-Assign Room Mesh Event")]
    private void AutoAssignRoomMeshEvent()
    {
        roomMeshEvent = FindAnyObjectByType<RoomMeshEvent>();
        if (roomMeshEvent != null)
        {
            Debug.Log($"[RoomMeshVisualizerDisabler] Auto-assigned RoomMeshEvent: {roomMeshEvent.gameObject.name}");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        else
        {
            Debug.LogWarning("[RoomMeshVisualizerDisabler] No RoomMeshEvent found in scene. Make sure the Scene Mesh Building Block is present.");
        }
    }
#endif

    private void Awake()
    {
        if (roomMeshEvent == null)
        {
            roomMeshEvent = FindAnyObjectByType<RoomMeshEvent>();
            if (roomMeshEvent != null)
            {
                Debug.Log($"[RoomMeshVisualizerDisabler] Auto-assigned RoomMeshEvent at runtime: {roomMeshEvent.gameObject.name}");
            }
        }

        _onMeshLoadedHandler = HandleRoomMeshLoaded;
    }

    private void OnEnable()
    {
        // Only subscribe to events if mesh hiding is enabled
        if (!enableMeshHiding)
        {
            Debug.Log("[RoomMeshVisualizerDisabler] Mesh hiding is disabled. Passthrough should work normally.");
            return;
        }
        
        if (roomMeshEvent != null)
        {
            roomMeshEvent.OnRoomMeshLoadCompleted.AddListener(_onMeshLoadedHandler);
        }
        else
        {
            Debug.LogWarning("[RoomMeshVisualizerDisabler] No RoomMeshEvent found. The room mesh will remain visible.");
        }
    }

    private void OnDisable()
    {
        if (roomMeshEvent != null)
        {
            roomMeshEvent.OnRoomMeshLoadCompleted.RemoveListener(_onMeshLoadedHandler);
        }
    }

    private void HandleRoomMeshLoaded(MeshFilter meshFilter)
    {
        if (meshFilter == null) return;
        
        // Don't do anything if mesh hiding is disabled
        if (!enableMeshHiding)
        {
            return;
        }

        if (destroyRoomMeshObject)
        {
            Destroy(meshFilter.gameObject);
        }
        else
        {
            // Only disable the renderer - keep the GameObject active for passthrough to work
            var renderer = meshFilter.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
                Debug.Log($"[RoomMeshVisualizerDisabler] Disabled MeshRenderer on {meshFilter.gameObject.name} (keeping GameObject active for passthrough)");
            }
            else
            {
                Debug.LogWarning($"[RoomMeshVisualizerDisabler] No MeshRenderer found on {meshFilter.gameObject.name}");
            }
            
            // DO NOT deactivate the GameObject - passthrough needs it active!
            // meshFilter.gameObject.SetActive(false); // REMOVED - breaks passthrough
        }
    }
}

#pragma warning restore CS0618

