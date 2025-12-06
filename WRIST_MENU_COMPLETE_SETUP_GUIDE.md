# Wrist Menu Complete Setup & Fix Guide

**Last Updated**: After Sonnet 4.5 Review  
**Purpose**: Single comprehensive guide for setting up and troubleshooting the wrist menu

---

## 🎯 Quick Start (For Experienced Users)

1. Set `WristMenuUI` layer to **UI (layer 5)**
2. Assign `MenuPanel` child GameObject to WristMenuController
3. Verify `CenterEyeAnchor` camera includes **UI layer** in culling mask
4. Build and test on device (passthrough requires build)
5. Press **Y button** on left controller to toggle

---

## 📋 Complete Setup Checklist

### Step 1: Find and Verify GameObject

- [ ] Open **MainGameScene** in Unity
- [ ] Search for `WristMenuUI` in Hierarchy
- [ ] Verify GameObject is **Active** (checkbox checked)
- [ ] Verify GameObject exists and is not deleted

**Location**: Should be a standalone GameObject, will automatically attach to `LeftHandAnchor` at runtime

---

### Step 2: WristMenuController Component Settings

Select `WristMenuUI` and check **Inspector** panel:

#### Required References
- [ ] **Menu Panel**: Assigned to child GameObject *(panel with UI elements)*
- [ ] **Wrist Transform**: Empty is OK *(auto-finds LeftHandAnchor)*

#### Canvas Configuration
- [ ] **Ignore Auto Configuration**: `true` ✓
- [ ] **Canvas Layer Name**: `"UI"`
- [ ] **UI Elements Layer Name**: `"UI"`

#### Menu Positioning
- [ ] **Menu Offset**: `(0, 0.1, 0.2)` or adjust as needed
  - X = 0 (centered on hand)
  - Y = 0.1 (10cm above hand)
  - Z = 0.2 (20cm in front of hand)
- [ ] **Menu Rotation Offset**: `(45, 0, 0)` *(tilted up 45°)*
- [ ] **Menu Scale**: `0.0005` *(very small for VR)*

#### Toggle Settings
- [ ] **Toggle Button**: `Three` *(Y button on Quest)*
- [ ] **Controller**: `LTouch` *(left controller)*

#### UI Element References
- [ ] **Pause Resume Button**: Assigned
- [ ] **Pause Resume Text**: Assigned  
- [ ] **Volume Slider**: Assigned
- [ ] **Brightness Slider**: Assigned
- [ ] **Volume Value Text**: Assigned
- [ ] **Brightness Value Text**: Assigned

#### Hand Interaction Fix (NEW)
- [ ] **Gun Game Object**: Assign your gun GameObject here
- [ ] **Disable Gun When Menu Open**: Should be `true` ✓
- **Why**: This frees your right hand to interact with menu when Y is pressed

---

### Step 3: Canvas Component Settings

With `WristMenuUI` selected, check **Canvas** component:

- [ ] **Render Mode**: `World Space`
- [ ] **World Camera**: Assigned to `CenterEyeAnchor`
- [ ] **Sorting Order**: Can be any value *(code sets it to 1000)*
- [ ] **Pixel Perfect**: Unchecked

---

### Step 4: Menu Panel Child GameObject

- [ ] Expand `WristMenuUI` in Hierarchy
- [ ] Verify **Menu Panel** child exists
- [ ] Menu Panel is **Inactive** initially (unchecked)
- [ ] Menu Panel contains UI elements (buttons, sliders, text)

**Hierarchy Structure**:
```
WristMenuUI (Canvas + WristMenuController)
├── MenuPanel (inactive) ← This is what shows/hides
│   ├── PauseButton
│   ├── VolumeSlider
│   ├── BrightnessSlider
│   └── ... (other UI elements)
└── ... (other elements)
```

---

### Step 5: Left Hand Anchor Verification

