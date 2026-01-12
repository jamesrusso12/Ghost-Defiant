using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

public class LightingDiagnostics : MonoBehaviour
{
    [Header("Logging")]
    [Tooltip("How often to sample and log state (seconds).")]
    public float sampleInterval = 0.5f;

    [Tooltip("Also log continuously even if no changes are detected.")]
    public bool forcePeriodicLog = false;

    [Header("Change thresholds")]
    public float lightIntensityDelta = 0.05f;
    public float ambientIntensityDelta = 0.05f;
    public float reflectionIntensityDelta = 0.05f;
    public float fogDensityDelta = 0.0005f;

    float _nextSampleTime;

    // Previous state (so we only log on changes)
    string _prevEnvHash = "";
    string _prevLightsHash = "";
    string _prevCameraHash = "";
    string _prevPipelineHash = "";
    string _prevPassthroughHash = "";

    void Start()
    {
        Log($"[LightingDiagnostics] Started. sampleInterval={sampleInterval}s forcePeriodicLog={forcePeriodicLog}");
        SampleAndLog(true);
    }

    void Update()
    {
        if (Time.time < _nextSampleTime) return;
        _nextSampleTime = Time.time + Mathf.Max(0.1f, sampleInterval);
        SampleAndLog(false);
    }

    void SampleAndLog(bool force)
    {
        // Environment
        string env = BuildEnvironmentState();
        bool envChanged = env != _prevEnvHash;

        // Lights
        string lights = BuildLightsState();
        bool lightsChanged = lights != _prevLightsHash;

        // Camera
        string cam = BuildCameraState();
        bool camChanged = cam != _prevCameraHash;

        // URP pipeline asset (if available)
        string pipe = BuildPipelineState();
        bool pipeChanged = pipe != _prevPipelineHash;

        // Passthrough layer (if present)
        string pt = BuildPassthroughState();
        bool ptChanged = pt != _prevPassthroughHash;

        bool shouldLog = force || forcePeriodicLog || envChanged || lightsChanged || camChanged || pipeChanged || ptChanged;

        if (!shouldLog) return;

        Log($"[LightingDiagnostics] ===== SAMPLE @ {Time.time:F2}s =====");
        if (envChanged || force) Log(env);
        if (lightsChanged || force) Log(lights);
        if (camChanged || force) Log(cam);
        if (pipeChanged || force) Log(pipe);
        if (ptChanged || force) Log(pt);

        _prevEnvHash = env;
        _prevLightsHash = lights;
        _prevCameraHash = cam;
        _prevPipelineHash = pipe;
        _prevPassthroughHash = pt;
    }

    string BuildEnvironmentState()
    {
        var sunName = RenderSettings.sun ? RenderSettings.sun.name : "None";
        var skyName = RenderSettings.skybox ? RenderSettings.skybox.name : "None";

        // ambientMode is in UnityEngine.Rendering
        var ambientMode = RenderSettings.ambientMode.ToString();

        string s =
            $"[ENV] sun={sunName} skyboxMat={skyName}\n" +
            $"[ENV] ambientMode={ambientMode} ambientIntensity={RenderSettings.ambientIntensity:F3} ambientLight={RenderSettings.ambientLight}\n" +
            $"[ENV] reflectionIntensity={RenderSettings.reflectionIntensity:F3} defaultReflectionMode={RenderSettings.defaultReflectionMode} reflectionBounces={RenderSettings.reflectionBounces}\n" +
            $"[ENV] fog={RenderSettings.fog} fogMode={RenderSettings.fogMode} fogColor={RenderSettings.fogColor} fogDensity={RenderSettings.fogDensity:F6} fogStart={RenderSettings.fogStartDistance:F2} fogEnd={RenderSettings.fogEndDistance:F2}";

        return s;
    }

    string BuildLightsState()
    {
        // Find all active enabled lights
        var lights = Object.FindObjectsByType<Light>(
    FindObjectsInactive.Include,
    FindObjectsSortMode.None
);


        int enabledCount = 0;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"[LIGHTS] totalInScene={lights.Length}");

        foreach (var l in lights)
        {
            if (l == null) continue;
            if (!l.enabled || !l.gameObject.activeInHierarchy) continue;

            enabledCount++;

            var t = l.transform;
            sb.AppendLine(
                $"[LIGHT] name={l.name} type={l.type} intensity={l.intensity:F3} color={l.color} range={l.range:F2} " +
                $"shadows={l.shadows} shadowStrength={l.shadowStrength:F2} " +
                $"pos=({t.position.x:F2},{t.position.y:F2},{t.position.z:F2}) rot=({t.eulerAngles.x:F1},{t.eulerAngles.y:F1},{t.eulerAngles.z:F1}) layer={l.gameObject.layer}"
            );
        }

