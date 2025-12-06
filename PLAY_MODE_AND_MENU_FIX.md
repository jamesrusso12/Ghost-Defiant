# Play Mode & Wrist Menu Issues - Complete Fix Guide

## 🚨 Critical Information

### Why You Can't Use Play Mode

**Meta Quest passthrough projects CANNOT run in Unity Play mode!**

This is **normal and expected** behavior because:
- Passthrough requires actual Quest hardware
- Room scanning requires physical space
- Hand/controller tracking needs Quest sensors
- MR features don't work in editor

**You MUST build to device to test!**

---

## 🔧 Wrist Menu Not Appearing - Solutions

Based on your logs, the menu IS initializing correctly, but you can't see it. Here are the likely causes:

### Issue 1: Canvas Layer Conflict with Passthrough

**Problem:** Your dual passthrough setup might be rendering over the UI

**Solution:** The updated WristMenuController now:
1. Forces correct layers (Overlay UI for canvas, UI for elements)
2. Ensures camera can see UI layers
3. Disables transparent mesh culling
4. Configures before MRUIConfigurator interferes

### Issue 2: Menu Scale Too Small

**Problem:** Menu might be too tiny to see

**Solution:** Try different scales in Inspector:

```
WristMenuController → Menu Scale:
- 0.0005 (default) - Very small
- 0.001 (try this) - Larger
- 0.002 (try this) - Even larger
- 0.005 (try this) - Much larger for testing
```

### Issue 3: Menu Position Wrong

**Problem:** Menu might be behind you or in wrong position

**Solution:** Try different offsets:

```
WristMenuController → Menu Offset:
Current: (0, 0.05, 0.1)

Try these:
- (0, 0.1, 0.2) - Higher and further forward
- (0, 0, 0.3) - Much further forward (easier to see)
- (0.1, 0.1, 0.2) - Offset to side
```

### Issue 4: Camera Culling Mask

**Problem:** Camera not rendering UI layers

**Solution:** The updated script now automatically adds UI layers to camera culling mask

### Issue 5: OVR Overlay Canvas Conflict

**Problem:** If you still have OVR Overlay Canvas on the wrist menu canvas

**Solution:** 
1. Select WristMenuCanvas
2. Remove "OVR Overlay Canvas (Script)" component
3. The updated WristMenuController will handle everything

---

## 🎯 Step-by-Step Fix Process

### Step 1: Update Your Scene

1. **Open MainGameScene in Unity**

2. **Find PlayerHitbox:**
   - Select PlayerHitbox in Hierarchy
   - Remove any "Script (Missing)" components
   - Or delete PlayerHitbox entirely (not needed without health system)

3. **Find WristMenuCanvas:**
   - If it doesn't exist, create it (follow WRIST_MENU_SETUP_GUIDE.md)
   - If it exists, select it

4. **Configure WristMenuCanvas:**
   ```
   Components needed:
   ✓ Canvas (Render Mode: World Space)
   ✓ Canvas Scaler
   ✓ Graphic Raycaster
   ✓ WristMenuController
   ✗ NO OVR Overlay Canvas (remove if present)
   
   Canvas Settings:
   - Render Mode: World Space
   - Event Camera: CenterEyeAnchor
   - Scale: (0.001, 0.001, 0.001) - Start larger for testing
   
   WristMenuController Settings:
   - Menu Panel: [Assign your MenuPanel]
   - Menu Scale: 0.001 (try 0.002 for testing)
   - Menu Offset: (0, 0.1, 0.2) - More visible
   - Toggle Button: Three (Y button)
   - Controller: LTouch
   - Ignore Auto Configuration: ✓ (checked)
   - Canvas Layer Name: "Overlay UI"
   - UI Elements Layer Name: "UI"
   ```

5. **Save Scene** (Ctrl+S)

### Step 2: Test Build

1. **Build Settings:**
   - File → Build Settings
   - Platform: Android
   - Scene in build: MainGameScene ✓ checked

2. **Build and Run:**
   - Connect Quest via USB
   - Click "Build and Run"
   - Wait for build to complete
   - App will auto-launch on Quest

