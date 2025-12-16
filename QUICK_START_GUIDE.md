# Quick Start Guide - Post Troubleshooting

## 🎯 What Was Fixed

✅ **Wrist Menu** - Now uses only WristMenuController (Y button on left controller)
✅ **Game UI Clipping** - Moved closer to camera (0.25m), better wall avoidance, proper render order
✅ **Performance** - Disabled debug logging by default, added configurable options
✅ **Duplicate Scripts** - Consolidated to single implementation for each feature

---

## 🚀 Next Steps (Do This Now!)

### 1. Test Wrist Menu (5 minutes)
```
1. Open your scene in Unity
2. Find your Wrist Menu Canvas GameObject
3. Make sure ONLY WristMenuController is attached (remove others)
4. Assign the menuPanel in inspector
5. Build and test on Quest
6. Press Y button on LEFT controller
```

### 2. Test Game UI (5 minutes)
```
1. Find your Game UI Canvas
2. Check GameUILazyFollow component
3. Verify m_TargetOffset is (0, 0, 0.25)
4. In collisionMask, include your wall/scene mesh layers
5. Build and test - UI should stay visible and not clip
```

### 3. Check Performance (10 minutes)
```
1. Add PerformanceDiagnostics to any GameObject in your scene
2. Assign gunObject and flashlightObject in inspector
3. Enter Play mode
4. Press 'P' key
5. Check Console for poly count reports
```

**If any model has >10,000 vertices:**
- Option A: Reduce poly count in Blender (Decimate modifier)
- Option B: Use Unity's "Optimize Mesh" and "Mesh Compression: High"
- Option C: Replace with lower-poly models from Asset Store

### 4. Before Building to Quest
```
✓ WristMenuController.enableDebugLogging = FALSE
✓ GunScript.enableDebugLogging = FALSE
✓ Disable VRPerformanceMonitor (or remove from scene)
✓ Remove PerformanceDiagnostics from scene
✓ Check all models are optimized (step 3 above)
```

---

## 📊 Expected Results

### Performance Targets
- **FPS:** 72+ on Quest 2, 90+ on Quest 3
- **UI:** No clipping, always visible
- **Menu:** Appears instantly on Y press
- **Console:** Minimal/no debug spam

### If Still Having Issues

**Wrist Menu Not Showing?**
→ Enable `WristMenuController.enableDebugLogging = true`
→ Check console for error messages
→ Verify OVRCameraRig is in scene

**UI Still Clipping?**
→ Reduce m_TargetOffset to 0.2m or 0.15m
→ Check collisionMask includes wall layers
→ Increase wallOffset to 0.15m

**Low FPS?**
→ Run PerformanceDiagnostics (press P)
→ Check console for high poly warnings
→ Use Unity Profiler (Window > Analysis > Profiler)
→ Check textures are 1024x1024 or less

---

## 📁 Files You Can Safely Delete

These are now in `Unused/` folder and not needed:
```
Assets/Scripts/Unused/SimpleWristMenu.cs
Assets/Scripts/Unused/WristMenu.cs
Assets/Scripts/Unused/BillboardMenu.cs
Assets/Scripts/Unused/... (many others)
```

You can delete the entire `Unused/` folder if you want to clean up.

---

## 🎮 Button Reference

| Button | Action |
|--------|--------|
| Y (Left Controller) | Toggle Wrist Menu |
| LEFT Grip | Toggle Performance Monitor |
| LEFT Thumbstick Press | Reset Performance Stats |
| P (Editor Only) | Run Performance Diagnostics |

---

## 📖 Full Documentation

See `TROUBLESHOOTING_FIXES.md` for:
- Detailed explanations of all fixes
- Performance optimization guide
- Model optimization tutorials
- Common issues & solutions
- Build recommendations

---

## ⚠️ Important Notes

1. **Always test on device** - Editor performance ≠ Quest performance
2. **Keep debug logging OFF** - Massive performance impact
3. **Check poly counts** - #1 cause of VR performance issues
4. **UI distance matters** - Too far = clips through walls
5. **Use ONE script per feature** - Multiple scripts = conflicts

---

## ✨ You're All Set!

The fixes are complete. Follow the "Next Steps" above to verify everything works, then continue building your awesome VR game! 🚀

If you encounter any new issues, check `TROUBLESHOOTING_FIXES.md` for detailed solutions.
