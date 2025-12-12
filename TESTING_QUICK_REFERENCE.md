# VR Testing Quick Reference Card

**Print this or keep it on your second monitor while testing!**

---

## 🎮 Controller Layout

### LEFT Controller
```
    [Y] ← Wrist Menu
     |
    [X] (unused)
     |
[Grip] ← Performance Monitor
  👍
[Trigger] ← Interact/Select

[Thumbstick Press] ← Reset Perf Stats
```

### RIGHT Controller
```
    [B] ← Clear Debug Logs
     |
    [A] ← Debug Console
     |
[Grip] ← (unused)
  👍
[Trigger] ← Shoot/Primary

[↑↓ Thumbstick] ← Adjust UI Distance
[Thumbstick Press] ← Reset UI Distance
```

### Both Controllers
```
[LEFT Grip] + [RIGHT Grip] → Help Overlay
```

---

## ✅ Testing Checklist

### Fix #1: Wrist Menu
- [ ] Press Y on LEFT controller
- [ ] Menu appears on wrist?
- [ ] Can interact with buttons?
- [ ] Menu hides when pressing Y again?
- [ ] Check debug console (A button) for `[WristMenu]` logs

**If not working**: Check debug console for errors

---

### Fix #2: Performance
- [ ] Press LEFT grip to show FPS overlay
- [ ] Is FPS above 60? (Green)
- [ ] Is FPS stable? (not dropping frequently)
- [ ] Check frame time (should be <14ms for 72fps)

**FPS Colors**:
- 🟢 Green (72+): Perfect!
- 🟡 Yellow (60-72): Acceptable
- 🔴 Red (<60): Problem!

**If FPS is low**:
1. Models likely too high-poly
2. Use Model Optimization Tool in Unity
3. Consider simpler models

---

### Fix #3: UI Clipping
- [ ] Walk up to a wall with UI visible
- [ ] Does UI stay in front of wall? (not clip through)
- [ ] Can you always read the UI?
- [ ] If needed: Use RIGHT thumbstick UP/DOWN to adjust distance

**If still clipping**:
1. Move UI closer (RIGHT thumbstick DOWN)
2. Press RIGHT thumbstick to reset to default
3. Try 0.25m-0.35m distance range

---

## 🐛 Debug Console Reference

**Toggle Console**: A button (RIGHT controller)
**Clear Logs**: B button (RIGHT controller)

### Important Log Prefixes
- `[WristMenu]` - Wrist menu status
- `[VRPerformanceMonitor]` - FPS info
- `[MRUIConfigurator]` - UI setup
- `[ModelOptimizer]` - Model optimization

### What to Look For
- ❌ Red text = Errors (bad!)
- 🟡 Yellow text = Warnings (might be ok)
- ⚪ White text = Info (normal)

---

## 🔧 Common Issues & Quick Fixes

### Issue: Can't see wrist menu
**Solution**: 
1. Check if Y button (LEFT) is working
2. Open debug console (A on RIGHT)
3. Look for "[WristMenu]" messages
4. Should see "Toggle button pressed" when you press Y

### Issue: Low FPS
**Solution**:
1. Show FPS overlay (LEFT grip)
2. If red/yellow, models are probably too heavy
3. Exit game, use Model Optimization Tool
4. Rebuild and test again

### Issue: UI disappears near walls
**Solution**:
1. Move UI closer to face (RIGHT thumbstick DOWN)
2. Keep adjusting until UI stays visible
3. Press RIGHT thumbstick to reset if needed

### Issue: Controls not responding
**Solution**:
1. Check battery on controllers
2. Re-pair controllers in Quest settings
3. Check debug console for any errors
4. Restart the app

---

## 📊 Performance Targets

| Metric | Target | Acceptable | Poor |
|--------|--------|------------|------|
| FPS | 72+ | 60-72 | <60 |
| Frame Time | <13.9ms | <16.7ms | >16.7ms |
| Vertices (weapons) | <10k | <20k | >20k |
| Triangles (weapons) | <5k | <10k | >10k |

---

## 🚀 Build Testing Workflow

1. **Make changes in Unity**
2. **Save scenes** (Ctrl+S)
3. **Quick Build**: `Tools → Quick Build → Development Build` (Ctrl+Shift+B)
4. **Wait for build** (will auto-install)
5. **Put on headset**
6. **Test the specific fix**
7. **Check debug console** (A button) for any errors
8. **Check FPS** (LEFT grip) if performance-related
9. **Take notes** on what worked/didn't work
10. **Repeat!**

---

## 💡 Pro Tips

✓ **Always use Development Builds** when testing
  - Better error messages
  - Debug console available
  - Script debugging enabled

✓ **Check debug console FIRST** when something doesn't work
  - Most issues will show up there
  - Look for red (errors) and yellow (warnings)

✓ **Test ONE thing at a time**
  - Change one thing
  - Build and test
  - Verify it works
  - Move to next change

✓ **Keep the Quick Build tool handy**
  - Ctrl+Shift+B is your friend
  - Much faster than manual building

✓ **Use the in-game tools**
  - Debug Console (A button)
  - Performance Monitor (LEFT grip)
  - Help Overlay (both grips)

✓ **Document your builds**
  - Note what you changed
  - Note the results
  - Makes debugging easier

---

## 📱 Quest Settings (If Needed)

### Enable Developer Mode
1. Open Oculus/Meta app on phone
2. Go to Settings → Your Quest
3. Enable Developer Mode

### Check Connection
1. Connect Quest via USB-C
2. Put on headset
3. Allow computer access popup
4. Check "Always allow"

### Performance Overlay (Quest Native)
In-headset: Long-press Oculus button → Performance Overlay
Shows native Quest performance metrics

---

## 🆘 Emergency Reset

If everything breaks:
1. Remove headset
2. Close the app completely (hold Home, close app)
3. Restart Quest
4. Rebuild fresh from Unity
5. Test again

---

**Remember**: Passthrough = BUILD REQUIRED. No Play mode testing! 🚀

**Quick Build Shortcut**: Ctrl+Shift+B

**Debug Console**: A button (RIGHT controller)

**Performance Monitor**: LEFT grip

**You got this!** 💪
