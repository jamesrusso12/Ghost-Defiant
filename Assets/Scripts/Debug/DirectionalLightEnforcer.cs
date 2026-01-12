using UnityEngine;
using System.Collections.Generic;

public class DirectionalLightEnforcer : MonoBehaviour
{
    [Tooltip("Assign YOUR real directional light here")]
    public Light keepThisDirectional;

    [Tooltip("How often to purge extra directional lights (seconds)")]
    public float interval = 0.25f;

    private float nextTime;

    void Update()
    {
        if (Time.time < nextTime) return;
        nextTime = Time.time + interval;

        var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int removed = 0;

        foreach (var l in lights)
        {
            if (l == null) continue;
            if (l.type != LightType.Directional) continue;
            if (keepThisDirectional != null && l == keepThisDirectional) continue;

            // Disable instead of destroy (safer for debugging)
            if (l.enabled)
            {
                l.enabled = false;
                removed++;
            }
        }

        if (removed > 0)
            Debug.Log($"[LightingDiagnostics] [ENFORCER] Disabled {removed} extra Directional Lights.");
    }
}
