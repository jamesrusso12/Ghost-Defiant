# Troubleshooting Fixes - December 12, 2025

## Summary of Issues Fixed

### ✅ 1. Wrist Menu Not Showing (Y Button)

**Problem:** Wrist menu wasn't appearing when pressing Y button on controller.

**Root Cause:** Multiple conflicting wrist menu scripts (WristMenuController, SimpleWristMenu, WristMenu.cs).

**Solution:**
- **Consolidated to WristMenuController** - This is the ONLY wrist menu script you should use
- Moved `SimpleWristMenu.cs` and `WristMenu.cs` to `Unused/` folder
- Added `enableDebugLogging` flag (set to FALSE by default for better performance)
- WristMenuController uses `Button.Two` on `LTouch` controller which is correct for Y button

**How to Use:**
1. In Unity, attach `WristMenuController` to your wrist menu Canvas
2. Assign the `menuPanel` GameObject in the inspector
3. Press **Y button on LEFT controller** to toggle the menu
4. Set `enableDebugLogging = true` only when debugging, keep it FALSE in builds

---

### ✅ 2. Game UI Clipping Through Walls

**Problem:** Game UI was constantly clipping through walls and becoming invisible.

**Root Causes:**
1. UI was too far from camera (0.5m) - more likely to clip through walls
2. Collision mask was only checking Default layer (layer 1) - didn't include scene mesh/walls
3. UI wasn't rendering on top of passthrough walls
4. UI response was too slow to avoid walls

**Solutions:**
- **Reduced UI distance** from 0.5m to **0.25m** (much closer to camera)
- **Changed collision mask** from `1` to `~0` (all layers) - adjust in inspector to exclude UI/Player
- **Increased wall offset** from 0.05m to 0.1m for better clearance
- **Added canvas render order configuration** - sets sorting order to 100 and disables culling
- **Increased lerp speed** from 5.0 to 8.0 for faster wall avoidance
- **Auto-sets UI layer** to proper "UI" layer
- **Increased smoothing speed** from 0.3s to 0.2s for faster response

**In Unity Inspector:**
1. Select your Game UI Canvas
2. Check the `collisionMask` - make sure it includes scene mesh layers
3. Verify `m_TargetOffset` is set to `(0, 0, 0.25)` for closer placement
4. The script now automatically configures render order on Awake

---

### ✅ 3. Performance Issues / Low FPS

**Problem:** Frame rate was poor, likely due to high-poly 3D models (gun/flashlight) and excessive debug logging.

**Solutions:**

#### Debug Logging Reduction
- **WristMenuController**: Added `enableDebugLogging` flag (default: FALSE)
  - Reduces dozens of Debug.Log calls per frame
  - Only enable when debugging menu issues
  
- **GunScript**: Already had `enableDebugLogging` flag
  - Added performance tooltips warning to keep it OFF in builds
  - Reduced collision check overhead

#### Performance Diagnostics Tool
You already have `PerformanceDiagnostics.cs` in the Debug folder. To use it:

```csharp
// In Unity:
1. Attach PerformanceDiagnostics to any GameObject
2. Assign your gun and flashlight GameObjects in inspector
3. Press 'P' key in Play mode (or it runs automatically on Start)
4. Check Console for poly count reports
```

**Expected Results:**
- **Good:** < 10,000 vertices per model
- **Warning:** 10,000 - 50,000 vertices (acceptable but not ideal for VR)
- **Critical:** > 50,000 vertices (THIS CAUSES LAG!)

#### Model Optimization Recommendations

If your models are high-poly (>10k vertices), here are your options:

**Option 1: Reduce Poly Count in Blender (Recommended)**
```
1. Open model in Blender
2. Select mesh
3. Add Modifier > Decimate
4. Set Ratio to 0.5 (halves poly count)
5. Apply modifier
6. Export as FBX
7. Re-import to Unity
```

**Option 2: Use Unity's Mesh Simplification**
```
1. Select model in Project window
2. Inspector > Model tab
3. Check "Optimize Mesh"
4. Set "Mesh Compression" to "High"
5. Click Apply
```

**Option 3: Use LOD (Level of Detail)**
```
1. Create 3 versions of your model (high, medium, low poly)
2. Add LOD Group component to your gun/flashlight
3. Assign models to LOD levels
4. Unity automatically switches based on distance
```

**Target Poly Counts for VR:**
- Gun: 3,000 - 5,000 triangles
- Flashlight: 1,000 - 2,000 triangles
- Props: < 1,000 triangles each

**Other Performance Tips:**
- **Textures:** Keep textures at 1024x1024 or smaller (2048x2048 max)
- **Materials:** Use Mobile/Unlit shaders when possible
- **Lighting:** Use baked lighting instead of real-time
- **Shadows:** Disable shadows on small objects
- **Colliders:** Use simple colliders (box/sphere) instead of mesh colliders when possible

---

### ✅ 4. Duplicate Scripts Consolidated

**Removed/Moved:**
- `SimpleWristMenu.cs` → Moved to `Unused/`
- `WristMenu.cs` → Moved to `Unused/`

**Active Scripts:**
- ✅ `WristMenuController.cs` - Use this for wrist menu
- ✅ `GameUILazyFollow.cs` - Use this for game HUD
- ✅ `PerformanceMonitor.cs` - UI-based FPS display
- ✅ `VRPerformanceMonitor.cs` - OnGUI overlay (toggle with LEFT grip)
- ✅ `PerformanceDiagnostics.cs` - Model analysis tool

---

## Testing Checklist

