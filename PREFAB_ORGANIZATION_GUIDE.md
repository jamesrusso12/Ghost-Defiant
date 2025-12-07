# Prefab Folder Organization Guide

## Current Issues
- ❌ Raw model files (FBX, textures) mixed with prefabs
- ❌ Source files (blend, obj, stl) in Prefabs folder
- ❌ No clear categorization
- ❌ Meshy imports cluttering the folder
- ❌ Loose prefabs at root level

## Recommended Structure

```
Assets/
├── Prefabs/
│   ├── Weapons/
│   │   ├── Gun.prefab
│   │   ├── Revolver/ (folder with revolver variants)
│   │   └── Projectiles/
│   │       ├── Specter Ammo.prefab
│   │       ├── Laser.prefab
│   │       └── Laser Impact.prefab
│   │
│   ├── Characters/
│   │   ├── Ghost/
│   │   │   └── Ghost.prefab
│   │   └── Space Trooper/ (if you're using this)
│   │
│   ├── UI/
│   │   ├── StartScreen.prefab
│   │   ├── UI.prefab
│   │   ├── Fade Screen.prefab
│   │   ├── Transition Manager.prefab
│   │   └── UIPrefabs/ (existing folder)
│   │
│   ├── Environment/
│   │   ├── Walkable Box.prefab
│   │   ├── Walkable Quad.prefab
│   │   ├── Obstacle Box.prefab
│   │   └── Debris/
│   │       ├── StoneTypeA.prefab
│   │       ├── StoneTypeB.prefab
│   │       └── StoneTypeC.prefab
│   │
│   ├── Effects/
│   │   ├── Orbs.prefab
│   │   └── OVRCursor.prefab
│   │
│   └── Player/
│       └── PlayerHitbox.prefab
│
├── Models/ (MOVE RAW MODELS HERE)
│   ├── Weapons/
│   │   ├── Specter_Blaster_3000/ (FBX + textures)
│   │   ├── Specter_Vision/ (flashlight FBX + textures)
│   │   └── Revolver/ (FBX files only)
│   │
│   ├── Characters/
│   │   ├── Ghost/ (Ghost.fbx + textures)
│   │   └── Space_Trooper/ (FBX + textures)
│   │
│   └── Unused/
│       └── Mother_Spaceship/ (source files)
│
└── Materials/
    └── (keep as is)
```

---

## Step-by-Step Organization

### Phase 1: Create Folder Structure

1. **In Unity Project window**, navigate to `Assets/Prefabs/`

2. **Create these folders** (right-click → Create → Folder):
   - `Weapons`
   - `Characters`
   - `UI`
   - `Environment`
   - `Effects`
   - `Player`

3. **Inside Weapons folder**, create:
   - `Projectiles` subfolder

4. **Inside Environment folder**, create:
   - `Debris` subfolder

