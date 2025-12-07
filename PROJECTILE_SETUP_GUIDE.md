# Projectile Gun Setup Guide

## Overview
Your gun has been successfully converted from laser mode to projectile mode! It now shoots your "Specter Ammo" sphere while keeping all the same functionality (ghost killing, wall destruction, hit detection, etc.).

## What Changed

### GunScript Modifications:
- ✅ Added **projectile mode** (shoots spheres)
- ✅ Kept **laser mode** as optional fallback
- ✅ **All existing functionality preserved**:
  - Raycast-based instant hit detection (damage/killing is instant)
  - Ghost killing
  - Wall destruction
  - Impact effects
  - Audio
  - All events (OnShoot, OnShootAndHit, OnShootAndMiss)
- ✅ **Collision detection**: Projectiles now disappear when they hit walls/ghosts/objects
- ✅ **Impact effects**: Spawn visual effects at collision points

### New Component:
- **ProjectileController**: Automatically added to projectiles to animate them from gun to target
  - Handles movement
  - Detects collisions with walls, ghosts, and furniture
  - Destroys projectile on impact
  - Creates impact effects at collision point

---

## Setup Instructions

### Step 1: Prepare Your Specter Ammo Prefab

1. **Find your "Specter Ammo" GameObject** in the Hierarchy

2. **Make it a Prefab**:
   - Drag it from Hierarchy → Project window (into `Assets/Prefabs/`)
   - This creates a reusable prefab

3. **Configure the Specter Ammo**:
   - **Size**: Adjust scale to your preference (e.g., 0.1 - 0.3 for a small projectile)
   - **Material**: Add a glowing/emission material for cool visual effect
   - **Collider**: A SphereCollider is automatically added by the gun (no need to add manually)
   - **NO Rigidbody needed** (movement is handled by ProjectileController, not physics)

4. **Optional - Make it Glow**:
   - Create a material for your sphere
   - Set Shader to `Universal Render Pipeline/Lit`
   - Set **Emission** color (e.g., bright cyan/blue)
   - Check **Emission** checkbox
   - Assign material to your sphere's MeshRenderer

### Step 2: Configure Gun Inspector

1. **Find your Gun GameObject** in the scene
   - Usually attached to `RightHandAnchor` or `RightControllerAnchor`

2. **In the Inspector, find the GunScript component**

3. **Configure these settings**:

   **Shooting Mode:**
   - ✅ **Use Projectile Mode**: Check this (enabled by default)
   
   **Projectile Mode:**
   - **Projectile Prefab**: Drag your "Specter Ammo" prefab here
   - **Projectile Speed**: `20` (adjust for faster/slower projectiles)
   - **Projectile Lifetime**: `2` (how long before auto-destroy)
   
   **Gun Settings:**
   - **Shooting Point**: Should already be set (the barrel tip)
   - **Max Line Distance**: `5` (how far projectile can travel)
   
   **Keep existing settings:**
   - Layer Mask
   - Shooting Button
   - Audio settings
   - Impact effects

### Step 3: Test In-Game

1. **Play the scene**

2. **Press the trigger** (right controller trigger by default)

3. **You should see**:
   - Specter Ammo sphere spawns at gun barrel
   - Flies towards where you're aiming
   - Hits and kills ghosts
   - Destroys walls
   - Creates impact effects

---

## How It Works: Dual Detection System

Your gun now uses a **hybrid system** that combines the best of both worlds:

### 1. **Instant Hit Detection (Raycast)**
When you pull the trigger:
- ✅ **Immediate raycast** fires from gun barrel
- ✅ Ghosts are killed **instantly** (no delay)
- ✅ Walls are destroyed **instantly**
- ✅ Hit events fire **immediately**
- ✅ This ensures responsive gameplay

### 2. **Visual Collision Detection (Projectile)**
At the same time:
- ✅ Specter Ammo sphere spawns and flies toward target
- ✅ Projectile has a **trigger collider** (SphereCollider)
- ✅ When projectile **touches** a wall/ghost/object, it **disappears**
- ✅ Optional impact effect spawns at collision point
- ✅ This provides visual feedback and realism

### Why This System?

**Instant damage** = Responsive, no lag, VR-friendly
**Visual projectile** = Player sees what happened, satisfying feedback
**Collision detection** = Projectile disappears naturally on contact

### What Objects Stop Projectiles?

Projectiles will disappear when they hit:
- ✅ Walls (destructible mesh)
- ✅ Ghosts
- ✅ Furniture (MRUK objects like tables, chairs)
- ✅ Any object with a collider

---

## Customization Options

### Projectile Appearance

**Make it a trail/streak:**
```
Add TrailRenderer component to Specter Ammo prefab:
- Time: 0.2-0.5
- Width: Curve from 0.2 to 0
- Material: Additive/glow material
- Color: Match your projectile color
```

**Make it glow brighter:**
```
Material settings:
- Emission Intensity: 2-5
- HDR Color: Enable
- Bloom (in URP Volume): Increase threshold
```

**Add particle effects:**
```
Add Particle System as child of Specter Ammo:
- Start Lifetime: 0.3
- Start Speed: 2
- Emission Rate: 20
- Size over Lifetime: Shrink curve
```

### Projectile Behavior

**Faster projectiles:**
- Increase `Projectile Speed` to 40-50+
- Feels more like bullets

