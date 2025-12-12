# Ghost Defiant - Complete Setup Guide

## Project Overview
Mixed reality wave-based survival shooter for Meta Quest. Ghost enemies spawn in your real room via passthrough. Defend with a projectile gun across 5 progressive rounds. Built with Unity & Meta XR SDK.

---

## Table of Contents
1. [Gun & Projectile System](#gun--projectile-system)
2. [Wrist Menu System](#wrist-menu-system)
3. [Wall Destruction](#wall-destruction)
4. [Ghost AI](#ghost-ai)
5. [Troubleshooting](#troubleshooting)

---

## Gun & Projectile System

### Overview
The gun shoots physical projectiles (Specter Ammo spheres) with realistic physics, reliable collision detection, and proper interaction with walls, ceilings, and ghosts.

### Key Features
- ✅ **Projectile Mode** - Shoots visible sphere projectiles with physics
- ✅ **Collision-Based Detection** - Works reliably with non-convex wall meshes
- ✅ **Dual Detection System** - Physics + Raycast backup for 100% hit detection
- ✅ **Self-Collision Prevention** - Never hits gun or player
- ✅ **Ceiling & Wall Detection** - Reliable detection on all surfaces
- ✅ **Ghost Killing** - One-hit kills on impact
- ✅ **Impact Effects** - Visual feedback at collision points

### Recent Fixes (December 2024)
1. **Spawn Offset** - Projectiles spawn 30cm forward to prevent immediate collision
2. **Physics.IgnoreCollision** - Permanent collision filtering for gun/player
3. **Time-Based Filtering** - Ignores collisions for first 0.1 seconds as backup
4. **Non-Trigger Colliders** - Switched from trigger to collision for reliable wall detection
5. **Raycast Backup System** - Catches ceiling hits that physics might miss
6. **Optimized Physics** - Lighter mass, reduced gravity, faster speed for better trajectories

### Setup Instructions

#### Step 1: Configure Specter Ammo Prefab
1. Find your "Specter Ammo" GameObject in Hierarchy
2. Make it a prefab by dragging to `Assets/Prefabs/Weapons/Projectiles/`
3. Configure appearance:
   - **Size**: Scale 0.1 - 0.3 (small projectile)
   - **Material**: Add emission for glow effect
   - **Shader**: Universal Render Pipeline/Lit
   - **Emission**: Bright cyan/blue color
   - **NO collider or Rigidbody needed** (auto-added by gun)

#### Step 2: Configure Gun Inspector Settings
Find your Gun GameObject (usually on `RightHandAnchor`) and set:

**Shooting Mode:**
- ✅ `Use Projectile Mode` = Checked

**Gun Settings:**
- `Shooting Point` = Barrel tip transform
- `Shooting Button` = RIndexTrigger (right trigger)
- `Max Line Distance` = 5-10 (projectile range)

**Projectile Mode:**
- `Projectile Prefab` = Your Specter Ammo prefab
- `Projectile Speed` = 25 (fast, reliable)
- `Projectile Lifetime` = 2 seconds
- `Gravity Multiplier` = 0.3 (reduced for straighter shots)

**Impact Effects:**
- `Ray Impact Prefab` = Your impact effect prefab
- `Layer Mask` = Everything except "Ignore Raycast"

**Audio:**
- `Source` = AudioSource component
- `Shooting Audio Clip` = Gun shot sound

**Debug:**
- `Enable Debug Logging` = True (for testing, disable for performance)

### How It Works

#### Dual Detection System
The gun uses two complementary detection methods:

1. **Physics Collision (Primary)**
   - Uses `OnCollisionEnter` for reliable wall/ceiling detection
   - Non-trigger SphereCollider with zero bounciness
   - Works with non-convex MeshColliders (walls/ceilings)

2. **Raycast Backup (Secondary)**
   - Runs every FixedUpdate (50-60 times/second)
   - Looks ahead 2.5x velocity distance
   - Catches hits physics might miss (especially ceilings)
   - Ensures 100% hit detection

#### Collision Prevention
Three layers of protection prevent self-collision:

1. **Spawn Offset** - 30cm forward from shooting point
2. **Physics.IgnoreCollision** - Permanent collision filtering
3. **Time-Based Filter** - Ignores all collisions first 0.1 seconds

#### Physics Configuration
- **Mass**: 0.05kg (light = less gravity effect)
- **Gravity**: 30% of normal (straighter trajectories)
- **Speed**: 25 units/second (fast, reliable)
- **Damping**: 0 (no air resistance)
- **Detection**: ContinuousDynamic (fast-moving objects)
- **Material**: Zero bounce, zero friction

### Code Architecture

**GunScript.cs** - Main gun controller
- `Shoot()` - Handles shooting button press
- `CreateProjectile()` - Spawns and configures projectile
- Events: `OnShoot`, `OnShootAndHit`, `OnShootAndMiss`

**ProjectileController.cs** - Projectile behavior (auto-added to projectiles)
- `Initialize()` - Sets up physics, collision filtering
- `FixedUpdate()` - Applies gravity, performs raycast backup
- `OnCollisionEnter()` - Handles impact detection
- `HandleImpact()` - Centralized impact processing

### Troubleshooting

**Projectile spawns but doesn't move:**
- Check `projectileSpeed` > 0
- Verify shootingPoint direction is correct

**Projectile hits gun immediately:**
- All three collision prevention layers should be active
- Check spawn offset is 0.3 or higher

**Walls don't destroy:**
- Verify projectile has non-trigger SphereCollider
- Check wall MeshColliders are enabled
- Enable debug logging to see hit events

**Ceiling doesn't destroy:**
- Raycast backup should catch these
- Check `gravityMultiplier` isn't too high (use 0.3)
- Increase `projectileSpeed` if needed

---

## Wrist Menu System

### Overview
Holographic menu attached to left wrist with hand interaction and raycasting support.

### Features
- ✅ Holographic appearance with emission/transparency
- ✅ Rotates to face player camera
- ✅ Shows/hides with left palm facing player
- ✅ Supports direct hand interaction and controller raycasting
- ✅ Auto-disables gun when menu is open

### Setup Instructions

1. **Create Wrist Menu Canvas**
   - Add Canvas to Left Hand Anchor
   - Canvas settings:
     - Render Mode: World Space
     - Event Camera: Center Eye Anchor camera
     - Sorting Layer: Default
     - Order in Layer: 10

2. **Add Menu Components**
   - Add `WristMenuController` script
   - Add `OVRRaycaster` for controller interaction
   - Add `Canvas Group` for fade effects

3. **Configure Buttons**
   - Each button needs:
     - `Button` component
     - `EventTrigger` component
     - `OVRInputModule` in scene

4. **Material Setup**
   - Use URP/Lit shader with:
     - Surface Type: Transparent
     - Alpha: 0.8-0.9
     - Emission: Enabled with cyan/blue glow

### Activation Logic
```
Show Menu When:
- Left hand palm facing camera (±30° tolerance)
- Hand within distance threshold
- Hand raised above minimum height

Hide Menu When:
- Palm facing away from camera
- Hand outside distance threshold
- Menu inactive timeout reached
```

---

## Wall Destruction

### Overview
Destructible room mesh that breaks into segments when shot, revealing passthrough.

### Setup
1. **DestructibleGlobalMeshManager** - Manages wall destruction
   - Listens to gun's `OnShootAndHit` event
   - Destroys mesh segments on hit
   - Spawns debris (optional, currently disabled)
   - Plays destruction audio

2. **Wall Configuration**
   - Non-convex MeshColliders (accurate hit detection)
   - Default layer (raycastable)
   - Segmented mesh from MRUK

3. **Ceiling Destruction**
   - Set `meshSpawner.ReservedTop = -1f` (allows ceiling destruction)

---

## Ghost AI

### Features
- NavMesh-based pathfinding
- Wanders room independently
- Hides when player looks at them
- Plays shocked sound on eye contact
- Freezes briefly when detected
- Explores furniture/objects
- One-hit kill on projectile impact

### Behavior States
1. **Wandering** - Explores room, ambient audio playing
2. **Detected** - Player looks at ghost, plays shocked sound
3. **Hiding** - Runs away behind furniture
4. **Lively** - Circles player when nearby

---

## Troubleshooting

### General Issues

**Gun not shooting:**
- Check OVRInput button assignment
- Verify shootingPoint is assigned
- Enable debug logging for diagnostics

**Projectiles phase through walls:**
- Verify using collision detection (not triggers)
- Check wall MeshColliders are non-convex and enabled
- Raycast backup should catch these

**Performance issues:**
- Disable debug logging in production
- Reduce projectile lifetime
- Limit max debris spawns

**Ghosts not dying:**
- Check ghost has collider
- Verify ghost has Ghost.cs component
- Enable debug logging to see hit detection

### Debug Mode

Enable `enableDebugLogging` on GunScript to see:
- Projectile spawn events
- Collision detection details
- Physics.IgnoreCollision calls
- Raycast backup triggers
- Hit registration events

Log format:
```
[GunScript] Created projectile at (x,y,z) (offset from origin)
[ProjectileController] Ignoring collision with gun/player
[ProjectileController] Raycast detected imminent collision
[ProjectileController] Projectile hit: WallSegment_012
```

---

## Performance Optimization

### Recommendations
1. **Disable debug logging in production builds**
2. **Limit projectile lifetime** to 2-3 seconds
3. **Use object pooling** for projectiles (future enhancement)
4. **Reduce debris spawn counts** or disable entirely
5. **Optimize ghost AI** check intervals (currently 0.1s)

### Current Performance
- 60 FPS target on Meta Quest 2/3
- Minimal GC allocation per shot
- Efficient collision detection
- Smooth projectile physics

---

## Version History

**December 7, 2024**
- Fixed projectile self-collision issues
- Implemented dual detection system (collision + raycast)
- Optimized ceiling hit detection
- Reduced gravity for straighter shots
- Increased projectile speed for reliability

**Earlier Versions**
- Implemented projectile mode
- Added wrist menu system
- Created wall destruction system
- Developed ghost AI with hiding behavior

---

## Credits

Built with:
- Unity 2022.3+
- Meta XR SDK
- MRUtilityKit for room mesh
- Universal Render Pipeline

---

## Quick Reference

### Key Files
- `Assets/Scripts/Combat/GunScript.cs` - Gun & projectile system
- `Assets/Scripts/Core/Ghost.cs` - Ghost AI
- `Assets/Scripts/MR_XR/DestructibleGlobalMeshManager.cs` - Wall destruction
- `Assets/Scripts/UI/WristMenuController.cs` - Wrist menu

### Prefab Locations
- Gun: `Assets/Prefabs/Weapons/Revolver/Gun.prefab`
- Projectile: `Assets/Prefabs/Weapons/Projectiles/Specter Ammo.prefab`
- Impact: `Assets/Prefabs/Weapons/Projectiles/Laser Impact.prefab`
- Ghost: `Assets/Prefabs/Enemies/Ghost.prefab`

### Important Settings
- Projectile Speed: 25
- Gravity Multiplier: 0.3
- Spawn Offset: 0.3m
- Collision Ignore Time: 0.1s
- Projectile Mass: 0.05kg
- Max Range: 5-10 units
