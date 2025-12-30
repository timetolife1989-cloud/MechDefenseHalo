#!/bin/bash
# One-button Android deploy script for MechDefenseHalo

set -e  # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}MechDefenseHalo - Android Deploy${NC}"
echo -e "${GREEN}=====================================${NC}"

# Check if godot is available
if ! command -v godot &> /dev/null && ! command -v godot4 &> /dev/null
then
    echo -e "${RED}Error: Godot not found in PATH${NC}"
    echo "Please install Godot 4.x or add it to your PATH"
    exit 1
fi

# Determine godot command
GODOT_CMD="godot"
if command -v godot4 &> /dev/null; then
    GODOT_CMD="godot4"
fi

echo -e "${YELLOW}Using Godot command: $GODOT_CMD${NC}"

# Create build directory if it doesn't exist
mkdir -p build

# Export the project
echo -e "${YELLOW}Building APK...${NC}"
$GODOT_CMD --headless --export-release "Android" build/MechDefenseHalo.apk

if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi

echo -e "${GREEN}Build successful!${NC}"

# Check if adb is available
if ! command -v adb &> /dev/null
then
    echo -e "${YELLOW}Warning: adb not found in PATH${NC}"
    echo "Skipping device installation. APK is available at: build/MechDefenseHalo.apk"
    exit 0
fi

# Check if device is connected
echo -e "${YELLOW}Checking for connected devices...${NC}"
DEVICES=$(adb devices | grep -v "List" | grep "device" | wc -l)

if [ "$DEVICES" -eq 0 ]; then
    echo -e "${YELLOW}No Android devices connected${NC}"
    echo "APK is available at: build/MechDefenseHalo.apk"
    echo "To install manually, connect your device and run:"
    echo "  adb install -r build/MechDefenseHalo.apk"
    exit 0
fi

# Install on device
echo -e "${YELLOW}Installing on device...${NC}"
adb install -r build/MechDefenseHalo.apk

if [ $? -ne 0 ]; then
    echo -e "${RED}Installation failed!${NC}"
    exit 1
fi

echo -e "${GREEN}Installation successful!${NC}"

# Launch the app
echo -e "${YELLOW}Launching MechDefenseHalo...${NC}"
adb shell am start -n com.mechdefense.halo/.GodotApp

echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}Deploy Complete!${NC}"
echo -e "${GREEN}=====================================${NC}"