### Wrist Menu
- [ ] Press Y button on left controller
- [ ] Menu appears on wrist
- [ ] Can interact with buttons/sliders
- [ ] Menu doesn't spam console logs
- [ ] Performance is smooth

### Game UI
- [ ] UI follows camera smoothly
- [ ] UI stays in front of you
- [ ] UI doesn't clip through walls
- [ ] UI is visible at all times
- [ ] UI is closer to camera (more comfortable)

### Performance
- [ ] FPS is stable (72+ fps for Quest 2, 90+ for Quest 3)
- [ ] No stuttering when shooting
- [ ] Console isn't flooded with debug messages
- [ ] Run PerformanceDiagnostics to check model poly counts

---

## Performance Monitoring

### Option 1: VRPerformanceMonitor (Recommended for VR)
```
- Shows on-screen overlay
- Toggle with LEFT controller grip button
- Shows FPS, min/max, average
- Color-coded performance indicators
- Reset stats with LEFT thumbstick press
```

### Option 2: PerformanceMonitor
```
- Requires UI Canvas with Text/Slider elements
- Shows FPS and memory usage
- Good for permanent HUD display
```

### Option 3: PerformanceDiagnostics
```
- One-time analysis tool
- Press 'P' to run diagnostics
- Reports poly counts and memory usage
- Identifies high-poly models
```

---

## Common Issues & Solutions

### "Wrist menu still not showing"
1. Check that WristMenuController is attached to Canvas
2. Check that menuPanel is assigned
3. Make sure no other wrist menu scripts are active
4. Enable `enableDebugLogging` and check console for errors
5. Verify OVRCameraRig exists in scene with LeftHandAnchor

### "UI still clipping through walls"
1. Check `collisionMask` includes your wall layers
2. Reduce `m_TargetOffset` even more (try 0.2m or 0.15m)
3. Increase `wallOffset` to 0.15m for more clearance
4. Make sure walls have colliders
5. Verify canvas sorting order is set to 100+

### "Still getting low FPS"
1. Run PerformanceDiagnostics (press P in Play mode)
2. Check console for high poly count warnings
3. Disable all `enableDebugLogging` flags
4. Use Unity Profiler (Window > Analysis > Profiler)
5. Check for expensive scripts in Update()
6. Reduce texture sizes
7. Disable real-time lighting/shadows

### "Too many scripts, feeling overwhelmed"
Here's what to keep:
- **UI/WristMenuController.cs** - Wrist menu (Y button)
- **UI/GameUILazyFollow.cs** - Game HUD positioning
- **Debug/VRPerformanceMonitor.cs** - FPS overlay
- **Debug/PerformanceDiagnostics.cs** - Model analysis

Everything else in `Unused/` folder can be deleted if you want.

---

## Build Recommendations

Before building for Quest:

1. **Disable ALL debug logging:**
   ```
   WristMenuController.enableDebugLogging = FALSE
   GunScript.enableDebugLogging = FALSE
   ```

2. **Remove/Disable debug tools:**
   ```
   - Disable VRPerformanceMonitor (or remove from scene)
   - Disable PerformanceDiagnostics
   - Remove DebugConsole if present
   ```

3. **Optimize models:**
   ```
   - Run PerformanceDiagnostics
   - Fix any models over 10k vertices
   - Compress textures to 1024x1024 or less
   ```

4. **Unity Quality Settings:**
   ```
   Edit > Project Settings > Quality
   - Anti Aliasing: 2x or 4x
   - Shadows: Soft Shadows (or disable)
   - Shadow Distance: 20-30m max
   - VSync: On (for Quest)
   ```

5. **Test on device:**
   ```
   - Use Oculus Performance HUD (on headset)
   - Check FPS during gameplay
   - Test in various room sizes
   ```

---

## Files Changed

### Modified
- `Assets/Scripts/UI/WristMenuController.cs` - Added debug logging toggle, cleaned up
- `Assets/Scripts/UI/GameUILazyFollow.cs` - Improved wall avoidance, closer UI, render order fix
- `Assets/Scripts/Combat/GunScript.cs` - Performance optimization, configurable collision ignore time

### Moved
- `Assets/Scripts/UI/SimpleWristMenu.cs` → `Assets/Scripts/Unused/SimpleWristMenu.cs`
- `Assets/Scripts/UI/WristMenu.cs` → `Assets/Scripts/Unused/WristMenu.cs`

### Added
- `TROUBLESHOOTING_FIXES.md` (this file)

---

## Contact & Support

If you continue having issues:
1. Enable debug logging on affected component
2. Note any error messages in console
3. Check Unity Profiler for performance bottlenecks
4. Test in a simple empty room first
5. Verify all OVR components are properly configured

---

## Quick Reference Card

### Button Mappings
- **Y Button (Left Controller)** - Toggle Wrist Menu
- **LEFT Grip** - Toggle VR Performance Monitor
- **LEFT Thumbstick Press** - Reset Performance Stats
- **P Key (Editor)** - Run Performance Diagnostics

### Performance Targets
- **FPS:** 72+ (Quest 2), 90+ (Quest 3)
- **Model Poly Count:** < 5,000 vertices
- **Texture Size:** 1024x1024 or less
- **UI Distance:** 0.25m from camera

### Debug Flags (DISABLE IN BUILDS!)
- `WristMenuController.enableDebugLogging`
- `GunScript.enableDebugLogging`
- `PerformanceDiagnostics` component
- `VRPerformanceMonitor` component

---

Good luck with your VR project! 🎮👻
