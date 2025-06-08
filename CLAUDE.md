# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Bio-Tag is a Unity-based multiplayer tag game that integrates biometric sensors (GSR/galvanic skin response) to influence gameplay. Players' stress levels detected through hardware sensors affect their movement speed and visual effects, creating a unique physiological gaming experience.

## Core Architecture

### Game Management System
- **GameManagerBase**: Abstract base class for all game modes at `Assets/Scprits/System/GameManagerBase.cs:8`
- **NetworkGameManager**: Handles PUN2 multiplayer gameplay at `Assets/Scprits/System/NetworkGameManager.cs:8`
- **NPCGameManager**: Manages AI-controlled games at `Assets/Scprits/System/NPCGameManager.cs`
- **OfflinePlayerGameManager**: Single-player offline mode at `Assets/Scprits/System/OfflinePlayerGameManager.cs`

### Networking Architecture
- **Photon PUN2**: Primary networking solution using App ID `ad94877c-57e5-4867-b36b-03c2443c6cc5` (Japan region)
- **NetWorkEntryPoint**: Main networking entry point at `Assets/Scprits/Network/NetWorkEntryPoint.cs:6`
- **Room Properties**: Custom room state management via `GameRoomProperty` and `PlayerProperty` classes
- **TCP Server**: Local sensor data communication via `TcpServer` singleton at `Assets/Scprits/Utils/TCPServer.cs:10`

### Player System
- **PlayerBase**: Abstract player controller at `Assets/Scprits/Player/PlayerBase.cs`
- **PUN2Player**: Networked player implementation using Photon
- **SingleMainPlayer/SingleSubPlayer**: Offline player variants for single-player mode

### Sensor Integration
- **SensorManager**: Core biometric sensor management at `Assets/Scprits/System/SensorManager.cs:9`
- **GSRGraph**: Real-time biometric data visualization at `Assets/Scprits/Utils/GSRGraph.cs:6`
- **Hardware Integration**: Arduino-based MAX30009 sensor via BLE and TCP communication

## Hardware Components

### Arduino MAX30009 Integration
- **BLE Communication**: Arduino Nano 33 BLE device advertising as "Nano33BLEDevice"
- **Sensor Calibration**: Frequency calibration at 31.25Hz with automatic PLL setup
- **Data Transmission**: 8-byte packets transmitted via BLE characteristics
- **TCP Bridge**: Spresense device bridges sensor data to Unity via TCP port 10001

### Python Sensor Processing
- **Poetry Project**: Located in `Arduino/python/` with dependencies for PyQt5, numpy, scipy
- **Real-time Processing**: Handles sensor calibration and data filtering

## Key Game Modes

### Multiplayer (PUN2)
- Scene: `PUN2.unity`
- Minimum 2 players required
- Real-time biometric influence on gameplay
- Master client handles game state and scoring

### Single Player
- Scene: `WithPlayer.unity` 
- Local gameplay with mock sensor data
- Testing environment for sensor integration

### NPC Mode
- Scene: `WithNPC.unity`
- AI-controlled opponents using Unity NavMesh
- Good for testing without multiple human players

## Development Commands

### Unity Build & Run
```bash
# Open project in Unity Editor
# File -> Build Settings -> Build and Run
# Or use Unity command line: 
# /Applications/Unity/Hub/Editor/[VERSION]/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -buildTarget [Target] -quit
```

### Arduino Development
```bash
# For MAX30009 sensor (Arduino IDE required)
# Open Arduino/BioZ/MAX30009_SPI.ino
# Select board: Arduino Nano 33 BLE
# Upload with libraries: ArduinoBLE, SoftSPI

# For Spresense TCP client
# Open spresense_sketch/TCPClient/TCPClient.ino  
# Requires Spresense board package and TelitWiFi library
```

### Python Sensor Tools
```bash
cd Arduino/python
poetry install
poetry run python MAX30009_GUI.py  # Launch sensor GUI
poetry run python determine_values.py  # Calibration tool
```

## Critical Integration Points

### Biometric Data Flow
1. **MAX30009 Sensor** → BLE → **Arduino Nano 33**
2. **Arduino** → BLE characteristics → **Python Script**
3. **Python/Spresense** → TCP (port 10001) → **Unity TcpServer**
4. **TcpServer** → **GSRGraph** → **SensorManager** → **Player Speed/VFX**

### State Management
- **Excited State**: GSR threshold exceeded → Player speed = 3f, red VFX, vignette effect
- **Calm State**: Below threshold → Player speed = 6.5f, white VFX, clear vision
- **Real-time Updates**: 100ms intervals with R3 reactive properties

### Networking Synchronization
- **Game States**: 0=Waiting, 1=Playing, 2=GameOver
- **"It" Player**: Tracked via `itIndex` in room properties
- **Score Sync**: Master client broadcasts scores every 0.1 seconds
- **Ready System**: Players press 'F' to ready up before game start

## Scene Structure

- **Title.unity**: Main menu and networking setup
- **GSRGame.unity**: Core biometric tag game
- **VFXTest.unity**: Visual effects testing environment  
- **UDPTest.unity**: Network communication testing

## Package Dependencies

### Key Unity Packages
- **Photon PUN2**: Multiplayer networking
- **DOTween**: Animation and tweening system
- **UniTask**: Async/await operations (`com.cysharp.unitask`)
- **R3**: Reactive extensions (`com.cysharp.r3`)
- **Universal Render Pipeline**: Graphics rendering
- **Visual Effect Graph**: Particle effects for biometric feedback

### Development Tools
- **Hot Reload**: Live code updates during development
- **Input System**: Modern Unity input handling
- **Cinemachine**: Camera control systems