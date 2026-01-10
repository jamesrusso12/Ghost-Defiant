# Detailed Guide: How to Get Bullet Debug Logs from Quest

## Overview
When you build your game to Quest and play it, Unity's Debug.Log messages are normally only visible in Unity Editor. Since you can only build-test (not play in editor), we need to get these logs from the Quest device itself.

The FileLogger script writes all bullet-related logs to a file on the Quest. This guide shows you exactly how to retrieve that file.

---

## Method 1: Using GET_LOGS.bat Script (EASIEST - Recommended)

### Step 1: Make Sure FileLogger is in Your Scene

**In Unity Editor:**

1. Open your scene (the one you build to Quest)
2. In the **Hierarchy** panel (left side), right-click anywhere
3. Select **Create Empty**
4. A new GameObject appears - rename it to **"FileLogger"**
5. With "FileLogger" selected, look at the **Inspector** panel (right side)
6. Click **Add Component** button at the bottom
7. Type "FileLogger" in the search box
8. Click on **FileLogger** script to add it
9. **Save your scene** (Ctrl+S)

**Visual Guide:**
```
Hierarchy Panel:
├── [Your Scene Objects]
└── FileLogger  ← New empty GameObject with FileLogger script attached
```

### Step 2: Build to Quest

1. **File → Build Settings**
2. Select **Android** platform
3. Click **Build and Run**
4. Wait for build to complete
5. Game launches on Quest automatically

### Step 3: Play and Test

1. **Put on Quest headset**
2. **Shoot at walls 5-10 times**
3. Try hitting different walls
4. Try hitting ghosts (if any)
5. **Exit the game** (press Oculus button, quit app)

### Step 4: Retrieve Log File Using Batch Script

**On Your PC:**

1. **Connect Quest to PC via USB cable**
   - Use the same cable you use for building
   - Quest should already be connected from building

2. **Put on Quest headset briefly**
   - You might see a popup: "Allow USB debugging?"
   - Click **"Allow"** or **"Always allow from this computer"**
   - Take off headset

3. **On your PC Desktop, find GET_LOGS.bat**
   - It should be at: `C:\Users\jruss\Desktop\GET_LOGS.bat`
   - If you don't see it, I can help you create it

4. **Double-click GET_LOGS.bat**
   - A black Command Prompt window will open
   - It will show messages like:
     ```
     Checking for Quest connection...
     List of devices attached
     1WMHH123456789    device
     
     Pulling log file from Quest...
     /sdcard/Android/data/com.Russo.VRprojectGame/files/bullet_debug.txt: 1 file pulled
     
     SUCCESS! Log file copied to Desktop: bullet_debug.txt
     ```

5. **Notepad will automatically open** with the log file
   - If not, go to Desktop and open **bullet_debug.txt**

6. **Copy all the text** from bullet_debug.txt
   - Select all (Ctrl+A)
   - Copy (Ctrl+C)
   - Paste it in a message to me

---

## Method 2: Manual ADB Command (If Script Doesn't Work)

### Step 1: Open Command Prompt

1. Press **Windows key + R**
2. Type: `cmd`
3. Press **Enter**
4. A black window opens (Command Prompt)

### Step 2: Navigate to ADB Location

**Copy and paste this EXACT command** (one line):

```cmd
cd "C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools"
```

Press **Enter**

**Expected result:** Command prompt shows:
```
C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools>
```

