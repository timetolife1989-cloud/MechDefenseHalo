@echo off
echo === MechDefenseHalo One-Button Deploy ===

set PROJECT_PATH=.
set BUILD_PATH=build
set APK_NAME=MechDefenseHalo.apk
set PACKAGE_NAME=com.mechdefense.halo

REM Check if godot is installed
where godot >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Error: Godot is not installed or not in PATH
    echo Please install Godot 4.3+ and ensure it's in your PATH
    exit /b 1
)

REM Check if adb is installed
where adb >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Error: ADB is not installed or not in PATH
    echo Please install Android SDK Platform Tools
    exit /b 1
)

REM Create build directory if it doesn't exist
if not exist "%BUILD_PATH%" mkdir "%BUILD_PATH%"

REM Step 1: Export APK
echo [1/4] Exporting APK...
godot --headless --export-release "Android" "%BUILD_PATH%\%APK_NAME%"

if %ERRORLEVEL% NEQ 0 (
    echo Export failed!
    echo Make sure you have Android export template installed
    echo and export preset configured in Godot project
    exit /b 1
)

echo Export complete

REM Check if APK was created
if not exist "%BUILD_PATH%\%APK_NAME%" (
    echo APK file not found: %BUILD_PATH%\%APK_NAME%
    exit /b 1
)

REM Step 2: Check device
echo [2/4] Checking device...
adb devices

REM Count devices (simple check)
adb devices | findstr /C:"device" >nul
if %ERRORLEVEL% NEQ 0 (
    echo No device connected!
    echo Please connect an Android device via USB and enable USB debugging
    exit /b 1
)

echo Device connected

REM Step 3: Install
echo [3/4] Installing APK...
adb install -r "%BUILD_PATH%\%APK_NAME%"

if %ERRORLEVEL% NEQ 0 (
    echo Install failed!
    echo Try manually uninstalling the app first or check device permissions
    exit /b 1
)

echo Install complete

REM Step 4: Launch
echo [4/4] Launching app...
adb shell am start -n %PACKAGE_NAME%/.GodotApp

if %ERRORLEVEL% NEQ 0 (
    echo Launch failed!
    echo Check if package name is correct: %PACKAGE_NAME%
    exit /b 1
)

echo App launched
echo === DEPLOY COMPLETE ===

REM Show logcat
echo Showing logcat (Ctrl+C to stop)...
echo --------------------------------------
adb logcat -s Godot:* GodotEngine:*
