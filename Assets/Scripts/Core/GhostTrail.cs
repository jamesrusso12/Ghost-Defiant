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
    public float startWidth = 0.3f;
    
    [Tooltip("Width of the trail at the end")]
    public float endWidth = 0.05f;
    
    [Tooltip("How long the trail persists (seconds)")]
    public float time = 1.5f;
    
    [Tooltip("Color gradient for the trail")]
    public Gradient colorGradient;
    
    [Tooltip("Minimum distance before trail adds a new point")]
    public float minVertexDistance = 0.05f;
    
    [Header("Ghostly Effect")]
    [Tooltip("Enable automatic material setup with ghostly properties")]
    public bool autoSetupMaterial = true;
    
    [Tooltip("Base color for the ghost trail")]
    public Color trailColor = new Color(0.4f, 0.9f, 1f, 0.3f);
    
    [Tooltip("Use smooth tapering for better appearance")]
    public bool useSmoothTaper = true;
    
    [Tooltip("Use additive blending for more ethereal/ghostly effect")]
    public bool useAdditiveBlending = true;
    
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
        
        // Smooth tapering for better appearance
        if (useSmoothTaper)
        {
            AnimationCurve widthCurve = new AnimationCurve();
            widthCurve.AddKey(0.0f, 1.0f);  // Full width at start
            widthCurve.AddKey(0.7f, 0.4f);  // Taper down
            widthCurve.AddKey(1.0f, 0.0f);  // Zero at end
            
            // Smooth the curve
            for (int i = 0; i < widthCurve.keys.Length; i++)
            {
                widthCurve.SmoothTangents(i, 0.5f);
            }
            
            trailRenderer.widthCurve = widthCurve;
        }
        
        // Better quality settings
        trailRenderer.numCornerVertices = 5;
        trailRenderer.numCapVertices = 5;
        trailRenderer.alignment = LineAlignment.View;
        trailRenderer.textureMode = LineTextureMode.Stretch;
        
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
        // Try URP/Lit shader for better quality
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }
        
        Material mat = new Material(shader);
        
        // Configure transparency with additive blending for ghostly effect
        mat.SetFloat("_Surface", 1); // Transparent
        
        if (useAdditiveBlending)
        {
            // Additive blending - more ethereal/ghostly
            mat.SetFloat("_Blend", 1); // Additive
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }
        else
        {
            // Normal alpha blending
            mat.SetFloat("_Blend", 0); // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        
        mat.SetInt("_ZWrite", 0);
        mat.SetFloat("_AlphaClip", 0);
        mat.renderQueue = 3000;
        
        // Set colors - make more transparent
        mat.SetColor("_BaseColor", trailColor);
        mat.SetColor("_Color", trailColor);
        
        // Add glow for ethereal effect
        Color emissionColor = new Color(
            trailColor.r * 1.5f,
            trailColor.g * 1.5f,
            trailColor.b * 1.5f
        );
        mat.SetColor("_EmissionColor", emissionColor);
        mat.EnableKeyword("_EMISSION");
        
        // Smooth and less metallic for softer look
        mat.SetFloat("_Smoothness", 0.3f);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_ReceiveShadows", 0);
        
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

