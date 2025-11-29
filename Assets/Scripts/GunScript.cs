using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunScript : MonoBehaviour
{
    public LayerMask layerMask;
    public OVRInput.RawButton shootingButton;
    public LineRenderer linePrefab;
    public GameObject rayImpactPrefab;
    public Transform shootingPoint;
    public float maxLineDistance = 5;
    public float lineShowTimer = 0.3f;
    public AudioSource source;
    public AudioClip shootingAudioClip;

    public UnityEvent OnShoot;
    public UnityEvent<GameObject> OnShootAndHit;
    public UnityEvent OnShootAndMiss;

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(shootingButton))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Debug.Log("[GunScript] ===== SHOOT TRIGGERED =====");
        OnShoot.Invoke();

        // Play audio with proper null check
        if (source != null && shootingAudioClip != null)
        {
            source.PlayOneShot(shootingAudioClip);
        }

        if (shootingPoint == null)
        {
            Debug.LogError("[GunScript] ShootingPoint is NULL! Cannot shoot.");
            return;
        }

        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        Debug.Log($"[GunScript] Raycast: From {shootingPoint.position}, Direction {shootingPoint.forward}, MaxDistance {maxLineDistance}, LayerMask {layerMask.value} (binary: {System.Convert.ToString(layerMask.value, 2)})");
        
        // Draw debug ray in scene view
        Debug.DrawRay(shootingPoint.position, shootingPoint.forward * maxLineDistance, Color.red, 2f);
        
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, maxLineDistance, layerMask);

        Vector3 endPoint = Vector3.zero;

        if (hasHit)
        {
            endPoint = hit.point;
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log($"[GunScript] ✓✓✓ HIT DETECTED ✓✓✓");
            Debug.Log($"[GunScript] Hit Object: {hitObject.name}");
            Debug.Log($"[GunScript] Hit Layer: {hitObject.layer} ({LayerMask.LayerToName(hitObject.layer)})");
            Debug.Log($"[GunScript] Hit Point: {hit.point}");
            Debug.Log($"[GunScript] Hit Collider: {hit.collider.GetType().Name} (Enabled: {hit.collider.enabled})");
            Debug.Log($"[GunScript] Has MeshCollider: {hit.collider is MeshCollider}");
            if (hit.collider is MeshCollider)
            {
                MeshCollider mc = hit.collider as MeshCollider;
                Debug.Log($"[GunScript] MeshCollider Convex: {mc.convex}, Has Mesh: {mc.sharedMesh != null}");
            }
            
            Debug.Log($"[GunScript] Invoking OnShootAndHit event with: {hitObject.name}");
            Debug.Log($"[GunScript] OnShootAndHit listener count: {OnShootAndHit.GetPersistentEventCount()}");
            OnShootAndHit.Invoke(hitObject);
            Debug.Log($"[GunScript] OnShootAndHit event invoked!");

            // Check for ghost component more efficiently
            Ghost ghost = hit.collider.GetComponent<Ghost>();
            if (ghost == null)
            {
                ghost = hit.collider.GetComponentInParent<Ghost>();
            }

            if (ghost != null)
            {
                ghost.Kill();
            }
            else
            {
                // Create impact effect with object pooling if available
                CreateImpactEffect(hit.point, hit.normal);
            }
        }
        else
        {
            endPoint = shootingPoint.position + shootingPoint.forward * maxLineDistance;
            Debug.Log($"[GunScript] No hit detected. Ray from {shootingPoint.position} direction {shootingPoint.forward}, distance {maxLineDistance}, layerMask: {layerMask.value}");
            
            // Try a raycast without layer mask to see if there's anything in the way
            if (Physics.Raycast(ray, out RaycastHit debugHit, maxLineDistance))
            {
                Debug.LogWarning($"[GunScript] Found object {debugHit.collider.gameObject.name} on layer {debugHit.collider.gameObject.layer} but it's not in layerMask!");
            }
            
            OnShootAndMiss.Invoke();
        }

        // Create laser line with object pooling if available
        CreateLaserLine(shootingPoint.position, endPoint);
    }

    private void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        if (rayImpactPrefab != null)
        {
            Quaternion rayImpactRotation = Quaternion.LookRotation(-normal);
            GameObject rayimpact = Instantiate(rayImpactPrefab, position, rayImpactRotation);
            
            // Keep on Default layer for visibility
            rayimpact.layer = LayerMask.NameToLayer("Default");
            
            Destroy(rayimpact, 1f);
        }
    }

    private void CreateLaserLine(Vector3 start, Vector3 end)
    {
        if (linePrefab != null)
        {
            LineRenderer line = Instantiate(linePrefab);
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            
            // Keep on Default layer (as it was working before)
            line.gameObject.layer = LayerMask.NameToLayer("Default");
            
            // Set sorting order to ensure it renders on top (even within same layer)
            line.sortingOrder = 32767; // Maximum sorting order value
            
            // CRITICAL FIX: Ensure the material renders in front by adjusting render queue and depth settings
            if (line.material != null)
            {
                // Create a material instance to avoid modifying the prefab material
                Material lineMaterial = new Material(line.material);
                
                // Use Overlay render queue (3000+) to render after ALL geometry including walls
                lineMaterial.renderQueue = 4000; // High value ensures it renders last/on top
                
                // Disable depth writing so it doesn't affect depth buffer but still renders
                if (lineMaterial.HasProperty("_ZWrite"))
                {
                    lineMaterial.SetFloat("_ZWrite", 0f);
                }
                
                // CRITICAL: Set ZTest to Always - this makes it always render regardless of depth
                // This is what makes it visible even when walls are in front
                if (lineMaterial.HasProperty("_ZTest"))
                {
                    // ZTest Always = 8 in Unity's CompareFunction enum
                    lineMaterial.SetInt("_ZTest", 8);
                }
                
                // Alternative property name some shaders use
                if (lineMaterial.HasProperty("_ZTestMode"))
                {
                    lineMaterial.SetInt("_ZTestMode", 8);
                }
                
                line.material = lineMaterial;
            }
            else
            {
                Debug.LogWarning("[GunScript] Laser line has no material! Cannot set render settings.");
            }
            
            // Also set the width to ensure it's visible
            if (line.widthMultiplier <= 0.01f)
            {
                line.widthMultiplier = 0.02f; // Ensure minimum visible width
            }
            
            Debug.Log($"[GunScript] Created laser line from {start} to {end}, Layer: {line.gameObject.layer} ({LayerMask.LayerToName(line.gameObject.layer)}), SortingOrder: {line.sortingOrder}, RenderQueue: {(line.material != null ? line.material.renderQueue : -1)}");
            
            Destroy(line.gameObject, lineShowTimer);
        }
    }
}
