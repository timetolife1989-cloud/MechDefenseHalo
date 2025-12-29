#!/bin/bash

echo "=== MechDefenseHalo One-Button Deploy ==="

# Configuration
PROJECT_PATH="."
BUILD_PATH="build"
APK_NAME="MechDefenseHalo.apk"
PACKAGE_NAME="com.mechdefense.halo"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if godot is installed
if ! command -v godot &> /dev/null; then
    echo -e "${RED}Error: Godot is not installed or not in PATH${NC}"
    echo "Please install Godot 4.3+ and ensure it's in your PATH"
    exit 1
fi

# Check if adb is installed
if ! command -v adb &> /dev/null; then
    echo -e "${RED}Error: ADB is not installed or not in PATH${NC}"
    echo "Please install Android SDK Platform Tools"
    exit 1
fi

# Create build directory if it doesn't exist
mkdir -p "${BUILD_PATH}"

# Step 1: Export APK
echo -e "${YELLOW}[1/4] Exporting APK...${NC}"
godot --headless --export-release "Android" "${BUILD_PATH}/${APK_NAME}"

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Export failed!${NC}"
    echo "Make sure you have Android export template installed"
    echo "and export preset configured in Godot project"
    exit 1
fi

echo -e "${GREEN}✓ Export complete${NC}"

# Check if APK was created
if [ ! -f "${BUILD_PATH}/${APK_NAME}" ]; then
    echo -e "${RED}✗ APK file not found: ${BUILD_PATH}/${APK_NAME}${NC}"
    exit 1
fi

# Step 2: Check device connected
echo -e "${YELLOW}[2/4] Checking device connection...${NC}"
adb devices

DEVICE_COUNT=$(adb devices | grep -w "device" | wc -l)

if [ $DEVICE_COUNT -eq 0 ]; then
    echo -e "${RED}✗ No device connected!${NC}"
    echo "Please connect an Android device via USB and enable USB debugging"
    exit 1
fi

echo -e "${GREEN}✓ Device connected${NC}"

# Step 3: Install APK
echo -e "${YELLOW}[3/4] Installing APK...${NC}"
adb install -r "${BUILD_PATH}/${APK_NAME}"

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Install failed!${NC}"
    echo "Try manually uninstalling the app first or check device permissions"
    exit 1
fi

echo -e "${GREEN}✓ Install complete${NC}"

# Step 4: Launch app
echo -e "${YELLOW}[4/4] Launching app...${NC}"
adb shell am start -n "${PACKAGE_NAME}/.GodotApp"

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Launch failed!${NC}"
    echo "Check if package name is correct: ${PACKAGE_NAME}"
    exit 1
fi

echo -e "${GREEN}✓ App launched${NC}"
echo -e "${GREEN}=== DEPLOY COMPLETE ===${NC}"

# Bonus: Show logcat
echo -e "${BLUE}Showing logcat (Ctrl+C to stop)...${NC}"
echo -e "${BLUE}--------------------------------------${NC}"
adb logcat -s Godot:* GodotEngine:*