5. **In Assets/** (root level), check if `Models` folder exists:
   - If not, create it
   - Inside Models, create: `Weapons`, `Characters`, `Unused`

---

### Phase 2: Move Prefabs (Only .prefab files!)

#### Weapons:
- [ ] `Assets/Prefabs/Ghost/Gun.prefab` → `Assets/Prefabs/Weapons/Gun.prefab`
- [ ] `Assets/Prefabs/Revolver/` (entire folder) → `Assets/Prefabs/Weapons/Revolver/`

#### Projectiles:
- [ ] `Assets/Prefabs/Specter Ammo.prefab` → `Assets/Prefabs/Weapons/Projectiles/Specter Ammo.prefab`
- [ ] `Assets/Prefabs/Revolver/Laser.prefab` → `Assets/Prefabs/Weapons/Projectiles/Laser.prefab`
- [ ] `Assets/Prefabs/Revolver/Laser Impact.prefab` → `Assets/Prefabs/Weapons/Projectiles/Laser Impact.prefab`

#### Characters:
- [ ] `Assets/Prefabs/Ghost/Ghost.prefab` → `Assets/Prefabs/Characters/Ghost/Ghost.prefab`
- [ ] Keep `Assets/Prefabs/Characters/Ghost/` folder for ghost-related materials

#### UI:
- [ ] `Assets/Prefabs/StartScreen.prefab` → `Assets/Prefabs/UI/StartScreen.prefab`
- [ ] `Assets/Prefabs/UI.prefab` → `Assets/Prefabs/UI/UI.prefab`
- [ ] `Assets/Prefabs/Fade Screen.prefab` → `Assets/Prefabs/UI/Fade Screen.prefab`
- [ ] `Assets/Prefabs/Transition Manager.prefab` → `Assets/Prefabs/UI/Transition Manager.prefab`
- [ ] `Assets/Prefabs/UIPrefabs/` → Already in good location

#### Environment:
- [ ] `Assets/Prefabs/Walkable Box.prefab` → `Assets/Prefabs/Environment/Walkable Box.prefab`
- [ ] `Assets/Prefabs/Walkable Quad.prefab` → `Assets/Prefabs/Environment/Walkable Quad.prefab`
- [ ] `Assets/Prefabs/Obstacle Box.prefab` → `Assets/Prefabs/Environment/Obstacle Box.prefab`
- [ ] `Assets/Prefabs/StoneTypeA.prefab` → `Assets/Prefabs/Environment/Debris/StoneTypeA.prefab`
- [ ] `Assets/Prefabs/StoneTypeB.prefab` → `Assets/Prefabs/Environment/Debris/StoneTypeB.prefab`
- [ ] `Assets/Prefabs/StoneTypeC.prefab` → `Assets/Prefabs/Environment/Debris/StoneTypeC.prefab`

#### Effects:
- [ ] `Assets/Prefabs/Orbs.prefab` → `Assets/Prefabs/Effects/Orbs.prefab`
- [ ] `Assets/Prefabs/OVRCursor.prefab` → `Assets/Prefabs/Effects/OVRCursor.prefab`

#### Player:
- [ ] `Assets/Prefabs/PlayerHitbox.prefab` → `Assets/Prefabs/Player/PlayerHitbox.prefab`

---

### Phase 3: Move Model Files (FBX + Textures)

**Important**: These should NOT be in Prefabs folder!

#### New Gun Models:
- [ ] `Assets/Prefabs/Specter_Blaster_3000_1206193946_texture_fbx/` → `Assets/Models/Weapons/Specter_Blaster_3000/`
- [ ] `Assets/Prefabs/Specter_Vision_1206200835_texture_fbx/` → `Assets/Models/Weapons/Specter_Vision/`

#### Ghost Model:
- [ ] `Assets/Prefabs/Ghost/Ghost.fbx` → `Assets/Models/Characters/Ghost/Ghost.fbx`
- [ ] `Assets/Prefabs/Ghost/GhostEyes.png` → `Assets/Models/Characters/Ghost/GhostEyes.png`

#### Revolver Models:
- [ ] `Assets/Prefabs/Revolver/CagedRevolver/` → `Assets/Models/Weapons/Revolver/CagedRevolver/`
- [ ] `Assets/Prefabs/Revolver/NakedRevolver/` → `Assets/Models/Weapons/Revolver/NakedRevolver/`
- [ ] `Assets/Prefabs/Revolver/sci-fi_pistol_low_uv_Metallic.png` → `Assets/Models/Weapons/Revolver/`

#### Space Trooper:
- [ ] `Assets/Prefabs/Space Trooper/` → `Assets/Models/Characters/Space_Trooper/`

---

### Phase 4: Clean Up Unused/Source Files

#### Meshy Imports:
**Option A - Keep for reference:**
- [ ] `Assets/Prefabs/MeshyImports/` → `Assets/Models/Weapons/MeshyImports/`

**Option B - Delete (if not using):**
- [ ] Delete `Assets/Prefabs/MeshyImports/` entirely

#### Mother Spaceship (appears unused):
- [ ] `Assets/Prefabs/Mother SpaceShip zip/` → `Assets/Models/Unused/Mother_Spaceship/`
- Or delete if you're not using it

#### Materials:
- [ ] Keep `Assets/Prefabs/Materials/` where it is
- Or move to `Assets/Materials/` if you want to consolidate all materials

---

### Phase 5: Delete Empty Folders

After moving everything:
1. Delete empty `Assets/Prefabs/Ghost/` folder (if empty)
2. Delete empty `Assets/Prefabs/Revolver/` folder (if empty)
3. Delete any other empty folders

---

## How to Move Files in Unity

### Method 1: Drag and Drop
1. Select file/folder in Project window
2. Drag to destination folder
3. Unity will update all references automatically

### Method 2: Cut and Paste
1. Right-click file/folder
2. Select "Show in Explorer"
3. Cut (Ctrl+X) and paste (Ctrl+V) in new location
4. Return to Unity, it will detect and reimport

### ⚠️ Important:
- **ALWAYS move files in Unity**, not in File Explorer while Unity is open
- Unity will automatically update all prefab references
- If you move in File Explorer, close Unity first!

---

## After Organization Checklist

- [ ] All .prefab files are in `Assets/Prefabs/` organized by category
- [ ] All .fbx files and textures are in `Assets/Models/`
- [ ] No loose prefabs at root of `Assets/Prefabs/`
- [ ] No empty folders
- [ ] Test that all prefabs still work (drag into scene)
- [ ] Build project to ensure no broken references

---

## Benefits of This Organization

✅ **Easy to find**: "Where's the gun prefab?" → `Prefabs/Weapons/Gun.prefab`
✅ **Clean separation**: Models vs Prefabs vs Materials
✅ **Better version control**: Organized folders = easier to track changes
✅ **Faster navigation**: Less scrolling through clutter
✅ **Professional structure**: Industry-standard organization
✅ **Scalable**: Easy to add new weapons, characters, etc.

---

## Quick Reference: Where Everything Goes

| Item Type | Location |
|-----------|----------|
| Prefab files (.prefab) | `Assets/Prefabs/[Category]/` |
| Model files (.fbx) | `Assets/Models/[Category]/` |
| Textures (for models) | Same folder as .fbx |
| Materials (.mat) | `Assets/Materials/` |
| Scripts (.cs) | `Assets/Scripts/` |
| Audio files | `Assets/Audio/` |
| Scenes | `Assets/Scenes/` |

---

## Time Estimate

- **Phase 1** (Create folders): 2 minutes
- **Phase 2** (Move prefabs): 5-10 minutes
- **Phase 3** (Move models): 5-10 minutes
- **Phase 4** (Clean up): 2-5 minutes
- **Phase 5** (Test): 5 minutes

**Total: ~20-30 minutes**

---

## Need Help?

If you encounter any issues:
1. **Broken prefab references**: Unity usually fixes these automatically
2. **Missing materials**: Check that materials moved with their models
3. **Can't find a file**: Use Unity search bar (top right)
4. **Accidentally deleted something**: Edit → Undo (Ctrl+Z)

---

Take your time and move files one category at a time. Unity will handle updating all the references automatically! 🗂️

