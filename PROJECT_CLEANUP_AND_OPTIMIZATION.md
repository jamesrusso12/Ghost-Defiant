# Project Cleanup & Optimization Guide

## 📋 Summary

This document outlines files to remove and optimizations to apply for better performance and cleaner codebase.

---

## 🗑️ Files to Delete

### 1. Redundant Documentation Files (Keep Only SPECTER_AMMO_FIX_README.md)

**Delete these:**
- `BULLET_NOT_WORKING_FIX.md` - Old troubleshooting doc, info now in SPECTER_AMMO_FIX_README.md
- `CRITICAL_FIX_APPLIED.md` - Temporary fix doc, no longer needed
- `SIMPLE_BULLET_SETUP.md` - SimpleGunScript not being used anymore
- `HOW_TO_GET_LOGS.md` - Duplicate of HOW_TO_GET_LOGS_DETAILED.md

**Keep:**
- `SPECTER_AMMO_FIX_README.md` - Complete bullet fix documentation
- `HOW_TO_GET_LOGS_DETAILED.md` - Comprehensive log retrieval guide
- `GET_LOGS.bat` - Useful utility script

### 2. Unused Scripts (SimpleGunScript & SimpleBullet)

**Delete:**
- `Assets/Scripts/Combat/SimpleGunScript.cs` - Not being used, GunScript works now
- `Assets/Scripts/Combat/SimpleBullet.cs` - Not being used, GunScript works now

**Reason:** These were created during troubleshooting but GunScript is now fixed and working properly.

### 3. Unused Scripts Folder (Review Before Deleting)

**Location:** `Assets/Scripts/Unused/`

**Scripts in folder:**
- Billboard.cs
- BillboardMenu.cs
- ControllerDebug.cs
- CustomSceneManager.cs
- EmergencyMetaSDKFix.cs
- ItemSpawner.cs
- LazyFollow.cs
- MetaSDKComponentFixer.cs
- MetaSDKFix.cs
- MetaSDKMaterialFixer.cs
- MetaXRProjectFixer.cs
- PlayAudioFromAudioManager.cs
- ProjectCleanup.cs
- SimpleCursor.cs
- SimpleMRUKManager.cs
- SimpleWristMenu.cs
- StartMenuPanelPlacement.cs
- UIAudio.cs
- WristMenu.cs

**Action:** Review each script - if not used in any scene/prefab, delete the entire `Unused` folder.

---

## ⚡ Performance Optimizations

### 1. AudioManager - Use Dictionary Instead of Array.Find

**Current Issue:** `Array.Find()` is O(n) - slow for many sounds.

**File:** `Assets/Scripts/Audio/AudioManager.cs`

**Fix:** Replace Array.Find with Dictionary lookup (O(1)).

```csharp
// Add at top of class:
private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

// In Awake(), after creating sounds:
soundDictionary.Clear();
foreach (Sound s in sounds)
{
    if (s != null && !string.IsNullOrEmpty(s.name))
    {
        soundDictionary[s.name] = s;
    }
}

// Replace Play() method:
public void Play(string name)
{
    if (soundDictionary.TryGetValue(name, out Sound s))
    {
        s.source.Play();
    }
    else
    {
        Debug.LogWarning("Sound: " + name + " not found");
    }
}

// Replace Stop() method:
public void Stop(string name)
{
    if (soundDictionary.TryGetValue(name, out Sound s))
    {
        s.source.Stop();
    }
}
```

**Performance Gain:** ~10-100x faster sound lookup depending on number of sounds.

---

### 2. GunScript - Cache Skybox Reference

**Current Issue:** `GameObject.Find()` called every time a bullet spawns - expensive!

**File:** `Assets/Scripts/Combat/GunScript.cs` (ProjectileController.SetupCollisionIgnores)

**Fix:** Cache skybox reference at class level, find once.

```csharp
// Add at top of ProjectileController class:
private static GameObject cachedSkybox = null;

// In SetupCollisionIgnores(), replace skybox finding:
if (cachedSkybox == null)
{
    cachedSkybox = GameObject.Find("NightSkybox");
    if (cachedSkybox == null) cachedSkybox = GameObject.Find("Skybox");
    if (cachedSkybox == null) cachedSkybox = GameObject.Find("Sky");
}

if (cachedSkybox != null)
{
    Collider[] skyboxColliders = cachedSkybox.GetComponentsInChildren<Collider>();
    // ... rest of code
}
```

**Performance Gain:** Eliminates expensive Find() call on every bullet spawn.

---

### 3. DestructibleGlobalMeshManager - Optimize Segment Finding

**Current Issue:** Linear search through segments list on every hit.

**File:** `Assets/Scripts/MR_XR/DestructibleGlobalMeshManager.cs`

**Fix:** Use Dictionary for O(1) segment lookup.

