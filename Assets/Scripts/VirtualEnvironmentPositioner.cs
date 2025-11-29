using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Positions virtual environment outside room boundaries to avoid occlusion issues.
/// This is often more reliable than trying to fix passthrough shader depth writing.
/// </summary>
public class VirtualEnvironmentPositioner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent GameObject containing all virtual environment objects")]
    public GameObject virtualEnvironmentParent;
    
    [Tooltip("DestructibleGlobalMeshSpawner to get room bounds")]
    public DestructibleGlobalMeshSpawner meshSpawner;
    
    [Header("Positioning")]
    [Tooltip("Offset from room boundaries (how far outside to place environment)")]
    public float offsetFromRoom = 2f;
    
    [Tooltip("Height offset (how high/low to place environment relative to room)")]
    public float heightOffset = 0f;
    
    [Tooltip("Scale factor for virtual environment")]
    public float scaleFactor = 1f;
    
    [Header("Auto-Positioning")]
    [Tooltip("Automatically position when room mesh is created")]
    public bool autoPositionOnMeshCreated = true;
    
    [Tooltip("Position relative to room center or room bounds")]
    public bool useRoomBounds = true;
    
    private DestructibleMeshComponent currentMeshComponent;
    private Bounds roomBounds;
    private bool hasPositioned = false;
    
    void Start()
    {
        if (meshSpawner != null && autoPositionOnMeshCreated)
        {
            meshSpawner.OnDestructibleMeshCreated.AddListener(OnRoomMeshCreated);
        }
        
        // Also try to position if mesh already exists
        StartCoroutine(CheckForExistingMesh());
    }
    
    private System.Collections.IEnumerator CheckForExistingMesh()
    {
        yield return new WaitForSeconds(1f);
        
        DestructibleMeshComponent existingMesh = FindFirstObjectByType<DestructibleMeshComponent>();
        if (existingMesh != null && !hasPositioned)
        {
            OnRoomMeshCreated(existingMesh);
        }
    }
    
    private void OnRoomMeshCreated(DestructibleMeshComponent component)
    {
        if (component == null || virtualEnvironmentParent == null) return;
        
        currentMeshComponent = component;
        CalculateRoomBounds();
        PositionVirtualEnvironment();
        hasPositioned = true;
    }
    
    /// <summary>
    /// Calculates the bounding box of the room mesh
    /// </summary>
    private void CalculateRoomBounds()
    {
        if (currentMeshComponent == null) return;
        
        System.Collections.Generic.List<GameObject> segments = new System.Collections.Generic.List<GameObject>();
        currentMeshComponent.GetDestructibleMeshSegments(segments);
        
        if (segments.Count == 0)
        {
            Debug.LogWarning("[VirtualEnvironmentPositioner] No room mesh segments found!");
            return;
        }
        
        bool boundsInitialized = false;
        roomBounds = new Bounds();
        
        foreach (GameObject segment in segments)
        {
            if (segment == null) continue;
            
            MeshFilter meshFilter = segment.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Bounds segmentBounds = meshFilter.sharedMesh.bounds;
                Vector3 worldCenter = segment.transform.TransformPoint(segmentBounds.center);
                Vector3 worldSize = segment.transform.TransformVector(segmentBounds.size);
                
                Bounds worldBounds = new Bounds(worldCenter, worldSize);
                
                if (!boundsInitialized)
                {
                    roomBounds = worldBounds;
                    boundsInitialized = true;
                }
                else
                {
                    roomBounds.Encapsulate(worldBounds);
                }
            }
        }
        
        Debug.Log($"[VirtualEnvironmentPositioner] Room bounds: Center={roomBounds.center}, Size={roomBounds.size}");
    }
    
    /// <summary>
    /// Positions the virtual environment outside the room boundaries
    /// </summary>
    [ContextMenu("Position Virtual Environment")]
    public void PositionVirtualEnvironment()
    {
        if (virtualEnvironmentParent == null)
        {
            Debug.LogError("[VirtualEnvironmentPositioner] Virtual Environment Parent is NULL!");
            return;
        }
        
        if (!hasPositioned && currentMeshComponent != null)
        {
            CalculateRoomBounds();
        }
        
        if (roomBounds.size == Vector3.zero)
        {
            Debug.LogWarning("[VirtualEnvironmentPositioner] Room bounds not calculated. Using manual positioning.");
            return;
        }
        
        // Calculate position outside room
        Vector3 roomCenter = roomBounds.center;
        Vector3 roomSize = roomBounds.size;
        
        // Position environment outside the largest dimension
        // Typically place it outside the front/back or sides
        float maxDimension = Mathf.Max(roomSize.x, roomSize.z);
        
        // Position outside (in front or to the side)
        Vector3 offset = Vector3.zero;
        
        // Option 1: Place outside front (positive Z)
        offset = new Vector3(0, heightOffset, roomSize.z * 0.5f + offsetFromRoom);
        
        // Option 2: Place outside side (positive X) - uncomment to use
        // offset = new Vector3(roomSize.x * 0.5f + offsetFromRoom, heightOffset, 0);
        
        Vector3 newPosition = roomCenter + offset;
        
        virtualEnvironmentParent.transform.position = newPosition;
        virtualEnvironmentParent.transform.localScale = Vector3.one * scaleFactor;
        
        Debug.Log($"[VirtualEnvironmentPositioner] Positioned virtual environment at: {newPosition}");
        Debug.Log($"[VirtualEnvironmentPositioner] Room center: {roomCenter}, Room size: {roomSize}");
    }
    
    /// <summary>
    /// Manually set position (useful for fine-tuning)
    /// </summary>
    public void SetManualPosition(Vector3 position)
    {
        if (virtualEnvironmentParent != null)
        {
            virtualEnvironmentParent.transform.position = position;
            Debug.Log($"[VirtualEnvironmentPositioner] Manually positioned at: {position}");
        }
    }
    
    /// <summary>
    /// Get room bounds for reference
    /// </summary>
    public Bounds GetRoomBounds()
    {
        return roomBounds;
    }
    
    void OnDestroy()
    {
        if (meshSpawner != null)
        {
            meshSpawner.OnDestructibleMeshCreated.RemoveListener(OnRoomMeshCreated);
        }
    }
}

