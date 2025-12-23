# Quick Start: Floating Passthrough Menu (3-Minute Summary)

## 🎯 What You're Building

A floating menu in **passthrough mode** (MR) that:
- ✅ Shows your real world with menu floating in it
- ✅ **Slowly follows** you as you move (Smooth Speed: 2.0)
- ✅ **Always faces** you when you turn your head
- ✅ Canvas size: **1920×1080** (like your screenshot)
- ✅ Has Start and Quit buttons
- ✅ Shows your game title/logo

---

## ⚡ Quick Setup Steps

### 1. Enable Passthrough (2 minutes)
```
1. Open StartScreen scene
2. Disable your old menu (uncheck it in Hierarchy)
3. Building Blocks (~ key) → Add "Passthrough Layer"
4. Find OVRManager → Enable "Insight Passthrough"
```

### 2. Create Menu Panel (5 minutes)
```
1. Create Empty GameObject → "StartMenuPanel"
2. Add UI → Canvas → "MenuCanvas"
   - Width: 1920, Height: 1080
   - Scale: 0.001, 0.001, 0.001
   - World Space mode
3. Add Component → OVROverlayCanvas
4. Add UI → Panel → "Background"
   - Color: White (255, 255, 255)
```

### 3. Add UI Elements (5 minutes)
```
1. Add your game title image (Assets/Textures/UI/Game Title (1).png)
2. Add Start button
3. Add Quit button
4. Assign buttons to StartMenuController
```

### 4. Make It Follow & Face You (2 minutes)
```
1. Select StartMenuPanel
2. Add Component → BillboardMenu
3. Settings:
   - Smooth Follow: ✓
   - Smooth Speed: 2.0 (slow follow)
   - Keep Upright: ✓
   - Distance: 2.5
   - Offset Y: 0
```

### 5. Test
```
1. Build to Quest
2. Put on headset
3. You should see real world + floating menu
4. Walk around → menu slowly follows
5. Turn head → menu always faces you
```

---

## 📋 Critical Settings

### Canvas (MenuCanvas)
```yaml
Render Mode: World Space
Width: 1920
Height: 1080
Scale: 0.001, 0.001, 0.001

Components:
  - Canvas
  - OVROverlayCanvas (for passthrough)
  - GraphicRaycaster (for interaction)
```

### Billboard (StartMenuPanel)
```yaml
Script: BillboardMenu
Settings:
  Smooth Follow: ✓ (checked)
  Smooth Speed: 2.0  ← SLOW following
  Keep Upright: ✓ (checked)
  Distance: 2.5
  Offset Y: 0 (eye level)
  Start Delay: 0.5
```

### Background Panel
```yaml
Component: Image
Color: White (255, 255, 255, 255)
OR Light Gray (230, 230, 230, 255)
RectTransform: Stretch to fill canvas
```

### Passthrough
```yaml
In Scene:
  - OVRPassthroughLayer GameObject

On OVRManager:
  - Insight Passthrough: ✓ Enabled
  - Pass-through Support: Supported
```

---

## 🎨 Scene Hierarchy

```
StartScreen
├── OVRCameraRig
│   ├── OVRManager (Passthrough enabled)
│   └── CenterEyeAnchor (Camera)
│
├── OVRPassthroughLayer ← Enables MR
│
├── EventSystem
│
└── StartMenuPanel
    ├── BillboardMenu ← Makes it follow & face you
    └── MenuCanvas (1920×1080, scale 0.001)
        ├── Background (White panel)
        ├── GameTitleImage
        ├── StartButton
        └── QuitButton
```

---

## ⚙️ Follow Speed Reference

| Speed | Behavior |
|-------|----------|
| 1.0 | Very slow, laggy |
| **2.0** | **Slow, comfortable** ← YOUR SETTING |
| 3.0 | Medium |
| 5.0 | Default, responsive |
| 10+ | Fast, almost instant |

**You want 2.0 for "slowly follows"**

---

## ✅ Checklist

**Scene Setup:**
- [ ] OVRPassthroughLayer in scene
- [ ] OVRManager has Insight Passthrough ✓
- [ ] Old menu disabled (unchecked)

**Menu Setup:**
- [ ] StartMenuPanel created
- [ ] BillboardMenu script added (Smooth Speed: 2.0)
- [ ] Canvas size: 1920×1080, scale: 0.001
- [ ] OVROverlayCanvas added to canvas
- [ ] White background panel added
- [ ] Game title image added
- [ ] Start/Quit buttons created

**Testing:**
- [ ] Built to Quest
- [ ] Can see real world (passthrough working)
- [ ] Menu visible (white/light panel)
- [ ] Menu slowly follows when you move
- [ ] Menu faces you when you turn
- [ ] Buttons work with controller/hands

---

## 🚨 Common Issues

### "I can't see passthrough"
- ✅ OVRPassthroughLayer in scene?
- ✅ OVRManager → Insight Passthrough enabled?
- ✅ Testing on Quest 2/3/Pro? (not Quest 1)
- ✅ Camera permission enabled in Quest settings?

### "Menu not visible"
- ✅ Background panel is WHITE (not transparent)?
- ✅ Canvas scale is 0.001 (not 1)?
- ✅ StartMenuPanel is active?

### "Menu doesn't follow me"
- ✅ BillboardMenu script on StartMenuPanel?
- ✅ Smooth Follow is checked?
- ✅ Smooth Speed is 2.0?

### "Menu doesn't face me"
- ✅ BillboardMenu script enabled?
- ✅ Keep Upright is checked?
- ✅ No errors in Console?

---

## 📖 Full Guide

For complete step-by-step instructions, see:
**NEW_START_MENU_SETUP_GUIDE.md**

---

*Quick Start v1.0 - Passthrough Floating Menu*

