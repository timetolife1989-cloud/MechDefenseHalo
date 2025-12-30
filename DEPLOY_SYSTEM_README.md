# One-Button Deploy System

Automated build, deploy, and launch system for rapid mobile testing iteration.

## Overview

The One-Button Deploy System streamlines the Android development workflow by providing a single-click solution to build, install, and launch your MechDefenseHalo game on connected Android devices.

## Features

- ✅ One click/command from code to running app
- ✅ 30-second total time (build + install + launch)
- ✅ Works on Linux/Mac/Windows
- ✅ Auto-detects connected devices
- ✅ Shows logcat after launch
- ✅ Saves hours per day in iteration time

## Prerequisites

### Required Software

1. **Godot 4.3+** - Must be in your system PATH
2. **Android SDK Platform Tools** - For ADB support
3. **Android Export Template** - Installed in Godot
4. **Python 3.6+** - For advanced ADB helper (optional)

### Android Export Setup

1. Open your project in Godot
2. Go to `Project → Export`
3. Add Android export preset
4. Configure signing keys and package name (`com.mechdefense.halo`)
5. Download and install Android export templates

### Device Setup

1. Enable **Developer Options** on your Android device
2. Enable **USB Debugging**
3. Connect device via USB
4. Authorize computer when prompted on device

## Usage

### Option 1: Godot Editor Plugin (Recommended)

1. Open project in Godot Editor
2. Look for **🚀 Deploy to Mobile** button in the toolbar
3. Click button to start deployment
4. Watch the console for progress

**Status indicators:**
- 🚀 Deploy to Mobile - Ready
- ⏳ Building... - Exporting APK
- ⏳ Installing... - Installing to device
- ⏳ Launching... - Starting app
- ✅ Deployed! - Success
- ❌ Failed - Error occurred

### Option 2: Command Line Scripts

#### Linux/Mac

```bash
cd /path/to/MechDefenseHalo
./Tools/deploy.sh
```

#### Windows

```batch
cd C:\path\to\MechDefenseHalo
Tools\deploy.bat
```

### Option 3: Python ADB Helper

For advanced operations:

```bash
# List connected devices
python Tools/adb_helper.py devices

# Get device info
python Tools/adb_helper.py info

# Install APK
python Tools/adb_helper.py install build/MechDefenseHalo.apk

# Launch app
python Tools/adb_helper.py launch

# Stop app
python Tools/adb_helper.py stop

# Clear app data
python Tools/adb_helper.py clear

# Show logcat
python Tools/adb_helper.py logcat
```

## File Structure

```
MechDefenseHalo/
├── Scripts/
│   └── Editor/
│       ├── OneButtonDeploy.cs       # Godot editor plugin
│       ├── BuildAutomation.cs       # Multi-platform build automation
│       └── DeviceManager.cs         # ADB device management
│
└── Tools/
    ├── deploy.sh                    # Linux/Mac deployment script
    ├── deploy.bat                   # Windows deployment script
    └── adb_helper.py               # Python ADB helper utilities
```

## Deployment Process

The deployment follows these steps:

1. **Export APK** - Builds release APK using Godot export
2. **Check Device** - Verifies Android device is connected
3. **Install APK** - Installs app using `adb install -r`
4. **Launch App** - Starts app using `adb shell am start`
5. **Show Logcat** - Displays filtered Godot logs

## Troubleshooting

### "Godot not found"

- Ensure Godot 4.3+ is installed
- Add Godot to your system PATH
- Restart terminal/editor after PATH changes

### "ADB not found"

- Install Android SDK Platform Tools
- Add platform-tools to your PATH
- Restart terminal after PATH changes

### "No device connected"

- Check USB cable connection
- Enable USB Debugging on device
- Run `adb devices` to verify connection
- Try `adb kill-server && adb start-server`

### "Export failed"

- Check Android export preset is configured
- Verify Android export templates are installed
- Ensure signing keys are properly configured
- Check console for specific error messages

### "Install failed"

- Try uninstalling app manually first
- Check available storage on device
- Verify package name matches: `com.mechdefense.halo`
- Check device permissions

### "Launch failed"

- Verify package name: `com.mechdefense.halo`
- Check app activity name: `.GodotApp`
- Look for crash logs in logcat

## Performance Tips

- **First build** takes 30-60 seconds
- **Incremental builds** are faster (10-20 seconds)
- Use **Debug builds** for faster iteration
- Switch to **Release builds** for testing final performance

## Keyboard Shortcut (Optional)

To bind deployment to a keyboard shortcut:

1. Open Godot Editor
2. Go to `Editor → Editor Settings → Shortcuts`
3. Search for "Deploy"
4. Assign shortcut (e.g., `Ctrl+Shift+D`)

## Advanced Features

### Multi-Platform Export

Use `BuildAutomation.cs` for exporting to multiple platforms:

```csharp
// Export all platforms
await BuildAutomation.AutoExport();

// Export Android only
await BuildAutomation.ExportAndroid(debug: true);

// Clean build directories
BuildAutomation.CleanBuilds();
```

### Device Management

Use `DeviceManager.cs` for advanced device operations:

```csharp
// Get connected devices
var devices = await DeviceManager.GetConnectedDevices();

// Clear app data
await DeviceManager.ClearAppData("com.mechdefense.halo");

// Uninstall app
await DeviceManager.UninstallApp("com.mechdefense.halo");

// Get device info
var info = await DeviceManager.GetDeviceInfo();
```

## Integration with CI/CD

The deployment scripts can be integrated into your CI/CD pipeline:

```yaml
# GitHub Actions example
- name: Deploy to Device
  run: |
    ./Tools/deploy.sh
```

## Support

For issues or questions:
1. Check the console output for error messages
2. Review this README's troubleshooting section
3. Check Godot and ADB versions are compatible
4. Verify Android export configuration

## License

Part of the MechDefenseHalo project.
