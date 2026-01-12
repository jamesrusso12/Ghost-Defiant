# How to Get game_debug.txt from Quest

## Method 1: Using ADB (Recommended - Most Reliable)

### Step 1: Install ADB
If you don't have ADB installed:
1. Download **Android Platform Tools** from: https://developer.android.com/tools/releases/platform-tools
2. Extract the ZIP file
3. Add the folder to your system PATH (optional but recommended)

Or use **SideQuest** - it includes ADB automatically.

### Step 2: Enable Developer Mode on Quest
1. Put on your Quest headset
2. Go to **Settings** → **System** → **Developer**
3. Enable **Developer Mode** (you may need to enable it in the mobile app first)

### Step 3: Connect Quest to PC
1. Connect Quest to PC via USB cable
2. Put on Quest headset
3. When prompted in Quest, select **"Allow USB Debugging"** and check **"Always allow from this computer"**
4. Click **Allow**

### Step 4: Pull the Log File
Open Command Prompt or PowerShell and run:

```bash
adb pull /storage/emulated/0/Android/data/com.Russo.VRprojectGame/files/game_debug.txt
```

The file will be copied to your current directory.

### Step 5: Verify Connection (if Step 4 fails)
If you get "device not found" error, verify connection:

```bash
adb devices
```

You should see your Quest listed. If not:
- Make sure USB debugging is enabled
- Try a different USB cable (some cables are charge-only)
- Try unplugging and replugging the cable
- In Quest, go to Settings → Developer and toggle USB connection mode

---

## Method 2: Using SideQuest File Manager

1. Install **SideQuest** from: https://sidequestvr.com/
2. Connect Quest via USB
3. Open SideQuest
4. Click **"File Manager"** in the left sidebar
5. Navigate to: `/Android/data/com.Russo.VRprojectGame/files/`
6. Right-click `game_debug.txt` → **Download** or **Export**

---

## Method 3: Using Quest's Built-in File Manager (Wireless)

1. In Quest, open **Files** app
2. Navigate to: `Android/data/com.Russo.VRprojectGame/files/`
3. You can view the file directly or use a file sharing method

---

## Troubleshooting "Device Unreachable" Error

**Error 0x80070141** usually means:
- Quest is not in file transfer mode
- USB debugging not properly enabled
- Driver issues on Windows
- Quest went to sleep/disconnected

**Solutions:**
1. **Restart Quest**: Power off and on
2. **Try Different USB Port**: Use USB 3.0 port if available
3. **Install Quest Drivers**: If using SideQuest, it should install drivers automatically
4. **Use ADB Instead**: File Explorer is unreliable; ADB is the standard method for developers

---

## Quick ADB Commands Reference

```bash
# Check if device is connected
adb devices

# Pull the log file
adb pull /storage/emulated/0/Android/data/com.Russo.VRprojectGame/files/game_debug.txt

# Pull to specific location
adb pull /storage/emulated/0/Android/data/com.Russo.VRprojectGame/files/game_debug.txt C:\Users\YourName\Desktop\

# View log file directly in terminal (useful for quick checks)
adb shell cat /storage/emulated/0/Android/data/com.Russo.VRprojectGame/files/game_debug.txt

# Clear the log file (start fresh)
adb shell rm /storage/emulated/0/Android/data/com.Russo.VRprojectGame/files/game_debug.txt
```

---

## After Getting the Log File

1. Open `game_debug.txt` in a text editor (Notepad++ recommended for large files)
2. Search for `[PERF]` to find performance diagnostics
3. Look for operations taking >5ms (frame budget is ~11ms for 90Hz VR)
4. Share the relevant sections if you need help interpreting the data

---

**Note**: The log file is overwritten each time you launch the game. If you want to keep old logs, rename or move the file after pulling it.
