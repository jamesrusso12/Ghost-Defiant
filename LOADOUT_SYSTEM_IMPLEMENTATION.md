# Pre-Game Loadout System Implementation

## Goal
Implement a pre-game initialization phase for a Unity Mixed Reality (MR) game where:
1. The game does **not** start immediately when the scene loads
2. The player must physically **pick up their equipment** (Gun and Flashlight) from the floor
3. Items spawn on the **physical floor** of the player's real room using Meta's MR Utility Kit (MRUK)
4. Items are **always visible** and **inside the room** (not outside walls or inside furniture)
5. Once both items are collected, the **main game loop starts** (enemy waves begin)

---

## Challenges Encountered & Solutions

### 1. Unity Crash Issues
**Problem:** Initial implementation using reflection to detect ISDK `Grabbable` components caused Unity to crash on scene load.

**Solution:** 
- Deleted `Library`, `Temp`, and `obj` folders to force a clean rebuild
- Removed all reflection-based component detection
- Switched to a simpler "dummy pickup" system instead of complex interaction SDK grabbing

### 2. Invisible Objects
**Problem:** Objects spawned but were invisible in builds.

**Root Cause:** 
- Gun prefab had disabled child mesh objects
- Items were falling through the floor before player could see them
- Scale inconsistencies (import scale 0.01 vs runtime scale)

**Solution:**
- Added `EnableAllChildRenderers()` to force-enable mesh renderers
- Disabled gravity on spawned items (`rb.useGravity = false`) so they float in place
- Added bright red debug spheres to verify spawn positions

### 3. Spawning Outside Room or Inside Furniture
**Problem:** Items would spawn:
- Outside the room boundaries (in passthrough void)
- Inside furniture (bed, couch, tables)
- Inconsistently (50/50 success rate)

**Solution:**
- Implemented **transactional paired spawning**: find valid positions for BOTH items before instantiating either
- Added `room.IsPositionInRoom(pos, true)` validation for every spawn candidate
- Added `Physics.OverlapBox` obstacle detection to avoid furniture
- Increased retry attempts and added multiple fallback strategies:
  1. Try `GenerateRandomPositionOnSurface` with FLOOR label
  2. Fallback to `GenerateRandomPositionInRoom` 
  3. Last resort: Room bounds center
  4. Ultimate fallback: Camera-relative floating spawn

### 4. Unnatural Floor Placement
**Problem:** Items floated unnaturally or stood upright instead of lying on their side.

**Solution:**
- Aligned items to floor normal from MRUK
- Added configurable rest pose (`gunRestEuler`, `flashlightRestEuler`)
- Added random yaw and tilt for organic "dropped" appearance
- Optional settle animation (items start slightly elevated and drop into place)

### 5. Gun Wouldn't Shoot / Flashlight Passthrough Missing
**Problem:** 
- Gun appeared in hand but wouldn't fire when trigger pressed
- Flashlight appeared but secondary passthrough cone stayed disabled

**Solution:**
- Added `isEquipped` flag to `GunScript` 
- `LoadoutManager` calls `GunScript.SetEquipped(true)` on pickup
- Added `enableOnGunCollected` and `enableOnFlashlightCollected` arrays in `LoadoutManager` to toggle additional scene objects (like passthrough cone)

---

## Final Implementation

### Core Components

#### 1. **LoadoutManager.cs**
Main orchestrator that handles the entire loadout sequence.

**Responsibilities:**
- Waits for scene initialization and MRUK to be ready
- Spawns dummy pickup items on the floor at validated positions
- Monitors pickup state via `SimplePickup` callbacks
- Activates real equipment in player's hands when dummies are collected
- Starts the game loop after all items collected

**Key Features:**
- Transactional paired spawning (both items or neither)
- Multiple validation layers:
  - `room.GenerateRandomPositionOnSurface(FACING_UP, FLOOR)`
  - `room.IsPositionInRoom(pos, true)`
  - `IsVerticalClear()` physics overlap check
- Configurable spawn attempts, clearances, and rest poses
- Fallback strategies if MRUK fails

#### 2. **SimplePickup.cs**
Component attached to dummy floor items.

**Responsibilities:**
- Detects collision with player hands/controllers
- Notifies `LoadoutManager` when picked up
- Self-destructs after pickup

