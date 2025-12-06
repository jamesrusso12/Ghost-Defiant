using UnityEngine;

/// <summary>
/// Creates a ghostly trail effect that follows behind the ghost.
/// Uses Trail Renderer for smooth, ethereal effect.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class GhostTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("Material for the trail (should be semi-transparent ghostly material)")]
    public Material trailMaterial;
    
    [Tooltip("Width of the trail at the start")]
    public float startWidth = 0.5f;
    
    [Tooltip("Width of the trail at the end")]
    public float endWidth = 0.1f;
    
    [Tooltip("How long the trail persists (seconds)")]
    public float time = 2f;
    
    [Tooltip("Color gradient for the trail")]
    public Gradient colorGradient;
    
    [Tooltip("Minimum distance before trail adds a new point")]
    public float minVertexDistance = 0.1f;
    
    [Header("Ghostly Effect")]
    [Tooltip("Enable automatic material setup with ghostly properties")]
    public bool autoSetupMaterial = true;
    
    [Tooltip("Base color for the ghost trail")]
    public Color trailColor = new Color(1f, 1f, 1f, 0.5f);
    
    private TrailRenderer trailRenderer;
    
    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        SetupTrail();
    }
    
    void SetupTrail()
    {
        if (trailRenderer == null) return;
        
        // Configure trail renderer
        trailRenderer.time = time;
        trailRenderer.startWidth = startWidth;
        trailRenderer.endWidth = endWidth;
        trailRenderer.minVertexDistance = minVertexDistance;
        trailRenderer.autodestruct = false;
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
        
        // Setup material
        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }
        else if (autoSetupMaterial)
        {
            CreateDefaultMaterial();
        }
        
        // Setup color gradient
        if (colorGradient != null && colorGradient.alphaKeys.Length > 0)
        {
            trailRenderer.colorGradient = colorGradient;
        }
        else
        {
            CreateDefaultGradient();
        }
        
        // Set rendering order to appear behind ghost
        trailRenderer.sortingOrder = -1;
    }
    
    void CreateDefaultMaterial()
    {
        // Create a simple unlit material for the trail
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.color = trailColor;
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        
        trailRenderer.material = mat;
    }
    
    void CreateDefaultGradient()
    {
        // Create a gradient that fades from visible to transparent
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(trailColor, 0.0f), 
                new GradientColorKey(trailColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(trailColor.a, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trailRenderer.colorGradient = gradient;
    }
    
    /// <summary>
    /// Enable or disable the trail
    /// </summary>
    public void SetTrailEnabled(bool enabled)
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = enabled;
            if (!enabled)
            {
                trailRenderer.Clear();
            }
        }
    }
    
    /// <summary>
    /// Clear the trail immediately
    /// </summary>
    public void ClearTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
}

