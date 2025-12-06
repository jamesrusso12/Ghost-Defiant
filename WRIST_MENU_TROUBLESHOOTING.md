# Wrist Menu Not Appearing - Troubleshooting Guide

## Quick Checklist

When the wrist menu doesn't appear in your build, check these in order:

### 1. ✅ Scene Setup
- [ ] WristMenuCanvas exists in the scene
- [ ] WristMenuCanvas is enabled (checkbox checked)
- [ ] MenuPanel is a child of WristMenuCanvas
- [ ] OVRCameraRig exists in the scene
- [ ] OVRCameraRig has LeftHandAnchor child

### 2. ✅ Canvas Configuration
- [ ] Canvas Render Mode is **World Space**
- [ ] Canvas Scale is **(0.001, 0.001, 0.001)**
- [ ] Event Camera is assigned to **CenterEyeAnchor**
- [ ] Canvas has **Graphic Raycaster** component
- [ ] NO "OVR Overlay Canvas" component (remove if present)

### 3. ✅ WristMenuController Script
- [ ] WristMenuController is on WristMenuCanvas
- [ ] Script is enabled (checkbox checked)
- [ ] Menu Panel reference is assigned
- [ ] All UI element references are assigned
- [ ] Toggle Button is set (default: Three = Y button)
- [ ] Controller is set (default: LTouch = Left hand)

### 4. ✅ MenuPanel Setup
- [ ] MenuPanel starts **disabled/hidden** in scene (unchecked)
- [ ] MenuPanel has RectTransform
- [ ] MenuPanel has Image component (for background)
- [ ] MenuPanel has child UI elements

### 5. ✅ Build Settings
- [ ] Scene is added to Build Settings
- [ ] Scene is enabled in Build Settings
- [ ] Build platform is Android (for Quest)
- [ ] All required scripts are included in build

---

## Debug Steps

### Step 1: Check Console Logs

After updating the WristMenuController script, you should see these logs when the game starts:

```
[WristMenu] Starting initialization...
[WristMenu] Menu panel found and hidden
[WristMenu] Pause button listener added
[WristMenu] Volume slider configured
[WristMenu] Brightness slider configured
[WristMenu] Found OVRCameraRig, left hand anchor: LeftHandAnchor
[WristMenu] Initialization complete
```

**If you don't see these logs:**
- Script is not running
- Script is on wrong GameObject
- Script is disabled

### Step 2: Test Toggle Button

Press the **Y button** on your left controller. You should see:

```
[WristMenu] Toggle button pressed
[WristMenu] Menu toggled: VISIBLE
```

**If you don't see these logs:**
- Button input is not being detected
- Wrong button configured
- Wrong controller configured

### Step 3: Check for Errors

Look for these error messages:

**"Menu panel is not assigned!"**
- Solution: Assign MenuPanel in Inspector

**"Could not find left hand anchor!"**
- Solution: Make sure OVRCameraRig is in scene
- Check LeftHandAnchor exists under OVRCameraRig

**"Menu is visible but wrist transform is null!"**
- Solution: Wrist transform not found, check OVRCameraRig setup

---

## Common Issues & Solutions

### Issue 1: Menu Never Appears

**Symptoms:**
- Press Y button, nothing happens
- No errors in console

**Solutions:**

1. **Check MenuPanel is assigned:**
   - Select WristMenuCanvas
   - Inspector → WristMenuController
   - Verify "Menu Panel" field has MenuPanel assigned

2. **Check MenuPanel starts hidden:**
   - Select MenuPanel in Hierarchy
   - It should be **unchecked** (disabled)
   - If checked, uncheck it and save scene

3. **Test manually:**
   - In Unity Editor, press Play
   - Select MenuPanel in Hierarchy
   - Check the checkbox to enable it
   - If it appears, the script is the problem
   - If it doesn't appear, the UI setup is the problem

### Issue 2: Menu Appears But Wrong Position