**If you get "The system cannot find the path specified":**
- Your Unity might be installed elsewhere
- Try: `cd "C:\Program Files (x86)\Unity\Hub\Editor\6000.2.12f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools"`
- Or check where Unity Hub installed Unity (usually `C:\Program Files\Unity\Hub\Editor\`)

### Step 3: Check Quest Connection

**Type this command:**

```cmd
adb devices
```

Press **Enter**

**Expected result:**
```
List of devices attached
1WMHH123456789    device
```

**If you see "unauthorized":**
- Put on Quest headset
- Look for USB debugging popup
- Click "Allow"

**If you see "no devices":**
- Check USB cable connection
- Make sure Quest is powered on
- Try unplugging and replugging USB cable

### Step 4: Pull the Log File

**Copy and paste this EXACT command:**

```cmd
adb pull /sdcard/Android/data/com.Russo.VRprojectGame/files/bullet_debug.txt %USERPROFILE%\Desktop\bullet_debug.txt
```

Press **Enter**

**Expected result:**
```
/sdcard/Android/data/com.Russo.VRprojectGame/files/bullet_debug.txt: 1 file pulled. 15.2 KB/s (12345 bytes in 0.812s)
```

**If you get "remote object doesn't exist":**
- FileLogger script is not in your scene, OR
- You haven't run the game yet, OR
- Game hasn't written logs yet
- **Solution:** Go back to Step 1 (add FileLogger to scene), rebuild, play, then try again

### Step 5: Open the Log File

**The file is now on your Desktop:**

1. Go to your **Desktop**
2. Look for **bullet_debug.txt**
3. **Double-click** to open in Notepad
4. **Copy all text** (Ctrl+A, Ctrl+C)
5. **Paste it here** in your message

---

## Method 3: Using Meta Quest Developer Hub (Alternative)

### Step 1: Download Meta Quest Developer Hub

1. Go to: https://developer.oculus.com/downloads/package/quest-developer-hub/
2. Download and install **Meta Quest Developer Hub**
3. Launch the application

### Step 2: Connect Quest

1. **Connect Quest to PC via USB**
2. **Put on Quest headset** - allow USB debugging if prompted
3. In Developer Hub, go to **Device Manager** tab
4. Your Quest should appear in the device list

### Step 3: View Logs in Real-Time

1. Click **Device Console** button (or "View Logs")
2. In the filter/search box, type: **Unity**
3. This filters to show only Unity logs
4. **Put on Quest headset and play**
5. **Shoot bullets** - logs appear in real-time!
6. **Copy the log text** from the console window
7. **Paste it here**

**Advantage:** You can see logs while playing!
**Disadvantage:** Need to download another tool

---

## Method 4: Using File Explorer (If Quest Shows Up)

### Step 1: Enable File Transfer Mode

1. **Connect Quest to PC via USB**
2. **Put on Quest headset**
3. Look for notification at top: **"USB for charging"**
4. **Click the notification**
5. Select **"Transfer files"** or **"File transfer"**

### Step 2: Open File Explorer

1. On PC, open **File Explorer** (Windows key + E)
2. Under **"This PC"**, you should now see:
   - **Quest 3** (or Quest 2)
   - Or **Meta Quest 3**

### Step 3: Navigate to Log File

**Click through these folders:**

```
This PC
└── Quest 3 (or Quest 2)
    └── Internal shared storage
        └── Android
            └── data
                └── com.Russo.VRprojectGame  ← Your app folder
                    └── files
                        └── bullet_debug.txt  ← YOUR LOG FILE!
```

### Step 4: Copy the File

1. **Right-click** on bullet_debug.txt
2. Select **Copy**
3. **Paste** to Desktop
4. **Open** with Notepad
5. **Copy all text** and send to me

---

## Troubleshooting

### Problem: "File not found" or "remote object doesn't exist"

**Causes:**
- FileLogger script not added to scene
- Game hasn't been run yet
- Log file hasn't been created

**Solution:**
1. Add FileLogger GameObject to scene (see Method 1, Step 1)
2. Save scene
3. Rebuild to Quest
4. Play game and shoot bullets
5. Exit game
6. Try pulling logs again

### Problem: "adb: command not found"

**Causes:**
- ADB not in PATH
- Wrong Unity installation path

**Solution:**
- Use Method 1 (GET_LOGS.bat script) - it finds ADB automatically
- Or use Method 3 (Meta Quest Developer Hub) - doesn't need ADB

### Problem: "no devices/unauthorized"

**Causes:**
- Quest not connected properly
- USB debugging not allowed

**Solution:**
1. Unplug and replug USB cable
2. Put on Quest headset
3. Look for "Allow USB debugging?" popup
4. Click "Allow" or "Always allow"
5. Try `adb devices` again

### Problem: Quest doesn't show in File Explorer

**Causes:**
- Quest is in charging mode, not file transfer mode

**Solution:**
1. Put on Quest headset
2. Click USB notification
3. Select "Transfer files"
4. Check File Explorer again

---

## What the Log File Should Look Like

When you open bullet_debug.txt, you should see something like:

```
===== BULLET DEBUG LOG STARTED =====
Time: 1/15/2026 3:45:23 PM
Log file: /storage/emulated/0/Android/data/com.Russo.VRprojectGame/files/bullet_debug.txt
=====================================
[12.34s] Log: [GunScript] ===== SHOOT TRIGGERED =====
[12.35s] Log: [GunScript] Created projectile at position (1.23, 2.45, 3.67)
[12.36s] Log: [ProjectileController] Initialized. Collider enabled immediately
[12.50s] Log: [ProjectileController] ===== COLLISION =====
[12.50s] Log: [ProjectileController]   Hit: DestructibleMeshSegment_123
[12.50s] Log: [ProjectileController]   Layer: Default
[12.50s] Log: [ProjectileController] Impact with: DestructibleMeshSegment_123
[12.50s] Log: [ProjectileController] Invoking OnShootAndHit event
[12.51s] Log: [DestructibleGlobalMeshManager] Destroying segment: DestructibleMeshSegment_123
```

**If you see this:** Perfect! The system is working, we just need to debug why walls aren't breaking.

**If you see nothing or only the header:** FileLogger might not be capturing logs, or bullets aren't spawning.

**If you see errors:** Copy the error messages and send them to me.

---

## Quick Checklist

Before trying to get logs, make sure:

- [ ] FileLogger GameObject exists in your scene
- [ ] FileLogger script component is attached to that GameObject
- [ ] Scene is saved
- [ ] Game has been built to Quest
- [ ] Game has been played (you shot bullets)
- [ ] Quest is connected to PC via USB
- [ ] USB debugging is allowed

---

## Still Stuck?

If none of these methods work, tell me:
1. Which method you tried
2. What error message you saw (exact text)
3. Whether Quest shows up in File Explorer
4. Whether `adb devices` shows your Quest

I'll help you troubleshoot!
