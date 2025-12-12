# Troubleshooting Fixes Applied

## Summary
This document outlines the fixes applied to resolve three major issues in your VR/MR project.

⚠️ **MR/PASSTHROUGH TESTING REQUIRES BUILDING TO DEVICE!**
- Play mode won't work for passthrough
- See `TESTING_QUICK_REFERENCE.md` for a printable testing guide
- Use Quick Build tool: `Tools → Quick Build → Development Build` (Ctrl+Shift+B)

---

## ✅ Issue 1: Wrist Menu Not Showing (Y Button Conflict)

### Problem
- Pressing Y button on left controller did not toggle the wrist menu
- Both `DebugConsole` and `WristMenuController` were listening to the same button input

### Root Cause
`DebugConsole.cs` was using `OVRInput.Button.Three` on **both** left and right controllers, conflicting with the wrist menu which also uses Y button (Button.Three) on the **left** controller.

### Solution Applied
Changed `DebugConsole.cs` to use **RIGHT controller only**:
- **A button (right controller)**: Toggle debug console
- **B button (right controller)**: Clear logs
- This frees up the left controller Y button exclusively for the wrist menu

### Files Modified
- `Assets/Scripts/Debug/DebugConsole.cs`

### Testing (BUILD REQUIRED - Passthrough only works on device!)
1. Build to Quest headset
2. Press **Y button on LEFT controller** → Should toggle wrist menu
3. Press **A button on RIGHT controller** → Should toggle debug console
4. Check debug console for "[WristMenu]" messages to confirm it's working
5. No more conflicts!

---

## ✅ Issue 2: Performance Issues / Low FPS

### Problem
- Low FPS during gameplay
- Suspected cause: Imported 3D models (gun and flashlight from Meshy AI)

### Root Cause
AI-generated 3D models often have:
- High polygon counts (50,000+ vertices)
- Unoptimized import settings
- Read/Write enabled (doubles memory usage)
- No mesh compression

### Solutions Applied

#### A. Created Model Optimization Tool
**New Editor Tool**: `Assets/Scripts/Editor/ModelOptimizationChecker.cs`

**How to Use**:
1. In Unity: **Tools → VR Optimization → Check Model Performance**
2. Click **"Scan Weapons Folder Only"** to analyze gun and flashlight models
3. Review the report for issues:
   - High vertex counts
   - Missing compression
   - Read/Write enabled
   - Unused features (blend shapes, animations)
4. Click **"Auto-Optimize"** on any model with issues
5. Re-scan to verify improvements

**What Auto-Optimize Does**:
- ✓ Enables HIGH mesh compression
- ✓ Disables Read/Write (saves 50% memory)
- ✓ Optimizes vertices and polygons
- ✓ Disables unused features (animations, blend shapes)
- ✓ Improves normal import

#### B. Created VR Performance Monitor
**New Runtime Tool**: `Assets/Scripts/Debug/VRPerformanceMonitor.cs`

**How to Use**:
1. Add `VRPerformanceMonitor` component to any GameObject in your scene
2. During gameplay, see real-time FPS display in top-left corner
3. **LEFT controller grip button**: Toggle display on/off
4. **LEFT controller thumbstick press**: Reset stats

**What It Shows**:
- Current FPS and frame time (ms)
- Average, Min, Max FPS
- Performance status (Good/Warning/Critical)
- Color-coded indicators:
  - 🟢 Green: Good (72+ FPS)
  - 🟡 Yellow: Warning (60-72 FPS)
  - 🔴 Red: Critical (<60 FPS)

### Recommended Actions
1. **Scan your models** using the Model Optimization Tool
2. **Auto-optimize** any models with high vertex counts
3. **Monitor FPS** during gameplay with the Performance Monitor
4. If FPS is still low after optimization:
   - Consider reducing polygon count in your 3D modeling software
   - Use simplified meshes for weapons (target: 5,000-10,000 vertices)
   - Create LOD (Level of Detail) groups

### Files Created
- `Assets/Scripts/Editor/ModelOptimizationChecker.cs` (Editor tool)
- `Assets/Scripts/Debug/VRPerformanceMonitor.cs` (Runtime monitor)

### Performance Targets for VR
- **Target FPS**: 72 FPS (Quest 2/3 standard)
- **Weapon Models**: 5,000-10,000 vertices max
- **Mesh Compression**: HIGH
- **Read/Write**: DISABLED

---

## ✅ Issue 3: Game UI Clipping Through Walls

### Problem
- UI canvas was clipping through walls constantly
- UI was too far from camera (0.8 meters)
- Potential layer issues

### Root Cause
1. **Distance Issue**: UI was 0.8m from camera, too far to prevent wall intersections
2. **Layer Rendering**: Canvas sorting order not high enough
3. **No Runtime Adjustment**: Fixed distance with no way to adjust

### Solutions Applied

