# Wrist Menu UI - Complete Setup Guide

## Overview
This guide will walk you through creating a complete wrist-mounted settings menu for your VR game, similar to what you see in games like Half-Life: Alyx.

---

## Part 1: Canvas Setup

### Step 1: Create the Canvas
1. In Hierarchy, right-click → **UI → Canvas**
2. Rename it to **"WristMenuCanvas"**
3. Select the Canvas in Hierarchy

### Step 2: Configure Canvas Component
In the Inspector, find the **Canvas** component:

```
Canvas Component Settings:
├── Render Mode: World Space
├── Event Camera: [Drag CenterEyeAnchor here]
├── Sorting Layer: Default
└── Order in Layer: 0
```

**Important:** Find your CenterEyeAnchor:
- Look for: `OVRCameraRig → TrackingSpace → CenterEyeAnchor`
- Drag it to the Event Camera field

### Step 3: Set Canvas Transform
In the Transform component:

```
Position: (0, 0, 0)
Rotation: (0, 0, 0)
Scale: (0.001, 0.001, 0.001)  ← Very important!
```

### Step 4: Set Canvas RectTransform
In the Rect Transform component:

```
Width: 1000
Height: 600
```

### Step 5: Add/Configure Canvas Scaler
If there's no Canvas Scaler, add it:
- Click **Add Component** → **Canvas Scaler**

```
Canvas Scaler Settings:
├── UI Scale Mode: Constant Physical Size
├── Physical Unit: Centimeters
├── Fallback Screen DPI: 96
└── Reference Pixels Per Unit: 100
```

### Step 6: Configure Graphic Raycaster
Should already exist, but verify:

```
Graphic Raycaster Settings:
├── Ignore Reversed Graphics: ✓ (checked)
├── Blocking Objects: Everything
└── Blocking Mask: Everything
```

### Step 7: Remove OVR Overlay Canvas (If Present)
- If you see "OVR Overlay Canvas (Script)" component
- Click the **3 dots** (⋮) → **Remove Component**
- This prevents the overlay errors

---

## Part 2: Menu Panel Structure

### Step 1: Create Main Panel
1. Right-click **WristMenuCanvas** → **UI → Panel**
2. Rename to **"MenuPanel"**
3. This will be your menu background

### Step 2: Configure MenuPanel RectTransform
```
Anchors: Center-Center
Pivot: (0.5, 0.5)
Position: (0, 0, 0)
Width: 800
Height: 500
```

### Step 3: Style MenuPanel Background
In the **Image** component:

```
Color: Dark semi-transparent
  R: 0.1
  G: 0.1
  B: 0.15
  A: 0.9 (90% opaque)
```

**Optional:** Add rounded corners
- Source Image: Use a rounded rectangle sprite
- Image Type: Sliced

---

## Part 3: UI Elements Layout

### Layout Structure:
```
WristMenuCanvas
└── MenuPanel
    ├── TitleText
    ├── PauseButton
    │   └── ButtonText
    ├── VolumeSection
    │   ├── VolumeLabel
    │   ├── VolumeSlider
    │   │   ├── Background
    │   │   ├── Fill Area
    │   │   │   └── Fill
    │   │   └── Handle Slide Area
    │   │       └── Handle
    │   └── VolumeValueText
    └── BrightnessSection
        ├── BrightnessLabel
        ├── BrightnessSlider
        │   ├── Background
        │   ├── Fill Area
        │   │   └── Fill
        │   └── Handle Slide Area
        │       └── Handle
        └── BrightnessValueText
```

---

## Part 4: Creating Each UI Element

### ⚠️ Important Unity UI Rule

**One Graphic Component Per GameObject!**

Unity only allows ONE of these components per GameObject:
- Image
- RawImage  
- Text
- TextMeshProUGUI
- TextMeshPro

This is why:
- Buttons need **Image** on the parent (for background)
- Buttons need **TextMeshPro on a CHILD** (for text)
- You CANNOT have both on the same GameObject

If you try to add a second Graphic component, you'll get an error. Always structure your UI with this in mind!

---

### A. Title Text

1. Right-click **MenuPanel** → **UI → Text - TextMeshPro**
2. Rename to **"TitleText"**
3. Configure:

