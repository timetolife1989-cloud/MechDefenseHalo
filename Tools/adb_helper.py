#!/usr/bin/env python3
"""
ADB Helper - Advanced ADB operations for MechDefenseHalo deployment
Provides utility functions for device management and app deployment
"""

import subprocess
import sys
import time
from typing import List, Dict, Optional


class ADBHelper:
    """Helper class for ADB operations"""
    
    def __init__(self, package_name: str = "com.mechdefense.halo"):
        self.package_name = package_name
        self._check_adb()
    
    def _check_adb(self):
        """Check if ADB is available"""
        try:
            subprocess.run(["adb", "version"], capture_output=True, check=True)
        except (subprocess.CalledProcessError, FileNotFoundError):
            print("Error: ADB is not installed or not in PATH")
            print("Please install Android SDK Platform Tools")
            sys.exit(1)
    
    def run_adb_command(self, args: List[str], timeout: int = 30) -> tuple:
        """
        Run an ADB command and return output
        
        Args:
            args: List of command arguments
            timeout: Command timeout in seconds
            
        Returns:
            Tuple of (stdout, stderr, returncode)
        """
        try:
            result = subprocess.run(
                ["adb"] + args,
                capture_output=True,
                text=True,
                timeout=timeout
            )
            return result.stdout, result.stderr, result.returncode
        except subprocess.TimeoutExpired:
            return "", "Command timed out", -1
        except Exception as e:
            return "", str(e), -1
    
    def get_connected_devices(self) -> List[str]:
        """Get list of connected devices"""
        stdout, stderr, returncode = self.run_adb_command(["devices"])
        
        if returncode != 0:
            print(f"Error getting devices: {stderr}")
            return []
        
        devices = []
        for line in stdout.split('\n')[1:]:  # Skip header
            if '\tdevice' in line:
                device_id = line.split('\t')[0].strip()
                if device_id:
                    devices.append(device_id)
        
        return devices
    
    def get_device_info(self, device_id: Optional[str] = None) -> Dict[str, str]:
        """Get device information"""
        args = []
        if device_id:
            args = ["-s", device_id]
        
        properties = {
            "model": "ro.product.model",
            "manufacturer": "ro.product.manufacturer",
            "android_version": "ro.build.version.release",
            "sdk_version": "ro.build.version.sdk",
            "brand": "ro.product.brand"
        }
        
        info = {}
        for key, prop in properties.items():
            stdout, stderr, returncode = self.run_adb_command(
                args + ["shell", "getprop", prop]
            )
            if returncode == 0:
                info[key] = stdout.strip()
        
        return info
    
    def install_apk(self, apk_path: str, reinstall: bool = True) -> bool:
        """Install APK to device"""
        args = ["install"]
        if reinstall:
            args.append("-r")
        args.append(apk_path)
        
        print(f"Installing {apk_path}...")
        stdout, stderr, returncode = self.run_adb_command(args, timeout=60)
        
        if returncode == 0 and "Success" in stdout:
            print("✓ Install successful")
            return True
        else:
            print(f"✗ Install failed: {stderr or stdout}")
            return False
    
    def uninstall_app(self) -> bool:
        """Uninstall app from device"""
        print(f"Uninstalling {self.package_name}...")
        stdout, stderr, returncode = self.run_adb_command(
            ["uninstall", self.package_name]
        )
        
        if returncode == 0 and "Success" in stdout:
            print("✓ Uninstall successful")
            return True
        else:
            print(f"✗ Uninstall failed: {stderr or stdout}")
            return False
    
    def launch_app(self) -> bool:
        """Launch app on device"""
        print(f"Launching {self.package_name}...")
        stdout, stderr, returncode = self.run_adb_command([
            "shell", "am", "start", "-n", 
            f"{self.package_name}/.GodotApp"
        ])
        
        if returncode == 0:
            print("✓ App launched")
            return True
        else:
            print(f"✗ Launch failed: {stderr or stdout}")
            return False
    
    def stop_app(self) -> bool:
        """Stop app on device"""
        print(f"Stopping {self.package_name}...")
        stdout, stderr, returncode = self.run_adb_command([
            "shell", "am", "force-stop", self.package_name
        ])
        
        if returncode == 0:
            print("✓ App stopped")
            return True
        else:
            print(f"✗ Stop failed: {stderr}")
            return False
    
    def clear_app_data(self) -> bool:
        """Clear app data"""
        print(f"Clearing app data for {self.package_name}...")
        stdout, stderr, returncode = self.run_adb_command([
            "shell", "pm", "clear", self.package_name
        ])
        
        if returncode == 0 and "Success" in stdout:
            print("✓ App data cleared")
            return True
        else:
            print(f"✗ Clear data failed: {stderr or stdout}")
            return False
    
    def show_logcat(self, tags: List[str] = None):
        """Show logcat output"""
        if tags is None:
            tags = ["Godot:*", "GodotEngine:*"]
        
        args = ["logcat", "-s"] + tags
        
        print("Starting logcat (Ctrl+C to stop)...")
        print("--------------------------------------")
        
        try:
            process = subprocess.Popen(
                ["adb"] + args,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True
            )
            
            for line in process.stdout:
                print(line.rstrip())
        except KeyboardInterrupt:
            process.terminate()
            print("\nLogcat stopped")
    
    def pull_file(self, remote_path: str, local_path: str) -> bool:
        """Pull file from device"""
        print(f"Pulling {remote_path} to {local_path}...")
        stdout, stderr, returncode = self.run_adb_command([
            "pull", remote_path, local_path
        ])
        
        if returncode == 0:
            print("✓ File pulled successfully")
            return True
        else:
            print(f"✗ Pull failed: {stderr}")
            return False
    
    def push_file(self, local_path: str, remote_path: str) -> bool:
        """Push file to device"""
        print(f"Pushing {local_path} to {remote_path}...")
        stdout, stderr, returncode = self.run_adb_command([
            "push", local_path, remote_path
        ])
        
        if returncode == 0:
            print("✓ File pushed successfully")
            return True
        else:
            print(f"✗ Push failed: {stderr}")
            return False


