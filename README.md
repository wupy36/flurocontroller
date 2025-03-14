# FluroController Project

## Overview

FluroController is an innovative project for integrating custom Bluetooth controllers with Unity applications. Built on the Unity Android Bluetooth Low Energy plugin ([Velorexe/Unity-Android-Bluetooth-Low-Energy](https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy)), this framework provides a flexible, modular system for discovering BLE devices, establishing connections, and processing input for precise control of virtual objects.

The system is designed with a component-based architecture that separates device discovery, connection management, input processing, and object control, making it highly extensible and customizable for various applications including medical simulations, industrial control systems, and interactive training environments.

## Features

- **Automatic Device Discovery**: Continuously scans for and connects to compatible BLE devices
- **Connection Management**: Robust handling of device connections, disconnections, and reconnections
- **Modular Input Processing**: Flexible system for mapping BLE input to virtual object behaviors
- **Component-Based Control**: Dedicated controllers for rotation and translation that can be attached directly to GameObjects
- **Visual Feedback**: UI components for displaying connection status and scanning progress
- **Constrained Movement**: Configurable limits on rotation angles and movement distances
- **Input Customization**: Support for different joystick axes, button mappings, and input inversion
- **Audio Feedback**: Optional sound effects for object movement and interactions

## System Architecture

The FluroController system consists of several interconnected components:

- **BleAutoConnector**: Manages device discovery and initiates connections
- **BleDeviceConnector**: Handles individual device connections and data routing
- **BleInputReceiver**: Receives raw input data from connected BLE devices
- **InputControlManager**: Distributes input data to active controllers
- **RotationController**: Handles object rotation based on joystick or button input
- **TranslationController**: Handles object movement based on joystick or button input
- **ScanningIndicator**: Provides visual feedback during device scanning

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or newer
- Android 12 (API 32) Build Support module installed in Unity
- Basic knowledge of C# and Unity development
- A compatible BLE controller device

### Installation

1. Clone the FluroController repository
2. Open the project in Unity
3. Import the Unity Android Bluetooth Low Energy plugin from [Velorexe/Unity-Android-Bluetooth-Low-Energy](https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy)
4. Configure the project settings for Android development:
   - Set minimum API level to Android 12 (API 32)
   - Enable Bluetooth permissions in Player Settings
   - Configure the AndroidManifest.xml with required permissions:
     ```xml
     <uses-permission android:name="android.permission.BLUETOOTH" />
     <uses-permission android:name="android.permission.BLUETOOTH_ADMIN" />
     <uses-permission android:name="android.permission.BLUETOOTH_SCAN" />
     <uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
     <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
     ```

## Usage

### Basic Setup

1. Create three empty GameObjects in your scene:
   - `BLE Manager` (for device discovery and connection)
   - `Input Manager` (for input distribution)
   - `Input Receiver` (for receiving BLE data)

2. Add the following components:
   - Add `BleAutoConnector` to the BLE Manager
   - Add `InputControlManager` to the Input Manager and tag it as "InputManager"
   - Add `BleInputReceiver` to the Input Receiver and tag it as "InputReceiver"

3. Create a UI for device connections:
   - Create a UI panel with a content area for device buttons
   - Create a button prefab with the `BleDeviceConnector` script
   - Assign the content area to the `_deviceList` field on BleAutoConnector
   - Assign the button prefab to the `_deviceButton` field on BleAutoConnector

4. Configure controlled objects:
   - Add `RotationController` to objects that need to rotate
   - Add `TranslationController` to objects that need to move
   - Configure the settings for each controller (axis, rate, limits, input source)

### Example: Medical C-Arm Setup