```
RectTransform:
├── Anchors: Top-Center
├── Pivot: (0.5, 1)
├── Position: (0, -20, 0)
├── Width: 700
└── Height: 80

TextMeshPro Settings:
├── Text: "SETTINGS"
├── Font: Roboto Bold (or your preferred font)
├── Font Size: 48
├── Alignment: Center + Top
├── Color: White or Cyan (0.3, 0.8, 1, 1)
└── Font Style: Bold
```

---

### B. Pause/Resume Button

**Important Note:** Unity only allows ONE Graphic component (Image, Text, TextMeshPro, etc.) per GameObject. The button needs an Image for the background and TextMeshPro for the text, so the text MUST be a child object.

#### Step 1: Create the Button
1. Right-click **MenuPanel** → **UI → Button - TextMeshPro**
2. Unity will automatically create the correct structure:
   ```
   Button (TMP)
   ├── Image component (background)
   ├── Button component
   └── Text (TMP) (child object)
       └── TextMeshProUGUI component
   ```

#### Step 2: Rename Objects
1. Rename **"Button (TMP)"** to **"PauseButton"**
2. Expand PauseButton and rename the child **"Text (TMP)"** to **"PauseButtonText"**

#### Step 3: Configure PauseButton (Parent)
Select **PauseButton** in Hierarchy:

```
RectTransform:
├── Anchors: Top-Center
├── Pivot: (0.5, 1)
├── Position: (0, -120, 0)
├── Width: 600
└── Height: 80

Image Component (Button Background):
├── Source Image: UI Sprite (default)
├── Color: (0.2, 0.2, 0.25, 1) - Dark gray
├── Material: Default UI Material
└── Raycast Target: ✓ (checked)

Button Component:
├── Interactable: ✓ (checked)
├── Transition: Color Tint
├── Target Graphic: Image (should auto-assign)
├── Normal Color: (1, 1, 1, 1) - White
├── Highlighted Color: (0.9, 0.9, 0.9, 1) - Light gray
├── Pressed Color: (0.7, 0.7, 0.7, 1) - Darker gray
├── Selected Color: (0.9, 0.9, 0.9, 1) - Light gray
├── Disabled Color: (0.5, 0.5, 0.5, 0.5) - Faded gray
└── Color Multiplier: 1
```

**Pro Tip:** The Button component's colors multiply with the Image color. So:
- Image Color = Base color (dark gray)
- Button Colors = Tint multipliers (white = no change, gray = darker)

#### Step 4: Configure PauseButtonText (Child)
Select **PauseButtonText** (the child object):

```
RectTransform:
├── Anchors: Stretch-Stretch (all corners)
├── Left: 0
├── Right: 0
├── Top: 0
├── Bottom: 0
└── Position: (0, 0, 0)

TextMeshPro - Text (UI) Component:
├── Text: "PAUSE"
├── Font Asset: LiberationSans SDF (or your preferred font)
├── Font Style: Bold
├── Font Size: 36
├── Auto Size: ✗ (unchecked)
├── Color: White (1, 1, 1, 1)
├── Alignment: Center + Middle (both horizontally and vertically)
├── Wrapping: ✗ (disabled)
├── Overflow: Overflow
└── Raycast Target: ✗ (unchecked - text shouldn't block clicks)
```

#### Troubleshooting Button Creation:

**Problem:** "Can't add 'Image' because TextMeshProUGUI already exists"
- **Cause:** You tried to add Image to an object that already has TextMeshPro
- **Solution:** Delete and recreate using "Button - TextMeshPro" (not plain Button)

**Problem:** Button created but no text visible
- **Solution:** Make sure the child TextMeshPro object exists and is enabled