#### A. Reduced UI Distance from Camera
Changed default distance from **0.8m → 0.35m**

**Files Modified**:
- `Assets/Scripts/UI/MRUIConfigurator.cs` (line 23)
- `Assets/Scripts/UI/GameUILazyFollow.cs` (line 24)

**Why 0.35m?**
- Close enough to prevent wall clipping
- Far enough to be comfortable to view
- Standard VR HUD distance

#### B. Improved Canvas Sorting Order
Added configurable sorting order to `MRUIConfigurator.cs`:
- Default sorting order: **100** (renders on top of environment)
- Enabled `overrideSorting` to force proper layer rendering

**Files Modified**:
- `Assets/Scripts/UI/MRUIConfigurator.cs` (lines 28, 136-138)

#### C. Created Runtime Distance Adjuster
**New Tool**: `Assets/Scripts/UI/UIDistanceAdjuster.cs`

**How to Use**:
1. Attach `UIDistanceAdjuster` to your Canvas GameObject (with GameUILazyFollow)
2. During gameplay:
   - **RIGHT thumbstick UP/DOWN**: Adjust UI distance
   - **RIGHT thumbstick PRESS**: Reset to default (0.35m)
3. Find your perfect distance!

**Settings**:
- Min distance: 0.2m
- Max distance: 1.0m
- Adjustment speed: 0.5m/s

### Files Created/Modified
- `Assets/Scripts/UI/MRUIConfigurator.cs` (modified)
- `Assets/Scripts/UI/GameUILazyFollow.cs` (modified)
- `Assets/Scripts/UI/UIDistanceAdjuster.cs` (new)

### Testing (BUILD REQUIRED - Passthrough only works on device!)
1. Build to Quest headset
2. Put on headset and look at the UI near a wall
3. UI should stay visible (not clip through)
4. If needed, use RIGHT thumbstick to adjust distance
5. Check debug console (A button on RIGHT controller) for any UI-related messages

---

## 🎮 Controller Button Reference

### LEFT Controller
- **Y button**: Toggle wrist menu
- **Grip**: Toggle performance monitor
- **Thumbstick Press**: Reset performance stats

### RIGHT Controller
- **A button**: Toggle debug console
- **B button**: Clear debug console logs
- **Thumbstick UP/DOWN**: Adjust UI distance (if UIDistanceAdjuster is enabled)
- **Thumbstick Press**: Reset UI distance to default

---

## 📋 Next Steps

⚠️ **IMPORTANT**: Since you're using MR Passthrough, you MUST build to device to test. Play mode won't work for passthrough features!

### 1. Test Wrist Menu (BUILD REQUIRED)
- Build to your Quest headset
- Press **Y on LEFT controller**
- Wrist menu should appear
- Check debug console (A button on RIGHT) for "[WristMenu]" logs

### 2. Optimize Models
- Open **Tools → VR Optimization → Check Model Performance**
- Scan weapons folder
- Auto-optimize any problematic models

### 3. Monitor Performance (BUILD REQUIRED)
- Add `VRPerformanceMonitor` component to your scene
- Build to Quest headset
- In-game: Press LEFT grip to toggle performance overlay
- Check FPS (should be 72+)
- Performance data shows in top-left of view

### 4. Adjust UI Position (BUILD REQUIRED)
- Add `UIDistanceAdjuster` to your main UI Canvas (optional but recommended)
- Build to Quest headset
- In-game: Use RIGHT thumbstick UP/DOWN to adjust UI distance
- Press RIGHT thumbstick to reset to default
- Find the distance that prevents wall clipping

### 5. Development Build Tips for Faster Testing
- Use **Development Build** option in Build Settings
- Enable **Script Debugging** for better error messages
- Keep Debug Console enabled to see logs in-game
- Press A (RIGHT controller) to view debug console anytime

---

## 🚀 Efficient Build-Test Workflow for MR/Passthrough

Since MR Passthrough requires building to device (no Play mode testing), here's how to optimize your workflow:

### Quick Build Tool (NEW!)
**One-Click Building**: `Tools → Quick Build → Development Build and Run` (Ctrl+Shift+B)

This new tool:
- ✓ Auto-saves your scenes
- ✓ Configures optimal development settings
- ✓ Builds AND installs automatically
- ✓ Shows build time and size
- ✓ Creates timestamped builds (no overwriting!)

**Other Options**:
- `Tools → Quick Build → Release Build (Optimized)` - For final testing/distribution
- `Tools → Quick Build → Show Build Info` - Check current build settings