**Slower projectiles:**
- Decrease `Projectile Speed` to 5-10
- Feels more like magic orbs

**Larger projectiles:**
- Increase scale of Specter Ammo prefab
- Try 0.3 - 0.5 for big energy balls

**Spinning projectiles:**
- Add to ProjectileController in `Update()`:
  ```csharp
  transform.Rotate(Vector3.forward * 360f * Time.deltaTime);
  ```

**Adjust collision size:**
The gun automatically adds a SphereCollider with radius 0.1. To adjust:
1. Find spawned projectile in Hierarchy during Play mode
2. Check SphereCollider radius in Inspector
3. If you want different size, modify this line in GunScript.cs:
   ```csharp
   collider.radius = 0.1f; // Change to match your sphere size
   ```
   - Smaller radius (0.05): Projectile only hits when very close
   - Larger radius (0.2): Projectile hits from further away

---

## Switching Between Laser and Projectile

The gun now supports both modes!

**To use Projectile mode (default):**
- ✅ Check "Use Projectile Mode" in Inspector
- Assign "Projectile Prefab"

**To use Laser mode:**
- ❌ Uncheck "Use Projectile Mode"
- Assign "Line Prefab" (LineRenderer)

You can switch between them at any time!

---

## Troubleshooting

### Projectiles don't appear
- Check "Projectile Prefab" is assigned in Inspector
- Check "Use Projectile Mode" is checked
- Check Specter Ammo has a MeshRenderer with material
- Check the sphere isn't too small (try scale 0.2)

### Projectiles spawn but don't move
- Check "Projectile Speed" is not 0
- Check shooting point is assigned
- Look in Console for errors

### Projectiles move wrong direction
- Shooting Point transform should point forward (blue arrow)
- Rotate the shooting point if needed

### Projectiles go through walls/ghosts without disappearing
- Check that objects have colliders (walls should have MeshCollider, ghosts should have colliders)
- Check Console for collision messages: `[ProjectileController] Projectile hit: ...`
- If no collision messages, objects may not have colliders
- Check projectile has SphereCollider (auto-added, but verify in Inspector)
- Check SphereCollider "Is Trigger" is checked ✓

### Can't see projectiles in scene
- Check material has proper shader (URP/Lit or URP/Unlit)
- Check Alpha is 1.0 (not transparent)
- Check layer is "Default" (not "Ignore Raycast")
- Try adding emission/glow to material

### Projectiles spawn at wrong position
- Check "Shooting Point" in Inspector
- Should be at gun barrel tip
- Create empty GameObject as child of gun, position at barrel

---

## Advanced: Projectile Trail Effect

For a cool energy ball with trail effect:

1. **Add TrailRenderer to Specter Ammo prefab**:
   - Component → Effects → Trail Renderer

2. **Configure Trail**:
   - **Time**: 0.3
   - **Min Vertex Distance**: 0.1
   - **Width**: 0.1 (or use curve)
   - **Color Gradient**: 
     - Start: Your projectile color (Alpha 1.0)
     - End: Same color (Alpha 0.0)
   - **Material**: Use a particle/additive material

3. **Create Trail Material** (if you don't have one):
   - Right-click in Project → Create → Material
   - Name it "Projectile Trail"
   - Shader: `Universal Render Pipeline/Particles/Unlit`
   - Surface Type: Transparent
   - Blend Mode: Additive
   - Base Map: Use a soft round texture or leave white

---

## Performance Tips

- Projectiles auto-destroy after 2 seconds (configurable)
- No physics calculations (uses simple movement)
- Efficient for VR/Mobile
- If spawning many projectiles, consider object pooling later

---

## Integration with Existing Systems

### Ghost Killing
✅ Works - Projectiles use raycast detection, ghosts die instantly

### Wall Destruction
✅ Works - DestructibleGlobalMeshManager receives hit events

### Wrist Menu
✅ Works - Gun still toggles off when menu opens

### Audio
✅ Works - Shooting sound still plays

### Events
✅ Works - All OnShoot/OnShootAndHit/OnShootAndMiss events fire

---

## Inspector Settings Reference

```
GunScript Component:

[Gun Settings]
├─ Layer Mask: Default + your layers
├─ Shooting Button: RIndexTrigger
├─ Shooting Point: (Transform at barrel tip)
├─ Max Line Distance: 5
└─ Projectile Lifetime: 2

[Shooting Mode]
└─ Use Projectile Mode: ✓ (checked)

[Projectile Mode]
├─ Projectile Prefab: Specter Ammo (prefab)
└─ Projectile Speed: 20

[Laser Mode (Legacy)]
├─ Line Prefab: (optional - for laser mode)
└─ Line Show Timer: 0.3

[Impact Effects]
└─ Ray Impact Prefab: (your impact effect)

[Audio]
├─ Source: (AudioSource component)
└─ Shooting Audio Clip: (your gun sound)

[Events]
├─ On Shoot
├─ On Shoot And Hit
└─ On Shoot And Miss
```

---

## Next Steps

1. ✅ Assign Specter Ammo prefab to Gun
2. ✅ Test shooting in Play mode
3. Optional: Add glow/trail effects to projectile
4. Optional: Adjust projectile speed/size
5. Optional: Create multiple projectile types (different colors/sizes)

Enjoy your new projectile-shooting gun! 🎯

