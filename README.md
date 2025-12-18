# Ghost Defiant - VR/MR Survival Shooter

**Mixed Reality Wave-Based Survival Game for Meta Quest**

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-Meta%20Quest-blue)
![Status](https://img.shields.io/badge/Status-Active%20Development-green)

---

## 🎮 About

Ghost Defiant is a mixed reality wave-based survival shooter where ghost enemies spawn in your real room using Meta Quest's passthrough technology. Defend yourself with a physics-based projectile gun across 5 progressively challenging rounds.

### Key Features
- 🌐 **Mixed Reality Passthrough** - Ghosts appear in your actual room
- ⌚ **Diegetic Wrist UI** - Check game stats on your wrist like a real watch
- 🔫 **Physics-Based Combat** - Realistic projectile shooting system
- 🧱 **Destructible Walls** - Shoot through room mesh to reveal passthrough
- 👻 **Smart Ghost AI** - NavMesh pathfinding with hiding behavior
- 📈 **Progressive Difficulty** - 5 rounds with increasing challenge
- ⚡ **Performance Optimized** - 72+ FPS on Quest 2, 90+ on Quest 3

---

## 🚀 Quick Start

### Prerequisites
- Unity 2022.3 LTS or newer
- Meta XR SDK (installed via Package Manager)
- Android Build Support
- Meta Quest 2/3/Pro with Developer Mode enabled

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/VR-MR-Project.git
   cd VR-MR-Project
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Add project from disk
   - Wait for packages to import (5-10 minutes)

3. **Build to Quest**
   - Press `Ctrl+Shift+B` for Quick Build
   - Or use `File > Build Settings > Build and Run`
   - Wait 2-3 minutes for build

4. **Play!**
   - Put on your Quest headset
   - Press Y (left controller) to open wrist menu
   - Use right trigger to shoot ghosts
   - Survive 5 rounds!

---

## 📖 Documentation

### Setup Guide
- **[WRIST_MENU_SETUP.md](WRIST_MENU_SETUP.md)** - Complete step-by-step wrist menu setup guide

---

## 🎯 Core Systems

### Game Manager
Handles round progression, scoring, and game state. Uses event-driven architecture for clean separation of concerns.

### Wrist UI
Diegetic UI system displaying game stats (Round, Score, Timer) on the player's wrist. Toggle with Y button on left controller.

### Gun & Projectiles
Physics-based projectile system with dual detection (collision + raycast backup) for reliable hit detection.

### Ghost AI
NavMesh-based pathfinding with smart behaviors: wandering, hiding, and player detection.

### Destructible Walls
Shoot through room mesh segments to reveal passthrough behind walls.

---

## 🎮 Controls

### LEFT Controller
- **Y Button** - Toggle Wrist Menu
- **Grip** - Toggle Performance Monitor
- **Thumbstick Press** - Reset Performance Stats

### RIGHT Controller
- **Trigger** - Shoot Gun
- **A Button** - Debug Console (Dev builds)
- **Thumbstick ↑↓** - Adjust UI Distance

---

## 🏗️ Project Structure

```
VR-MR-Project/
├── Assets/
│   ├── Scenes/              # Unity scenes
│   ├── Scripts/             # C# scripts (organized by category)
│   │   ├── Core/            # GameManager, Ghost, GhostSpawner
│   │   ├── UI/              # WristMenuController, UI systems
│   │   ├── Combat/          # Gun, projectiles
│   │   ├── MR_XR/           # Mixed reality systems
│   │   └── Systems/         # General systems
│   ├── Prefabs/             # Reusable game objects
│   ├── Materials/           # Material assets
│   └── Settings/            # URP and quality settings
├── Builds/                  # Build output (APK files)
└── MASTER_DOCUMENTATION.md  # Complete documentation
```

---

## 🔧 Development

### Building
```bash
# Quick Build (recommended)
Ctrl+Shift+B

# Or use Unity menu
File > Build Settings > Build and Run
```

### Performance Monitoring
```bash
# In-game FPS overlay
Press LEFT grip button

# Model analysis (Editor)
Tools > VR Optimization > Check Model Performance
```

### Debug Mode
Enable debug logging for diagnostics:
```csharp
WristMenuController.enableDebugLogging = true;
GunScript.enableDebugLogging = true;
```

**⚠️ Important:** Disable debug logging in production builds!

---

## 📊 Performance Targets

| Device | Target FPS | Minimum FPS |
|--------|-----------|-------------|
| Quest 2 | 90 FPS | 72 FPS |
| Quest 3 | 120 FPS | 90 FPS |

### Optimization Guidelines
- Models: < 5,000 triangles
- Textures: 1024x1024 or smaller
- Shaders: URP/Lit (not Standard)
- Debug Logging: Disabled in builds

---

## 🐛 Troubleshooting

### Wrist Menu Not Showing
1. Verify WristMenuController attached to Canvas
2. Check menuPanel is assigned
3. Enable debug logging and check console

### Can't Click Menu Buttons
1. Check EventSystem has PointableCanvasModule (not OVRInputModule)
2. Verify UIRayInteractor is assigned in WristMenuController
3. Check Graphic Raycaster is on WristMenuCanvas
4. See [WRIST_MENU_SETUP.md](WRIST_MENU_SETUP.md#-troubleshooting) for full guide

### Game Stats Not Updating
1. Check WristUI reference in GameManager
2. Verify event subscriptions
3. Enable debug logging

### Low FPS
1. Run Performance Diagnostics (press P in editor)
2. Disable all debug logging
3. Optimize high-poly models
4. Reduce texture sizes

For more help, see [Troubleshooting Guide](WRIST_MENU_SETUP.md#-troubleshooting)

---

## 📝 Recent Updates

### December 15, 2025 - UI Migration
- ✅ Migrated from screen-space HUD to diegetic wrist UI
- ✅ Implemented event-driven UI architecture
- ✅ Consolidated documentation into master file
- ✅ Improved code organization and maintainability

### December 12, 2025 - Performance & Fixes
- ✅ Fixed wrist menu Y button conflict
- ✅ Resolved UI clipping through walls
- ✅ Added performance monitoring tools
- ✅ Optimized debug logging

### December 18, 2025 - Ray Interactor Fix
- ✅ Fixed menu button clicking using Meta Building Blocks Ray Interactor
- ✅ Consolidated all documentation into single setup guide

---

## 🤝 Contributing

This is a senior project, but feedback and suggestions are welcome!

1. Check existing issues
2. Create detailed bug reports
3. Suggest features via issues
4. Follow Unity coding conventions

---

## 📄 License

This project is for educational purposes as part of a senior project.

---

## 🙏 Credits

### Built With
- Unity 2022.3 LTS
- Meta XR SDK
- MRUtilityKit
- Universal Render Pipeline

### Resources
- [Meta Quest Developer Hub](https://developer.oculus.com/)
- [Unity XR Documentation](https://docs.unity3d.com/Manual/XR.html)
- [MRUK Documentation](https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview/)

---

## 📞 Support

For questions or issues:
1. Check [WRIST_MENU_SETUP.md](WRIST_MENU_SETUP.md)
2. Review [Troubleshooting](WRIST_MENU_SETUP.md#-troubleshooting)
3. Enable debug logging for diagnostics
4. Open an issue with details

---

**Made with ❤️ for Meta Quest**

*Last Updated: December 18, 2025*

