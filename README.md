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
- Calculating orientation using sensor fusion (complementary filter and quaternions)
- Transmitting data to the PC

### Desktop Application (C#)

The desktop application:
- Receives real-time data from the device
- Visualizes the orientation and gestures in a 3D
- Displays raw sensor readings for debugging

## 🎯 Features

- Real-time 3D visualization of device orientation
- Hand gesture recognition based on sensor data
- Sensor fusion for improved accuracy
- Modular and expandable codebase
- Separate program for testing gesture detection using the DTW (Dynamic Time Warping) algorithm, implemented solely in the Arduino IDE

## 🖼️ Preview


https://github.com/user-attachments/assets/81ae36fd-7e36-43f3-bef5-344fb1679d90

## 🚀 Getting Started

### Requirements

- Arduino IDE (for firmware)
- Visual Studio (for C# desktop app)
![image](https://github.com/user-attachments/assets/7d90d6a8-949e-4ade-b039-f2f614804d88)


### How to Run

1. Flash the firmware to the microcontroller. Make sure to update the Wi-Fi name, password, and PC IP address in the code (look for comment marked "nr 1").
   
![image](https://github.com/user-attachments/assets/d8811f5f-706c-438a-81aa-102335fc0796)

3. Launch the C# desktop application.
4. Connect the device and start receiving live data.

