# 🎯 Wrist Menu Complete Setup Guide

**One document. All the steps. Follow top to bottom.**

---

## 📋 Table of Contents

1. [Prerequisites](#-prerequisites)
2. [Part 1: Create the Canvas](#-part-1-create-the-canvas)
3. [Part 2: Create Menu Panel](#-part-2-create-menu-panel)
4. [Part 3: Create Game Stats Panel](#-part-3-create-game-stats-panel)
5. [Part 4: Create Settings Panel](#-part-4-create-settings-panel)
6. [Part 5: Create End Game Panel](#-part-5-create-end-game-panel)
7. [Part 6: Configure WristMenuController](#-part-6-configure-wristmenucontroller)
8. [Part 7: Configure EventSystem](#-part-7-configure-eventsystem)
9. [Part 8: Add Pointable Canvas Components](#-part-8-add-pointable-canvas-components)
10. [Part 9: Create Canvas Mesh Child](#-part-9-create-canvas-mesh-child)
11. [Part 10: Setup Ray Interactor](#-part-10-setup-ray-interactor)
12. [Part 11: Configure Gun/Flashlight Hiding](#-part-11-configure-gunflashlight-hiding)
13. [Part 12: Configure GameManager](#-part-12-configure-gamemanager)
14. [Part 13: Testing](#-part-13-testing)
15. [Troubleshooting](#-troubleshooting)

---

## ✅ Prerequisites

Before starting, make sure you have:
- [ ] Unity project open with your main game scene
- [ ] OVRCameraRig in your scene
- [ ] Meta XR SDK installed
- [ ] `WristMenuController.cs` script in `Assets/Scripts/UI/`

---

## 📦 Part 1: Create the Canvas

### Step 1.1: Delete Old HUD (If Exists)
1. Look in Hierarchy for any old Canvas with game UI (Round, Score, Timer)
2. **Delete it** or disable it (backup)

### Step 1.2: Create WristMenuCanvas
1. In Hierarchy, find: `OVRCameraRig > TrackingSpace > LeftHandAnchor`
2. Right-click **LeftHandAnchor** → **UI** → **Canvas**
3. Rename to **"WristMenuCanvas"**

### Step 1.3: Configure Canvas Component
Select **WristMenuCanvas** and set:

| Setting | Value |
|---------|-------|
| Render Mode | **World Space** |
| Camera | Drag **CenterEyeAnchor** here |
| Plane Distance | 100 |
| Receives Events | ✅ Checked |
| Override Sorting | ✅ Checked |
| Additional Shader Channels Flag | **Tex Coord 1** |

### Step 1.4: Configure Canvas Scaler
| Setting | Value |
|---------|-------|
| UI Scale Mode | **World** |
| Dynamic Pixels Per Unit | 1 |
| Reference Pixels Per Unit | 100 |

### Step 1.5: Add WristMenuController Script
1. Click **Add Component**
2. Search for **"Wrist Menu Controller"**
3. Add it (we'll configure it later in Part 6)

---

## 📦 Part 2: Create Menu Panel

### Step 2.1: Create MenuPanel
1. Right-click **WristMenuCanvas** → **UI** → **Panel**
2. Rename to **"MenuPanel"**

### Step 2.2: Configure MenuPanel RectTransform
| Setting | Value |
|---------|-------|
| Anchor | Center (click the square icon, select center) |
| Pos X, Y, Z | 0, 0, 0 |
| Width | 500 |
| Height | 700 |

### Step 2.3: Style Background (Optional)
- Set Image color to semi-transparent dark (e.g., RGBA: 20, 20, 30, 220)

---

## 📦 Part 3: Create Game Stats Panel

### Step 3.1: Create GameStatsPanel
1. Right-click **MenuPanel** → **UI** → **Panel**
2. Rename to **"GameStatsPanel"**

### Step 3.2: Configure RectTransform
| Setting | Value |
|---------|-------|
| Anchor | Stretch (0,0 to 1,1) |
| Left, Top, Right, Bottom | 0, 0, 0, 0 |

### Step 3.3: Add Vertical Layout Group
Click **Add Component** → **Vertical Layout Group**

| Setting | Value |
|---------|-------|
| Padding | 20 (all sides) |
| Spacing | 15 |
| Child Alignment | Upper Center |
| Control Child Size - Width | ✅ |
| Control Child Size - Height | ✅ |
| Child Force Expand - Width | ❌ |
| Child Force Expand - Height | ❌ |

> ⚠️ **Note:** The Vertical Layout Group will grey out position/size fields on children. This is normal! Use **Layout Element** on children to control size.

### Step 3.4: Create RoundText
1. Right-click **GameStatsPanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"RoundText"**
3. Configure:

| Setting | Value |
|---------|-------|
| Text | "Round 0" |
| Font Size | 32 |
| Alignment | Center |
| Color | White |

4. Add **Layout Element** component:

| Setting | Value |
|---------|-------|
| Preferred Width | 460 |
| Preferred Height | 50 |

### Step 3.5: Create ScoreText
1. Right-click **GameStatsPanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"ScoreText"**
3. Configure:

| Setting | Value |
|---------|-------|
| Text | "Score: 0" |
| Font Size | 36 |
| Alignment | Center |
| Color | Green (#00FF00) |

4. Add **Layout Element** component:

| Setting | Value |
|---------|-------|
| Preferred Width | 460 |
| Preferred Height | 60 |

### Step 3.6: Create TimerText
1. Right-click **GameStatsPanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"TimerText"**
3. Configure:

| Setting | Value |
|---------|-------|
| Text | "60s" |
| Font Size | 36 |
| Alignment | Center |
| Color | Yellow (#FFFF00) |

4. Add **Layout Element** component:

| Setting | Value |
|---------|-------|
| Preferred Width | 460 |
| Preferred Height | 60 |

### Step 3.7: Create Spacer
1. Right-click **GameStatsPanel** → **Create Empty**
2. Rename to **"Spacer"**
3. Add **Layout Element** component:

| Setting | Value |
|---------|-------|
| Preferred Height | 300 |

### Step 3.8: Create SettingsButton
1. Right-click **GameStatsPanel** → **UI** → **Button - TextMeshPro**
2. Rename to **"SettingsButton"**
3. Configure button text: **"⚙️ Settings"** (Font Size: 24)
4. Configure Button colors:

| State | Color |
|-------|-------|
| Normal | Light Blue |
| Highlighted | Cyan |
| Pressed | Blue |

5. Add **Layout Element** component:

| Setting | Value |
|---------|-------|
| Preferred Width | 200 |
| Preferred Height | 60 |

---

## 📦 Part 4: Create Settings Panel

### Step 4.1: Create SettingsPanel
1. Right-click **MenuPanel** → **UI** → **Panel**
2. Rename to **"SettingsPanel"**

### Step 4.2: Configure RectTransform
| Setting | Value |
|---------|-------|
| Anchor | Stretch (0,0 to 1,1) |
| Left, Top, Right, Bottom | 0, 0, 0, 0 |

### Step 4.3: Add Vertical Layout Group
| Setting | Value |
|---------|-------|
| Padding | 20 (all sides) |
| Spacing | 20 |
| Child Alignment | Upper Center |
| Control Child Size - Width | ✅ |
| Control Child Size - Height | ✅ |
| Child Force Expand - Width | ❌ |
| Child Force Expand - Height | ❌ |

### Step 4.4: Create VolumeLabel
1. Right-click **SettingsPanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"VolumeLabel"**
3. Text: **"Volume"**, Font Size: 28, Center
4. Add Layout Element: Preferred Width: 460, Preferred Height: 40

### Step 4.5: Create VolumeSlider
1. Right-click **SettingsPanel** → **UI** → **Slider**
2. Rename to **"VolumeSlider"**
3. Configure: Min: 0, Max: 1, Value: 1
4. Add Layout Element: Preferred Width: 400, Preferred Height: 50

### Step 4.6: Create BrightnessLabel
1. Right-click **SettingsPanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"BrightnessLabel"**
3. Text: **"Brightness"**, Font Size: 28, Center
4. Add Layout Element: Preferred Width: 460, Preferred Height: 40

### Step 4.7: Create BrightnessSlider
1. Right-click **SettingsPanel** → **UI** → **Slider**
2. Rename to **"BrightnessSlider"**
3. Configure: Min: 0, Max: 1, Value: 1
4. Add Layout Element: Preferred Width: 400, Preferred Height: 50

### Step 4.8: Create Spacer
1. Right-click **SettingsPanel** → **Create Empty**
2. Rename to **"Spacer"**
3. Add Layout Element: Preferred Height: 100

### Step 4.9: Create PauseButton
1. Right-click **SettingsPanel** → **UI** → **Button - TextMeshPro**
2. Rename to **"PauseButton"**
3. Button text: **"Pause"** (Font Size: 24)
4. Colors: Normal: Yellow, Highlighted: Orange, Pressed: Red
5. Add Layout Element: Preferred Width: 200, Preferred Height: 60

### Step 4.10: Create BackButton
1. Right-click **SettingsPanel** → **UI** → **Button - TextMeshPro**
2. Rename to **"BackButton"**
3. Button text: **"← Back"** (Font Size: 24)
4. Colors: Normal: Gray, Highlighted: Light Gray, Pressed: Dark Gray
5. Add Layout Element: Preferred Width: 200, Preferred Height: 60

---

## 📦 Part 5: Create End Game Panel

### Step 5.1: Create EndGamePanel
1. Right-click **MenuPanel** → **UI** → **Panel**
2. Rename to **"EndGamePanel"**

### Step 5.2: Configure RectTransform
| Setting | Value |
|---------|-------|
| Anchor | Stretch (0,0 to 1,1) |
| Left, Top, Right, Bottom | 0, 0, 0, 0 |

### Step 5.3: Add Vertical Layout Group
| Setting | Value |
|---------|-------|
| Padding | 40 (all sides) |
| Spacing | 30 |
| Child Alignment | Middle Center |
| Control Child Size - Width | ✅ |
| Control Child Size - Height | ✅ |
| Child Force Expand - Width | ❌ |
| Child Force Expand - Height | ❌ |

### Step 5.4: Create GameOverText
1. Right-click **EndGamePanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"GameOverText"**
3. Text: **"GAME OVER"**, Font Size: 48, Center, Color: Red, Bold
4. Add Layout Element: Preferred Width: 400, Preferred Height: 80

### Step 5.5: Create FinalScoreText
1. Right-click **EndGamePanel** → **UI** → **Text - TextMeshPro**
2. Rename to **"FinalScoreText"**
3. Text: **"Final Score: 0"**, Font Size: 40, Center, Color: Yellow
4. Add Layout Element: Preferred Width: 400, Preferred Height: 70

### Step 5.6: Create Spacer
1. Right-click **EndGamePanel** → **Create Empty**
2. Rename to **"Spacer"**
3. Add Layout Element: Preferred Height: 100

### Step 5.7: Create RestartButton
1. Right-click **EndGamePanel** → **UI** → **Button - TextMeshPro**
2. Rename to **"RestartButton"**
3. Button text: **"🔄 Restart Game"** (Font Size: 28)
4. Colors: Normal: Green, Highlighted: Light Green, Pressed: Dark Green
5. Add Layout Element: Preferred Width: 250, Preferred Height: 70

---

## 📦 Part 6: Configure WristMenuController

### Step 6.1: Select WristMenuCanvas
1. Click on **WristMenuCanvas** in Hierarchy
2. Find the **Wrist Menu Controller** component in Inspector

### Step 6.2: Assign Menu References
Drag these from Hierarchy into the corresponding slots:

| Field | Drag This |
|-------|-----------|
| Menu Panel | MenuPanel |

### Step 6.3: Assign Panels
| Field | Drag This |
|-------|-----------|
| Game Stats Panel | GameStatsPanel |
| Settings Panel | SettingsPanel |
| End Game Panel | EndGamePanel |

### Step 6.4: Assign Game Stats UI
| Field | Drag This |
|-------|-----------|
| Round Text | RoundText |
| Score Text | ScoreText |
| Timer Text | TimerText |
| Settings Button | SettingsButton |

### Step 6.5: Assign End Game UI
| Field | Drag This |
|-------|-----------|
| Final Score Text | FinalScoreText |
| Restart Button | RestartButton |

### Step 6.6: Assign Settings UI
| Field | Drag This |
|-------|-----------|
| Volume Slider | VolumeSlider |
| Brightness Slider | BrightnessSlider |
| Pause Button | PauseButton |
| Pause Button Text | The Text (TMP) child of PauseButton |
| Back Button | BackButton |

### Step 6.7: Verify Positioning Settings (Defaults are usually fine)
| Field | Value |
|-------|-------|
| Menu Offset | (0, 0.05, 0.1) |
| Menu Rotation Offset | (45, 0, 0) |
| Menu Scale | 0.0005 |

### Step 6.8: Verify Input Settings
| Field | Value |
|-------|-------|
| Toggle Button | Two (Y button) |
| Controller | LTouch |

---

## 📦 Part 7: Configure EventSystem

### Step 7.1: Find EventSystem
1. Look in Hierarchy for **EventSystem** (usually a root GameObject)
2. If it doesn't exist, create one: **GameObject** → **UI** → **Event System**

### Step 7.2: Disable OVRInputModule (CRITICAL!)
1. Select **EventSystem**
2. Find **OVR Input Module** component
3. **Uncheck the checkbox** to disable it (or Remove Component)
4. ⚠️ **Why?** OVRInputModule conflicts with Pointable Canvas system

### Step 7.3: Add PointableCanvasModule
1. Click **Add Component**
2. Search for **"Pointable Canvas Module"**
3. Add it
4. Leave settings at default

### Step 7.4: Verify EventSystem Setup
Your EventSystem should now have:
- ✅ Event System component
- ❌ OVR Input Module (DISABLED or REMOVED)
- ✅ Pointable Canvas Module

---

## 📦 Part 8: Add Pointable Canvas Components

### Step 8.1: Select WristMenuCanvas

### Step 8.2: Add Graphic Raycaster
1. Click **Add Component** → Search **"Graphic Raycaster"**
2. Add it (leave defaults)
3. **Why?** Translates ray hits into button clicks

### Step 8.3: Add Ray Interactable
1. Click **Add Component** → Search **"Ray Interactable"**
2. Configure:

| Setting | Value |
|---------|-------|
| Max Interactors | -1 |
| Max Selecting Interactors | -1 |

### Step 8.4: Add Pointable Canvas
1. Click **Add Component** → Search **"Pointable Canvas"**
2. **CRITICAL:** Click the circle icon next to **Canvas** field
3. Select **WristMenuCanvas** (the Canvas component)
4. ⚠️ This field CANNOT be "None (Canvas)"!

### Step 8.5: Add Pointable Canvas Mesh
1. Click **Add Component** → Search **"Pointable Canvas Mesh"**
2. Leave defaults (Canvas Mesh will auto-populate)

### Step 8.6: Add Pointable Canvas Unity Event Wrapper
1. Click **Add Component** → Search **"Pointable Canvas Unity Event Wrapper"**
2. Should auto-link to Pointable Canvas component
3. Suppress While Dragging: ✅ Checked

---

## 📦 Part 9: Create Canvas Mesh Child

### Step 9.1: Create Mesh Child
1. Right-click **WristMenuCanvas** → **Create Empty**
2. Rename to **"Mesh"**
3. Reset Transform (gear icon → Reset)
4. **Set Layer to "UI"** (dropdown at top of Inspector)

### Step 9.2: Add Canvas Rect
1. Click **Add Component** → Search **"Canvas Rect"**
2. The component will auto-configure:

| Setting | Value |
|---------|-------|
| Canvas Render Texture | Leave as None |
| Mesh Filter | Will auto-link to Mesh Filter component |
| Mesh Collider (Optional) | Will auto-link to Mesh Collider component |

### Step 9.3: Add Mesh Filter
1. Click **Add Component** → **"Mesh Filter"**

### Step 9.4: Add Mesh Renderer
1. Click **Add Component** → **"Mesh Renderer"**

### Step 9.5: Add OVR Canvas Mesh Renderer
1. Click **Add Component** → Search **"OVR Canvas Mesh Renderer"**
2. Configure:

| Setting | Value |
|---------|-------|
| Rendering Mode | OVR/Overlay |

### Step 9.6: Add Mesh Collider
1. Click **Add Component** → **"Mesh Collider"**
2. Configure:

| Setting | Value |
|---------|-------|
| Convex | ❌ Unchecked |
| Is Trigger | ❌ Unchecked |
| Cooking Options | Everything checked |

### Step 9.7: Test Mesh Generation
1. Press **Play** in Unity Editor
2. Select **Mesh** child in Hierarchy
3. Check Inspector:
   - Mesh Filter should show a generated mesh
   - Mesh Collider should reference the same mesh
4. **Exit Play Mode**

---

## 📦 Part 10: Setup Ray Interactor

**⚠️ IMPORTANT:** This is what makes button clicking work!

### Option A: You Already Have OVRInteractionComprehensive (Recommended)

If you see `[BuildingBlock] OVRInteractionComprehensive` in your Hierarchy with `RightInteractions` underneath, **you may already have ray interaction working!**

1. **Skip to Part 13 (Testing)** and test if clicking works
2. If it works → You're done with this part!
3. If it doesn't work → Try Option B below

### Option B: Manual Ray Interactor Setup

If you need to manually add ray interaction:

#### Step 10.1: Delete Extra Building Block Stuff (If Added)
If Building Blocks added extra objects causing errors:
1. Delete `[BuildingBlock] OVRInteractionComprehensive` (if you don't need it)
2. Delete any `UIRayInteractor` or `ISDK_RayInteraction` objects
3. Close and reopen Unity if you see GUI errors

#### Step 10.2: Create Simple Ray Interactor
1. Right-click **RightHandAnchor** (or **RightControllerAnchor**) → **Create Empty**
2. Rename to **"UIRayInteractor"**

#### Step 10.3: Add Components Manually
Select **UIRayInteractor** and add these components:

1. **Add Component** → Search **"Ray Interactor"**
   - If not found, search **"ISDK Ray Interactor"**
   
2. Configure Ray Interactor:
   | Setting | Value |
   |---------|-------|
   | Max Interactable Selected | 1 |

#### Step 10.4: Disable By Default
1. Select **UIRayInteractor**
2. **Uncheck the checkbox** next to the name to disable it
3. **Why?** Should be OFF during gameplay, ON only when menu opens

#### Step 10.5: Assign to WristMenuController
1. Select **WristMenuCanvas**
2. Find **Wrist Menu Controller** component
3. Look for **"UI Interaction"** section
4. Drag **UIRayInteractor** into the **"Ui Ray Interactor"** slot

### Option C: Skip Ray Interactor Entirely

If you're having too many issues, you can test without the ray interactor toggling:

1. Leave **Ui Ray Interactor** slot empty in WristMenuController
2. The existing interaction system from OVRInteractionComprehensive should still work
3. The ray will always be available (not toggled with menu)

---

## 📦 Part 11: Configure Gun/Flashlight Hiding

### Step 11.1: Select WristMenuCanvas

### Step 11.2: Configure Gun Management
In the **Wrist Menu Controller** component:

| Field | Action |
|-------|--------|
| Gun Script | Should auto-populate OR drag your gun |
| Auto Find Gun | ✅ Check |

### Step 11.3: Configure Right Hand Objects
| Field | Action |
|-------|--------|
| Gun Game Object | Drag your gun model (e.g., "Specter Blaster 3000") |
| Flashlight Game Object | Drag your flashlight model |
| Auto Find Right Hand Objects | ✅ Check |

### Step 11.4: Verify References
All fields should show **blue text** (linked), not "None (GameObject)"

---

## 📦 Part 12: Configure GameManager

### Step 12.1: Find GameManager
1. Find **GameManager** in Hierarchy
2. Select it

### Step 12.2: Assign Wrist UI
1. Find the **wristUI** field in GameManager
2. Drag **WristMenuCanvas** into this slot

### Step 12.3: Remove Old References (If Present)
Delete or clear any old HUD references like:
- ~~Round Text~~
- ~~Score Text~~
- ~~Timer Text~~

---

## 📦 Part 13: Testing

### Step 13.1: Save Scene
Press **Ctrl+S**

### Step 13.2: Test in Editor
1. Press **Play**
2. Check Console for these logs:
   ```
   [WristUI] Gun script found and linked!
   [WristUI] Gun GameObject auto-linked: [Name]
   ```
3. Check Mesh child has generated mesh
4. Exit Play Mode

### Step 13.3: Build to Quest
1. **File** → **Build Settings** → **Build and Run**
2. Wait for installation

### Step 13.4: Test on Quest

**Test 1: Menu Toggle**
- Press **Y button** (left controller)
- ✅ Menu appears on left wrist
- ✅ Gun model disappears
- ✅ Flashlight model disappears
- ✅ Console: `[WristUI] ===== MENU OPEN: Gun hidden, UI Ray ENABLED =====`

**Test 2: Ray Visibility**
- Point **right controller** at menu
- ✅ Ray appears from controller
- ✅ Ray changes color when hovering buttons
- ✅ Buttons highlight when ray is over them

**Test 3: Button Clicks**
- Point at Settings button
- Press **Index Trigger** (right controller)
- ✅ Panel switches to Settings

**Test 4: Menu Close**
- Press **Y button** again
- ✅ Menu disappears
- ✅ Gun reappears
- ✅ Flashlight reappears
- ✅ Can shoot again

**Test 5: Game Stats**
- Kill a ghost → Score should update
- Wait → Timer should count down
- Complete a round → Round counter should update

---

## 🐛 Troubleshooting

### ❌ Menu appears but no ray visible

**Check these in order:**

1. **EventSystem Configuration**
   - Is OVRInputModule disabled? (Must be disabled!)
   - Is PointableCanvasModule added?

2. **UIRayInteractor Assignment**
   - Is UIRayInteractor assigned in WristMenuController?
   - Was it created from Building Blocks (not an empty object)?

3. **Mesh Child**
   - Does it have a Mesh Collider?
   - Did the mesh generate? (Press Play to check)

---

### ❌ Ray visible but can't click buttons

**Check these in order:**

1. **Graphic Raycaster**
   - Is Graphic Raycaster added to WristMenuCanvas?

2. **Buttons**
   - Does each button have a **Button** component?
   - Is **Interactable** checkbox checked?

3. **Pointable Canvas**
   - Is Canvas field assigned (not "None")?

4. **Mesh Layer**
   - Is Mesh child set to **UI** layer?

---

### ❌ Gun/flashlight still visible when menu opens

**Check these in order:**

1. **WristMenuController References**
   - Is Gun Game Object assigned?
   - Is Flashlight Game Object assigned?
   - Check console for auto-find warnings

2. **Manual Assignment**
   - Drag gun/flashlight from Hierarchy into slots

---

### ❌ Menu positioning is wrong

**Adjust in WristMenuController:**
- Menu Offset: Try `(0, 0.05, 0.1)` - adjust Y (height) and Z (distance)
- Menu Rotation Offset: Try `(45, 0, 0)` - adjust X for tilt
- Menu Scale: Try `0.0005` - increase/decrease for size

---

### ❌ Canvas mesh not generating

**Check these in order:**

1. **Canvas Rect**
   - Is Canvas assigned in Canvas Rect component?

2. **Components Order**
   - Remove all components from Mesh child
   - Re-add in exact order: Canvas Rect → Mesh Filter → Mesh Renderer → OVR Canvas Mesh Renderer → Mesh Collider

3. **Play Mode**
   - Press Play to trigger mesh generation
   - Check Mesh Filter for generated mesh

---

## 🎯 Final Hierarchy Structure

Your hierarchy should look like this:

```
OVRCameraRig
├── TrackingSpace
│   ├── LeftHandAnchor
│   │   └── WristMenuCanvas          ← Your wrist menu
│   │       ├── MenuPanel
│   │       │   ├── GameStatsPanel
│   │       │   │   ├── RoundText
│   │       │   │   ├── ScoreText
│   │       │   │   ├── TimerText
│   │       │   │   ├── Spacer
│   │       │   │   └── SettingsButton
│   │       │   ├── SettingsPanel
│   │       │   │   ├── VolumeLabel
│   │       │   │   ├── VolumeSlider
│   │       │   │   ├── BrightnessLabel
│   │       │   │   ├── BrightnessSlider
│   │       │   │   ├── Spacer
│   │       │   │   ├── PauseButton
│   │       │   │   └── BackButton
│   │       │   └── EndGamePanel
│   │       │       ├── GameOverText
│   │       │       ├── FinalScoreText
│   │       │       ├── Spacer
│   │       │       └── RestartButton
│   │       └── Mesh                  ← Canvas collision mesh
│   │
│   ├── RightHandAnchor
│   │   ├── [Your Gun]
│   │   ├── [Your Flashlight]
│   │   └── UIRayInteractor          ← Ray for clicking (disabled by default)
│   │
│   └── CenterEyeAnchor
│
└── ...

EventSystem                           ← With PointableCanvasModule
GameManager                           ← With wristUI reference
```

---

## ✅ Complete Component Checklist

### EventSystem
- [ ] Event System component
- [ ] Pointable Canvas Module
- [ ] OVR Input Module **DISABLED**

### WristMenuCanvas
- [ ] Canvas (World Space)
- [ ] Canvas Scaler
- [ ] Graphic Raycaster
- [ ] Ray Interactable
- [ ] Pointable Canvas (Canvas field ASSIGNED)
- [ ] Pointable Canvas Mesh
- [ ] Pointable Canvas Unity Event Wrapper
- [ ] Wrist Menu Controller (all references assigned)

### Mesh (child of WristMenuCanvas)
- [ ] Canvas Rect (Canvas assigned)
- [ ] Mesh Filter
- [ ] Mesh Renderer
- [ ] OVR Canvas Mesh Renderer
- [ ] Mesh Collider
- [ ] Layer: UI

### UIRayInteractor (on RightHandAnchor)
- [ ] Created from Meta Building Blocks
- [ ] Disabled by default
- [ ] Assigned in WristMenuController

---

## 🎉 You're Done!

When everything is set up correctly:

1. **Press Y** → Menu opens, gun hides, ray activates
2. **Point at button** → Ray visible, button highlights
3. **Pull trigger** → Button clicks!
4. **Press Y** → Menu closes, gun returns

**Expected Console Logs:**
```
[WristUI] Gun script found and linked!
[WristUI] Gun GameObject auto-linked: Specter Blaster 3000
[WristUI] ===== MENU OPEN: Gun hidden, UI Ray ENABLED =====
[WristUI] ===== MENU CLOSED: Gun shown, UI Ray DISABLED =====
```

---

*Last Updated: December 18, 2025*