**Detection Methods:**
- `OnTriggerEnter` (if collider is trigger)
- `OnCollisionEnter` (fallback)
- Checks for "hand", "controller", or "player" in name/tag

#### 3. **GameManager.cs** (Modified)
Added support for deferred game start.

**Changes:**
- Added `enableIntroSequence` bool flag
- Added `StartGame()` public method
- Modified `Start()` to conditionally defer `StartNextRound()` call

#### 4. **GunScript.cs** (Modified)
Added equipment state checking.

**Changes:**
- Added `requireEquippedToShoot` flag and `isEquipped` state
- Added `SetEquipped(bool)` method
- Modified `Update()` to check equipped state before allowing firing
- Improved `ProjectileController` with `SweepTest` instead of raw raycast

---

## Workflow

### Scene Start Sequence
1. `LoadoutManager.Start()` hides real gun/flashlight in hands
2. Waits for `initialDelay` (default 2 seconds)
3. Waits for MRUK initialization (max 3 second timeout)
4. Finds valid floor spawn positions (validated, obstacle-free, paired)
5. Instantiates dummy gun and flashlight prefabs

### Dummy Items
- Simple visual-only prefabs with:
  - Mesh Renderer (the gun/flashlight model)
  - Rigidbody (useGravity = **false**, isKinematic = **false**)
  - Box Collider (isTrigger = **true**)
  - SimplePickup script
- Spawned with:
  - Floor-aligned rotation + configurable rest pose
  - Random yaw/tilt for natural appearance
  - Optional settle animation
  - Red debug sphere marker (can be disabled)

### Pickup Detection
1. Player reaches out and touches dummy item
2. `SimplePickup.OnTriggerEnter` detects hand collision
3. Calls `LoadoutManager.OnItemPickedUp(itemType)`
4. `LoadoutManager`:
   - Activates real item in hand (`SetActive(true)`)
   - Enables all child renderers
   - Calls `GunScript.SetEquipped(true)` for gun
   - Enables extra objects (flashlight passthrough cone, etc.)
   - Destroys dummy item
5. Checks if both items collected → starts game

### Game Start
1. `LoadoutManager.CheckAllCollected()` detects both items picked up
2. Calls `GameManager.instance.StartGame()`
3. `GameManager.StartNextRound()` begins enemy spawning

---

## Key Files

### Created
- `Assets/Scripts/Core/LoadoutManager.cs` - Main loadout orchestrator
- `Assets/Scripts/Core/SimplePickup.cs` - Dummy item pickup detection
- `Assets/Scripts/Core/LoadoutItem.cs` - (Created but ultimately unused; replaced by SimplePickup)

### Modified
- `Assets/Scripts/Core/GameManager.cs` - Added intro sequence support
- `Assets/Scripts/Combat/GunScript.cs` - Added equip gating and improved projectile physics

---

## Inspector Configuration

### LoadoutManager Settings

**Dummy Prefabs:**
- Stripped-down visual-only versions of gun/flashlight
- Must have: MeshRenderer, Rigidbody, Trigger Collider, SimplePickup

**Real Items:**
- Drag actual gun/flashlight from scene hierarchy (children of hand anchors)
- These start disabled and activate on pickup

**Flashlight/Gun Extras:**
- Array of additional GameObjects to enable on pickup
- Used for passthrough cone, building block components, etc.

**Snap (Orientation):**
- Gun Mount / Flashlight Mount: Transform references (usually ControllerInHandAnchors)
- Local Position/Euler Offsets: Fine-tune in-hand placement

**Spawn Settings:**
- Initial Delay: 2s
- Min Edge Distance: 0.3
- Max Spawn Attempts: 200-400
- In Room Min Radius: 0.2
- Max Pair Spawn Attempts: 50-100
- Min Distance Between Items: 0.5-0.6

**Furniture/Obstacle Avoidance:**
- Spawn Blocker Mask: Layers with furniture colliders
- Spawn Clearance Radius: 0.18-0.24
- Spawn Clearance Height: 0.45-0.6