3. **Test in Headset:**
   - Put on Quest
   - Look at your left wrist
   - Press Y button on left controller
   - Menu should appear

### Step 3: If Menu Still Doesn't Appear

**Try Force Visible Test:**

1. In Unity, select MenuPanel
2. Check the checkbox (enable it)
3. Build and Run
4. If you can see it now (always visible), the toggle is the problem
5. If you still can't see it, the rendering/layer is the problem

**Try Larger Scale:**

1. Select WristMenuCanvas
2. WristMenuController → Menu Scale: **0.005** (10x larger)
3. Build and Run
4. If you can see it now, scale was too small

**Try Different Position:**

1. WristMenuController → Menu Offset: **(0, 0, 0.5)**
2. This puts it 0.5m in front of your hand (very visible)
3. Build and Run
4. If you can see it now, position was wrong

---

## 🔍 Advanced Debugging

### Enable Visual Debugging

Add a visible cube to test positioning:

1. **Create a Cube in scene**
2. **Name it "WristDebugCube"**
3. **Scale: (0.05, 0.05, 0.05)**
4. **Add this to WristMenuController:**

```csharp
[Header("Debug")]
public GameObject debugCube; // Assign in Inspector
public bool showDebugCube = true;

void Update()
{
    // ... existing code ...
    
    // Debug: Show cube at wrist position
    if (showDebugCube && debugCube != null && wristTransform != null)
    {
        debugCube.SetActive(true);
        Vector3 cubePos = wristTransform.position + wristTransform.TransformDirection(menuOffset);
        debugCube.transform.position = cubePos;
        debugCube.transform.rotation = wristTransform.rotation;
    }
    else if (debugCube != null)
    {
        debugCube.SetActive(false);
    }
}
```

5. **Assign WristDebugCube to Debug Cube field**
6. **Build and Run**
7. **If you see the cube but not the menu**, the problem is with the UI rendering
8. **If you don't see the cube**, the problem is with wrist transform finding

### Check Layers in Unity

1. **Edit → Project Settings → Tags and Layers**
2. **Verify these layers exist:**
   - Layer 5: UI
   - Layer 10 (or similar): Overlay UI

3. **If "Overlay UI" doesn't exist:**
   - Add it to an empty layer slot
   - Or change WristMenuController → Canvas Layer Name to "UI"

### Alternative: Use UI Layer Only

If Overlay UI is causing issues:

1. WristMenuController → Canvas Layer Name: **"UI"**
2. WristMenuController → UI Elements Layer Name: **"UI"**
3. This uses standard UI layer for everything

---

## 🎮 Testing Procedure

### Build Settings Checklist:
- [ ] Platform: Android
- [ ] Texture Compression: ASTC
- [ ] MainGameScene in build list
- [ ] MainGameScene checkbox checked
- [ ] Development Build ✓ (for debugging)

### In-Headset Testing:
1. Put on Quest
2. Wait for game to fully load
3. Look at left hand/wrist
4. Press **Y button** (left controller)
5. Listen for haptic feedback (if implemented)
6. Look around your wrist area
7. Try pressing Y multiple times
8. Try different hand positions

### If Still Not Visible:

**Test 1: Force Menu Always Visible**
- In Unity: Enable MenuPanel (checkbox)
- Build and Run
- Menu should always be visible
- If you see it: Toggle logic is the problem
- If you don't: Rendering is the problem

**Test 2: Huge Scale**
- Menu Scale: 0.01 (20x larger)
- Build and Run
- Should be impossible to miss
- If you see it: Scale was too small

**Test 3: In Front of Face**
- Menu Offset: (0, 0, 0.5)
- Menu Rotation Offset: (0, 0, 0)
- Build and Run
- Menu will float in front of hand
- If you see it: Position was wrong

---

## 🔧 Quick Fixes to Try

### Fix 1: Simplify Everything

Temporarily simplify to test:

```csharp
// In WristMenuController, replace Update() with:
void Update()
{
    // Force menu always visible for testing
    if (menuPanel != null)
    {
        menuPanel.SetActive(true);
    }
    
    // Position in front of camera instead of wrist
    if (menuPanel != null)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 pos = cam.transform.position + cam.transform.forward * 1f;
            menuPanel.transform.position = pos;
            menuPanel.transform.rotation = Quaternion.LookRotation(menuPanel.transform.position - cam.transform.position);
            menuPanel.transform.localScale = Vector3.one * 0.001f;
        }
    }
}
```