- [ ] Search for `[BuildingBlock] Camera Rig` or `OVRCameraRig`
- [ ] Expand to find `LeftHandAnchor`
- [ ] Verify `LeftHandAnchor` exists and is **Active**

**Optional**: Manually drag `LeftHandAnchor` to `Wrist Transform` field (otherwise auto-finds)

---

### Step 6: Layer Configuration

**CRITICAL**: Must use UI layer (layer 5) only

1. Go to **Edit > Project Settings > Tags and Layers**
2. Verify **Layer 5** = `"UI"`

Back in scene:
- [ ] Select `WristMenuUI`
- [ ] Set **Layer** dropdown to `UI`
- [ ] When prompted, click **"Yes, change children"**

**Result**: All WristMenuUI children should be on UI layer

---

### Step 7: Camera Culling Mask

**THIS IS CRITICAL - Menu won't be visible if wrong!**

1. Find `CenterEyeAnchor` camera (under OVRCameraRig)
2. Select it
3. Check **Camera** component → **Culling Mask**
4. Verify **UI** layer is **checked** ✓

**To Fix**: Click Culling Mask dropdown → Check `UI` layer

---

### Step 8: Build Settings Verification

Before building:
- [ ] **File > Build Settings** → Platform = Android
- [ ] **Build Settings** → Target device = Quest / Quest 2 / Quest 3
- [ ] **Build Settings** → Run Device = your connected headset

---

## 🔨 Build and Test Process

**Important**: Passthrough does NOT work in Unity Play Mode. You MUST build to device.

### Build Steps:
1. **File > Build Settings**
2. Click **Build and Run**
3. Wait for build to complete and install
4. Put on headset

### Testing Steps:
1. Look at your **left hand**
2. Press **Y button** on left controller
3. Menu should appear attached to hand
4. Press Y again to hide

### Expected Behavior:
- ✅ Menu appears instantly when Y pressed
- ✅ Gun disappears (right hand free for interaction)
- ✅ Menu follows left hand movement smoothly
- ✅ Menu stays above and in front of hand
- ✅ Menu is tilted up 45° for easy reading
- ✅ Menu is visible even through walls
- ✅ Can interact with buttons/sliders using right hand
- ✅ Y button again: menu closes, gun reappears

---

## 🐛 Debug Log Monitoring

### Via ADB (Recommended):
```bash
# Connect Quest via USB, then run:
adb logcat | grep WristMenu

# Or for more context:
adb logcat | grep -A 2 -B 2 WristMenu
```

### Success Messages to Look For:
```
[WristMenu] ========== STARTING INITIALIZATION ==========
[WristMenu] GameObject: WristMenuUI
[WristMenu] Active: True
[WristMenu] Menu Panel assigned: MenuPanel
[WristMenu] ✓ Found left hand anchor: LeftHandAnchor
[WristMenu] ✓ Attached to left hand anchor
[WristMenu] ✓ Set Canvas and all children to UI layer (layer 5)
[WristMenu] ✓ Set Canvas sorting order to 1000 (renders on top)
[WristMenu] ✓ Added UI layer (5) to camera culling mask
[WristMenu] ========== READY FOR TESTING ==========
```

### When You Press Y Button:
```
[WristMenu] ✓✓✓ TOGGLE BUTTON PRESSED ✓✓✓
[WristMenu] ========== MENU TOGGLE ==========
[WristMenu] Menu state: VISIBLE
[WristMenu] Menu Panel SetActive(True)
```

### Error Messages:
```
❌ [WristMenu] Could not find left hand anchor!
   → Fix: Verify OVRCameraRig exists in scene

❌ [WristMenu] Menu panel is not assigned!
   → Fix: Assign MenuPanel child to WristMenuController

❌ [WristMenu] No Canvas component found!
   → Fix: Add Canvas component to WristMenuUI
```

---

## 🔧 Troubleshooting Guide

### ❌ Problem: Menu Doesn't Appear at All

**Check in this order**:

1. **Verify Y button works**
   - Look in logs for `TOGGLE BUTTON PRESSED`
   - If not appearing, button detection is broken
   - Try A button: Change `toggleButton` to `One`

2. **Check parent attachment**
   - In logs, look for `Attached to left hand anchor`
   - If missing, left hand anchor wasn't found
   - Manually assign `LeftHandAnchor` to `Wrist Transform` field

3. **Verify camera can see UI**
   - Check `CenterEyeAnchor` camera culling mask
   - Must include `UI` layer
   - If not, manually check it in Inspector

4. **Check layer assignment**
   - `WristMenuUI` must be on `UI` layer (5)
   - If on wrong layer, manually set it
   - Apply to children when prompted

5. **Check Menu Panel reference**
   - `MenuPanel` field must be assigned
   - Child GameObject must exist
   - Try reassigning the reference

6. **Check scale**
   - Menu might be too small to see
   - Try `menuScale = 0.002` (larger)
   - Or too large: try `0.0003` (smaller)

---

### ❌ Problem: Menu Appears in Wrong Position

**Adjust these settings**:

1. **Too far/close**: Change `menuOffset.Z`
   - Closer: `0.1` (10cm)
   - Further: `0.3` (30cm)

2. **Too high/low**: Change `menuOffset.Y`
   - Lower: `0.05` (5cm)
   - Higher: `0.15` (15cm)

3. **Wrong angle**: Change `menuRotationOffset.X`
   - Flatter: `30` (30°)
   - More tilted: `60` (60°)

4. **Not facing you**: 
   - Should auto-face camera
   - If rotated wrong, adjust X/Y/Z rotation offset

---

### ❌ Problem: Menu Clips Through Walls

**Solutions**:

1. **Check sorting order** *(auto-set by code)*
   - Should be 1000 in Canvas component
   - If not, code didn't run - check logs

2. **Verify layer**
   - Menu must be on UI layer (5)
   - UI should render on top of everything

3. **Check passthrough setup**
   - Make sure passthrough is actually working
   - Menu should be visible through walls

**Note**: The code automatically sets high sorting order (1000) to render above passthrough.

---

### ❌ Problem: Menu Too Small or Large

**Quick fixes**:

| Size Issue | Menu Scale Value | Result |
|-----------|-----------------|---------|
| Too small | `0.001` or `0.002` | 2-4x larger |
| Good size | `0.0005` | Default |
| Too large | `0.0003` or `0.0002` | Smaller |

**How to adjust**:
1. Select `WristMenuUI`
2. In `WristMenuController`, change `Menu Scale`
3. Build and test again

---

### ❌ Problem: Button Doesn't Work

**Checklist**:
- [ ] Verify `Toggle Button` = `Three` (Y button)
- [ ] Verify `Controller` = `LTouch` (left hand)
- [ ] Check if button press is detected in logs
- [ ] Try different button (A = `One`, B = `Two`, X = `Four`)
- [ ] Verify controller is connected and tracked

**Alternative buttons**:
- A button: `Button.One` + `Controller.RTouch`
- B button: `Button.Two` + `Controller.RTouch`
- X button: `Button.Four` + `Controller.LTouch`
- Y button: `Button.Three` + `Controller.LTouch`

---

### ❌ Problem: Menu Visible But UI Elements Don't Work

**Check interaction**:
- [ ] Gun GameObject is assigned and disabling correctly
- [ ] Right hand is free when menu opens (gun hidden)
- [ ] CanvasGroup `blocksRaycasts` = true
- [ ] CanvasGroup `interactable` = true
- [ ] GraphicRaycaster component exists
- [ ] UI elements have correct components (Button, Slider, etc.)
- [ ] Verify right hand has interaction capability (ray or poke interactor)

---

## 🎨 Advanced: Visual Debug Helper

Add this to help debug visibility issues:

1. Select `WristMenuUI`
2. Add component: **Image**
3. Set **Color**: Bright color (red, cyan, yellow)
4. Set **Alpha**: 128 (semi-transparent)
5. Build and test

**Result**: You'll see a colored background on the canvas, making it easy to spot even if UI elements aren't rendering.

---

## 📊 Technical Details

### How It Works:
1. **On Start**: Code finds `LeftHandAnchor` automatically
2. **Attachment**: `WristMenuUI` becomes child of `LeftHandAnchor`
3. **Positioning**: Local position/rotation set relative to hand
4. **Following**: Menu follows hand automatically (parent-child relationship)
5. **Toggle**: Y button shows/hides `MenuPanel` child GameObject

### Key Code Features:
- **Auto-attachment**: No manual positioning needed
- **Layer enforcement**: Forces UI layer (5) on everything
- **Camera visibility**: Auto-adds UI layer to camera culling mask
- **High sorting order**: Ensures rendering above passthrough
- **Cull disable**: Prevents UI from disappearing at angles
- **CanvasGroup**: Better visibility and interaction control

### Why Direct Parenting:
- ✅ Simple and reliable
- ✅ No per-frame updates needed
- ✅ Perfect tracking with hand
- ✅ Same approach as flashlight
- ✅ No complex positioning math

---

## 🔄 Comparison: Old vs New Approach

| Aspect | Old Method | New Method |
|--------|-----------|------------|
| **Attachment** | Offset calculation every frame | Direct parent to hand |
| **Performance** | Update() calculations | Zero overhead |
| **Reliability** | Position drift possible | Perfect tracking |
| **Setup** | Complex offset tuning | Simple local position |
| **Layer** | Overlay UI (confusing) | UI layer only (simple) |

---

## ✅ Final Pre-Build Checklist

Before clicking "Build and Run":

- [ ] `WristMenuUI` GameObject is active
- [ ] `WristMenuUI` layer = `UI` (layer 5)
- [ ] `MenuPanel` is assigned in WristMenuController
- [ ] `MenuPanel` child GameObject exists and is inactive
- [ ] All UI element references are assigned (buttons, sliders, text)
- [ ] `CenterEyeAnchor` camera includes UI layer in culling mask
- [ ] `LeftHandAnchor` exists in scene
- [ ] `Toggle Button` = `Three` (Y button)
- [ ] `Controller` = `LTouch` (left controller)
- [ ] `Menu Scale` is reasonable (0.0005 default)

---

## 📱 ADB Quick Reference

```bash
# Connect device
adb devices

# Watch logs live
adb logcat | grep WristMenu

# Save logs to file
adb logcat | grep WristMenu > wrist_menu_log.txt

# Clear old logs first
adb logcat -c && adb logcat | grep WristMenu

# Get just errors
adb logcat | grep "WristMenu.*❌"

# Check if device connected
adb shell pm list packages | grep oculus
```

---

## 🎯 Success Criteria

Your wrist menu is working correctly when:

✅ Logs show successful initialization  
✅ Y button press detected in logs  
✅ Menu appears instantly when Y pressed  
✅ Menu is visible on your left hand  
✅ Menu follows hand movement perfectly  
✅ Menu is positioned comfortably for viewing  
✅ Menu stays visible through walls  
✅ Menu toggles on/off reliably  
✅ UI elements (buttons/sliders) are visible  
✅ UI elements respond to pointer/hand interaction  

---

## 🆘 Still Not Working?

If you've followed all steps and it still doesn't work:

1. **Check the logs** - They tell you exactly what's wrong
2. **Verify layer 5** - This is the #1 issue
3. **Check camera mask** - This is the #2 issue
4. **Try manual assignment** - Assign `LeftHandAnchor` manually
5. **Test button input** - Make sure Y button actually works in your build
6. **Check scale** - Menu might be invisible due to wrong scale
7. **Share logs** - Post the initialization logs for help

---

**Pro Tip**: The comprehensive logging will tell you exactly what's wrong. Always check logs first!