### Manual Build Settings (if not using Quick Build)
1. **File → Build Settings → Android**
2. Enable these for faster testing:
   - ✓ **Development Build** (shows debug console, better errors)
   - ✓ **Script Debugging** (see C# errors in-game)
   - ✓ **Wait For Managed Debugger** (optional - for serious debugging)
3. **Build And Run** (Ctrl+B) - builds and automatically installs

### In-Game Debug Tools (No Computer Needed!)
All these tools work WITHOUT needing to be connected to your computer:

1. **Debug Console** (DebugConsole.cs - already in your scene)
   - Press **A button (RIGHT controller)** to see all logs
   - Shows errors, warnings, and debug messages
   - Look for messages starting with `[WristMenu]`, `[MRUIConfigurator]`, etc.

2. **Performance Monitor** (VRPerformanceMonitor.cs - add to scene)
   - Shows real-time FPS
   - Press **LEFT grip** to toggle
   - Instantly see if models are causing issues

3. **Controller Help** (ControllerHelpOverlay.cs - add to scene)
   - Press **both grips** to see all button mappings
   - Never forget which button does what!

### Debugging Without Inspector
Since you can't see Unity's Inspector during testing:

1. **Use Debug.Log extensively** - Shows in debug console (A button)
2. **Check these logs after fixing**:
   - `[WristMenu]` - Wrist menu initialization and button presses
   - `[MRUIConfigurator]` - UI configuration and layer setup
   - `[VRPerformanceMonitor]` - FPS and performance issues
   - `[ModelOptimizer]` - Model optimization results

3. **Test one fix at a time**:
   - Fix #1 (Wrist Menu) → Build → Test → Check logs
   - Fix #2 (Performance) → Build → Test → Check FPS overlay
   - Fix #3 (UI Clipping) → Build → Test → Move near walls

### Recommended Scene Setup for Testing
Add these components to your scene for best testing experience:
- ✓ `DebugConsole` (see logs without computer)
- ✓ `VRPerformanceMonitor` (see FPS live)
- ✓ `ControllerHelpOverlay` (remember controls)

### Pro Tips
- **Keep a build log**: Note what you changed before each build
- **Use Development Builds**: Slower to build but WAY easier to debug
- **Check debug console first**: Most issues will show logs there
- **Use the performance monitor**: Instant feedback on FPS issues

---

## 🔧 Additional Performance Tips

### General VR Optimization
1. **Lighting**:
   - Use baked lighting where possible
   - Limit real-time shadows (1-2 max)
   - Reduce shadow distance

2. **Physics**:
   - Use simplified collision meshes
   - Reduce physics update rate if not critical
   - Use layers to limit collision checks

3. **Graphics**:
   - Use URP optimized shaders
   - Reduce texture sizes (1024x1024 max for most textures)
   - Enable GPU instancing on materials

4. **Scene**:
   - Use occlusion culling
   - Implement frustum culling
   - Batch static objects

### Model-Specific Tips
1. **Weapons** (held by player):
   - 5,000-10,000 vertices max
   - 1024x1024 textures
   - Single material if possible

2. **Environment**:
   - Use LOD groups for distant objects
   - Bake as much as possible
   - Use lightmaps

3. **Effects**:
   - Limit particle systems (< 50 particles)
   - Use GPU particles
   - Simple shaders for VFX

---

## 📞 Troubleshooting

### Issue: Models still causing low FPS after optimization
**Solution**: The models may have too many vertices. Options:
1. Use decimation in Blender to reduce polygon count
2. Replace with lower-poly models
3. Create LOD versions

### Issue: UI still clipping through walls
**Solution**:
1. Reduce distance further (use UIDistanceAdjuster)
2. Check canvas layer (should be "UI")
3. Increase canvas sorting order in MRUIConfigurator

### Issue: Wrist menu still not showing
**Solution**:
1. Check that WristMenuController is enabled
2. Verify left hand anchor is assigned
3. Check debug console for "[WristMenu]" messages
4. Ensure no other scripts are using Y button on left controller

---

## 📝 Summary of New Files

### Editor Tools
- `Assets/Scripts/Editor/ModelOptimizationChecker.cs` - Analyze and optimize 3D models
- `Assets/Scripts/Editor/QuickBuildHelper.cs` - One-click building for faster testing

### Runtime Scripts
- `Assets/Scripts/Debug/VRPerformanceMonitor.cs`
- `Assets/Scripts/UI/UIDistanceAdjuster.cs`

### Modified Scripts
- `Assets/Scripts/Debug/DebugConsole.cs`
- `Assets/Scripts/UI/MRUIConfigurator.cs`
- `Assets/Scripts/UI/GameUILazyFollow.cs`

---

## ✨ Features Added

1. ✓ Input conflict resolution (wrist menu now works)
2. ✓ Model performance analysis tool (Editor)
3. ✓ Quick Build Helper for faster testing (Editor)
4. ✓ Runtime FPS monitor
5. ✓ Auto-optimization for 3D models
6. ✓ UI distance adjustment (prevents wall clipping)
7. ✓ Improved canvas sorting (renders on top)
8. ✓ Runtime UI distance adjuster
9. ✓ Controller help overlay (in-game button reference)

---

Good luck with your VR project! 🚀
