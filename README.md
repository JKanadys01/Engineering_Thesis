# Praca_Dyplomowa
# Hand Gesture Detection System Using MEMS Sensors

This project was developed as part of my engineering thesis and focuses on detecting hand gestures using data from MEMS sensors. The system consists of a hardware device based on a microcontroller and a PC application for visualizing motion in real-time.

## 📌 Project Overview

The system detects hand gestures using readings from the following sensors:
- **Accelerometer**
- **Gyroscope**
- **Magnetometer**

It combines sensor data to determine orientation and movement, enabling gesture recognition.

## 🛠 Hardware

The device is built on:
- **ESP-WROOM-32** (microcontroller)
- **MPU6050** (accelerometer + gyroscope)
- **QMC5883L** (magnetometer)

The microcontroller reads and processes sensor data, which is then sent to a PC via serial or UDP communication.

## 💻 Software

### Embedded (C++)

The firmware written in **C++** performs:
- Reading and filtering sensor data
- Calculating orientation using sensor fusion (complementary filter or quaternions)
- Transmitting data to the PC

### Desktop Application (C#)

The desktop application:
- Receives real-time data from the device
- Visualizes the orientation and gestures in a 3D simulation
- Displays raw sensor readings for debugging

## 🎯 Features

- Real-time 3D visualization of device orientation
- Hand gesture recognition based on sensor data
- Sensor fusion for improved accuracy
- Modular and expandable codebase

## 🖼️ Preview

*(Optional: Add a screenshot or GIF here if available)*

https://github.com/user-attachments/assets/81ae36fd-7e36-43f3-bef5-344fb1679d90

## 🚀 Getting Started

### Requirements

- Visual Studio (for C# desktop app)
- Arduino IDE or PlatformIO (for firmware)
- .NET Framework or .NET Core installed

### How to Run

1. Flash the firmware to the microcontroller, and remember to update your Wi-Fi name and password.
2. Launch the C# desktop application.
3. Connect the device and start receiving live data.

## 📁 Folder Structure

