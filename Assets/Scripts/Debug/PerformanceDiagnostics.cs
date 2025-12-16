using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceDiagnostics : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode triggerKey = KeyCode.P;
    public bool checkOnStart = true;
    
    [Header("Targets (Optional)")]
    public GameObject gunObject;
    public GameObject flashlightObject;

    void Start()
    {
        if (checkOnStart)
        {
            Invoke(nameof(RunDiagnostics), 1.0f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            RunDiagnostics();
        }
    }

    [ContextMenu("Run Diagnostics")]
    public void RunDiagnostics()
    {
        Debug.Log("========== PERFORMANCE DIAGNOSTICS ==========");
        
        // Check Gun
        if (gunObject != null)
        {
            AnalyzeObject("Gun", gunObject);
        }
        else
        {
            GameObject gun = GameObject.Find("Specter_Blaster"); // Guessing name
            if (gun == null) gun = GameObject.FindFirstObjectByType<GunScript>()?.gameObject;
            
            if (gun != null) AnalyzeObject("Gun (Found)", gun);
        }

        // Check Flashlight
        if (flashlightObject != null)
        {
            AnalyzeObject("Flashlight", flashlightObject);
        }
        else
        {
            // Try to find flashlight
            GameObject light = GameObject.Find("Flashlight");
            if (light != null) AnalyzeObject("Flashlight (Found)", light);
        }

        Debug.Log($"Total Allocated Memory: {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024} MB");
        Debug.Log($"Total Reserved Memory: {Profiler.GetTotalReservedMemoryLong() / 1024 / 1024} MB");
        Debug.Log("===========================================");
    }

    void AnalyzeObject(string label, GameObject obj)
    {
        Debug.Log($"--- Analyzing {label}: {obj.name} ---");
        
        // Check Meshes
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>(true);
        SkinnedMeshRenderer[] skinners = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        int totalVerts = 0;
        int totalTris = 0;
        
        foreach (var mf in filters)
        {
            if (mf.sharedMesh != null)
            {
                totalVerts += mf.sharedMesh.vertexCount;
                totalTris += mf.sharedMesh.triangles.Length / 3;
                if (mf.sharedMesh.vertexCount > 10000)
                {
                    Debug.LogWarning($"[High Poly] {mf.name} has {mf.sharedMesh.vertexCount} verts!");
                }
            }
        }
        
        foreach (var smr in skinners)
        {
            if (smr.sharedMesh != null)
            {
                totalVerts += smr.sharedMesh.vertexCount;
                totalTris += smr.sharedMesh.triangles.Length / 3;
                if (smr.sharedMesh.vertexCount > 10000)
                {
                    Debug.LogWarning($"[High Poly] {smr.name} has {smr.sharedMesh.vertexCount} verts!");
                }
            }
        }
        
        Debug.Log($"{label} Total: {totalVerts} Vertices, {totalTris} Triangles");
        
        if (totalVerts > 50000)
        {
            Debug.LogError($"{label} IS EXTREMELY HIGH POLY! This is likely causing lag.");
        }
        else if (totalVerts > 10000)
        {
            Debug.LogWarning($"{label} is somewhat high poly for VR/MR.");
        }
        else
        {
            Debug.Log($"{label} poly count seems reasonable.");
        }

        // Check Textures (Renderer materials)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null)
                {
                    if (mat.mainTexture != null)
                    {
                        Debug.Log($"Material {mat.name} uses texture: {mat.mainTexture.name} ({mat.mainTexture.width}x{mat.mainTexture.height})");
                        if (mat.mainTexture.width > 2048)
                        {
                            Debug.LogWarning($"[Large Texture] {mat.mainTexture.name} is {mat.mainTexture.width}x{mat.mainTexture.height}!");
                        }
                    }
                }
            }
        }
    }
}