```csharp
// Add at top of class:
private Dictionary<GameObject, GameObject> segmentLookup = new Dictionary<GameObject, GameObject>();

// In SetupDestructibleComponents(), after populating segments:
segmentLookup.Clear();
foreach (var seg in segments)
{
    if (seg != null)
    {
        segmentLookup[seg] = seg;
        // Also add children for fast lookup
        foreach (Transform child in seg.transform)
        {
            segmentLookup[child.gameObject] = seg;
        }
    }
}

// In DestroyMeshSegment(), replace search logic:
GameObject segmentToDestroy = null;

// Fast dictionary lookup
if (segmentLookup.TryGetValue(segment, out segmentToDestroy))
{
    // Found it!
}
else
{
    // Fallback to parent hierarchy check (existing code)
    Transform current = segment.transform;
    while (current != null && segmentToDestroy == null)
    {
        if (segmentLookup.TryGetValue(current.gameObject, out segmentToDestroy))
            break;
        current = current.parent;
    }
}
```

**Performance Gain:** O(1) lookup instead of O(n) - critical for many segments.

---

### 4. PerformanceMonitor - Optimize Update Loop

**Current Issue:** Update() runs every frame, but only updates UI every 0.5s.

**File:** `Assets/Scripts/Systems/PerformanceMonitor.cs`

**Fix:** Use Coroutine instead of Update().

```csharp
// Replace Update() with:
void Start()
{
    timeleft = updateInterval;
    StartCoroutine(UpdatePerformanceStats());
}

IEnumerator UpdatePerformanceStats()
{
    while (true)
    {
        yield return new WaitForSeconds(updateInterval);
        
        fps = accum / frames;
        accum = 0.0f;
        frames = 0;
        
        UpdateUI();
    }
}

// Remove Update() method entirely
```

**Performance Gain:** Eliminates Update() overhead - only runs when needed.

---

### 5. Ghost Script - Optimize Player Detection

**Current Issue:** Likely using expensive operations in Update() for player detection.

**File:** `Assets/Scripts/Core/Ghost.cs`

**Recommendation:** Review Ghost.cs Update() method:
- Cache player reference instead of Find() every frame
- Use distance checks before expensive operations
- Consider using Physics.OverlapSphere instead of raycasts for detection

---

## 📊 Build Size Optimizations

### 1. Remove Unused Assets

**Check these folders for unused assets:**
- `Assets/Prefabs/` - Check if all prefabs are used in scenes
- `Assets/Materials/` - Remove unused materials
- `Assets/Textures/` - Remove unused textures
- `Assets/Audio/` - Remove unused audio clips

**How to check:**
1. In Unity: Window → Asset Usage → Find References in Scene
2. Or use: Assets → Find References in Scene

### 2. Compress Textures

**Action:** Select textures → Inspector → Compression → ASTC 4x4 block (for Quest)

**Saves:** Significant build size reduction.

### 3. Remove Debug Scripts from Builds

**File:** `Assets/Scripts/Combat/FileLogger.cs`

**Fix:** Add `#if UNITY_EDITOR || DEVELOPMENT_BUILD` around FileLogger code, or disable in builds.

---

## 🔧 Code Quality Improvements

### 1. Remove Magic Numbers

**Example:** Replace hardcoded values with named constants.

```csharp
// Instead of:
if (distance < 8f)

// Use:
private const float HIDE_DISTANCE = 8f;
if (distance < HIDE_DISTANCE)
```

### 2. Add Null Checks

**Review all scripts for missing null checks**, especially:
- GunScript.cs - Check shootingPoint before use
- DestructibleGlobalMeshManager.cs - Check currentComponent before use
- Ghost.cs - Check player reference before use

### 3. Use Object Pooling for Bullets

**Current:** Instantiate/Destroy bullets on every shot (GC pressure)

**Fix:** Implement object pooling for bullets to reduce GC allocations.

**Performance Gain:** Eliminates GC spikes, smoother frame rate.

---

## 📝 Recommended Action Order

1. **Quick Wins (Do First):**
   - Delete redundant markdown files
   - Delete SimpleGunScript/SimpleBullet
   - Cache skybox reference in GunScript
   - Optimize AudioManager with Dictionary

2. **Medium Effort:**
   - Optimize DestructibleGlobalMeshManager segment lookup
   - Optimize PerformanceMonitor Update loop
   - Review and delete Unused scripts folder

3. **Long Term:**
   - Implement bullet object pooling
   - Optimize Ghost script player detection
   - Remove unused assets
   - Compress textures

---

## ✅ Verification Checklist

After cleanup, verify:
- [ ] Game still builds successfully
- [ ] All scenes load correctly
- [ ] Bullets still work
- [ ] Audio still plays
- [ ] No console errors
- [ ] Performance improved (check FPS)

---

## 📈 Expected Performance Gains

- **AudioManager Dictionary:** 10-100x faster sound lookup
- **Cached Skybox:** Eliminates Find() overhead (~1-5ms per bullet)
- **Segment Dictionary:** O(1) lookup vs O(n) - critical for many segments
- **PerformanceMonitor Coroutine:** Eliminates Update() overhead
- **Object Pooling:** Eliminates GC spikes, smoother frame rate

**Total Expected FPS Improvement:** 5-15 FPS depending on scene complexity.