```csharp
// C-Arm main rotation (around Z-axis)
GameObject cArmBase = new GameObject("C-Arm Base");
RotationController mainRotation = cArmBase.AddComponent<RotationController>();
mainRotation.rotationAxis = RotationController.RotationAxis.Z;
mainRotation.rotationRate = 30.0f;
mainRotation.minRotation = -90.0f;
mainRotation.maxRotation = 90.0f;
mainRotation.primaryJoystick = RotationController.JoystickIndex.Joystick1X;

// C-Arm secondary rotation (around X-axis)
GameObject cArmSecondary = new GameObject("C-Arm Secondary");
cArmSecondary.transform.parent = cArmBase.transform;
RotationController secondaryRotation = cArmSecondary.AddComponent<RotationController>();
secondaryRotation.rotationAxis = RotationController.RotationAxis.X;
secondaryRotation.rotationRate = 30.0f;
secondaryRotation.minRotation = -80.0f;
secondaryRotation.maxRotation = 22.0f;
secondaryRotation.primaryJoystick = RotationController.JoystickIndex.Joystick1Y;

// Table height adjustment (Y-axis translation)
GameObject table = new GameObject("Table");
TranslationController tableHeight = table.AddComponent<TranslationController>();
tableHeight.translationAxis = TranslationController.TranslationAxis.Y;
tableHeight.translationRate = 0.1f;
tableHeight.minPosition = 0.5f;
tableHeight.maxPosition = 0.7f;
tableHeight.buttonControl.controlType = TranslationController.ButtonControlType.ButtonSet;
tableHeight.buttonControl.buttonSetIndex = 2;
tableHeight.buttonControl.posButtonBit = 4;
tableHeight.buttonControl.negButtonBit = 1;
```

## Advanced Configuration

### Custom Device Filtering

To connect only to specific BLE devices, modify the `OnDeviceFound` method in the `BleAutoConnector` script:

```csharp
private void OnDeviceFound(string name, string device)
{
    Debug.Log($"Device found: {device}");

    // Modify this condition to match your device naming convention
    if (device.Contains("YourDeviceName"))
    {
        BleDeviceConnector button = Instantiate(_deviceButton, _deviceList).GetComponent<BleDeviceConnector>();
        button.Show(name, device);
        button.SetBleInteractor(this);
        button.Connect();
    }
}
```

### Custom Input Mapping

To change how joystick values are mapped to normalized values, modify the `MapJoystickValue` method in the controller scripts:

```csharp
private float MapJoystickValue(byte rawValue)
{
    // Customize the deadzone range
    const byte deadZoneStart = 110;
    const byte deadZoneEnd = 140;

    // Check if the value is within the deadzone
    if (rawValue >= deadZoneStart && rawValue <= deadZoneEnd)
    {
        return 0.0f;
    }
    
    // Custom mapping logic
    if (rawValue <= deadZoneStart)
    {
        return Mathf.Lerp(-1f, 0f, (rawValue / (float)deadZoneStart));
    }
    else
    {
        return Mathf.Lerp(0f, 1f, ((rawValue - deadZoneEnd) / (float)(255 - deadZoneEnd)));
    }
}
```

## Troubleshooting

### Common Issues

1. **Bluetooth permissions denied**
   - Ensure all required permissions are included in AndroidManifest.xml
   - Make sure your app requests runtime permissions correctly

2. **No devices found during scanning**
   - Verify the BLE device is powered on and in range
   - Check battery levels on the BLE device
   - Increase the scan time in BleAutoConnector

3. **Device connects but no input is received**
   - Verify that service and characteristic UUIDs match your device
   - Ensure all GameObject tags are set correctly
   - Check debug text field for error messages

4. **Objects not moving when input is received**
   - Verify controller components are properly configured
   - Check min/max limits to ensure they allow movement
   - Enable debug mode to see movement attempts in the console

### Debugging Tools

- Enable `debugMode` on controller components to see detailed logs
- Use the debug text field in BleInputReceiver to monitor input values
- Check the Unity console for error messages and warnings

## Contact

For questions, bug reports, or feature requests, please open an issue. For general inquiries, contact:

Andrew Duit  
Email: duit.andrew@mayo.edu or andrewduit@gmail.com

## License

The FluroController project is provided as-is without any express or implied warranties. All rights reserved.

## Acknowledgements

This project builds upon the excellent work of:
- [Velorexe/Unity-Android-Bluetooth-Low-Energy](https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy) for the core BLE functionality