        sb.Insert(0, $"[LIGHTS] enabledActive={enabledCount}\n");
        return sb.ToString();
    }

    string BuildCameraState()
    {
        // Try to find the main camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            var cams = Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            if (cams != null && cams.Length > 0) cam = cams[0];
        }

        if (cam == null) return "[CAM] No camera found.";

        string s =
            $"[CAM] name={cam.name} clearFlags={cam.clearFlags} bg={cam.backgroundColor} hdr={cam.allowHDR} msaa={cam.allowMSAA}\n" +
            $"[CAM] near={cam.nearClipPlane:F3} far={cam.farClipPlane:F1} cullingMask={cam.cullingMask} depth={cam.depth:F2}";

        return s;
    }

    string BuildPipelineState()
    {
        var rp = GraphicsSettings.currentRenderPipeline;
        if (rp == null)
            return "[PIPE] currentRenderPipeline=None (Built-in or not assigned)";

        string s = $"[PIPE] currentRenderPipeline={rp.name} ({rp.GetType().Name})";

        // Try to read URP-specific settings if present
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        if (rp is UniversalRenderPipelineAsset urp)
        {
            // These property names exist in many URP versions, but Unity sometimes changes APIs.
            // We’ll log what we can without crashing.
            s += "\n[PIPE-URP] (If some fields show as 'n/a', that’s just API differences.)";

            // Use reflection to avoid compile errors across URP versions
            s += "\n" + ReflectURP(urp);
        }
#endif
        return s;
    }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
    string ReflectURP(UniversalRenderPipelineAsset urp)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        var t = urp.GetType();

        sb.Append($"[PIPE-URP] asset={urp.name}");

        string[] fieldsToTry =
        {
            "supportsHDR",
            "msaaSampleCount",
            "mainLightRenderingMode",
            "additionalLightsRenderingMode",
            "additionalLightsPerObjectLimit",
            "supportsMainLightShadows",
            "supportsAdditionalLightShadows",
            "shadowDistance"
        };

        foreach (var f in fieldsToTry)
        {
            var prop = t.GetProperty(f);
            if (prop != null)
            {
                object val = null;
                try { val = prop.GetValue(urp); } catch {}
                sb.Append($"\n[PIPE-URP] {f}={(val != null ? val.ToString() : "n/a")}");
                continue;
            }

            var field = t.GetField(f);
            if (field != null)
            {
                object val = null;
                try { val = field.GetValue(urp); } catch {}
                sb.Append($"\n[PIPE-URP] {f}={(val != null ? val.ToString() : "n/a")}");
            }
        }

        return sb.ToString();
    }
#endif

    string BuildPassthroughState()
    {
        // OVRPassthroughLayer is in Oculus/Meta Integration; we can’t reference the type safely without the package,
        // so we’ll detect it by component name + reflect its common fields.
        var allBehaviours = Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var b in allBehaviours)
        {
            if (b == null) continue;
            var typeName = b.GetType().Name;
            if (typeName != "OVRPassthroughLayer") continue;

            var t = b.GetType();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append($"[PT] found on={b.gameObject.name} enabled={b.enabled}");

            // Common fields/properties to reflect
            string[] names =
            {
                "overlayType",
                "textureOpacity",
                "edgeRenderingEnabled",
                "colorMapEditorType",
                "brightness",
                "contrast",
                "saturation"
            };

            foreach (var n in names)
            {
                var prop = t.GetProperty(n);
                if (prop != null)
                {
                    object val = null;
                    try { val = prop.GetValue(b); } catch { }
                    sb.Append($"\n[PT] {n}={(val != null ? val.ToString() : "n/a")}");
                    continue;
                }

                var field = t.GetField(n);
                if (field != null)
                {
                    object val = null;
                    try { val = field.GetValue(b); } catch { }
                    sb.Append($"\n[PT] {n}={(val != null ? val.ToString() : "n/a")}");
                }
            }

            return sb.ToString();
        }

        return "[PT] OVRPassthroughLayer not found in scene.";
    }

    void Log(string msg)
    {
        if (FileLogger.Instance != null)
        {
            FileLogger.Instance.LogExternal(msg);
        }
        else
        {
            // Fallback if FileLogger wasn't initialized yet
            Debug.Log(msg);
        }
    }
}
