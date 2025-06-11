# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Bio-Tag is a Unity-based multiplayer tag game that integrates biometric sensors (GSR/galvanic skin response) to influence gameplay. Players' stress levels detected through hardware sensors affect their movement speed and visual effects, creating a unique physiological gaming experience.

## Core Architecture

### Dependency Injection with VContainer
- **VContainer**: Lightweight DI container for Unity (jp.hadashikick.vcontainer v4.x)
- **LifetimeScope Pattern**: Each scene has its own LifetimeScope (e.g., `TitleLifetimeScope`)
- **Service Registration**: Services registered as Singleton, components via `RegisterComponentInHierarchy<T>()`
- **Injection**: Use `[Inject]` attribute for dependency injection in MonoBehaviours

### Game Management System
- **GameManagerBase**: Abstract base class for all game modes at `Assets/Scprits/System/GameManagerBase.cs:5`
- **GSRGameManager**: Core tag game logic at `Assets/Scprits/GSRGame/GSRGameManager.cs`
- **NPCGameManager**: Manages AI-controlled games at `Assets/Scprits/System/NPCGameManager.cs`
- **OfflinePlayerGameManager**: Single-player offline mode at `Assets/Scprits/System/OfflinePlayerGameManager.cs`

### Service Architecture (VContainer)
- **IPlayerDataService/PlayerDataService**: Player data management (name, preferences)
- **ISceneService/SceneService**: Scene transition management
- **IThemeService/ThemeService**: UI theme management with CSS Custom Properties
- All services use interfaces for abstraction and are registered in scene-specific LifetimeScopes

### Networking Architecture
- **TCP Server**: Local sensor data communication via `TcpServer` singleton at `Assets/Scprits/Utils/TCPServer.cs:10`
- **Node.js Servers**: External sensor data handling via `spresense_sketch/tcp_server.js` (port 10001)
- **Game State Management**: Centralized via GameManagerBase with states 0=Waiting, 1=Playing, 2=GameOver

### Player System
- **PlayerBase**: Abstract player controller at `Assets/Scprits/Player/PlayerBase.cs`
- **SingleMainPlayer/SingleSubPlayer**: Offline player variants for single-player mode
- **NPC**: AI-controlled player using Unity NavMesh at `Assets/Scprits/Player/NPC.cs`

### Sensor Integration
- **SensorManager**: Core biometric sensor management at `Assets/Scprits/System/SensorManager.cs:9`
- **GSRGraph**: Real-time biometric data visualization at `Assets/Scprits/Utils/GSRGraph.cs:6`
- **Hardware Integration**: Arduino-based MAX30009 sensor via BLE and TCP communication

### UI System (UI Toolkit)
- **Migration to UI Toolkit**: Using Unity 6's UI Toolkit instead of legacy uGUI
- **UXML Documents**: UI layouts defined in .uxml files
- **USS Styling**: CSS-like styling with Custom Properties for theming
- **Theme System**: Dynamic theme switching via CSS classes (default, theme-forest-gold, theme-ocean-blue)
- **No EventSystem Required**: UI Toolkit handles input independently

## Hardware Components

### Arduino MAX30009 Integration
- **BLE Communication**: Arduino Nano 33 BLE device advertising as "Nano33BLEDevice"
- **Sensor Calibration**: Frequency calibration at 31.25Hz with automatic PLL setup
- **Data Transmission**: 8-byte packets transmitted via BLE characteristics
- **TCP Bridge**: Spresense device bridges sensor data to Unity via TCP port 10001

### Python Sensor Processing
- **Poetry Project**: Located in `Arduino/python/` with Python 3.11-3.12 constraint
- **Dependencies**: PyQt5, numpy, scipy, bleak (BLE), keyboard/pynput (input handling)
- **Real-time Processing**: Handles sensor calibration and data filtering

## Development Environment
- **Unity Version**: 6000.0.42f1 (Unity 6)
- **Python Version**: 3.11-3.12
- **Node.js**: Required for TCP/HTTP sensor servers
- **Platform**: macOS (AppleScript dependencies in build tools)

## Development Commands

### Unity Development Tools
```bash
# Check current compilation errors
./unity-tools/unity-compile.sh check .

# Trigger Unity compilation
./unity-tools/unity-compile.sh trigger .

# Hot Reload is integrated - code changes apply automatically during development
```

### Unity Build & Run
```bash
# Open project in Unity Editor
# File -> Build Settings -> Build and Run
# Or use Unity command line: 
# /Applications/Unity/Hub/Editor/[VERSION]/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -buildTarget [Target] -quit
```

### Node.js Sensor Servers
```bash
# Start TCP server for sensor data (port 10001)
cd spresense_sketch
node tcp_server.js [log_file_name]

# Start HTTP server for REST API (port 10080)  
node HTTP_Server.js
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

### Game Synchronization
- **Game States**: 0=Waiting, 1=Playing, 2=GameOver
- **"It" Player**: Tracked via `itIndex` in GameManagerBase
- **Score Tracking**: Player scores and names managed via List<float> and List<string>
- **Ready System**: Players press 'F' to ready up before game start

## Scene Structure

- **Title.unity**: Main menu with VContainer setup (TitleLifetimeScope)
- **GSRGame.unity**: Core biometric tag game
- **WithPlayer.unity**: Single player mode with mock sensor data
- **WithNPC.unity**: AI opponent mode
- **Test/UIToolkitTitle.unity**: UI Toolkit test scene

## Package Dependencies

### Key Unity Packages
- **VContainer**: v4.x for dependency injection (`jp.hadashikick.vcontainer`)
- **R3**: Reactive extensions v1.2.9 (`com.cysharp.r3` and `org.nuget.r3`)
- **UniTask**: Async/await operations v2.5.10 (`com.cysharp.unitask`)
- **DOTween**: Animation and tweening system
- **Universal Render Pipeline**: Graphics rendering v17.0.4
- **Visual Effect Graph**: Particle effects for biometric feedback v17.0.4
- **Unity Multiplayer Tools**: v2.2.1 for networking debugging
- **AI Navigation**: v2.0.6 for NPC pathfinding

### Development Tools
- **Hot Reload**: SingularityGroup v1.13.7 for live code updates
- **Input System**: Modern Unity input handling v1.13.1
- **Cinemachine**: Camera control systems v2.10.3
- **Unity Test Framework**: v1.4.6 (integrated but no custom tests)