# Complete Start Menu Setup Guide - Meta XR SDK

## 📋 Table of Contents
1. [Quick Start (3 Minutes)](#quick-start-3-minutes)
2. [What You're Building](#what-youre-building)
3. [Prerequisites](#prerequisites)
4. [Full Setup Instructions](#full-setup-instructions)
5. [Configuration Reference](#configuration-reference)
6. [Troubleshooting](#troubleshooting)
7. [Advanced Customization](#advanced-customization)
8. [Alternative: Fix Existing Menu](#alternative-fix-existing-menu)

---

## Quick Start (3 Minutes)

### ⚡ Your Requirements
- **Passthrough mode** (MR - floating menu in real world)
- **Canvas size: 1920×1080** (matching your screenshot)
- **Slowly follows** player (Smooth Speed: 2.0)
- **Always faces** player (billboard behavior)
- White/light background panel
- Start and Quit buttons

### ⚡ Essential Settings

**Canvas (MenuCanvas):**
```yaml
Render Mode: World Space
Width: 1920
Height: 1080
Scale: 0.001, 0.001, 0.001
```

**Billboard (StartMenuPanel):**
```yaml
Script: BillboardMenu
Smooth Follow: ✓
Smooth Speed: 2.0  ← SLOW following
Keep Upright: ✓
Distance: 2.5
Offset Y: 0
```

**Passthrough:**
```yaml
OVRPassthroughLayer: In scene
OVRManager → Insight Passthrough: ✓ Enabled
Background Panel: White (255, 255, 255)
```

### ⚡ Quick Steps

1. **Enable Passthrough:** Building Blocks → Add "Passthrough Layer"
2. **Create Panel:** Empty GameObject → "StartMenuPanel"
3. **Add Canvas:** UI → Canvas (1920×1080, scale 0.001)
4. **Add OVROverlayCanvas** to Canvas
5. **Add White Background Panel**
6. **Add UI:** Game title, Start button, Quit button
7. **Add BillboardMenu** to StartMenuPanel (Speed: 2.0)
8. **Build & Test on Quest**

### ✅ Quick Checklist

- [ ] OVRPassthroughLayer in scene
- [ ] Canvas: 1920×1080, scale 0.001
- [ ] BillboardMenu: Smooth Speed 2.0
- [ ] White background panel
- [ ] Game title image added
- [ ] Buttons wired to StartMenuController

---

## What You're Building

### Professional VR/MR Start Menu

**Features:**
- ✅ **Passthrough Mode** - Floating menu in real world (MR)
- ✅ **Slow Following** - Menu drifts to stay in front (comfortable)
- ✅ **Always Facing** - Billboard effect (no neck strain)
- ✅ **Proper Size** - 1920×1080 canvas (readable, not overwhelming)
- ✅ **VR Interaction** - Works with hands and controllers
- ✅ **High Performance** - OVROverlayCanvas optimization
- ✅ **Game Title** - Your logo displayed
- ✅ **Start/Quit Buttons** - With haptic feedback

**Scene Hierarchy:**
```
StartScreen
├── OVRCameraRig (with Passthrough enabled)
├── OVRPassthroughLayer
├── EventSystem
└── StartMenuPanel
    ├── BillboardMenu (slow following)
    └── MenuCanvas (1920×1080)
        ├── SolidBackground (White panel - full size)
        ├── MenuBackground (Your title image 500×700)
        └── ButtonContainer (Horizontal Layout Group)
            ├── StartButton
            └── QuitButton
```

---

## Prerequisites

### Required
- ✅ Unity project with Meta XR SDK v62+
- ✅ StartScreen scene
- ✅ Game title image: `Assets/Textures/UI/Game Title (1).png`
- ✅ Quest 2, Quest 3, or Quest Pro (for passthrough)

### Check Your Setup
1. Window → Package Manager
2. Verify "Meta XR Core SDK" is installed
3. Verify "Meta XR Interaction SDK" is installed

---

## Full Setup Instructions

### Part 1: Scene Preparation

#### Step 1: Open Your Scene
1. Open `Assets/Scenes/MainGameScene/StartScreen.unity`
2. **Disable old menu** (if exists):
   - Find old StartMenu in Hierarchy
   - Uncheck to deactivate (keep as reference)

#### Step 2: Enable Passthrough Mode

**Using Building Blocks (Recommended):**
1. Click Scene view
2. Press **`~` (tilde)** key to open Building Blocks overlay
3. Find **"Passthrough"** section
4. Click **"Passthrough Layer"** to add

**OR Manually:**
- GameObject → XR → Passthrough → Passthrough Layer

**Configure OVRManager:**
1. Find OVRManager (on OVRCameraRig or separate GameObject)
2. Create if missing: GameObject → XR → OVR → OVRManager
3. Inspector settings:
   ```
   Insight Passthrough: ✓ Enabled
   Pass-through Support: Supported
   ```

#### Step 3: Add/Verify OVR Camera Rig

**Check if exists:**
- Look for "OVRCameraRig" in Hierarchy
- If found, skip to Step 4

**Add if missing:**
1. Building Blocks → "Camera Rig" OR
2. GameObject → XR → Camera Rig → OVR Camera Rig

**Verify structure:**
```
OVRCameraRig
├── TrackingSpace
├── LeftHandAnchor
├── RightHandAnchor
└── CenterEyeAnchor
    └── Camera (Main Camera)
```

#### Step 4: Add Interaction Support

**Using Building Blocks:**
1. Building Blocks → "Hand Interaction"
2. Building Blocks → "Controller Interaction"

**OR Manually:**
- GameObject → XR → Interaction → Hand Interaction
- GameObject → XR → Interaction → Controller Interaction

**Result:** Hands/controllers can point and click UI

#### Step 5: Add EventSystem

**Check if exists:**
- Look for "EventSystem" in Hierarchy

**Add if missing:**
- GameObject → UI → Event System

**Verify components:**
- EventSystem component
- OVRInputModule (or StandaloneInputModule)

**Note:** The current Meta XR SDK uses OVRInputModule for VR input handling.

---

### Part 2: Create the Menu

#### Step 6: Create Menu Panel GameObject

1. Hierarchy → Right-click → Create Empty
2. Rename: **"StartMenuPanel"**
3. Transform:
   ```
   Position: (0, 1.5, 2.5)
   Rotation: (0, 0, 0)
   Scale: (1, 1, 1)
   ```
   *(Position will be overridden by billboard script)*

#### Step 7: Add Canvas

1. Select StartMenuPanel
2. Right-click → UI → Canvas
3. Rename: **"MenuCanvas"**

**Configure Canvas Component:**
```
Render Mode: World Space
Camera: (leave empty - will auto-assign to CenterEyeAnchor)
Plane Distance: 100 (default is fine)
Receives Events: ✓ (checked)
```

**Note:** In current Unity versions, Sorting Layer options are hidden by default. You can access them by checking "Override Sorting" if needed, but it's not required for this setup.

**Configure RectTransform:**
```
Width:  1920  ← Matching your screenshot
Height: 1080
Scale:  X: 0.001  ← CRITICAL!
        Y: 0.001
        Z: 0.001
Position: (0, 0, 0)
Rotation: (0, 0, 0)
```

**Result:** Menu is 1.92m × 1.08m in world space

#### Step 8: Add OVROverlayCanvas

**For better performance in passthrough:**
1. Select MenuCanvas
2. Add Component → **OVROverlayCanvas**

**⚠️ IMPORTANT: Understanding Layer Behavior in SDK v83+ **

**NEW in Meta XR SDK v83+:** Canvas Layer and GameObject Layer are now **automatically synced**!

**What this means:**
- When you change "Canvas Layer" in OVROverlayCanvas, GameObject layer changes to match
- When you change GameObject layer, Canvas Layer changes to match
- They CANNOT be set to different values anymore
- **This is intentional SDK behavior, not a bug**

**You'll see this error/warning:**
```
"This GameObject's Layer is the same as Overlay Layer ('Overlay UI')"
```

**The "error" is actually informational in v83+. Here's what to do:**

**Option 1: Set Both to "UI" (Recommended)**
1. **Before adding OVROverlayCanvas**, set GameObject layer to "UI"
2. Add OVROverlayCanvas component
3. In OVROverlayCanvas, set Canvas Layer to "UI" (GameObject will auto-sync)
4. Set Overlay Layer to "Overlay UI" (this can be different)

**After setup:**
```
GameObject Layer (top of Inspector): UI ← Set this first
OVROverlayCanvas Component Settings:
  Canvas Layer: UI ← Synced with GameObject
  Overlay Layer: Overlay UI ← This one can be different
  Canvas Shape: Flat
  Overlay Type: Underlay
  Opacity: Transparent
```

**Option 2: Set Both to "Overlay UI"**
1. GameObject layer: Overlay UI
2. Canvas Layer: Overlay UI (auto-synced)
3. Overlay Layer: Overlay UI

**Option 3: Ignore the Warning**
- The warning/error won't prevent your menu from working
- It's more of an informational message in the current SDK
- Your menu will render correctly either way

**Why the SDK changed this:**
- Simplifies layer management
- Reduces configuration errors
- Canvas Layer and GameObject Layer are now treated as one unified setting

**⚠️ You'll see another warning after fixing this:**
```
"Main Camera 'CenterEyeAnchor' does not cull this GameObject's Layer 'UI'. 
This Canvas might be rendered by both the Main Camera and the OVROverlay system."
```

**This warning is optional to fix** - your menu will work either way. But for best performance:

**Option 1: Remove UI from Camera Culling Mask (Recommended)**
1. Select **OVRCameraRig → CenterEyeAnchor** in Hierarchy
2. Find the **Camera** component
3. Look for **Culling Mask** dropdown
4. **Uncheck "UI"** layer (leave other layers checked)
5. This prevents double-rendering of your UI

**Option 2: Click the Helper Button**
- Unity shows a button: **"Remove UI from Camera cullingMask"**
- Click it and Unity fixes it automatically

**Option 3: Ignore It**
- Menu will still work fine
- Might have minor performance impact (rendering twice)
- Choose this if you want other UI elements to be visible to the main camera

#### Step 9: Add Background Panel

**Makes menu visible in passthrough:**

**OPTION A: If you have a custom background image with your title on it**
*(Like "Game Title (1).png" that includes both background and title)*

1. Select MenuCanvas
2. Right-click → UI → Image (not Panel)
3. Rename: **"MenuBackground"**
4. Add your image (see Step 10 for import instructions)
5. In Image component, click **"Set Native Size"** button
   - This sets it to the image's actual size (e.g., 500×700)
   - OR manually set Width/Height to your image dimensions
6. Configure Image component:
   ```
   Preserve Aspect: ✓ (keeps proportions correct)
   ```
7. Position it on the canvas:
   ```
   Anchor Preset: Middle-Center (or Top-Center)
   Pos X: 0 (centered)
   Pos Y: 0 (or adjust to taste)
   ```
8. **Add a background Panel** behind it if you want a solid color:
   - Right-click MenuCanvas → UI → Panel
   - Rename: "SolidBackground"
   - Color: White or light color
   - Stretch to fill canvas
   - In Hierarchy, **drag it ABOVE MenuBackground** (so it renders behind)
9. **Skip Step 10** since your title is already on the image
10. Continue to Step 11 (buttons) - position them around your background image

**OPTION B: If you want a simple colored background**
*(For adding a separate logo/title later)*

1. Select MenuCanvas
2. Right-click → UI → Panel
3. Rename: **"Background"**
4. Configure Image Component:
   ```
   Color: White (255, 255, 255, 255)
   OR Light Gray (230, 230, 230, 255)
   ```
5. Configure RectTransform:
   ```
   Anchor Preset: Stretch (both H and V)
   Left: 0, Right: 0, Top: 0, Bottom: 0
   ```
6. Continue to Step 10 to add a separate title image

**Result:** Background fills canvas, visible against passthrough

#### Step 10: Add Game Title Image (Optional - Skip if title is on background)

**If you already used your Game Title image as the background in Step 9, skip this step!**

**If you need a separate title/logo image at the top:**

1. Select MenuCanvas
2. Right-click → UI → Image
3. Rename: **"GameTitleImage"**

**⚠️ IMPORTANT: Import PNG as Sprite First!**

Before you can use the PNG in an Image component, you need to import it correctly:

**Fix PNG Import Settings:**
1. In **Project** window, navigate to `Assets/Textures/UI/`
2. Find your **Game Title (1).png** file
3. **Click on it** to select it
4. Look at **Inspector** window (on the right)
5. Find **Texture Type** dropdown
6. Change it to **"Sprite (2D and UI)"** (if it's not already)
7. **CRITICAL:** Find **Sprite Mode** dropdown
8. Change it to **"Single"** (NOT "Multiple"!) ← This is key!
9. Click **Apply** button at the bottom of Inspector
10. The PNG is now ready to use!

**Why Sprite Mode matters:**
- **Single** = One sprite, can be used in UI Image components ✓
- **Multiple** = Sprite sheet/atlas, needs slicing first, CAN'T be dragged to UI ✗

**Assign Image:**
1. Select your GameTitleImage GameObject
2. In Inspector → Image component
3. Click the **circle icon** next to "Source Image"
4. A sprite selector window appears
5. Search for "Game Title" or scroll to find it
6. **Click on your sprite** to select it
7. OR simply **drag** `Assets/Textures/UI/Game Title (1).png` from Project window into the "Source Image" field

**If PNG still doesn't appear or Source Image field is grayed out:**
- **Check Sprite Mode:** Must be "Single" not "Multiple"! (Most common issue)
- Make sure it's actually in `Assets/Textures/UI/` folder
- Verify you clicked Apply after changing settings
- Try reimporting: Right-click PNG → Reimport

**Configure Image:**
```
Source Image: Game Title (1) ← Should show the sprite now
Image Type: Simple
Preserve Aspect: ✓ (keeps aspect ratio)
Raycast Target: ✗ (not interactive)
```

**Configure RectTransform:**
```
Method 1 - Use Native Size:
  Click "Set Native Size" button in Image component
  This auto-sets Width and Height to image dimensions

Method 2 - Manual Size:
  Anchor Preset: Top-Center (or Middle-Center)
  Width: 500 (or your image width)
  Height: 700 (or your image height)
  Pos X: 0
  Pos Y: 0 (adjust for positioning)
```

**For a centered menu with native-sized image:**
```
Anchor: Middle-Center
Width: 500, Height: 700
Pos X: 0, Pos Y: 0
```

#### Step 11: Create Button Container with Horizontal Layout

**Create container for buttons:**
1. Select MenuCanvas
2. Right-click → Create Empty
3. Rename: **"ButtonContainer"**

**Configure ButtonContainer RectTransform:**
```
Anchor Preset: Bottom-Center
Width: 900
Height: 120
Pos X: 0
Pos Y: 100 (distance from bottom - adjust to taste)
```

**Add Horizontal Layout Group:**
1. With ButtonContainer selected
2. Add Component → **Horizontal Layout Group**

**Configure Horizontal Layout Group:**
```
Padding: Left: 20, Right: 20, Top: 10, Bottom: 10
Spacing: 40 (space between buttons)
Child Alignment: Middle Center
Child Controls Size: ✗ Width, ✗ Height (uncheck both)
Child Force Expand: ✗ Width, ✗ Height (uncheck both)
```

#### Step 12: Create Start Button

1. Select **ButtonContainer** (important!)
2. Right-click → UI → Button - TextMeshPro
3. Rename: **"StartButton"**

**Configure RectTransform:**
```
Width: 400
Height: 100
(Position will be controlled by Layout Group)
```

**Configure Button:**
```
Interactable: ✓
Transition: Color Tint
Normal Color: (0.8, 0.8, 0.8, 1)
Highlighted Color: (1, 1, 1, 1)
Pressed Color: (0.6, 1, 0.6, 1) ← Greenish
```

**Edit Text:**
1. Expand StartButton
2. Select "Text (TMP)" child
3. Configure:
   ```
   Text: "START"
   Font Size: 36
   Alignment: Center + Middle
   Color: Black or White (for contrast)
   ```

#### Step 13: Create Quit Button

1. Select **ButtonContainer** (important!)
2. Right-click → UI → Button - TextMeshPro
3. Rename: **"QuitButton"**

**Configure RectTransform:**
```
Width: 400
Height: 100
(Will auto-position next to Start button)
```

**Configure Button:**
```
Interactable: ✓
Transition: Color Tint
Normal Color: (0.8, 0.8, 0.8, 1)
Highlighted Color: (1, 1, 1, 1)
Pressed Color: (1, 0.6, 0.6, 1) ← Reddish
```

**Edit Text:**
```
Text: "QUIT"
Font Size: 36
Alignment: Center + Middle
Color: Black or White
```

**Result:** Both buttons are now side-by-side, centered at the bottom, automatically spaced!

---

### Part 3: Add Behavior

#### Step 14: Add Billboard Slow Following

**REQUIRED - Makes menu follow and face player:**

1. Select **StartMenuPanel**
2. Add Component → **BillboardMenu**

**Configure for Slow Following:**
```
Target Camera: (leave empty - auto-detects)
Distance: 2.5

Offset:
  X: 0   (centered)
  Y: 0   (eye level)
  Z: 0

Smooth Follow: ✓ CHECKED
Smooth Speed: 2.0 ← SLOW, comfortable following
Keep Upright: ✓ CHECKED
Start Delay: 0.5
```

**Speed Reference:**
- `1.0` = Very slow, laggy
- `2.0` = Slow, comfortable ✓ **YOUR SETTING**
- `3.0` = Medium
- `5.0` = Default, responsive
- `10+` = Fast, almost instant

#### Step 15: Add Menu Controller

1. Select **StartMenuPanel** (or MenuCanvas)
2. Add Component → **StartMenuController**

**Assign References:**
```
Start Button: [Drag StartButton here]
Quit Button: [Drag QuitButton here]
Main Game Scene Name: "MainGameScene"
Enable Haptic Feedback: ✓
Haptic Strength: 0.5
Haptic Duration: 0.1
Load Delay: 0.5
```

#### Step 16: Verify Canvas Interaction

1. Select MenuCanvas
2. Verify **GraphicRaycaster** component exists (auto-added)

**Configure:**
```
Ignore Reversed Graphics: ✓
Blocking Objects: None or ThreeD
```

---

### Part 4: Testing

#### Step 17: Test in Editor

1. Press **Play**
2. Expected:
   - Menu appears in front of camera
   - Game title visible
   - Start/Quit buttons visible
   - Buttons highlight on hover (with mouse)

**Common issues:**
- Menu not visible? Check Canvas scale (0.001)
- Too big/small? Adjust Canvas scale or Distance
- Not facing you? Verify BillboardMenu is on StartMenuPanel

#### Step 18: Test on Quest (Passthrough)

**Build to Quest:**
1. File → Build Settings
2. Android platform
3. Build and Run

**On Quest, you should see:**
- ✅ **Real world visible** (passthrough working)
- ✅ **White menu floating** in front of you
- ✅ Menu **slowly follows** as you walk
- ✅ Menu **always faces** you when you turn
- ✅ Game title visible
- ✅ Buttons respond to controller ray
- ✅ Buttons respond to hand pokes
- ✅ Haptic feedback on button press

#### Step 19: Refine Following Behavior

**Adjust to taste:**

**If follows too slowly:**
- Increase Smooth Speed to `3.0` or `4.0`

**If follows too quickly:**
- Decrease Smooth Speed to `1.5` or `1.0`

**If menu at wrong height:**
- Adjust Offset Y (positive = higher, negative = lower)

**If menu too close/far:**
- Adjust Distance (`2.0` = closer, `3.0` = further)

---

## Configuration Reference

### Complete Settings Summary

#### Canvas Configuration
```yaml
GameObject: MenuCanvas
Layer: UI (not Overlay UI!)

Components:
  Canvas:
    Render Mode: World Space
    Camera: CenterEyeAnchor (auto-assigned)
    Plane Distance: 100
    Receives Events: ✓
    Override Sorting: ✗ (not needed)
    
  OVROverlayCanvas:
    Canvas Layer: Overlay UI
    Overlay Layer: Overlay UI
    Curved: ✗ (flat)
    
  GraphicRaycaster:
    Ignore Reversed Graphics: ✓
    Blocking Mask: Everything

RectTransform:
  Width: 1920
  Height: 1080
  Scale: 0.001, 0.001, 0.001
```

#### Billboard Configuration
```yaml
GameObject: StartMenuPanel

Component: BillboardMenu
  Target Camera: (empty - auto-detect)
  Distance: 2.5
  Offset: (0, 0, 0)
  Smooth Follow: ✓
  Smooth Speed: 2.0  ← Slow following
  Keep Upright: ✓
  Start Delay: 0.5
```

#### Passthrough Configuration
```yaml
Scene:
  - OVRPassthroughLayer GameObject

OVRManager:
  Insight Passthrough: ✓ Enabled
  Pass-through Support: Supported
```

#### UI Elements
```yaml
Background Panel:
  Color: White (255, 255, 255, 255)
  RectTransform: Stretch to fill

Game Title Image:
  Source: Assets/Textures/UI/Game Title (1).png
  Width: 800, Height: 200
  Position: Top-center, Y: -150

Start Button:
  Width: 400, Height: 100
  Position: Center, Y: 50
  Text: "START", Size: 36

Quit Button:
  Width: 400, Height: 100
  Position: Center, Y: -70
  Text: "QUIT", Size: 36
```

---

## Troubleshooting

### Passthrough Not Working

**Symptoms:** Black background or virtual environment, can't see real world

**Solutions:**
1. ✅ **Verify OVRPassthroughLayer exists**
   - Check Hierarchy
   - GameObject → XR → Passthrough → Passthrough Layer
   
2. ✅ **Check OVRManager settings:**
   - Insight Passthrough: ✓
   - Pass-through Support: Supported
   
3. ✅ **Test on actual Quest** (not editor)
   - Passthrough doesn't work in Unity editor
   - Must build to Quest device
   
4. ✅ **Check Quest permissions:**
   - Quest Settings → Apps → Your App → Permissions
   - Camera permission enabled
   
5. ✅ **Verify Quest model:**
   - Works on: Quest 2, Quest 3, Quest Pro
   - Does NOT work on: Quest 1, Rift

### Menu Not Visible

**Symptoms:** Menu doesn't show up at all

**Solutions:**
1. ✅ Canvas Scale is **0.001** (not 1)
2. ✅ StartMenuPanel is **Active** in Hierarchy
3. ✅ MenuCanvas is **Active**
4. ✅ Background panel is **White/visible** color
5. ✅ Z position is positive (in front of camera)
6. ✅ Increase BillboardMenu Start Delay to `1.0`

**Check Console:**
```
Look for: "[BillboardMenu] Initialized successfully."
If missing: Camera not detected, verify OVRCameraRig exists
```

### Menu Too Large/Small

**Symptoms:** Menu fills view or is tiny

**Solutions:**

**If TOO LARGE:**
- Reduce Canvas Scale to `(0.0008, 0.0008, 0.0008)`
- OR increase Distance to `3.5`

**If TOO SMALL:**
- Increase Canvas Scale to `(0.0015, 0.0015, 0.0015)`
- OR decrease Distance to `2.0`

**Best Practice:** Keep scale at `0.001` and adjust Distance first

### Menu Doesn't Follow

**Symptoms:** Menu stays in one place when you move

**Solutions:**
1. ✅ BillboardMenu script on **StartMenuPanel**?
2. ✅ **Smooth Follow** is checked?
3. ✅ **Smooth Speed** is set (try `2.0`)?
4. ✅ Script is **enabled** (checkbox next to script)?
5. ✅ No errors in Console?

### Menu Doesn't Face Player

**Symptoms:** Menu stays facing one direction

**Solutions:**
1. ✅ BillboardMenu on correct GameObject (StartMenuPanel)?
2. ✅ **Keep Upright** is checked?
3. ✅ Script is enabled?
4. ✅ Check Console for errors

**Test:** Turn your head left/right - menu should rotate to face you

### Follow Speed Issues

**Too slow (wanted slow):**
- Speed `2.0` is correct for "slowly follows"
- This is intentional!

**Want faster:**
- Increase to `4.0` or `5.0`

**Want instant (no slow follow):**
- Uncheck Smooth Follow

### Buttons Not Working

**Symptoms:** Can point but clicking doesn't work

**Solutions:**
1. ✅ **GraphicRaycaster** on MenuCanvas?
2. ✅ **EventSystem** in scene?
3. ✅ **OVRInputModule** on EventSystem?
4. ✅ Hand/Controller interactors added?
5. ✅ Buttons assigned in StartMenuController?
6. ✅ Button **Interactable** checkbox is on?

**Check Button Assignment:**
```
Select StartMenuPanel/MenuCanvas (wherever StartMenuController is)
→ Inspector → StartMenuController
→ Start Button: [Should show reference, not "None"]
→ Quit Button: [Should show reference, not "None"]
```

### OVROverlayCanvas Layer Error ⚠️ COMMON ISSUE

**Error Message:**
```
"This GameObject's Layer is the same as Overlay Layer ('Overlay UI'). 
To control camera visibility, this GameObject should have a Layer 
that is not the Overlay Layer."
```

**This is the #1 most common issue with OVROverlayCanvas!**

**Step-by-Step Fix:**

1. **Select MenuCanvas** in Hierarchy
2. **Look at the very TOP of the Inspector** (above all components)
3. You'll see: **Tag:** [Untagged] **Layer:** [Overlay UI] ← This is the problem!
4. **Click the Layer dropdown**
5. **Select "UI"** from the list
6. If dialog appears asking about children, click **"Yes, change children"**
7. **Verify the error is gone**

**What should it look like after fix:**
```
Top of Inspector:
  Layer: UI  ← Fixed!

OVROverlayCanvas Component (scroll down):
  Canvas Layer: Overlay UI  ← Stay as is
  Overlay Layer: Overlay UI  ← Stay as is
```

**Why this happens in SDK v83+:**
- **NEW BEHAVIOR:** Canvas Layer and GameObject Layer are now automatically synced by design
- Changing one automatically changes the other
- This is intentional behavior in Meta XR SDK v83.0.1+
- The warning message is informational, not a critical error
- Your menu will work correctly with both set to the same layer

**The Fix (if you want to clear the warning):**
The warning is basically saying "Hey, these are the same now!" but it's not actually breaking anything. If you want to minimize warnings:

1. Set GameObject Layer to "UI" first (before or after adding component)
2. This syncs Canvas Layer to "UI" automatically
3. Keep Overlay Layer as "Overlay UI" (this one can be different)

**Result:** Warning may still appear but menu functions perfectly

### OVROverlayCanvas Camera Culling Warning (Optional)

**Warning Message:**
```
"Main Camera 'CenterEyeAnchor' does not cull this GameObject's Layer 'UI'"
```

**What it means:**
- Your canvas might be rendered by BOTH the main camera AND OVROverlay
- This can cause minor performance overhead
- **Menu will still work fine** - this is just an optimization

**To fix (optional):**

**Easy Way:**
- Click the button Unity shows: **"Remove UI from Camera cullingMask"**

**Manual Way:**
1. Hierarchy → OVRCameraRig → CenterEyeAnchor
2. Camera component → Culling Mask dropdown
3. Uncheck "UI" layer
4. Click away to apply

**After fix:**
- Camera won't render UI layer (saves performance)
- OVROverlay will still render your menu perfectly
- Warning disappears

**When to ignore it:**
- If you have other UI elements you want the main camera to see
- If you don't care about the minor performance difference

### OVROverlayCanvas Other Errors

**Error:** "Singular Matrix" or rendering glitches

**Fix:**
1. Canvas Scale must not be (0, 0, 0)
2. Canvas Scale must be uniform (0.001, 0.001, 0.001)
3. Restart Unity if persists

**Error:** Canvas not rendering or invisible

**Fix:**
1. Check Overlay Enabled is checked
2. Verify Opacity is not fully transparent
3. Check Canvas Shape matches your needs (Flat vs Curved)

### No Haptic Feedback

**Symptoms:** Controllers don't vibrate

**Solutions:**
1. ✅ Enable Haptic Feedback in StartMenuController
2. ✅ Test on **actual Quest** (not editor)
3. ✅ Haptic Strength above `0.3`
4. ✅ OVR controllers properly initialized

---

## Advanced Customization

### Adjust Canvas Size

**Different aspect ratios:**
```yaml
Square Menu:
  Width: 1080
  Height: 1080

Wide Menu (Cinematic):
  Width: 2560
  Height: 1080

Tall Menu (Portrait):
  Width: 1080
  Height: 1920
```

**Remember:** Keep scale at 0.001

### Change Following Speed

**Profiles:**

```yaml
Very Slow (Lazy):
  Smooth Speed: 1.0

Slow (Comfortable):
  Smooth Speed: 2.0  ← Your setting

Medium:
  Smooth Speed: 4.0

Responsive:
  Smooth Speed: 5.0

Fast (Arcade):
  Smooth Speed: 10.0

Instant (No lag):
  Smooth Follow: ✗
```

### Curved Canvas

**Enable curve:**
1. Select MenuCanvas
2. OVROverlayCanvas component
3. Curved: ✓

**Adjust curve amount:**
- Experiment in OVROverlayCanvas properties
- Higher = more pronounced curve

**Note:** Your screenshot shows flat panel, so keep Curved: ✗

### Add Sound Effects

**Button click sounds:**
1. Select button
2. Add Component → Audio Source
3. Configure:
   ```
   AudioClip: [Your click sound]
   Play On Awake: ✗
   Spatial Blend: 0 (2D)
   ```
4. Button → On Click() event:
   - Add event
   - Drag button GameObject
   - Function: AudioSource → PlayOneShot

### Add Fade In/Out

**Using CanvasGroup:**
1. Select MenuCanvas
2. Add Component → Canvas Group
3. Add to StartMenuController:

```csharp
CanvasGroup canvasGroup;

void Start()
{
    canvasGroup = GetComponentInChildren<CanvasGroup>();
    StartCoroutine(FadeIn());
}

IEnumerator FadeIn()
{
    canvasGroup.alpha = 0;
    float duration = 1f;
    float elapsed = 0;
    
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
        yield return null;
    }
}
```

### Change Menu Position

**Seated VR:**
```yaml
Offset Y: -0.5  (lower)
Distance: 2.5
```

**Standing VR:**
```yaml
Offset Y: 0  (eye level)
Distance: 3.0
```

**Room-Scale:**
```yaml
Offset Y: 0
Distance: 3.5
Canvas Scale: 0.0015  (larger for distance)
```

### Use Existing UI Prefabs as Reference

**Your project has great prefabs:**
- `Assets/Prefabs/UIPrefabs/LocalUI/Menu Bar Button.prefab`
- `Assets/Prefabs/UIPrefabs/PlayerMenu/Player Menu UI.prefab`

**How to use:**
1. Drag into scene temporarily
2. Inspect styling, colors, materials
3. Apply similar settings to your menu
4. Delete temporary instance

---

## Alternative: Fix Existing Menu

**If you want to quickly fix your old menu instead:**

### Quick Fix Steps

1. **Select your old StartMenu GameObject**
2. **Remove old billboard scripts** (if any)
3. **Add optimized BillboardMenu script:**
   ```yaml
   Distance: 2.5
   Offset Y: -0.3
   Smooth Follow: ✓
   Smooth Speed: 2.0
   Keep Upright: ✓
   ```
4. **Fix Canvas settings:**
   ```yaml
   Render Mode: World Space
   Scale: 0.001, 0.001, 0.001
   ```
5. **Test**

### Optimized Scripts Available

Your project has optimized billboard scripts:
- `Assets/Scripts/Unused/BillboardMenu.cs` ← Use this
- `Assets/Scripts/Unused/Billboard.cs` ← Alternative

**What was optimized:**
- ✅ OVRCameraRig auto-detection
- ✅ Proper initialization delays
- ✅ Modern Unity API (FindFirstObjectByType)
- ✅ Better error handling
- ✅ Keep Upright option

---

## Complete Checklist

### ✅ Scene Setup
- [ ] OVRPassthroughLayer in scene
- [ ] OVRManager → Insight Passthrough enabled
- [ ] OVRCameraRig present
- [ ] EventSystem with OVRInputModule
- [ ] Hand/Controller interactors added
- [ ] Old menu disabled (if exists)

### ✅ Menu Panel
- [ ] StartMenuPanel created
- [ ] BillboardMenu script added
- [ ] Smooth Follow: ✓
- [ ] Smooth Speed: 2.0
- [ ] Keep Upright: ✓
- [ ] Distance: 2.5

### ✅ Canvas
- [ ] MenuCanvas created
- [ ] Width: 1920, Height: 1080
- [ ] Scale: 0.001, 0.001, 0.001
- [ ] Render Mode: World Space
- [ ] OVROverlayCanvas added
- [ ] **GameObject layer set to "UI" or "Overlay UI"** (your choice)
- [ ] **Canvas Layer synced** (auto-matches GameObject layer in SDK v83+)
- [ ] **Overlay Layer set to "Overlay UI"**
- [ ] Layer warning visible (this is normal in SDK v83+, menu still works)
- [ ] Camera culling warning (optional to fix - menu works either way)
- [ ] GraphicRaycaster present

### ✅ UI Elements
- [ ] White background panel added
- [ ] Game title image added (from Textures/UI)
- [ ] Start button created and styled
- [ ] Quit button created and styled
- [ ] Button texts visible and readable

### ✅ Controller
- [ ] StartMenuController added
- [ ] Start button reference assigned
- [ ] Quit button reference assigned
- [ ] Main game scene name correct
- [ ] Haptic feedback enabled

### ✅ Testing
- [ ] Visible in editor Play mode
- [ ] Buttons work with mouse
- [ ] Built to Quest successfully
- [ ] **Passthrough working (see real world)**
- [ ] **Menu slowly follows when moving**
- [ ] **Menu faces you when turning**
- [ ] Buttons work with controller
- [ ] Buttons work with hands
- [ ] Haptic feedback works
- [ ] Scene transitions correctly

---

## Success! 🎉

You now have a professional passthrough floating menu that:
- ✅ Shows real world with menu overlay
- ✅ Slowly follows you (Speed: 2.0)
- ✅ Always faces you (billboard)
- ✅ Proper size (1920×1080)
- ✅ VR interaction ready
- ✅ High performance (OVROverlayCanvas)
- ✅ Professional appearance

### What You Built
A complete MR start menu using:
- Meta XR SDK Building Blocks
- OVRCameraRig with passthrough
- World Space UI with OVROverlayCanvas
- Billboard slow following behavior
- Hand and controller interaction
- Your custom game title
- Start/Quit functionality with haptics

### Next Steps
- Customize colors and styling
- Add sound effects
- Add fade animations
- Add more menu options
- Polish and refine
- Test with players

---

*Complete Guide v1.0 - Meta XR SDK Passthrough Menu*  
*Compatible with Meta XR SDK v62+ | Unity 2021.3+ | Quest 2/3/Pro*

