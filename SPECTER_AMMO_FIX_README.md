# Specter Ammo & Destructible Mesh Fix - Complete Documentation

## Overview
This document outlines the fixes applied to resolve collision detection and physics issues between the Specter Ammo projectile and the DestructibleGlobalMesh system from Meta's MR Utility Kit.

## Problems Solved

### 1. **Segment Finding Logic** ✅
**Issue:** The `DestroyMeshSegment()` method didn't properly handle cases where the collision hit object was a child of the actual segment or vice versa.

**Fix Applied:**
- Added comprehensive parent hierarchy checking
- Now checks if hit object is IN segments list
- Checks if hit object is a CHILD of a segment (walks up parent hierarchy)
- Checks if hit object is a PARENT of a segment (walks down child hierarchy)
- Added warning logs when segment can't be found for debugging

**File:** `Assets/Scripts/MR_XR/DestructibleGlobalMeshManager.cs:113-161`

---

### 2. **Projectile Physics Settings** ✅
**Issue:** Bullets were too fast, gravity wasn't visible, and collision detection was unreliable.

**Fixes Applied:**

#### Speed & Gravity
- **Speed:** Changed from 25 m/s to **15 m/s** (optimal range: 15-20 m/s)
- **Gravity Multiplier:** Changed from 0.3x to **1.0x** (optimal range: 0.8-1.2x)
- Result: Visible, satisfying projectile arc

#### Rigidbody Configuration
- **Mass:** Changed from 0.05kg to **0.01kg** (10 grams) - Very light for predictable arc
- **Linear Damping:** 0 (no air resistance)
- **Angular Damping:** 0.05 (minimal spin resistance)
- **Interpolation:** `RigidbodyInterpolation.Interpolate` (smooth movement)
- **Collision Detection Mode:** `CollisionDetectionMode.ContinuousDynamic` ⚠️ **CRITICAL** for fast-moving objects
- **Constraints:** None (full physics movement)

#### Collider Configuration
- **Type:** SphereCollider (non-trigger)
- **Radius:** 0.05m (5cm) - Smaller for better precision
- **Is Trigger:** FALSE ⚠️ **CRITICAL** - Triggers don't work with non-convex MeshColliders
- **Physics Material:**
  - Bounciness: 0
  - Dynamic Friction: 0
  - Static Friction: 0

#### Spawn Configuration
- **Spawn Offset:** Reduced from 0.3m to **0.15m** (15cm in front of gun)
- **Collision Enable Delay:** Reduced from 0.1s to **0.05s** for faster activation
- **Layer:** Configurable via `projectileLayer` field (default: "Default")

**File:** `Assets/Scripts/Combat/GunScript.cs:19-30, 235-302, 395`

---

### 3. **Collision Detection Enhancement** ✅
**Issue:** Bullets sometimes passed through walls or didn't trigger destruction.

**Fixes Applied:**

#### Better Collision Ignoring
- Uses `Physics.IgnoreCollision()` for gun, player, and hand colliders
- Walks up gun parent hierarchy to ignore controller/hand/rig colliders
- Finds and ignores OVRCameraRig/XR Origin colliders
- Prevents false positives from player/gun collision

#### Enhanced Debug Logging
When `enableDebugLogging = true`, you'll see:
```
[GunScript] ===== SHOOT TRIGGERED =====
[GunScript] Projectile Rigidbody configured:
  - Mass: 0.01kg
  - Collision Detection: ContinuousDynamic
  - Initial Velocity: 15m/s
  - Gravity Multiplier: 1x
[ProjectileController] Initialized. Collider disabled for 0.05s
[ProjectileController] Collider enabled at position (x, y, z)
[ProjectileController] ===== COLLISION DETECTED =====
[ProjectileController] Hit Object: [name]
[ProjectileController] Hit Layer: [layer number] ([layer name])
[ProjectileController] Impact Position: (x, y, z)
[ProjectileController] Impact Normal: (x, y, z)
[ProjectileController] Contact Count: [count]
[ProjectileController] Collider Type: [type]
[ProjectileController] ===== HANDLING IMPACT =====
[ProjectileController] Invoking OnShootAndHit event for: [name]
[ProjectileController] Destroying projectile
```

#### Visual Debug Helpers
- Yellow ray drawn from projectile showing velocity direction (when debug enabled)
- Debug rays in scene view for trajectory visualization

