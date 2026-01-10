@echo off
echo ========================================
echo Quest Bullet Debug Log Retriever
echo ========================================
echo.

cd /d "C:\Program Files\Unity\Hub\Editor\6000.2.12f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools"

echo Checking for Quest connection...
adb devices
echo.

if errorlevel 1 (
    echo ERROR: Could not find ADB or Quest not connected!
    echo Make sure:
    echo   1. Quest is connected via USB
    echo   2. USB debugging is allowed in headset
    echo   3. Unity version 6000.2.12f1 is installed
    pause
    exit /b 1
)

echo Pulling log file from Quest...
adb pull /sdcard/Android/data/com.Russo.VRprojectGame/files/bullet_debug.txt "%USERPROFILE%\Desktop\bullet_debug.txt"

if exist "%USERPROFILE%\Desktop\bullet_debug.txt" (
    echo.
    echo SUCCESS! Log file copied to Desktop: bullet_debug.txt
    echo.
    echo Opening file...
    start notepad "%USERPROFILE%\Desktop\bullet_debug.txt"
) else (
    echo.
    echo WARNING: Log file not found on Quest!
    echo This means either:
    echo   1. FileLogger script is not in your scene
    echo   2. You haven't run the game yet
    echo   3. Log file is in a different location
    echo.
    echo Try running the game first, then run this script again.
)

echo.
pause
