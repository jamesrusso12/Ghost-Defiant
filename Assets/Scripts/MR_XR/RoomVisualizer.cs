using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

/// <summary>
/// Visualizes MRUK room walls, anchors, and surfaces in Play Mode for positioning reference.
/// Add this component to any GameObject in your scene to see room boundaries.
/// </summary>
public class RoomVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [Tooltip("Enable room visualization")]
    public bool showVisualization = true;
    
    [Tooltip("Show room walls")]
    public bool showWalls = true;
    
    [Tooltip("Show room anchors")]
    public bool showAnchors = true;
    
    [Tooltip("Show room surfaces (floors, ceilings, etc.)")]
    public bool showSurfaces = true;
    
    [Tooltip("Show room bounds (bounding box)")]
    public bool showBounds = true;
    
    [Header("Visual Style")]
    [Tooltip("Color for walls")]
    public Color wallColor = new Color(0f, 1f, 0f, 0.5f); // Green, semi-transparent
    
    [Tooltip("Color for anchors")]
    public Color anchorColor = new Color(1f, 0f, 0f, 1f); // Red
    
    [Tooltip("Color for surfaces")]
    public Color surfaceColor = new Color(0f, 0f, 1f, 0.3f); // Blue, semi-transparent
    
    [Tooltip("Color for room bounds")]
    public Color boundsColor = new Color(1f, 1f, 0f, 0.5f); // Yellow, semi-transparent
    
    [Tooltip("Line width for visualization")]
    public float lineWidth = 0.02f;
    
    [Tooltip("Anchor marker size")]
    public float anchorSize = 0.1f;
    
    [Header("Update Settings")]
    [Tooltip("Update visualization every frame (for dynamic rooms)")]
    public bool updateEveryFrame = false;
    
    [Tooltip("Update interval in seconds (if not updating every frame)")]
    public float updateInterval = 1f;
    
    private MRUK mrukInstance;
    private MRUKRoom currentRoom;
    private float lastUpdateTime = 0f;
    
    // Storage for visualization objects
    private List<GameObject> visualizationObjects = new List<GameObject>();
    private GameObject visualizationParent;
    
    // OPTIMIZATION: Cache materials to avoid creating new ones on every update
    private Material cachedLineMaterial = null;
    private Material cachedMaterial = null;
    
    void Start()
    {
        if (!showVisualization) return;
        
        // Create parent object for all visualization objects
        visualizationParent = new GameObject("RoomVisualization");
        visualizationParent.transform.SetParent(transform);
        
        // Wait for MRUK to initialize
        StartCoroutine(WaitForMRUKAndVisualize());
    }
    
    private System.Collections.IEnumerator WaitForMRUKAndVisualize()
    {
        // Wait for MRUK instance
        yield return new WaitUntil(() => MRUK.Instance != null);
        mrukInstance = MRUK.Instance;
        
        // Wait for initialization
        yield return new WaitUntil(() => mrukInstance.IsInitialized);
        
        // Wait a bit more for room data to be ready
        yield return new WaitForSeconds(0.5f);
        
        // Create initial visualization
        UpdateVisualization();
    }
    
    void Update()
    {
        if (!showVisualization) return;
        
        // Update visualization if needed
        if (updateEveryFrame || (Time.time - lastUpdateTime >= updateInterval))
        {
            UpdateVisualization();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Updates the room visualization
    /// </summary>
    [ContextMenu("Update Visualization")]
    public void UpdateVisualization()
    {
        if (mrukInstance == null || !mrukInstance.IsInitialized)
        {
            return;
        }
        
        // Get current room
        currentRoom = mrukInstance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogWarning("[RoomVisualizer] No current room found!");
            return;
        }
        
        // Clear existing visualization
        ClearVisualization();
        
        // Create new visualization
        if (showWalls)
        {
            VisualizeWalls();
        }
        
        if (showAnchors)
        {
            VisualizeAnchors();
        }
        
        if (showSurfaces)
        {
            VisualizeSurfaces();
        }
        
        if (showBounds)
        {
            VisualizeBounds();
        }
    }
    
    /// <summary>
    /// Visualizes room walls using anchors with VERTICAL surface type
    /// </summary>
    private void VisualizeWalls()
    {
        if (currentRoom == null) return;
        
        foreach (var anchor in currentRoom.Anchors)
        {
            if (anchor == null) continue;
            
            // Check if this anchor represents a wall (vertical surface)
            bool isWall = false;
            
            // Check anchor label using the Label property (SceneLabels enum)
            MRUKAnchor.SceneLabels label = anchor.Label;
            if (label.HasFlag(MRUKAnchor.SceneLabels.WALL_FACE))
            {
                isWall = true;
            }
            
            if (isWall)
            {
                CreateWallVisualization(anchor);
            }
        }
    }
    
    /// <summary>
    /// Creates visualization for a single wall anchor
    /// </summary>
    private void CreateWallVisualization(MRUKAnchor anchor)
    {
        if (anchor == null) return;
        
        // Get anchor plane rect (for walls/floors)
        Rect? planeRect = anchor.PlaneRect;
        if (!planeRect.HasValue)
        {
            // Try volume bounds as fallback
            Bounds? volumeBounds = anchor.VolumeBounds;
            if (!volumeBounds.HasValue)
            {
                return; // No bounds available
            }
            
            // Use volume bounds
            CreateWallFromBounds(anchor, volumeBounds.Value);
            return;
        }
        
        // Create a line renderer for plane rect
        GameObject wallObj = new GameObject($"Wall_{anchor.name}");
        wallObj.transform.SetParent(visualizationParent.transform);
        
        LineRenderer lr = wallObj.AddComponent<LineRenderer>();
        lr.material = CreateLineMaterial(wallColor);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.positionCount = 4;
        lr.loop = true;
        
        // Calculate wall corners from plane rect
        Vector3 center = anchor.transform.position;
        Vector3 right = anchor.transform.right;
        Vector3 up = anchor.transform.up;
        
        Rect rect = planeRect.Value;
        float width = rect.width;
        float height = rect.height;
        
        // Create rectangle corners
        Vector3 bottomLeft = center + right * (rect.x - width * 0.5f) + up * (rect.y - height * 0.5f);
        Vector3 bottomRight = center + right * (rect.x + width * 0.5f) + up * (rect.y - height * 0.5f);
        Vector3 topRight = center + right * (rect.x + width * 0.5f) + up * (rect.y + height * 0.5f);
        Vector3 topLeft = center + right * (rect.x - width * 0.5f) + up * (rect.y + height * 0.5f);
        
        lr.SetPosition(0, bottomLeft);
        lr.SetPosition(1, bottomRight);
        lr.SetPosition(2, topRight);
        lr.SetPosition(3, topLeft);
        
        visualizationObjects.Add(wallObj);
    }
    
    /// <summary>
    /// Creates wall visualization from volume bounds
    /// </summary>
    private void CreateWallFromBounds(MRUKAnchor anchor, Bounds bounds)
    {
        GameObject wallObj = new GameObject($"Wall_{anchor.name}");
        wallObj.transform.SetParent(visualizationParent.transform);
        
        LineRenderer lr = wallObj.AddComponent<LineRenderer>();
        lr.material = CreateLineMaterial(wallColor);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.positionCount = 4;
        lr.loop = true;
        
        Vector3 center = anchor.transform.TransformPoint(bounds.center);
        Vector3 size = bounds.size;
        
        // Assume wall is vertical (largest dimension is height)
        float width = Mathf.Max(size.x, size.z);
        float height = size.y;
        
        Vector3 right = anchor.transform.right;
        Vector3 up = anchor.transform.up;
        
        Vector3 bottomLeft = center - right * (width * 0.5f) - up * (height * 0.5f);
        Vector3 bottomRight = center + right * (width * 0.5f) - up * (height * 0.5f);
        Vector3 topRight = center + right * (width * 0.5f) + up * (height * 0.5f);
        Vector3 topLeft = center - right * (width * 0.5f) + up * (height * 0.5f);
        
        lr.SetPosition(0, bottomLeft);
        lr.SetPosition(1, bottomRight);
        lr.SetPosition(2, topRight);
        lr.SetPosition(3, topLeft);
        
        visualizationObjects.Add(wallObj);
    }
    
    /// <summary>
    /// Visualizes room anchors as markers
    /// </summary>
    private void VisualizeAnchors()
    {
        if (currentRoom == null) return;
        
        foreach (var anchor in currentRoom.Anchors)
        {
            if (anchor == null) continue;
            
            CreateAnchorMarker(anchor);
        }
    }
    
    /// <summary>
    /// Creates a visual marker for an anchor
    /// </summary>
    private void CreateAnchorMarker(MRUKAnchor anchor)
    {
        GameObject markerObj = new GameObject($"Anchor_{anchor.name}");
        markerObj.transform.SetParent(visualizationParent.transform);
        markerObj.transform.position = anchor.transform.position;
        markerObj.transform.rotation = anchor.transform.rotation;
        
        // Create a small sphere or cube to mark the anchor
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(markerObj.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * anchorSize;
        
        // Set color
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = CreateMaterial(anchorColor);
            renderer.material = mat;
        }
        
        // Remove collider (we don't need it for visualization)
        Collider collider = sphere.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        visualizationObjects.Add(markerObj);
    }
    
    /// <summary>
    /// Visualizes room surfaces (floors, ceilings, etc.)
    /// </summary>
    private void VisualizeSurfaces()
    {
        if (currentRoom == null) return;
        
        foreach (var anchor in currentRoom.Anchors)
        {
            if (anchor == null) continue;
            
            // Check if this is a horizontal surface (floor/ceiling)
            bool isSurface = false;
            
            // Check anchor label using the Label property (SceneLabels enum)
            MRUKAnchor.SceneLabels label = anchor.Label;
            if (label.HasFlag(MRUKAnchor.SceneLabels.FLOOR) || label.HasFlag(MRUKAnchor.SceneLabels.CEILING))
            {
                isSurface = true;
            }
            
            if (isSurface)
            {
                CreateSurfaceVisualization(anchor);
            }
        }
    }
    
    /// <summary>
    /// Creates visualization for a surface (floor/ceiling)
    /// </summary>
    private void CreateSurfaceVisualization(MRUKAnchor anchor)
    {
        if (anchor == null) return;
        
        // Get plane rect for floor/ceiling
        Rect? planeRect = anchor.PlaneRect;
        if (!planeRect.HasValue)
        {
            // Try volume bounds as fallback
            Bounds? volumeBounds = anchor.VolumeBounds;
            if (!volumeBounds.HasValue)
            {
                return; // No bounds available
            }
            
            CreateSurfaceFromBounds(anchor, volumeBounds.Value);
            return;
        }
        
        GameObject surfaceObj = new GameObject($"Surface_{anchor.name}");
        surfaceObj.transform.SetParent(visualizationParent.transform);
        
        // Create a plane or wireframe rectangle
        LineRenderer lr = surfaceObj.AddComponent<LineRenderer>();
        lr.material = CreateLineMaterial(surfaceColor);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.positionCount = 5; // 4 corners + back to start
        lr.loop = true;
        
        Vector3 center = anchor.transform.position;
        Vector3 right = anchor.transform.right;
        Vector3 forward = anchor.transform.forward;
        
        Rect rect = planeRect.Value;
        float width = rect.width;
        float depth = rect.height; // For horizontal surfaces, height becomes depth
        
        Vector3 bottomLeft = center + right * (rect.x - width * 0.5f) + forward * (rect.y - depth * 0.5f);
        Vector3 bottomRight = center + right * (rect.x + width * 0.5f) + forward * (rect.y - depth * 0.5f);
        Vector3 topRight = center + right * (rect.x + width * 0.5f) + forward * (rect.y + depth * 0.5f);
        Vector3 topLeft = center + right * (rect.x - width * 0.5f) + forward * (rect.y + depth * 0.5f);
        
        lr.SetPosition(0, bottomLeft);
        lr.SetPosition(1, bottomRight);
        lr.SetPosition(2, topRight);
        lr.SetPosition(3, topLeft);
        lr.SetPosition(4, bottomLeft);
        
        visualizationObjects.Add(surfaceObj);
    }
    
    /// <summary>
    /// Creates surface visualization from volume bounds
    /// </summary>
    private void CreateSurfaceFromBounds(MRUKAnchor anchor, Bounds bounds)
    {
        GameObject surfaceObj = new GameObject($"Surface_{anchor.name}");
        surfaceObj.transform.SetParent(visualizationParent.transform);
        
        LineRenderer lr = surfaceObj.AddComponent<LineRenderer>();
        lr.material = CreateLineMaterial(surfaceColor);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.positionCount = 5;
        lr.loop = true;
        
        Vector3 center = anchor.transform.TransformPoint(bounds.center);
        Vector3 size = bounds.size;
        
        float width = size.x;
        float depth = size.z;
        
        Vector3 right = anchor.transform.right;
        Vector3 forward = anchor.transform.forward;
        
        Vector3 bottomLeft = center - right * (width * 0.5f) - forward * (depth * 0.5f);
        Vector3 bottomRight = center + right * (width * 0.5f) - forward * (depth * 0.5f);
        Vector3 topRight = center + right * (width * 0.5f) + forward * (depth * 0.5f);
        Vector3 topLeft = center - right * (width * 0.5f) + forward * (depth * 0.5f);
        
        lr.SetPosition(0, bottomLeft);
        lr.SetPosition(1, bottomRight);
        lr.SetPosition(2, topRight);
        lr.SetPosition(3, topLeft);
        lr.SetPosition(4, bottomLeft);
        
        visualizationObjects.Add(surfaceObj);
    }
    
    /// <summary>
    /// Visualizes room bounds as a wireframe box
    /// </summary>
    private void VisualizeBounds()
    {
        if (currentRoom == null) return;
        
        // Calculate room bounds from all anchors
        Bounds roomBounds = new Bounds();
        bool boundsInitialized = false;
        
        foreach (var anchor in currentRoom.Anchors)
        {
            if (anchor == null) continue;
            
            Bounds anchorBounds = default(Bounds);
            bool hasBounds = false;
            
            // Try to get bounds from plane rect or volume bounds
            if (anchor.PlaneRect.HasValue)
            {
                Rect rect = anchor.PlaneRect.Value;
                Vector3 planeCenter = anchor.transform.position;
                Vector3 planeSize = new Vector3(rect.width, rect.height, 0.1f); // Small depth for plane
                anchorBounds = new Bounds(planeCenter, planeSize);
                hasBounds = true;
            }
            else if (anchor.VolumeBounds.HasValue)
            {
                Bounds localBounds = anchor.VolumeBounds.Value;
                Vector3 worldCenter = anchor.transform.TransformPoint(localBounds.center);
                Vector3 worldSize = anchor.transform.TransformVector(localBounds.size);
                anchorBounds = new Bounds(worldCenter, worldSize);
                hasBounds = true;
            }
            
            if (hasBounds)
            {
                if (!boundsInitialized)
                {
                    roomBounds = anchorBounds;
                    boundsInitialized = true;
                }
                else
                {
                    roomBounds.Encapsulate(anchorBounds);
                }
            }
        }
        
        if (!boundsInitialized) return;
        
        // Create wireframe box
        GameObject boundsObj = new GameObject("RoomBounds");
        boundsObj.transform.SetParent(visualizationParent.transform);
        
        LineRenderer lr = boundsObj.AddComponent<LineRenderer>();
        lr.material = CreateLineMaterial(boundsColor);
        lr.startWidth = lineWidth * 2f; // Slightly thicker for bounds
        lr.endWidth = lineWidth * 2f;
        lr.useWorldSpace = true;
        lr.positionCount = 16; // 8 corners, drawn twice (top and bottom rectangles + vertical lines)
        lr.loop = false;
        
        Vector3 center = roomBounds.center;
        Vector3 size = roomBounds.size;
        
        // Calculate 8 corners of the bounding box
        Vector3 min = center - size * 0.5f;
        Vector3 max = center + size * 0.5f;
        
        Vector3[] corners = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z), // 0: bottom-left-back
            new Vector3(max.x, min.y, min.z), // 1: bottom-right-back
            new Vector3(max.x, min.y, max.z), // 2: bottom-right-front
            new Vector3(min.x, min.y, max.z), // 3: bottom-left-front
            new Vector3(min.x, max.y, min.z), // 4: top-left-back
            new Vector3(max.x, max.y, min.z), // 5: top-right-back
            new Vector3(max.x, max.y, max.z), // 6: top-right-front
            new Vector3(min.x, max.y, max.z)  // 7: top-left-front
        };
        
        // Draw bottom rectangle
        lr.SetPosition(0, corners[0]);
        lr.SetPosition(1, corners[1]);
        lr.SetPosition(2, corners[2]);
        lr.SetPosition(3, corners[3]);
        lr.SetPosition(4, corners[0]);
        
        // Draw top rectangle
        lr.SetPosition(5, corners[4]);
        lr.SetPosition(6, corners[5]);
        lr.SetPosition(7, corners[6]);
        lr.SetPosition(8, corners[7]);
        lr.SetPosition(9, corners[4]);
        
        // Draw vertical lines
        lr.SetPosition(10, corners[0]);
        lr.SetPosition(11, corners[4]);
        lr.SetPosition(12, corners[1]);
        lr.SetPosition(13, corners[5]);
        lr.SetPosition(14, corners[2]);
        lr.SetPosition(15, corners[6]);
        
        visualizationObjects.Add(boundsObj);
    }
    
    /// <summary>
    /// Clears all visualization objects
    /// </summary>
    private void ClearVisualization()
    {
        foreach (var obj in visualizationObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        visualizationObjects.Clear();
    }
    
    /// <summary>
    /// Creates a material for line rendering
    /// OPTIMIZATION: Cache materials to avoid allocation on every update
    /// </summary>
    private Material CreateLineMaterial(Color color)
    {
        // Reuse cached material if it exists
        if (cachedLineMaterial == null)
        {
            cachedLineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (cachedLineMaterial == null)
            {
                cachedLineMaterial = new Material(Shader.Find("Unlit/Color"));
            }
        }
        
        // Update color if needed (materials are instanced per renderer anyway)
        if (cachedLineMaterial != null)
        {
            cachedLineMaterial.color = color;
        }
        return cachedLineMaterial;
    }
    
    /// <summary>
    /// Creates a material for solid objects
    /// OPTIMIZATION: Cache materials to avoid allocation on every update
    /// </summary>
    private Material CreateMaterial(Color color)
    {
        // Reuse cached material if it exists
        if (cachedMaterial == null)
        {
            cachedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (cachedMaterial == null)
            {
                cachedMaterial = new Material(Shader.Find("Standard"));
            }
        }
        
        // Update color if needed
        if (cachedMaterial != null)
        {
            cachedMaterial.color = color;
            if (cachedMaterial.HasProperty("_BaseColor"))
            {
                cachedMaterial.SetColor("_BaseColor", color);
            }
        }
        return cachedMaterial;
    }
    
    void OnDestroy()
    {
        ClearVisualization();
        if (visualizationParent != null)
        {
            Destroy(visualizationParent);
        }
    }
    
    /// <summary>
    /// Toggle visualization on/off
    /// </summary>
    [ContextMenu("Toggle Visualization")]
    public void ToggleVisualization()
    {
        showVisualization = !showVisualization;
        if (visualizationParent != null)
        {
            visualizationParent.SetActive(showVisualization);
        }
        if (showVisualization)
        {
            UpdateVisualization();
        }
    }
}