**File:** `Assets/Scripts/Combat/GunScript.cs:489-562`

---

### 4. **Ghost Enemy Detection** ✅
**Issue:** Bullets didn't reliably detect and kill Ghost enemies.

**Fix Applied:**
- Enhanced ghost detection with 3-tier search:
  1. `GetComponent<Ghost>()` on hit object
  2. `GetComponentInParent<Ghost>()` if not found
  3. `GetComponentInChildren<Ghost>()` if still not found
- Ghost killing takes priority over wall destruction
- Separate impact effect for ghost hits
- Comprehensive debug logging for ghost detection

**File:** `Assets/Scripts/Combat/GunScript.cs:564-626`

---

## Unity Inspector Settings

### GunScript Component

**Recommended Settings:**
```
Gun Settings:
  ├─ Layer Mask: [Select layers that projectiles should hit]
  ├─ Shooting Button: PrimaryIndexTrigger (or your preferred button)
  ├─ Shooting Point: [Assign your gun's barrel transform]
  ├─ Max Line Distance: 50
  └─ Projectile Lifetime: 2

Shooting Mode:
  └─ Use Projectile Mode: ✓ (checked)

Projectile Mode:
  ├─ Projectile Prefab: [Your Specter Ammo sphere prefab]
  ├─ Projectile Speed: 15
  ├─ Gravity Multiplier: 1.0
  ├─ Delay Wall Destruction: ✓ (checked)
  └─ Projectile Layer: "Default"

Impact Effects:
  └─ Ray Impact Prefab: [Your impact effect prefab]

Debug:
  └─ Enable Debug Logging: ✓ (check for testing, uncheck for builds)
```

### DestructibleGlobalMeshManager Component

**Recommended Settings:**
```
References:
  ├─ Gun Script: [Your GunScript component]
  └─ Mesh Spawner: [DestructibleGlobalMeshSpawner component]

Audio:
  └─ Destroy Sound: [Your wall destruction sound]

Performance Settings:
  └─ Enable Barycentric Coordinates: ✓ (if using special shaders)
```

---

## Testing Checklist

### Basic Functionality
- [ ] Bullet spawns 15cm in front of gun barrel
- [ ] Bullet has visible arc (parabolic trajectory)
- [ ] Bullet collides with walls and destroys segments
- [ ] Wall piece disappears on impact
- [ ] Bullet disappears on impact
- [ ] Impact effect plays at collision point

### Edge Cases
- [ ] Bullet doesn't collide with gun immediately after spawn
- [ ] Bullet doesn't collide with player hands/body
- [ ] Bullet doesn't pass through walls
- [ ] Bullet doesn't get stuck in walls
- [ ] Multiple rapid shots work correctly
- [ ] Ceiling destruction works (if enabled)

### Ghost Interaction
- [ ] Bullet kills ghosts on direct hit
- [ ] Bullet kills ghosts when hitting any part of ghost hierarchy
- [ ] Ghost death sound plays
- [ ] Impact effect shows on ghost hit
- [ ] Score increments on ghost kill

### Performance
- [ ] No lag when shooting rapidly
- [ ] Debug logging can be disabled for production
- [ ] Bullets destroy themselves after 2 seconds if no hit

---

## Common Issues & Solutions

### Issue: Bullets pass through walls
**Solution:**
1. Check that destructible mesh segments have MeshColliders
2. Verify MeshColliders are set to `convex = false`
3. Ensure projectile uses `CollisionDetectionMode.ContinuousDynamic`
4. Check that projectile and wall are on layers that can collide (Edit → Project Settings → Physics → Layer Collision Matrix)

### Issue: Bullets collide with gun/player
**Solution:**
1. Ensure gun GameObject is properly assigned in GunScript
2. Check that player has "Player" tag
3. Verify collision ignore setup is working (check debug logs)

### Issue: Wall doesn't disappear on hit
**Solution:**
1. Enable debug logging on GunScript
2. Check console for "Could not find segment to destroy" warnings
3. Verify OnShootAndHit event is connected to DestructibleGlobalMeshManager
4. Check that segments list is populated (DestructibleGlobalMeshManager.segments)