**Problem:** Can't click the button
- **Solution:** 
  - Check Image component has "Raycast Target" checked
  - Check Button component's "Target Graphic" is assigned to the Image
  - Make sure child text has "Raycast Target" unchecked (so it doesn't block clicks)

---

### C. Volume Section

#### 1. Create Container
1. Right-click **MenuPanel** → **UI → Empty** (or Panel)
2. Rename to **"VolumeSection"**

```
RectTransform:
├── Anchors: Top-Center
├── Pivot: (0.5, 1)
├── Position: (0, -230, 0)
├── Width: 700
└── Height: 100
```

#### 2. Volume Label
1. Right-click **VolumeSection** → **UI → Text - TextMeshPro**
2. Rename to **"VolumeLabel"**

```
RectTransform:
├── Anchors: Top-Left
├── Pivot: (0, 1)
├── Position: (20, -10, 0)
├── Width: 300
└── Height: 40

TextMeshPro Settings:
├── Text: "Volume"
├── Font Size: 28
├── Alignment: Left + Middle
└── Color: White (1, 1, 1, 1)
```

#### 3. Volume Slider
1. Right-click **VolumeSection** → **UI → Slider**
2. Rename to **"VolumeSlider"**

```
RectTransform:
├── Anchors: Top-Stretch (left-right stretch)
├── Pivot: (0.5, 1)
├── Position: (0, -60, 0)
├── Left: 20
├── Right: 20
└── Height: 30

Slider Component:
├── Fill Rect: [Assign Fill object]
├── Handle Rect: [Assign Handle object]
├── Direction: Left to Right
├── Min Value: 0
├── Max Value: 1
├── Whole Numbers: ✗ (unchecked)
└── Value: 1
```

**Customize Slider Appearance:**

**Background:**
```
Image Component:
└── Color: (0.15, 0.15, 0.2, 1) - Dark gray
```

**Fill:**
```
Image Component:
└── Color: (0.2, 0.7, 1, 1) - Cyan/Blue
```

**Handle:**
```
RectTransform:
├── Width: 20
└── Height: 30

Image Component:
└── Color: (1, 1, 1, 1) - White
```

#### 4. Volume Value Text
1. Right-click **VolumeSection** → **UI → Text - TextMeshPro**
2. Rename to **"VolumeValueText"**

```
RectTransform:
├── Anchors: Top-Right
├── Pivot: (1, 1)
├── Position: (-20, -10, 0)
├── Width: 100
└── Height: 40

TextMeshPro Settings:
├── Text: "100%"
├── Font Size: 28
├── Alignment: Right + Middle
└── Color: Cyan (0.2, 0.7, 1, 1)
```

---

### D. Brightness Section

Repeat the same process as Volume Section, but:

1. Create **"BrightnessSection"**
```
Position: (0, -360, 0)  ← Lower than Volume
```

2. Create **"BrightnessLabel"**
```
Text: "Brightness"
```

3. Create **"BrightnessSlider"**
```
Fill Color: (1, 0.9, 0.3, 1) - Yellow/Orange
```

4. Create **"BrightnessValueText"**
```
Text: "100%"
Color: Yellow (1, 0.9, 0.3, 1)
```

---

## Part 5: Add WristMenuController Script

### Step 1: Add Script to Canvas
1. Select **WristMenuCanvas**
2. Click **Add Component**
3. Search for **"WristMenuController"**
4. Click to add it

### Step 2: Assign References
In the WristMenuController component:

```
Menu References:
├── Menu Panel: [Drag MenuPanel here]
└── Wrist Transform: [Leave empty - auto-finds left hand]

Menu Positioning:
├── Menu Offset: (0, 0.05, 0.1)
├── Menu Rotation Offset: (45, 0, 0)
└── Menu Scale: 0.0005

Toggle Settings:
├── Toggle Button: Three (Y button on left controller)
└── Controller: LTouch (Left hand)

UI Elements:
├── Pause Resume Button: [Drag PauseButton here]
├── Pause Resume Text: [Drag PauseButtonText here]
├── Volume Slider: [Drag VolumeSlider here]
├── Brightness Slider: [Drag BrightnessSlider here]
├── Volume Value Text: [Drag VolumeValueText here]
└── Brightness Value Text: [Drag BrightnessValueText here]
```

---

## Part 6: Visual Polish (Optional)

### A. Add Separator Lines

1. Right-click **MenuPanel** → **UI → Image**
2. Rename to **"Separator1"**

```
RectTransform:
├── Anchors: Top-Stretch
├── Position Y: -210
├── Left: 50
├── Right: 50
└── Height: 2

Image:
└── Color: (0.3, 0.3, 0.35, 0.5) - Semi-transparent gray
```

Repeat for **"Separator2"** at Position Y: -340

---

### B. Add Background Glow (Optional)

1. Duplicate **MenuPanel**
2. Rename to **"MenuGlow"**
3. Move it behind MenuPanel (drag above in hierarchy)

```
RectTransform:
├── Width: 820 (slightly larger)
└── Height: 520 (slightly larger)

Image:
├── Color: (0.2, 0.5, 1, 0.3) - Cyan glow
└── Material: UI/Default (or custom glow shader)
```

---

### C. Add Icons (Optional)

Add icons next to labels:
- 🔊 Volume icon
- ☀️ Brightness icon

1. Right-click **VolumeLabel** → **UI → Image**
2. Position to the left of text
3. Assign icon sprite

---

## Part 7: Testing

### Test in Unity Editor:
1. Press **Play**
2. The menu should be hidden initially
3. Press **Y button** on left controller to toggle
4. Menu should appear on your left wrist

### Test Interactions:
- ✓ Click Pause button → Game pauses
- ✓ Drag Volume slider → Audio volume changes
- ✓ Drag Brightness slider → Scene brightness changes
- ✓ Press Y again → Menu hides

---

## Part 8: Troubleshooting

### Menu doesn't appear:
- Check WristMenuController is on Canvas
- Check MenuPanel is assigned
- Check left hand anchor exists in scene
- Check menu starts hidden (MenuPanel active = false in script)

### Menu appears but wrong position:
- Adjust Menu Offset values
- Try: (0, 0.05, 0.15) for further forward
- Try: (0, 0.1, 0.1) for higher up

### Menu appears but wrong size:
- Check Canvas scale is (0.001, 0.001, 0.001)
- Adjust Menu Scale in WristMenuController
- Try values between 0.0003 and 0.001

### Can't add Image component to button:
**Error:** "Can't add 'Image' because TextMeshProUGUI already exists"
- **Cause:** Unity only allows ONE Graphic component per GameObject
- **Solution:** 
  1. Delete the button
  2. Create new: Right-click → UI → **Button - TextMeshPro** (not plain Button)
  3. Unity will create it with correct structure (Image on parent, Text on child)
- **Alternative Fix:**
  1. Remove TextMeshProUGUI from button GameObject
  2. Add Image component to button
  3. Create child object for text
  4. Add TextMeshProUGUI to child

### Can't click buttons:
- Check Image component has "Raycast Target" ✓ checked
- Check Button component's "Target Graphic" is assigned to the Image
- Check child text has "Raycast Target" ✗ unchecked (so it doesn't block clicks)
- Check Event Camera is assigned on Canvas
- Check Graphic Raycaster is on Canvas
- Check OVR Interaction system is in scene
- Add OVRInputModule to EventSystem if missing

### Sliders don't work:
- Check Fill Rect is assigned in Slider component
- Check Handle Rect is assigned in Slider component
- Check slider references are assigned in WristMenuController
- Make sure slider Handle has "Raycast Target" checked

### Text not visible:
- Check TextMeshPro color is not transparent (Alpha = 1)
- Check font asset is assigned
- Check font size is appropriate (try 36)
- Check RectTransform width/height are not zero
- Check Canvas Scaler settings

### Button doesn't respond to hover/click:
- Check Button "Interactable" is ✓ checked
- Check Button "Transition" is set to "Color Tint"
- Check "Target Graphic" is assigned in Button component
- Verify different colors are set for Normal/Highlighted/Pressed states

---

## Part 9: Advanced Customization

### Custom Fonts:
1. Import your font (TTF/OTF)
2. Window → TextMeshPro → Font Asset Creator
3. Generate font atlas
4. Assign to TextMeshPro components

### Custom Colors Theme:
```csharp
// Cyberpunk Theme
Background: (0.05, 0.05, 0.1, 0.95)
Primary: (1, 0, 0.5, 1) - Magenta
Secondary: (0, 1, 1, 1) - Cyan
Text: (1, 1, 1, 1) - White

// Sci-Fi Theme
Background: (0.1, 0.15, 0.2, 0.9)
Primary: (0.2, 0.7, 1, 1) - Blue
Secondary: (0.3, 1, 0.5, 1) - Green
Text: (0.9, 0.95, 1, 1) - Light blue

// Military Theme
Background: (0.15, 0.2, 0.15, 0.95)
Primary: (0.4, 0.8, 0.3, 1) - Green
Secondary: (1, 0.7, 0.2, 1) - Orange
Text: (0.9, 1, 0.9, 1) - Light green
```

### Add More Settings:
Duplicate sections and add:
- Difficulty slider
- Graphics quality dropdown
- Haptic feedback toggle
- Subtitle toggle
- Controller sensitivity

---

## Part 10: Final Checklist

### Canvas Setup:
- [ ] Canvas is World Space
- [ ] Canvas scale is (0.001, 0.001, 0.001)
- [ ] Event Camera assigned to CenterEyeAnchor
- [ ] Canvas Scaler added and configured
- [ ] Graphic Raycaster present
- [ ] OVR Overlay Canvas removed (if it was there)

### UI Structure:
- [ ] MenuPanel created and configured
- [ ] TitleText created
- [ ] PauseButton created with correct structure (Image on parent, Text on child)
- [ ] VolumeSection with Label, Slider, and ValueText
- [ ] BrightnessSection with Label, Slider, and ValueText
- [ ] All RectTransforms positioned correctly

### Script Setup:
- [ ] WristMenuController script added to Canvas
- [ ] MenuPanel reference assigned
- [ ] PauseButton reference assigned
- [ ] PauseButtonText reference assigned
- [ ] VolumeSlider reference assigned
- [ ] BrightnessSlider reference assigned
- [ ] VolumeValueText reference assigned
- [ ] BrightnessValueText reference assigned

### Button Configuration:
- [ ] PauseButton has Image component (background)
- [ ] PauseButton has Button component
- [ ] PauseButtonText is a CHILD of PauseButton
- [ ] PauseButtonText has TextMeshProUGUI component
- [ ] Button "Target Graphic" is assigned to Image
- [ ] Image "Raycast Target" is checked
- [ ] Text "Raycast Target" is unchecked

### Slider Configuration:
- [ ] VolumeSlider Fill Rect assigned
- [ ] VolumeSlider Handle Rect assigned
- [ ] BrightnessSlider Fill Rect assigned
- [ ] BrightnessSlider Handle Rect assigned
- [ ] Sliders set to 0-1 range
- [ ] Sliders default value is 1

### Testing:
- [ ] MenuPanel starts hidden in scene
- [ ] Tested in VR headset
- [ ] Press Y button → Menu appears on wrist
- [ ] Press Y again → Menu disappears
- [ ] Click Pause button → Game pauses (Time.timeScale = 0)
- [ ] Click Resume button → Game resumes
- [ ] Drag Volume slider → Audio volume changes
- [ ] Drag Brightness slider → Scene brightness changes
- [ ] Menu positioned correctly on left wrist
- [ ] Menu faces player correctly
- [ ] All interactions work smoothly

---

## Quick Reference: Recommended Sizes

```
Canvas:
- Scale: (0.001, 0.001, 0.001)
- Width: 1000, Height: 600

MenuPanel:
- Width: 800, Height: 500

Title Text:
- Font Size: 48, Height: 80

Buttons:
- Width: 600, Height: 80
- Font Size: 36

Labels:
- Font Size: 28, Height: 40

Sliders:
- Height: 30
- Handle: 20x30

Spacing:
- Between sections: 100-120 units
- Padding from edges: 20 units
- Between label and slider: 50 units
```

---

## Example Color Palette

```
Dark Background: (0.1, 0.1, 0.15, 0.9)
Light Background: (0.2, 0.2, 0.25, 1)
Primary Accent: (0.3, 0.7, 1, 1) - Cyan
Secondary Accent: (0.5, 0.3, 1, 1) - Purple
Success: (0.3, 1, 0.5, 1) - Green
Warning: (1, 0.7, 0.2, 1) - Orange
Error: (1, 0.3, 0.3, 1) - Red
Text Primary: (1, 1, 1, 1) - White
Text Secondary: (0.7, 0.7, 0.75, 1) - Gray
```

---

## Resources

### Unity Documentation:
- Canvas: https://docs.unity3d.com/Manual/UICanvas.html
- TextMeshPro: https://docs.unity3d.com/Manual/com.unity.textmeshpro.html
- UI Slider: https://docs.unity3d.com/Manual/script-Slider.html

### Design Inspiration:
- Half-Life: Alyx wrist UI
- Beat Saber menu system
- Meta Quest system UI

---

Good luck creating your wrist menu! 🎮