def main():
    """Main entry point for CLI usage"""
    if len(sys.argv) < 2:
        print("ADB Helper - Advanced ADB operations")
        print("\nUsage:")
        print("  python adb_helper.py devices          - List connected devices")
        print("  python adb_helper.py info             - Get device info")
        print("  python adb_helper.py install <apk>    - Install APK")
        print("  python adb_helper.py uninstall        - Uninstall app")
        print("  python adb_helper.py launch           - Launch app")
        print("  python adb_helper.py stop             - Stop app")
        print("  python adb_helper.py clear            - Clear app data")
        print("  python adb_helper.py logcat           - Show logcat")
        sys.exit(1)
    
    helper = ADBHelper()
    command = sys.argv[1].lower()
    
    if command == "devices":
        devices = helper.get_connected_devices()
        print(f"Found {len(devices)} device(s):")
        for device in devices:
            print(f"  - {device}")
    
    elif command == "info":
        info = helper.get_device_info()
        print("Device Information:")
        for key, value in info.items():
            print(f"  {key}: {value}")
    
    elif command == "install":
        if len(sys.argv) < 3:
            print("Error: APK path required")
            sys.exit(1)
        success = helper.install_apk(sys.argv[2])
        sys.exit(0 if success else 1)
    
    elif command == "uninstall":
        success = helper.uninstall_app()
        sys.exit(0 if success else 1)
    
    elif command == "launch":
        success = helper.launch_app()
        sys.exit(0 if success else 1)
    
    elif command == "stop":
        success = helper.stop_app()
        sys.exit(0 if success else 1)
    
    elif command == "clear":
        success = helper.clear_app_data()
        sys.exit(0 if success else 1)
    
    elif command == "logcat":
        helper.show_logcat()
    
    else:
        print(f"Unknown command: {command}")
        sys.exit(1)


if __name__ == "__main__":
    main()
