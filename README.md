# FluroController Project

## Overview
FluroController is an innovative project designed to integrate a custom Bluetooth controller with Unity for VR headset applications. Leveraging the capabilities of the Unity Android Bluetooth Low Energy plugin ([Velorexe/Unity-Android-Bluetooth-Low-Energy](https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy)) and the [Pico Integration SDK](https://developer.picoxr.com/document/unity/), this project aims to enhance VR experiences by providing seamless interaction between physical controllers and virtual environments.

## Features
- **Device Discovery**: Automatically detect nearby Bluetooth Low Energy (BLE) devices.
- **Connection Management**: Establish and maintain connections with selected BLE devices.
- **Characteristic Interaction**: Read, write, and subscribe to characteristics of connected BLE devices.
- **VR Integration**: Seamless integration with VR headsets for immersive gaming experiences.
- **Window Passthrough** allows developers to display content outside of the VR headset's default field of view, enabling the creation of more immersive and interactive VR experiences.

## Getting Started
To get started with the FluroController project, ensure you have Unity installed along with the Android Build Support module and the Meta XR All-in-One SDK. 

Additionally, familiarity with C# and Unity's development environment is required.

### Prerequisites
- Unity 2022.3 LTS or newer
- Android 12 (API 32)  Build Support module installed in Unity
- Basic knowledge of C# and Unity development
- PICO needs atleast Android 10 (API 29)

### Installation
1. Clone the FluroController repository.
2. Open the project in Unity.
3. Import the Unity Android Bluetooth Low Energy plugin from [Velorexe/Unity-Android-Bluetooth-Low-Energy](https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy).
4. Import the [Pico Integration SDK](https://developer.picoxr.com/document/unity/)
5. Configure the project settings for Android development according to Unity's documentation.
6. Use the Older Permission Requests before Android 11 

## Usage
The FluroController project provides a comprehensive framework for integrating BLE devices with Unity VR applications. Utilize the provided scripts and examples to discover devices, connect to your custom Bluetooth controller, and interact with VR environments.

## Contact
For questions, bug reports, or feature requests, please open an issue. For general inquiries, contact us via email at duit.andrew@mayo.edu or andrewduit@gmail.com.
