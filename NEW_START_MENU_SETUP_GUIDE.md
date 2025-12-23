# Complete Start Menu Setup Guide - Meta XR SDK Building Blocks

## 📋 Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Scene Setup from Scratch](#scene-setup-from-scratch)
4. [Using Building Blocks](#using-building-blocks)
5. [Creating the Start Menu UI](#creating-the-start-menu-ui)
6. [Adding Interaction](#adding-interaction)
7. [Adding Your Game Title](#adding-your-game-title)
8. [Testing & Refinement](#testing--refinement)
9. [Advanced Customization](#advanced-customization)
10. [Troubleshooting](#troubleshooting)

---

## Overview

This guide will help you build a professional start menu for your VR/MR game using:
- ✅ **Meta XR SDK Building Blocks** - Pre-made components
- ✅ **OVRCameraRig** - Proper VR camera setup
- ✅ **OVROverlayCanvas** - High-performance UI rendering
- ✅ **World Space UI** - VR-optimized interface
- ✅ **Meta Interaction SDK** - Proper pointer/hand interaction

### What You'll Build
A professional start menu that:
- Appears in **passthrough mode** (floating in real world)
- **Slowly follows** the player as they move
- **Always faces** the player (billboard behavior)
- Has Start and Quit buttons
- Displays your game title/logo
- Works with hand tracking and controllers

---

## Prerequisites

### What You Need
- ✅ Unity project with Meta XR SDK installed
- ✅ StartScreen scene (`Assets/Scenes/MainGameScene/StartScreen.unity`)
- ✅ Game title image (`Assets/Textures/UI/Game Title (1).png`)
- ✅ Meta XR Building Blocks available (part of Meta XR SDK)

### Check Your SDK Version
1. Window → Package Manager
2. Find "Meta XR Core SDK" or "Oculus Integration"
3. Verify version 62+ (for latest features)

---

## Scene Setup from Scratch

### Step 1: Open/Create Your Start Screen Scene

1. Open Unity
2. Navigate to `Assets/Scenes/MainGameScene/StartScreen.unity`
3. **Disable your old start menu**:
   - In Hierarchy, find your old StartMenu GameObject
   - Uncheck the checkbox next to its name (deactivates it)
   - We'll keep it as reference but not use it

### Step 2: Enable Passthrough Mode

For your floating menu in the real world:

**A. Add Passthrough Layer**

**Using Building Blocks:**
1. Click in Scene view and press **`~` (tilde)** key to open Building Blocks
2. Find **"Passthrough"** section
3. Click **"Passthrough Layer"** to add it

**OR Manually:**
1. GameObject → XR → Passthrough → Passthrough Layer
2. This adds OVRPassthroughLayer to your scene

**B. Configure OVRManager (If Present)**

Find OVRManager in scene (usually on OVRCameraRig or separate GameObject):
1. Locate or add OVRManager
2. In Inspector:
   ```
   Pass-through Support: Supported
   Insight Passthrough: ✓ Enabled
   ```

### Step 3: Access Building Blocks

**Method 1: Scene View Overlay (Recommended)**
1. Click in the Scene view to focus it
2. Press the **`~` (tilde)** key OR
3. Click the **hamburger menu** (≡) in the top-right of Scene view → Overlays
4. Find and enable **"Building Blocks"** overlay
5. The Building Blocks panel will appear in your Scene view

**Method 2: Menu Bar**
1. In Unity menu: **GameObject → XR →** (various Building Blocks listed here)

---

## Using Building Blocks

### Step 4: Add OVR Camera Rig (If Not Present)

Check if you already have an OVRCameraRig in your scene:
- Look in Hierarchy for "OVRCameraRig"
- If found, skip to Step 4
- If not found, add it:

**Using Building Blocks:**
1. Open Building Blocks overlay
2. Find **"Camera Rig"** or **"OVR Camera Rig"**
3. Click to add it to your scene

**OR Manually:**
1. GameObject → XR → Camera Rig → OVR Camera Rig

**Verify Camera Rig Structure:**
```
OVRCameraRig
├── TrackingSpace
├── LeftHandAnchor
│   └── LeftHandVisual (optional)
├── RightHandAnchor
│   └── RightHandVisual (optional)
├── CenterEyeAnchor
│   └── Camera (Main Camera component here)
```

### Step 5: Add Interaction Building Blocks

For proper UI interaction, you need:

**A. Add Hand/Controller Interaction**

**Using Building Blocks:**
1. Building Blocks overlay → Find **"Interaction"** section
2. Add **"Hand Interaction"** (for hand tracking)
3. Add **"Controller Interaction"** (for Quest controllers)

**OR Manually:**
- GameObject → XR → Interaction → Hand Interaction
- GameObject → XR → Interaction → Controller Interaction

**B. Verify Interaction Setup**

Your OVRCameraRig should now have:
- Left/Right Hand visuals
- Ray interactors
- Poke interactors
- Input modules

### Step 6: Add EventSystem

Check if EventSystem exists:
- Look in Hierarchy for "EventSystem"
- If not present:
  - GameObject → UI → Event System

**Verify EventSystem has:**
- EventSystem component
- OVR Input Module component (or XR UI Input Module)

---

## Creating the Start Menu UI

### Step 7: Create the Menu Panel GameObject

1. In Hierarchy, Right-click → Create Empty
2. Rename it to **"StartMenuPanel"**
3. Set Transform (will be overridden by billboard script):
   - Position: `(0, 1.5, 2.5)` (in front of player, eye level)
   - Rotation: `(0, 0, 0)`
   - Scale: `(1, 1, 1)`

### Step 8: Add Canvas to Panel

1. Select **StartMenuPanel**
2. Right-click → UI → Canvas
3. Rename the Canvas to **"MenuCanvas"**

**Configure Canvas Component:**
```
Render Mode: World Space
Dynamic Pixels Per Unit: 1
Event Camera: (leave empty - auto-assigned)
Sorting Layer: Default
Order in Layer: 0
```

**Configure Canvas RectTransform (Matching Your Screenshot Size):**
```
Width:  1920  ← Standard HD width
Height: 1080  ← Standard HD height
Scale: X: 0.001
       Y: 0.001
       Z: 0.001  ← IMPORTANT! Makes it visible at proper size
Position: (0, 0, 0)
Rotation: (0, 0, 0)
```

**Result:** Your menu will be 1.92m × 1.08m in world space (like a floating TV screen)

### Step 9: Add OVROverlayCanvas (Recommended - For Passthrough)

1. Select **MenuCanvas**
2. Add Component → Search for **"OVROverlayCanvas"**
3. Click Add

**Configure OVROverlayCanvas:**
```
Add Canvas If Needed: ✓
Canvas Layer: Overlay UI
Overlay Layer: Overlay UI
Curved: ✓ (if you want curved effect)
```

**Fix Layer Settings (Important!):**
- GameObject Layer: **UI** (not Overlay UI!)
- Canvas Layer (in OVROverlayCanvas): **Overlay UI**
- If you see errors, ensure GameObject layer is different from Overlay layer

### Step 10: Add Background Panel (Recommended for Passthrough)

Makes your menu visible against passthrough:

1. Select **MenuCanvas**
2. Right-click → UI → Panel
3. Rename to **"Background"**
4. Configure Image component:
   - Color: **White** `(255, 255, 255, 255)` or Light gray `(230, 230, 230, 255)`
   - This creates the floating panel look from your screenshot
5. Set RectTransform to fill canvas:
   - Anchor Preset: **Stretch** (both horizontal and vertical)
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`

### Step 11: Add Title Image

Your game logo:

1. Select **MenuCanvas**
2. Right-click → UI → Image
3. Rename to **"GameTitleImage"**

**Configure Image:**
```
Source Image: Assets/Textures/UI/Game Title (1).png
Image Type: Simple
Preserve Aspect: ✓ (recommended)
Raycast Target: ✗ (uncheck - it's not interactive)
```

**Configure RectTransform:**
```
Anchor Preset: Top-Center
Width: 800
Height: 200
Pos X: 0
Pos Y: -150 (from top)
```

### Step 12: Create Start Button

1. Select **MenuCanvas**
2. Right-click → UI → Button - TextMeshPro
3. Rename to **"StartButton"**

**Configure RectTransform:**
```
Anchor Preset: Middle-Center
Width: 400
Height: 100
Pos X: 0
Pos Y: 50 (above center)
```

**Configure Button Component:**
```
Interactable: ✓
Transition: Color Tint
Normal Color: Light gray (0.8, 0.8, 0.8, 1)
Highlighted Color: White (1, 1, 1, 1)
Pressed Color: Green (0.6, 1, 0.6, 1)
Selected Color: Light gray
```

**Edit Button Text:**
1. Expand StartButton in Hierarchy
2. Select "Text (TMP)" child
3. TextMeshPro component:
   - Text: **"START"**
   - Font Size: **36**
   - Alignment: Center + Middle
   - Color: **Black** or **White** (choose contrast)

### Step 13: Create Quit Button

1. Duplicate StartButton (Ctrl+D or Cmd+D)
2. Rename to **"QuitButton"**

**Configure RectTransform:**
```
Pos Y: -70 (below center)
```

**Configure Button:**
```
Pressed Color: Red (1, 0.6, 0.6, 1)
```

**Edit Text:**
- Text: **"QUIT"**

### Final Menu Hierarchy

Your hierarchy should look like:

```
StartScreen (Scene)
├── OVRCameraRig
│   ├── OVRManager (component - with Passthrough enabled)
│   ├── TrackingSpace
│   ├── LeftHandAnchor (with interactors)
│   ├── RightHandAnchor (with interactors)
│   └── CenterEyeAnchor (with Camera)
│
├── OVRPassthroughLayer (GameObject - enables MR mode)
│
├── EventSystem
│   └── OVRInputModule
│
└── StartMenuPanel
    ├── BillboardMenu (component) ← Makes menu follow & face player
    └── MenuCanvas
        ├── Canvas (component - World Space)
        ├── OVROverlayCanvas (component - for passthrough)
        ├── GraphicRaycaster (component - for interaction)
        │
        ├── Background (Panel) ← White/light panel visible in passthrough
        ├── GameTitleImage (Image)
        ├── StartButton (Button)
        │   └── Text (TMP)
        └── QuitButton (Button)
            └── Text (TMP)
```

---

## Adding Interaction

### Step 14: Add Billboard Behavior (REQUIRED - Always Faces Player)

To make the menu always face and slowly follow the player:

1. Select **StartMenuPanel**
2. Add Component → **BillboardMenu** (your optimized script)

**Configure for Slow Following:**
```
Target Camera: (leave empty - auto-detects)
Distance: 2.5 (how far in front of player)
Offset:
  X: 0   (centered horizontally)
  Y: 0   (at eye level)
  Z: 0
Smooth Follow: ✓ (CHECKED - enables slow following)
Smooth Speed: 2.0 ← SLOW following (default is 5)
Keep Upright: ✓ (CHECKED - prevents weird tilting)
Start Delay: 0.5 (gives VR time to initialize)
```

**Result:** Menu will slowly drift to stay in front of you and always face you.

**Speed Guide:**
- `1.0` = Very slow, laggy follow
- `2.0` = Slow, comfortable follow ✓ **RECOMMENDED**
- `3.0` = Medium follow
- `5.0` = Default, responsive follow
- `10+` = Fast, almost instant

### Step 15: Create Start Menu Controller Script

You can use your existing enhanced `StartMenuController.cs`:

1. Select **StartMenuPanel** (or MenuCanvas)
2. Add Component → **StartMenuController**

**Assign References:**
```
Start Button: Drag StartButton here
Quit Button: Drag QuitButton here
Main Game Scene Name: "MainGameScene" (or your actual game scene)
Enable Haptic Feedback: ✓
```


### Step 16: Configure Canvas for Interaction

1. Select **MenuCanvas**
2. Verify it has **GraphicRaycaster** component
3. Configure GraphicRaycaster:
   ```
   Ignore Reversed Graphics: ✓
   Blocking Objects: None (or Three D for physics blocking)
   Blocking Mask: Everything
   ```

---

## Adding Your Game Title

Your game title image is at: `Assets/Textures/UI/Game Title (1).png`

### Already Done in Step 10!

If you skipped it, here's the quick version:

1. Select MenuCanvas
2. Right-click → UI → Image
3. Rename: **"GameTitleImage"**
4. Inspector → Image component:
   - Click circle next to Source Image
   - Find "Game Title (1)" and select it
5. Position above buttons (see Step 10 for exact values)

---

## Testing & Refinement

### Step 17: Initial Test in Unity Editor

1. Press **Play**
2. You should see:
   - Menu appears in front of camera
   - Game title at top
   - Start and Quit buttons
   - Buttons highlight when you look at them (if ray interactor is working)

**Common Issues at This Stage:**
- Menu too big/small? Adjust Canvas Scale
- Menu not visible? Check Canvas layer and OVROverlayCanvas settings
- Buttons don't highlight? Verify GraphicRaycaster and EventSystem

### Step 18: Test Passthrough & Interaction

**In Editor:**
- Use mouse to click buttons (should work if GraphicRaycaster is set up)

**On Quest Device (Passthrough Mode):**
1. Build and deploy to Quest
2. **You should see your real world with the menu floating in it**
3. Walk around - menu should slowly follow you
4. Turn your head - menu should always face you
5. Point controller ray at buttons (should highlight)
6. Pull trigger to click
7. Test hand tracking by reaching out and poking buttons

### Step 19: Refine Following Behavior

**If menu follows too slowly:**
- Increase BillboardMenu **Smooth Speed** to `3.0` or `4.0`

**If menu follows too quickly:**
- Decrease BillboardMenu **Smooth Speed** to `1.5` or `1.0`

**If menu is at wrong height:**
- Adjust BillboardMenu **Offset Y**:
  - Positive = higher
  - `0` = eye level
  - Negative = lower

**If menu is too close/far:**
- Adjust BillboardMenu **Distance**:
  - `2.0` = closer
  - `2.5` = default
  - `3.0` = further away

### Step 20: Refine Positioning

**If menu is too close:**
- Increase StartMenuPanel Z position (e.g., 3.0)
- OR increase BillboardMenu Distance

**If menu is too high/low:**
- Adjust StartMenuPanel Y position
- OR adjust BillboardMenu Offset Y

**If menu is too big/small:**
- Adjust MenuCanvas Scale (try 0.0008 for smaller, 0.0015 for bigger)

---

## Advanced Customization

### Making the Canvas Curved (Optional)

**Using OVROverlayCanvas:**
1. Select MenuCanvas
2. OVROverlayCanvas component:
   - Curved: ✓
3. Adjust curve amount in component properties

**Note:** For a simple floating menu like in your screenshot, keep it flat (Curved: ✗)

### Adding Sound Effects

**To Button Presses:**
1. Select StartButton or QuitButton
2. Add Component → Audio Source
3. Configure:
   - AudioClip: [Your click sound]
   - Play On Awake: ✗
   - Spatial Blend: 0 (2D sound)
4. In Button component → On Click():
   - Add event
   - Drag button GameObject
   - Function: AudioSource → PlayOneShot

### Adding Button Animations

**Using Unity Animations:**
1. Select button
2. Animation component or Animator
3. Create animation for:
   - Scale bounce on press
   - Color pulse on hover
   - Fade in on menu open

### Adding Fade In/Out

**Using CanvasGroup:**
1. Select MenuCanvas
2. Add Component → Canvas Group
3. Create script to animate Alpha from 0 to 1

**Example code snippet:**
```csharp
// Add to StartMenuController or new script
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
    
    canvasGroup.alpha = 1;
}
```

### Using Existing UI Prefabs as Reference

You have great UI prefabs to learn from:

**Reference Prefabs:**
- `Assets/Prefabs/UIPrefabs/LocalUI/Menu Bar Button.prefab` - Button styling
- `Assets/Prefabs/UIPrefabs/PlayerMenu/Player Menu UI.prefab` - Panel layout

**How to Use as Reference:**
1. Drag prefab into scene temporarily
2. Inspect components and settings
3. Note colors, sizes, materials used
4. Apply similar settings to your menu
5. Delete the temporary prefab instance

---

## Troubleshooting

### Menu Not Visible

**Check:**
- ✅ Canvas Render Mode is World Space
- ✅ Canvas Scale is 0.001 (not 1!)
- ✅ StartMenuPanel is Active in Hierarchy
- ✅ MenuCanvas is Active
- ✅ Z position is positive (in front of camera)
- ✅ Canvas layer is correct

**Try:**
- Temporarily set Canvas Scale to 1 to see if it's positioning issue
- Check Scene view - is menu visible there?
- Disable OVROverlayCanvas temporarily to isolate issue

### Buttons Not Interactable

**Check:**
- ✅ GraphicRaycaster on MenuCanvas
- ✅ EventSystem in scene
- ✅ OVRInputModule (or equivalent) on EventSystem
- ✅ Hand/Controller interactors on OVRCameraRig
- ✅ Button Interactable checkbox is checked
- ✅ Button has Button component

**Try:**
- Check Console for errors
- Verify ray interactors are enabled on hands/controllers
- Test with mouse click in editor first
- Check button's Raycast Target is enabled

### OVROverlayCanvas Errors

**Common Error:** "GameObject layer must be different from Overlay Layer"

**Fix:**
1. Select MenuCanvas GameObject
2. Inspector top → Layer dropdown
3. Set to **UI** (not Overlay UI)
4. Keep OVROverlayCanvas component settings as Overlay UI

**Common Error:** "Singular Matrix" or rendering issues

**Fix:**
1. Ensure Canvas Scale is not (0, 0, 0)
2. Ensure Canvas Scale is uniform (0.001, 0.001, 0.001)
3. Restart Unity if issue persists

### Menu Appears Behind Player

**Fix:**
- StartMenuPanel Rotation should be (0, 180, 0) if camera starts facing opposite
- OR use BillboardMenu script to auto-face camera
- OR adjust StartMenuPanel Z position to negative value

### Buttons Highlight But Don't Click

**Check:**
- ✅ StartMenuController has button references assigned
- ✅ Button's On Click() event is set up
- ✅ In VR: Pull trigger, don't just point

**Fix:**
- Verify StartMenuController.Start() assigns button listeners
- Check Console for error messages
- Test buttons work with mouse in editor

### Performance Issues

**If menu is laggy:**
- ✅ Use OVROverlayCanvas (more efficient than regular canvas)
- ✅ Reduce canvas overdraw (don't overlap too many UI elements)
- ✅ Use texture atlases for multiple images
- ✅ Disable raycast on non-interactive elements

### Building Blocks Not Showing

**If Building Blocks overlay is missing:**
1. Window → Package Manager
2. Verify Meta XR Core SDK is installed
3. Check Unity version compatibility
4. Try reinstalling Meta XR SDK

**Alternative:**
- Use GameObject → XR menu instead
- Or manually create components

---

## Best Practices Summary

### ✅ DO:
- **Enable passthrough for MR floating menu**
- Use World Space canvas for VR/MR
- Use OVROverlayCanvas for better performance
- Keep canvas scale small (0.001)
- **Use BillboardMenu for slow following behavior**
- Set Smooth Speed to 2.0 for comfortable follow
- Position UI 2-4 meters from camera
- Use Building Blocks when available
- Test on actual Quest hardware
- Keep UI elements large enough to read
- **Use white/light background for visibility in passthrough**
- Add haptic feedback to buttons

### ❌ DON'T:
- Use Screen Space Overlay in VR (doesn't work well)
- Make UI too close (< 1m) or too far (> 5m)
- Use tiny text or buttons
- Forget GraphicRaycaster on canvas
- Forget EventSystem in scene
- Use complex animations (performance)
- Ignore layer conflicts with OVROverlayCanvas

---

## Quick Settings Reference

### Exact Configuration (Matching Your Screenshot)

**Canvas Size:**
```
Width: 1920
Height: 1080
Scale: 0.001, 0.001, 0.001
```

**Billboard Slow Following:**
```
Smooth Follow: ✓
Smooth Speed: 2.0 (slow, comfortable)
Keep Upright: ✓
Distance: 2.5
Offset Y: 0 (eye level)
```

**Passthrough:**
```
OVRManager → Insight Passthrough: ✓
OVRPassthroughLayer in scene
Background Panel: White (255, 255, 255)
```

---

## Quick Reference Checklist

### Scene Setup
- [ ] **Passthrough enabled (OVRPassthroughLayer added)**
- [ ] **OVRManager has Insight Passthrough enabled**
- [ ] OVRCameraRig in scene
- [ ] EventSystem with OVRInputModule
- [ ] Hand/Controller interactors added
- [ ] StartMenuPanel created
- [ ] **BillboardMenu script added (REQUIRED for following)**

### Canvas Setup
- [ ] MenuCanvas with World Space render mode
- [ ] **Canvas Size: Width 1920, Height 1080**
- [ ] Canvas Scale: 0.001, 0.001, 0.001
- [ ] GraphicRaycaster component present
- [ ] **OVROverlayCanvas added (for passthrough)**
- [ ] GameObject layer: UI
- [ ] OVROverlayCanvas layers: Overlay UI
- [ ] **White/light background panel added**

### UI Elements
- [ ] Game title image added and visible
- [ ] Start button created with proper settings
- [ ] Quit button created with proper settings
- [ ] Button texts are readable
- [ ] Buttons highlight on hover

### Billboard Following
- [ ] **BillboardMenu added to StartMenuPanel**
- [ ] **Smooth Follow checked**
- [ ] **Smooth Speed set to 2.0 (slow follow)**
- [ ] **Keep Upright checked**
- [ ] Distance set to 2.5
- [ ] Offset Y set to 0

### Interaction
- [ ] StartMenuController script added
- [ ] Button references assigned
- [ ] Scene name configured correctly
- [ ] Buttons respond to clicks
- [ ] Haptic feedback enabled

### Testing
- [ ] Menu visible in editor Play mode
- [ ] Buttons work with mouse in editor
- [ ] **Built to Quest and tested in passthrough**
- [ ] **Can see real world behind/around menu**
- [ ] **Menu slowly follows as you walk around**
- [ ] **Menu always faces you when you turn**
- [ ] Buttons work with controller
- [ ] Buttons work with hand tracking
- [ ] Scene transitions correctly

---

## Success! 🎉

You now have a professional start menu built with Meta XR SDK Building Blocks!

**Your menu features:**
- ✅ Proper VR/MR world-space UI
- ✅ OVROverlayCanvas for performance
- ✅ Working Start/Quit buttons
- ✅ Your game title/logo
- ✅ Hand and controller interaction
- ✅ Optional billboard behavior
- ✅ Optional curved canvas
- ✅ Haptic feedback

**Next Steps:**
- Customize colors and styling
- Add sound effects
- Add fade in/out animations
- Add more menu options (settings, etc.)
- Polish and test on Quest

---

*Guide Version: 1.0 - Meta XR SDK Building Blocks Approach*  
*Compatible with Meta XR SDK v62+ and Unity 2021.3+*

