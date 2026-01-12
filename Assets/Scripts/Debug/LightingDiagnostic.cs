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

    [Tooltip("Log every interval even if nothing changed (will grow the file fast).")]
    public bool forcePeriodicLog = false;

    float _nextSampleTime;

    // Previous state (so we only log on changes)
    string _prevEnv = "";
    string _prevLights = "";
    string _prevCamera = "";
    string _prevPipeline = "";
    string _prevPassthrough = "";

    void Start()
    {
        Log($"[LightingDiagnostics] Started. sampleInterval={sampleInterval}s forcePeriodicLog={forcePeriodicLog}");
        SampleAndLog(force: true);
    }

    void Update()
    {
        if (Time.time < _nextSampleTime) return;
        _nextSampleTime = Time.time + Mathf.Max(0.1f, sampleInterval);
        SampleAndLog(force: false);
    }

    void SampleAndLog(bool force)
    {
        string env = BuildEnvironmentState();
        string lights = BuildLightsState();
        string cam = BuildCameraState();
        string pipe = BuildPipelineState();
        string pt = BuildPassthroughState();

        bool envChanged = env != _prevEnv;
        bool lightsChanged = lights != _prevLights;
        bool camChanged = cam != _prevCamera;
        bool pipeChanged = pipe != _prevPipeline;
        bool ptChanged = pt != _prevPassthrough;

        bool shouldLog = force || forcePeriodicLog || envChanged || lightsChanged || camChanged || pipeChanged || ptChanged;
        if (!shouldLog) return;

        Log($"[LightingDiagnostics] ===== SAMPLE @ {Time.time:F2}s =====");

        if (force || envChanged) Log(env);
        if (force || lightsChanged) Log(lights);
        if (force || camChanged) Log(cam);
        if (force || pipeChanged) Log(pipe);
        if (force || ptChanged) Log(pt);

        _prevEnv = env;
        _prevLights = lights;
        _prevCamera = cam;
        _prevPipeline = pipe;
        _prevPassthrough = pt;
    }

    // ---------------------------
    // ENVIRONMENT (RenderSettings)
    // ---------------------------
    string BuildEnvironmentState()
    {
        string sunName = RenderSettings.sun ? RenderSettings.sun.name : "None";
        string skyName = RenderSettings.skybox ? RenderSettings.skybox.name : "None";
        string ambientMode = RenderSettings.ambientMode.ToString();

        return
            $"[ENV] sun={sunName} skyboxMat={skyName}\n" +
            $"[ENV] ambientMode={ambientMode} ambientIntensity={RenderSettings.ambientIntensity:F3} ambientLight={RenderSettings.ambientLight}\n" +
            $"[ENV] reflectionIntensity={RenderSettings.reflectionIntensity:F3} defaultReflectionMode={RenderSettings.defaultReflectionMode} reflectionBounces={RenderSettings.reflectionBounces}\n" +
            $"[ENV] fog={RenderSettings.fog} fogMode={RenderSettings.fogMode} fogColor={RenderSettings.fogColor} fogDensity={RenderSettings.fogDensity:F6} fogStart={RenderSettings.fogStartDistance:F2} fogEnd={RenderSettings.fogEndDistance:F2}";
    }

    // ---------------------------
    // LIGHTS (with hierarchy path)
    // ---------------------------
    string BuildLightsState()
    {
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

            var go = l.gameObject;
            var t = l.transform;

            sb.AppendLine(
                $"[LIGHT] id={go.GetInstanceID()} scene={go.scene.name} path={GetPath(t)}\n" +
                $"        name={l.name} type={l.type} intensity={l.intensity:F3} color={l.color} range={l.range:F2}\n" +
                $"        shadows={l.shadows} shadowStrength={l.shadowStrength:F2}\n" +
                $"        pos=({t.position.x:F2},{t.position.y:F2},{t.position.z:F2}) rot=({t.eulerAngles.x:F1},{t.eulerAngles.y:F1},{t.eulerAngles.z:F1}) layer={go.layer}"
            );
        }

        sb.Insert(0, $"[LIGHTS] enabledActive={enabledCount}\n");
        return sb.ToString();
    }

    // ---------------------------
    // CAMERA
    // ---------------------------
    string BuildCameraState()
    {
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

        return
            $"[CAM] name={cam.name} clearFlags={cam.clearFlags} bg={cam.backgroundColor} hdr={cam.allowHDR} msaa={cam.allowMSAA}\n" +
            $"[CAM] near={cam.nearClipPlane:F3} far={cam.farClipPlane:F1} cullingMask={cam.cullingMask} depth={cam.depth:F2}";
    }

    // ---------------------------
    // PIPELINE (URP best-effort)
    // ---------------------------
    string BuildPipelineState()
    {
        var rp = GraphicsSettings.currentRenderPipeline;
        if (rp == null)
            return "[PIPE] currentRenderPipeline=None (Built-in or not assigned)";

        string baseLine = $"[PIPE] currentRenderPipeline={rp.name} ({rp.GetType().Name})";

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        if (rp is UniversalRenderPipelineAsset urp)
        {
            return baseLine + "\n" + ReflectURP(urp);
        }
#endif
        return baseLine;
    }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
    string ReflectURP(UniversalRenderPipelineAsset urp)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        var t = urp.GetType();

        sb.Append($"[PIPE-URP] asset={urp.name}");

        // These names vary across URP versions; reflection avoids compile failures.
        string[] members =
        {
            "supportsHDR",
            "msaaSampleCount",
            "mainLightRenderingMode",
            "additionalLightsRenderingMode",
            "additionalLightsPerObjectLimit",
            "supportsMainLightShadows",
            "supportsAdditionalLightShadows",
            "shadowDistance",
            "supportsSoftShadows"
        };

        foreach (var name in members)
        {
            object val = GetMemberValue(urp, t, name);
            sb.Append($"\n[PIPE-URP] {name}={(val != null ? val.ToString() : "n/a")}");
        }

        return sb.ToString();
    }

    object GetMemberValue(object obj, System.Type type, string name)
    {
        var prop = type.GetProperty(name);
        if (prop != null)
        {
            try { return prop.GetValue(obj); } catch { return null; }
        }

        var field = type.GetField(name);
        if (field != null)
        {
            try { return field.GetValue(obj); } catch { return null; }
        }

        return null;
    }
