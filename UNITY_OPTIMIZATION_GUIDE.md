# Unity Built-in Optimization Guide

## Overview
This guide shows you how to optimize your VR game using **only Unity's built-in settings** - no custom scripts needed.

---

## Quick Wins (Do These First!)

### 1. Disable Debug Logging in GunScript ⚡
**Impact**: +5-10 FPS immediately

1. Find your Gun GameObject in scene
2. `GunScript` component → **Debug** section
3. **Enable Debug Logging**: ❌ (uncheck this)

This alone gives you a significant FPS boost!

---

## Unity Quality Settings (Most Important!)

### Step 1: Open Quality Settings
- **Edit → Project Settings → Quality**

### Step 2: Select Android Quality Level
- At the top, find the **"Android"** column
- Click to select which quality level is used for Android/Quest builds

### Step 3: Configure for VR Performance

**Recommended Settings for Quest:**

#### Rendering:
- **Pixel Light Count**: `2` (fewer lights = better performance)
- **Texture Quality**: `Full Res` (or `Half Res` for more performance)
- **Anisotropic Textures**: `Forced On` (better texture quality at distance)
- **Anti Aliasing**: `4x Multi Sampling` (or `2x` for more performance)

#### Shadows:
- **Shadows**: `Hard Shadows Only` (or `Disable` for maximum performance)
- **Shadow Resolution**: `Medium Resolution` (512x512)
- **Shadow Projection**: `Stable Fit`
- **Shadow Distance**: `15-20` (lower = better performance)
- **Shadow Near Plane Offset**: `3`
- **Shadow Cascades**: `No Cascades` (VR doesn't need cascades)

#### Other:
- **Soft Particles**: ❌ (unchecked for performance)
- **Realtime Reflection Probes**: ❌ (unchecked)
- **Billboards Face Camera**: ✓
- **VSync Count**: `Don't Sync` (Oculus handles frame timing)

### Quality Presets:

**For Maximum Performance** (if you're getting low FPS):
```
Pixel Light Count: 1
Shadows: Disable
Shadow Distance: 0
Anti Aliasing: 2x
Texture Quality: Half Res
```

**For Balanced** (recommended):
```
Pixel Light Count: 2
Shadows: Hard Shadows Only
Shadow Distance: 15
Anti Aliasing: 4x
Texture Quality: Full Res
```

**For Maximum Quality** (only if hitting 72+ FPS):
```
Pixel Light Count: 4
Shadows: All
Shadow Distance: 30
Anti Aliasing: 4x
Texture Quality: Full Res
```

---

## Unity Player Settings

### Step 1: Open Player Settings
- **Edit → Project Settings → Player**

### Step 2: Android/Quest Settings

#### Resolution and Presentation:
- **Default Orientation**: `Landscape Left`
- **Run in Background**: ✓ (important for VR)

#### Other Settings:
- **Color Space**: `Linear` (better for VR)
- **Auto Graphics API**: ❌ (uncheck)
- **Graphics APIs**: Keep only `OpenGLES3` and `Vulkan`
  - Vulkan is faster on Quest 2/3
- **Multithreaded Rendering**: ✓ (important!)
- **Static Batching**: ✓
- **Dynamic Batching**: ✓ (helps with small objects)
- **GPU Skinning**: ✓ (offload to GPU)

#### Optimization:
- **Prebake Collision Meshes**: ✓
- **Keep Loaded Shaders Alive**: ✓
- **Preloaded Assets**: Add frequently used assets

#### Target Devices:
- **Target Devices**: `Quest`, `Quest 2`, `Quest 3` (or whatever you're targeting)

---

## Unity Physics Settings

### Step 1: Open Physics Settings
- **Edit → Project Settings → Physics**

### Step 2: Optimize Physics

**Recommended Settings:**

- **Gravity**: `(0, -9.81, 0)` (normal Earth gravity)
- **Default Material**: (leave as is)
- **Bounce Threshold**: `2`
- **Sleep Threshold**: `0.005` (objects sleep faster = better performance)
- **Default Solver Iterations**: `6` (can reduce to `4` for more performance)
- **Default Solver Velocity Iterations**: `1`
- **Queries Hit Backfaces**: ❌ (better performance)
- **Queries Hit Triggers**: ✓
- **Enable Adaptive Force**: ❌
- **Enable PCM**: ✓ (Persistent Contact Manifold - better performance)
- **Layer Collision Matrix**: Uncheck unnecessary collision pairs

#### Optimize Layer Collision Matrix:
At the bottom, uncheck collisions that don't need to happen:
- UI layer doesn't need to collide with anything
- Projectiles don't need to collide with other projectiles
- Etc.

---

## Unity Time Settings

### Step 1: Open Time Settings
- **Edit → Project Settings → Time**

### Step 2: Set Target Frame Rate

**For Quest 1:**
- **Fixed Timestep**: `0.01389` (72 Hz = 1/72)

**For Quest 2/3:**
- **Fixed Timestep**: `0.01111` (90 Hz = 1/90)

**Other Settings:**
- **Maximum Allowed Timestep**: `0.1`

---

## XR Plugin Management Settings

### Step 1: Open XR Settings
- **Edit → Project Settings → XR Plugin Management → Oculus**

### Step 2: Optimize Oculus Settings

**Stereo Rendering Mode:**
- **Multiview**: Best performance (recommended for Quest)
- Or **Multi Pass**: Better compatibility but slower

**Other Settings:**
- **Low Overhead Mode**: ✓ (better performance)
- **Phase Sync**: ✓
- **Optimized Buffer Discards**: ✓
- **Symmetric Projection**: ✓ (better performance in some cases)

**Target Devices:**
- ✓ Quest
- ✓ Quest 2
- ✓ Quest 3

---

## Model Import Settings (For Your New Models)

### Step 1: Select Your Gun/Flashlight Model
- In Project window, select the FBX file

### Step 2: Model Tab Settings

**Scene:**
- **Scale Factor**: `1` (adjust if model is wrong size)
- **Convert Units**: ✓

**Meshes:**
- **Mesh Compression**: `Medium` or `High` (smaller size, better performance)
- **Read/Write Enabled**: ❌ (unchecked - saves memory)
- **Optimize Mesh**: ✓ (better rendering performance)
- **Generate Colliders**: ❌ (add manually if needed)

**Geometry:**
- **Keep Quads**: ❌
- **Weld Vertices**: ✓
- **Index Format**: `Auto`
- **Import BlendShapes**: ❌ (unless you need them)
- **Import Visibility**: ✓
- **Import Cameras**: ❌
- **Import Lights**: ❌

**Normals & Tangents:**
- **Normals**: `Import` (or `Calculate` if none)
- **Tangents**: `Calculate Mikk T Space`

### Step 3: Materials Tab

- **Material Creation Mode**: `Import via MaterialDescription`
- **Location**: `Use External Materials (Legacy)`

### Step 4: Click Apply

---

## Texture Import Settings

### Step 1: Select Your Textures
- In Project window, select texture files

### Step 2: Configure Each Texture

**Texture Type:**
- **Albedo/Diffuse**: `Default`
- **Normal Maps**: `Normal Map`
- **Other**: Appropriate type

**Size:**
- **Max Size**: `1024` or `2048` (NOT 4096!)
  - Smaller = better performance
  - 1024 is usually enough for VR

**Compression:**
- **Compression**: `Normal Quality`
- **Use Crunch Compression**: ✓ (smaller file size)

**Advanced:**
- **Generate Mip Maps**: ✓ (important for performance!)
- **Streaming Mip Maps**: ✓ (loads textures progressively)
- **Border Mip Maps**: ❌

**Platform Override (Android):**
- **Override For Android**: ✓
- **Max Size**: `1024` or `512` for small textures
- **Format**: `ASTC 6x6` (best compression for Quest)
- **Compression Quality**: `Normal`

### Step 3: Click Apply

---

## Scene Optimization

### Lighting:

1. **Use Baked Lighting** where possible:
   - Window → Rendering → Lighting
   - Generate Lighting (bake lights)
   - Much faster than real-time lights

2. **Limit Real-time Lights**:
   - Keep to 2-4 real-time lights max
   - Use baked for static objects

3. **Flashlight Light Settings**:
   - **Mode**: `Realtime` (needs to move)
   - **Render Mode**: `Important`
   - **Culling Mask**: Only objects you want lit
   - **Shadow Type**: `No Shadows` (or `Hard Shadows` if needed)

### Cameras:

1. **Main Camera Settings**:
   - **Clear Flags**: `Solid Color` or `Skybox`
   - **Culling Mask**: Uncheck layers you don't need to render
   - **Occlusion Culling**: ✓
   - **Allow HDR**: ❌ (better performance)
   - **Allow MSAA**: ✓
   - **Allow Dynamic Resolution**: ✓

2. **Occlusion Culling**:
   - Window → Rendering → Occlusion Culling
   - Bake occlusion data (culls objects behind walls)
   - Can give +10-20 FPS in complex scenes

### GameObjects:

1. **Static Objects**:
   - Select non-moving objects (walls, furniture, etc.)
   - Inspector → Check **Static** (top right)
   - Allows batching and occlusion culling

2. **Mesh Renderers**:
   - **Cast Shadows**: `Off` for small objects (projectiles, debris)
   - **Receive Shadows**: `Off` for objects that don't need it
   - **Light Probes**: `Off` if not using light probes

3. **LOD Groups** (for complex models):
   - Add LOD Group component
   - Add simplified models for distance
   - Objects further away use fewer polygons

---

## Build Settings Optimization

### Step 1: Build Settings
- **File → Build Settings**

### Step 2: Android Settings

**Texture Compression:**
- **ASTC** (best for Quest)

**Build:**
- **Development Build**: ❌ (uncheck for production)
- **Autoconnect Profiler**: ❌
- **Deep Profiling**: ❌
- **Script Debugging**: ❌

**Compression Method:**
- **LZ4** (faster loading) or **LZ4HC** (smaller size)

### Step 3: Player Settings Button

- **Scripting Backend**: `IL2CPP` (much faster than Mono)
- **API Compatibility Level**: `.NET Standard 2.1`
- **Target Architectures**: ✓ ARM64 (uncheck ARMv7)
- **Optimize Mesh Data**: ✓

---

## URP (Universal Render Pipeline) Settings

### If you're using URP:

1. **Find URP Asset**:
   - Project window → Assets → Settings
   - Or wherever your URP asset is

2. **Quality Settings**:
   - **Rendering Scale**: `1.0` (reduce to 0.8 for more performance)
   - **HDR**: ❌ (better performance for VR)
   - **MSAA**: `4x` (or `2x` for more performance)
   - **Render Scale**: `1.0`

3. **Lighting**:
   - **Main Light**: `Per Pixel`
   - **Additional Lights**: `Per Pixel` (or `Disabled` for max performance)
   - **Cast Shadows**: ✓ (or uncheck for max performance)
   - **Shadow Resolution**: `1024` or `2048`

4. **Shadows**:
   - **Max Distance**: `15-20`
   - **Cascade Count**: `1` (no cascades needed for VR)
   - **Depth Bias**: `1`
   - **Normal Bias**: `1`

5. **Post Processing**:
   - **Disable** most post-processing for VR performance
   - Keep only essential effects

---

## Performance Testing

### In Unity Editor:

1. **Stats Window**:
   - Game view → Stats button
   - Shows FPS, batches, tris, verts

2. **Frame Debugger**:
   - Window → Analysis → Frame Debugger
   - See exactly what's being rendered

3. **Profiler**:
   - Window → Analysis → Profiler
   - Shows CPU/GPU/Memory usage
   - Identify bottlenecks

### On Quest Device:

1. **Build and Run** to Quest

2. **Check Performance**:
   - Use Oculus Developer Hub
   - Shows real-time FPS on device
   - Much more accurate than editor

3. **Target FPS**:
   - Quest 1: 72 FPS minimum
   - Quest 2/3: 72-90 FPS

---

## Quick Performance Checklist

### ✅ Settings Applied:
- [ ] Quality Settings configured (shadows, lights, etc.)
- [ ] Player Settings optimized (multithreading, batching)
- [ ] Physics Settings optimized (sleep threshold)
- [ ] XR Settings optimized (multiview rendering)
- [ ] Model Import Settings optimized (mesh compression)
- [ ] Texture Settings optimized (ASTC compression, mip maps)
- [ ] GunScript debug logging DISABLED
- [ ] Build Settings optimized (IL2CPP, ARM64)
- [ ] Static objects marked as Static
- [ ] Unnecessary collisions disabled (Layer Collision Matrix)
- [ ] Occlusion Culling baked (if applicable)

### 🎯 Expected Results:
- **+25-40 FPS improvement** from all optimizations
- Smooth 72 FPS on Quest 1
- Smooth 90 FPS on Quest 2/3
- No stuttering or frame drops

---

## Troubleshooting

### Still Low FPS?

1. **Profile your game**:
   - Use Unity Profiler
   - Find what's taking the most time

2. **Common Issues**:
   - **Too many lights**: Reduce to 2-4 max
   - **High poly models**: Reduce polygon count
   - **Large textures**: Reduce to 1024x1024
   - **Too many draw calls**: Enable batching
   - **Debug logging**: Make sure it's disabled!

3. **Nuclear Options** (if desperate):
   - Disable ALL shadows
   - Reduce texture quality to Half Res
   - Reduce MSAA to 2x
   - Disable post-processing

---

## Summary

All optimization done through Unity's built-in settings:

1. ✅ **Quality Settings** - Shadow/light optimization
2. ✅ **Player Settings** - Multithreading, batching, IL2CPP
3. ✅ **Physics Settings** - Sleep threshold, solver iterations
4. ✅ **XR Settings** - Multiview rendering
5. ✅ **Model/Texture Settings** - Compression, mip maps
6. ✅ **Build Settings** - IL2CPP, ARM64 only
7. ✅ **Scene Optimization** - Static objects, occlusion culling
8. ✅ **URP Settings** - Rendering scale, shadow distance

**No custom scripts needed!** Everything is built into Unity. 🚀

Apply these settings and you should see significant performance improvements!