**Symptoms:**
- Menu appears but not on wrist
- Menu is too far/close
- Menu is rotated wrong

**Solutions:**

1. **Adjust Menu Offset:**
   ```
   Try these values:
   - (0, 0.05, 0.1) - Default
   - (0, 0.1, 0.15) - Higher and further forward
   - (0, 0, 0.12) - Closer to hand
   - (-0.05, 0.05, 0.1) - Offset to side
   ```

2. **Adjust Menu Rotation Offset:**
   ```
   Try these values:
   - (45, 0, 0) - Default, tilted up
   - (60, 0, 0) - More tilted
   - (30, 0, 0) - Less tilted
   - (45, 15, 0) - Tilted and rotated
   ```

3. **Adjust Menu Scale:**
   ```
   Try these values:
   - 0.0005 - Default
   - 0.0008 - Larger
   - 0.0003 - Smaller
   ```

### Issue 3: Menu Appears But Can't Click

**Symptoms:**
- Menu is visible
- Can't interact with buttons/sliders

**Solutions:**

1. **Check Event Camera:**
   - Select WristMenuCanvas
   - Canvas component → Event Camera
   - Must be assigned to CenterEyeAnchor

2. **Check Graphic Raycaster:**
   - Select WristMenuCanvas
   - Should have "Graphic Raycaster" component
   - If missing, Add Component → Graphic Raycaster

3. **Check Button Raycast Target:**
   - Select PauseButton
   - Image component → Raycast Target ✓ (checked)
   - Select PauseButtonText
   - TextMeshPro → Raycast Target ✗ (unchecked)

4. **Check EventSystem:**
   - Look for EventSystem in Hierarchy
   - Should have OVRInputModule component
   - If missing, create: GameObject → UI → Event System

### Issue 4: Works in Editor, Not in Build

**Symptoms:**
- Menu works perfectly in Unity Editor
- Doesn't work in Quest build

**Solutions:**

1. **Check Build Settings:**
   - File → Build Settings
   - Verify scene is in "Scenes In Build"
   - Scene checkbox is checked
   - Scene index is correct

2. **Check Script Compilation:**
   - Look for compile errors in Console
   - Make sure all scripts are in Assets/Scripts
   - No missing script references

3. **Check OVR Settings:**
   - Edit → Project Settings → XR Plug-in Management
   - Android tab → Oculus checked
   - Oculus → Target Devices includes Quest

4. **Use Debug Build:**
   - Build Settings → Development Build ✓
   - Connect Quest via USB
   - View logs: adb logcat -s Unity

### Issue 5: Menu Flickers or Jitters

**Symptoms:**
- Menu appears but shakes/jitters
- Menu position unstable

**Solutions:**

1. **Check Update vs LateUpdate:**
   - Menu position updates in Update()
   - Consider moving to LateUpdate() for smoother tracking

2. **Reduce Update Frequency:**
   - Don't update position every frame
   - Add a timer to update every 0.1 seconds

3. **Check Hand Tracking:**
   - Make sure hand tracking is stable
   - Controller tracking might be better than hand tracking

### Issue 6: Button Press Not Detected

**Symptoms:**
- Menu appears
- Y button doesn't toggle menu

**Solutions:**

1. **Test Different Buttons:**
   ```csharp
   // In Inspector, try these:
   - Button.One (A/X button)
   - Button.Two (B/Y button)
   - Button.Three (Thumbstick click)
   - Button.Four (Grip)
   ```

2. **Test Different Controller:**
   ```csharp
   // In Inspector, try:
   - LTouch (Left controller)
   - RTouch (Right controller)
   - Touch (Both controllers)
   ```

3. **Add Debug Input Test:**
   - Add this to Update() temporarily:
   ```csharp
   if (OVRInput.GetDown(OVRInput.Button.Any, OVRInput.Controller.LTouch))
   {
       Debug.Log("Left controller button pressed!");
   }
   ```

---

## Testing Procedure

### In Unity Editor:

1. **Press Play**
2. **Check Console** for initialization logs
3. **Press Y key** on keyboard (simulates Y button)
4. **Menu should appear** in scene view
5. **Check Console** for toggle logs

### On Quest Device:

1. **Build and Run** to Quest
2. **Put on headset**
3. **Look at left hand**
4. **Press Y button** on left controller
5. **Menu should appear** on wrist

### If Still Not Working:

1. **Enable Development Build**
2. **Connect Quest via USB**
3. **Run:** `adb logcat -s Unity`
4. **Look for [WristMenu] logs**
5. **Share logs for further debugging**

---

## Advanced Debugging

### Add Visual Indicator

Add a always-visible object to test positioning:

1. Create a Cube in scene
2. Scale to (0.05, 0.05, 0.05)
3. Add this script to WristMenuController:

```csharp
public GameObject debugCube; // Assign in Inspector

void Update()
{
    // ... existing code ...
    
    // Debug: Show cube at wrist position
    if (debugCube != null && wristTransform != null)
    {
        debugCube.transform.position = wristTransform.position;
        debugCube.transform.rotation = wristTransform.rotation;
    }
}
```

If cube appears on wrist but menu doesn't, the problem is with the menu, not positioning.

### Check Transform Values

Add this to see actual values:

```csharp
void UpdateMenuPosition()
{
    Vector3 position = wristTransform.position + wristTransform.TransformDirection(menuOffset);
    menuPanel.transform.position = position;
    
    Debug.Log($"[WristMenu] Wrist: {wristTransform.position}, Menu: {position}");
    Debug.Log($"[WristMenu] Menu Scale: {menuPanel.transform.localScale}");
}
```

### Force Menu Visible for Testing

Temporarily force menu visible:

```csharp
void Start()
{
    // ... existing code ...
    
    // DEBUG: Force menu visible after 2 seconds
    Invoke("ForceShowMenu", 2f);
}

void ForceShowMenu()
{
    Debug.Log("[WristMenu] FORCING MENU VISIBLE FOR DEBUG");
    isMenuVisible = true;
    if (menuPanel != null)
    {
        menuPanel.SetActive(true);
    }
}
```

---

## Quick Fixes

### Fix 1: Manual Wrist Transform Assignment

Instead of auto-finding, assign manually:

1. In Hierarchy, expand OVRCameraRig
2. Find LeftHandAnchor
3. Select WristMenuCanvas
4. Drag LeftHandAnchor to "Wrist Transform" field

### Fix 2: Simplify Toggle Input

Change to simpler input:

```csharp
void Update()
{
    // Simple test - any button on left controller
    if (OVRInput.GetDown(OVRInput.Button.Any, OVRInput.Controller.LTouch))
    {
        ToggleMenu();
    }
}
```

### Fix 3: Make Menu Always Visible (Testing)

For testing, make menu always visible:

1. Select MenuPanel in Hierarchy
2. Check the checkbox (enable it)
3. Menu should now always be visible
4. If you can see it now, the toggle logic is the problem

---

## Contact Info for Further Help

If none of these solutions work, provide:

1. **Console logs** (especially [WristMenu] logs)
2. **Screenshots** of:
   - WristMenuCanvas Inspector
   - WristMenuController component
   - Hierarchy showing OVRCameraRig
3. **Build platform** (Quest 2, Quest 3, etc.)
4. **Unity version**
5. **What you've already tried**

---

## Summary Checklist

Before asking for help, verify:

- [ ] Updated WristMenuController script (with debug logs)
- [ ] Canvas is World Space with scale 0.001
- [ ] MenuPanel is assigned in Inspector
- [ ] MenuPanel starts disabled in scene
- [ ] OVRCameraRig exists in scene
- [ ] Event Camera is assigned
- [ ] Tested in Unity Editor first
- [ ] Checked console for [WristMenu] logs
- [ ] Tried different toggle buttons
- [ ] Tried manual wrist transform assignment
- [ ] Checked Build Settings includes scene

Good luck! 🎮