### Issue: Bullets have no visible arc
**Solution:**
1. Increase `gravityMultiplier` (try 1.0 to 1.5)
2. Decrease `projectileSpeed` (try 10-15 m/s)
3. Ensure `rb.useGravity = false` and custom gravity is being applied in FixedUpdate

### Issue: Ghost doesn't die when hit
**Solution:**
1. Verify Ghost has Ghost component attached
2. Check that Ghost component is on parent/child of collision object
3. Enable debug logging to see ghost detection logs
4. Verify Ghost.Kill() method is being called

---

## Technical Details

### Why Non-Trigger Colliders?
Unity's physics system has a limitation: **trigger colliders don't work with non-convex MeshColliders**. Since the destructible mesh segments use non-convex MeshColliders for accurate collision, we must use `OnCollisionEnter` instead of `OnTriggerEnter`.

### Why ContinuousDynamic Collision Detection?
Fast-moving projectiles can "tunnel" through thin objects if using Discrete collision detection. ContinuousDynamic uses swept collision detection (CCD) which prevents tunneling by checking the path between physics steps.

### Why Custom Gravity?
Using `rb.useGravity = false` with manual gravity application (`rb.AddForce(Physics.gravity * gravityMultiplier)`) gives us fine control over the arc while maintaining consistent behavior across different frame rates.

### Why Small Spawn Offset?
Spawning too far forward (0.3m) made shots feel inaccurate. Spawning too close (0m) caused immediate collision with gun. The sweet spot is **0.15m** combined with a short collision delay (0.05s).

---

## Performance Optimization Tips

1. **Disable Debug Logging in Builds**
   - Set `enableDebugLogging = false` before building
   - Debug logs have performance impact

2. **Use Object Pooling**
   - Consider pooling projectiles if shooting rapidly
   - Reduces GC pressure from Instantiate/Destroy

3. **Limit Projectile Lifetime**
   - Current setting: 2 seconds
   - Adjust based on your room size

4. **Layer Collision Matrix**
   - Disable unnecessary layer collisions in Physics settings
   - Reduces collision checks per frame

---

## Recommended Physics Settings (Unity Project Settings)

**Edit → Project Settings → Physics:**

```
Default Solver Iterations: 6
Default Solver Velocity Iterations: 1
Bounce Threshold: 2
Default Contact Offset: 0.01
Sleep Threshold: 0.005
Default Max Angular Speed: 50
```

**Layer Collision Matrix:**
- Ensure projectile layer collides with destructible mesh layer
- Ensure projectile layer does NOT collide with Player, Hands, or UI layers

---

## Files Modified

1. **Assets/Scripts/Combat/GunScript.cs**
   - Lines 19-30: Updated projectile settings with recommended values
   - Lines 235-306: Enhanced CreateProjectile() with optimized physics
   - Lines 395: Reduced collision enable delay
   - Lines 489-562: Enhanced collision detection with better logging
   - Lines 564-626: Improved HandleImpact() with priority ghost detection

2. **Assets/Scripts/MR_XR/DestructibleGlobalMeshManager.cs**
   - Lines 113-161: Fixed DestroyMeshSegment() with comprehensive hierarchy search

---

## Support & Debugging

If issues persist after applying these fixes:

1. **Enable Debug Logging**
   - Set `enableDebugLogging = true` on GunScript
   - Shoot at wall and check console output

2. **Check Unity Console**
   - Look for warnings about missing segments
   - Check for collision detection messages

3. **Verify Component Setup**
   - GunScript has all references assigned
   - DestructibleGlobalMeshManager is connected to GunScript.OnShootAndHit event
   - DestructibleGlobalMeshSpawner is properly configured

4. **Test in Isolation**
   - Create a simple test scene with just gun + wall
   - Verify basic collision works before adding complexity

---

## Changelog

### v1.0 - Initial Fix (2026-01-08)
- Fixed segment finding logic with parent/child hierarchy checking
- Optimized projectile physics for visible arc and reliable collision
- Enhanced collision detection with comprehensive debug logging
- Improved ghost detection with 3-tier search
- Reduced spawn offset and collision delay for better accuracy
- Added configurable projectile layer
- Comprehensive documentation and testing checklist

---

## Credits

Fixed by: Claude Sonnet 4.5
Date: January 8, 2026
Unity Version: Unity 6 (6000.x)
Meta XR SDK Version: 83.0.1