This will:
- Force menu always visible
- Position it 1m in front of camera
- Ignore wrist positioning
- If this works, wrist positioning is the problem

### Fix 2: Use Different Render Mode

Try Overlay Canvas mode:

1. Canvas → Render Mode: **Screen Space - Camera**
2. Canvas → Render Camera: CenterEyeAnchor
3. Canvas → Plane Distance: 1.0
4. Build and Run

### Fix 3: Check Layer Conflicts

Your dual passthrough might be using layers that conflict:

1. **Edit → Project Settings → Tags and Layers**
2. **Check what layers are used**
3. **Try using layer 31** (usually unused):
   - Add "WristUI" to layer 31
   - WristMenuController → Canvas Layer Name: "WristUI"

---

## 📊 Diagnostic Checklist

When you build and run, check these in order:

### Console Logs (via adb logcat):
```bash
adb logcat -s Unity | grep WristMenu
```

Look for:
- [ ] "[WristMenu] Starting initialization..."
- [ ] "[WristMenu] Menu panel found and hidden"
- [ ] "[WristMenu] Found OVRCameraRig, left hand anchor: LeftHandAnchor"
- [ ] "[WristMenu] Configuring Canvas for wrist menu..."
- [ ] "[WristMenu] Canvas configuration complete"
- [ ] "[WristMenu] Toggle button pressed" (when you press Y)
- [ ] "[WristMenu] Menu toggled: VISIBLE"

### In Headset:
- [ ] Game loads successfully
- [ ] Passthrough is visible
- [ ] Virtual environment is visible
- [ ] Can see your hands/controllers
- [ ] Y button press is detected
- [ ] Menu appears on wrist

### If Menu Appears But Wrong:
- [ ] Menu too small → Increase Menu Scale
- [ ] Menu wrong position → Adjust Menu Offset
- [ ] Menu wrong rotation → Adjust Menu Rotation Offset
- [ ] Can't click buttons → Check Event Camera and Graphic Raycaster

---

## 🎯 Most Likely Solutions

Based on your dual passthrough setup:

### Solution 1: Layer Configuration (Most Likely)

Your passthrough layers might be blocking UI. The updated script now:
- Sets Canvas to "Overlay UI" layer
- Sets UI elements to "UI" layer
- Forces camera to see both layers
- Should render on top of passthrough

### Solution 2: Scale Too Small

Menu might be microscopic:
- Try Menu Scale: **0.002** (instead of 0.0005)
- Or try: **0.005** for testing

### Solution 3: Position Behind Hand

Menu might be behind your hand:
- Try Menu Offset: **(0, 0.1, 0.3)** - Further forward
- Try Menu Offset: **(0, 0.2, 0.2)** - Higher and forward

---

## 🆘 Emergency Fallback

If nothing works, use a simpler approach:

### Create Floating Menu Instead of Wrist Menu

1. **Don't attach to wrist**
2. **Position in front of player at start**
3. **Use GameUILazyFollow** (like your other UI)
4. **Toggle with Y button**

This avoids wrist positioning complexity and should definitely work.

Would you like me to create this simpler version?

---

## 📋 Summary

**Why no Play mode:**
- ✅ Normal for passthrough projects
- ✅ Must build to Quest device
- ✅ Not a bug, expected behavior

**Why menu not appearing:**
- ❓ Possibly layer conflict with passthrough
- ❓ Possibly scale too small
- ❓ Possibly position wrong
- ❓ Possibly camera not seeing UI layer

**Next steps:**
1. Fix PlayerHitbox missing script
2. Update WristMenuController script (already done)
3. Try larger scale (0.002 or 0.005)
4. Try different position (0, 0.1, 0.3)
5. Build and test
6. Check console logs
7. Try debug cube method

**If still not working:**
- Share full console logs (adb logcat)
- Try the emergency fallback (floating menu)
- Or we can create a simpler wrist menu approach

Good luck! 🎮

