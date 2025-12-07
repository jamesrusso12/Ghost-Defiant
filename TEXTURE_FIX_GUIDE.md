# Fixing Missing Textures on FBX Models

## Problem
You've imported new Gun and Flashlight FBX models, but they appear gray/pink (missing textures).

## Quick Fix Solutions

### Solution 1: Extract Materials & Textures from FBX (Most Common)

1. **Select the FBX file** in your Unity Project window (the imported .fbx file)

2. **In the Inspector**, click on the **"Materials"** tab

3. You'll see material settings. Look for:
   - **Location**: Change to "Use External Materials (Legacy)"
   - OR click **"Extract Materials..."** button

4. Choose a folder to extract materials to (create a "Materials" folder next to your FBX if needed)

5. Unity will create .mat files for each material

6. Still in FBX Inspector, look for **"Extract Textures..."** button
   - Click it and choose where to save textures (create a "Textures" folder)
   - Unity will extract any embedded textures from the FBX

7. Click **Apply** at the bottom of the Inspector

### Solution 2: Import Texture Files Separately

If your textures came as separate files (PNG, JPG, TGA, etc.):

1. **Create a folder** next to your FBX:
   ```
   Assets/
     YourNewGun/
       gun_model.fbx
       Textures/        <- Create this folder
   ```

2. **Drag texture files** into the Textures folder

3. **Select your FBX** in Unity

4. **Extract materials** (see Solution 1, step 3-4)

5. **Open each material** in the Materials folder

6. In the material Inspector:
   - Find the **Albedo/Base Map** slot
   - Drag your texture file into it
   - Do the same for Normal, Metallic, etc. if you have those textures

### Solution 3: Fix Material Remapping

If you extracted materials but they're still gray:

1. **Find the extracted .mat files** (Materials folder)

2. **Double-click a material** to open it in Inspector

3. Check the **Shader** dropdown at the top:
   - For URP (your project): Use `Universal Render Pipeline/Lit`
   - For standard textures: Use `Standard` or `Universal Render Pipeline/Simple Lit`

4. **Assign textures manually**:
   - **Albedo** = Base color/diffuse texture (usually named like: model_diffuse.png, model_color.png, model_albedo.png)
   - **Normal Map** = Bump/normal texture (usually: model_normal.png)
   - **Metallic** = Metallic/smoothness (usually: model_metallic.png)
   - **Emission** = Glow map (if applicable)

### Solution 4: Auto-Fix Materials (Unity 2020+)

1. **Select the FBX file**

2. In Inspector → **Materials tab**

3. Click **"Material Creation Mode"** dropdown:
   - Try **"Import (legacy)"** 
   - OR **"Import via MaterialDescription"**

4. Click **"Remapped Materials"** section
   - Click **"Fix Now"** if you see any warnings

5. Click **Apply**

---

## Common Texture Naming Conventions

Unity looks for textures with these suffixes:
- `_Albedo` or `_Diffuse` or `_Color` → Base color
- `_Normal` → Normal map
- `_Metallic` → Metallic/smoothness
- `_AO` or `_Occlusion` → Ambient occlusion
- `_Emission` → Emission/glow
- `_Height` → Height map

**Example**: If your model is named "GunModel", textures might be:
- `GunModel_Albedo.png`
- `GunModel_Normal.png`
- `GunModel_Metallic.png`

---

## Checklist After Importing New Models

### For Gun Model:
- [ ] FBX imported into `Assets/Prefabs/` or `Assets/Models/`
- [ ] Materials extracted to Materials folder
- [ ] Textures extracted/imported to Textures folder
- [ ] All materials have correct shader (URP/Lit)
- [ ] Albedo texture assigned to each material
- [ ] Normal/Metallic maps assigned (if available)
- [ ] Model looks correct in Scene view
- [ ] Add to scene and assign to `RightHandAnchor`
- [ ] Add `GunScript` component
- [ ] Create child GameObject for `shootingPoint` (at gun barrel tip)
- [ ] Assign references in Inspector:
  - `GunScript.shootingPoint`
  - `GunScript.source` (AudioSource)
  - `WristMenuController.gunGameObject`

### For Flashlight Model:
- [ ] FBX imported
- [ ] Materials extracted
- [ ] Textures assigned
- [ ] Model looks correct
- [ ] Add to scene and assign to `LeftHandAnchor`
- [ ] Add child `Light` component (Spot light)
- [ ] Configure Light settings:
  - **Type**: Spot Light
  - **Range**: 10-15
  - **Spot Angle**: 30-60
  - **Intensity**: 1-2
  - **Color**: Warm white

---

## Troubleshooting

### Textures are still missing after extraction
- Check if texture files actually exist in your folder
- Textures might be embedded in FBX differently - try importing FBX from a different 3D software export
- Make sure texture files are supported formats (PNG, JPG, TGA, TIF, PSD)

### Model is completely black
- Check lighting in your scene
- Material shader might be wrong - switch to URP/Lit
- Check if textures are actually assigned (click material, look in Inspector)

### Model is bright pink/magenta
- Shader error - wrong shader for your render pipeline
- Change shader to `Universal Render Pipeline/Lit` (for URP)
- OR `Standard` (for Built-in pipeline)

### Textures are blurry or wrong size
- Select texture in Project window
- In Inspector, increase **Max Size** (try 2048 or 4096)
- Change **Compression** to "None" or "High Quality"
- Click Apply

### Can't find texture files
- Check the original folder where you got the FBX
- Textures might be in a subfolder
- Ask the model creator for texture files
- Try extracting textures from the FBX using a 3D program like Blender

---

## Where to Get Your Model's Original Folder Structure

If you downloaded the models, check:
1. Original download folder (might have textures separate from FBX)
2. Inside the FBX file (embedded textures - use "Extract Textures")
3. Ask whoever created/gave you the models for the texture files

---

## Unity Import Settings Tips

For best results when importing FBX:

1. **Model tab**:
   - Scale Factor: 1 (or adjust if too big/small)
   - Import BlendShapes: ✓ (if model has them)
   - Import Visibility: ✓
   - Import Cameras: ✗ (usually not needed)
   - Import Lights: ✗ (add your own Light components)

2. **Materials tab**:
   - Location: "Use External Materials (Legacy)"
   - Naming: "By Base Texture Name"
   - Search: "Recursive-Up"
   - Then click "Extract Materials..."

3. **Apply** changes

---

## Quick Reference: Where Are Your Current Models?

Based on your project structure:
- **Old Flashlight**: `Assets/Flashlight/`
  - Models: `Assets/Flashlight/Model/`
  - Textures: `Assets/Flashlight/Textures/`
  - Materials: `Assets/Flashlight/Materials/`

- **Gun Material**: `Assets/Materials/Gun.mat`

You can organize your new models the same way:
```
Assets/
  Models/
    NewGun/
      gun.fbx
      Materials/
        gun_material.mat
      Textures/
        gun_albedo.png
        gun_normal.png
    NewFlashlight/
      flashlight.fbx
      Materials/
      Textures/
```

---

## Need Help?

If textures still won't show:
1. Take a screenshot of the material Inspector
2. Check what texture files exist in your import folder
3. Verify the FBX came with textures (check original download)
4. Try exporting from the 3D software with "Embed Textures" option enabled