**Spawn Look (Natural Rest Pose):**
- Animate Settle: On/Off
- Settle Start Height: 0.12
- Settle Duration: 0.25
- Random Yaw Degrees: 180
- Random Tilt Degrees: 4-8
- Gun Rest Euler: (0, 0, ±90) to lay on side
- Flashlight Rest Euler: (90, 0, 0) or as needed

---

## Common Issues & Fixes

### Items don't spawn at all
- Check Console for `[LoadoutManager]` logs
- Verify MRUK is initialized and has a room
- Check dummy prefabs are assigned
- Try checking "Debug Force Start" to bypass spawning

### Items spawn outside room
- Increase `Max Pair Spawn Attempts` to 100+
- Verify MRUK room data is valid (has floor anchors)
- Check `Spawn Blocker Mask` isn't blocking floor itself

### Items spawn inside furniture
- Increase `Spawn Clearance Height` to 0.6-0.8
- Increase `Spawn Clearance Radius` to 0.3
- Verify furniture has colliders on correct layer

### Items fall through floor
- Verify `useGravity = false` is set in spawn code
- Check floor has a collider (though disabled gravity should prevent falling anyway)

### Gun won't shoot
- Verify `GunScript.requireEquippedToShoot` is checked
- Check `LoadoutManager` calls `SetEquipped(true)` in `OnItemPickedUp`
- Verify shooting button is assigned and working

### Flashlight missing passthrough effect
- Drag passthrough cone GameObject into `Enable On Flashlight Collected` array
- Ensure cone object starts disabled in scene

### Items oriented wrong on floor
- Adjust `Gun Rest Euler` / `Flashlight Rest Euler`
- For gun lying on side: try `(0, 0, 90)` or `(0, 0, -90)`
- Set Random Yaw/Tilt to 0 while testing to isolate rest pose

### Items positioned wrong in hand
- Adjust `Gun Local Position Offset` / `Gun Local Euler Offset`
- Verify Gun Mount / Flashlight Mount are correct hand anchor references
- Try different anchors (RightControllerAnchor vs RightControllerInHandAnchor)

---

## Technical Notes

### Why "Dummy Pickup" Instead of ISDK Grab?
- Meta Interaction SDK `Grabbable` API varies between versions
- Physics interaction with MR floor colliders is unreliable
- Simpler "touch to equip" feels more like a tutorial/loot system
- Avoids complex hand pose recording and transformer setup
- More reliable across different SDK versions

### Why Disable Gravity on Spawned Items?
- MR floor colliders often load after scene start
- Items would fall through floor before collider is ready
- Floating items are easier to spot in mixed reality
- Prevents items rolling under furniture or out of bounds

### MRUK Spawn Strategy Hierarchy
1. **Primary:** `GenerateRandomPositionOnSurface(FACING_UP, FLOOR)` + validation
2. **Secondary:** `GenerateRandomPositionInRoom()` + validation
3. **Tertiary:** Room bounds center + validation
4. **Fallback:** Camera-relative floating spawn (chest level)

Each validated with:
- `room.IsPositionInRoom(pos, true)`
- `IsVerticalClear()` obstacle check
- Paired spawn (both items found before instantiation)
- Minimum separation distance

---

## Build Testing Checklist

Before each APK build:
1. Verify `GameManager.enableIntroSequence` is **checked**
2. Real gun/flashlight in hands start **disabled**
3. Dummy prefabs assigned and have correct components
4. `LoadoutManager` references valid scene objects (not prefabs)
5. Spawn Blocker Mask includes furniture layers
6. Gun/Flashlight Mount transforms assigned
7. No compile errors in Console

Expected behavior:
1. Scene loads → hands empty
2. After 2s → items spawn on floor with settle animation
3. Touch gun → gun vanishes → real gun appears in hand and can shoot
4. Touch flashlight → flashlight vanishes → real flashlight + cone appear
5. Both collected → enemy waves begin

---

## Summary
This system provides a natural tutorial/orientation phase that:
- Ensures player is physically oriented in their space
- Forces player to look around their room before combat
- Creates a satisfying "gear up" moment
- Eliminates the jarring "guns already in hand" start
- Acts as a soft introduction to the MR environment before chaos begins

The implementation prioritizes **reliability** (multiple fallbacks) and **configurability** (extensive Inspector controls) over complex SDK interactions, making it stable across different Meta XR SDK versions and room setups.