#endif

    // ---------------------------
    // PASSTHROUGH (reflect OVRPassthroughLayer safely)
    // ---------------------------
    string BuildPassthroughState()
    {
        var behaviours = Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var b in behaviours)
        {
            if (b == null) continue;

            // Avoid direct type dependency so this compiles even if OVR changes namespaces
            if (b.GetType().Name != "OVRPassthroughLayer") continue;

            var t = b.GetType();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append($"[PT] found on={b.gameObject.name} enabled={b.enabled}");

            string[] members =
            {
                "overlayType",
                "textureOpacity",
                "edgeRenderingEnabled",
                "colorMapEditorType",
                "brightness",
                "contrast",
                "saturation",
                "sharpness"
            };

            foreach (var name in members)
            {
                object val = GetAnyMemberValue(b, t, name);
                if (val != null)
                    sb.Append($"\n[PT] {name}={val}");
            }

            return sb.ToString();
        }

        return "[PT] OVRPassthroughLayer not found in scene.";
    }

    object GetAnyMemberValue(object obj, System.Type type, string name)
    {
        var prop = type.GetProperty(name);
        if (prop != null)
        {
            try { return prop.GetValue(obj); } catch { return null; }
        }

        var field = type.GetField(name);
        if (field != null)
        {
            try { return field.GetValue(obj); } catch { return null; }
        }

        return null;
    }

    // ---------------------------
    // UTIL
    // ---------------------------
    static string GetPath(Transform t)
    {
        if (t == null) return "<null>";
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    void Log(string msg)
    {
        // Writes directly to your file if FileLogger is present + set up
        if (FileLogger.Instance != null)
        {
            FileLogger.Instance.LogExternal(msg);
        }
        else
        {
            Debug.Log(msg);
        }
    }
}
