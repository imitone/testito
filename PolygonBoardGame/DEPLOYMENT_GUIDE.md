# 🎮 Polygon Board Game - Deployment Guide

## Overview
This guide will help you build and deploy the Polygon Board Game from source code. The game is built with Unity and features automatic setup for immediate gameplay.

## 📋 Prerequisites

### System Requirements
- **Unity Editor**: 2021.3 LTS or newer
- **Operating System**: Windows 10+, macOS 10.15+, or Linux Ubuntu 18.04+
- **Memory**: 4GB RAM minimum, 8GB recommended
- **Storage**: 2GB free space for Unity + game files

### Unity Installation
1. Download Unity Hub from [unity.com/download](https://unity.com/download)
2. Install Unity 2021.3 LTS through Unity Hub
3. Ensure the following modules are installed:
   - Linux Build Support (for Linux builds)
   - Windows Build Support (for Windows builds)
   - Mac Build Support (for macOS builds)

## 🚀 Quick Start

### Method 1: Automatic Build (Recommended)
```bash
# Navigate to the project directory
cd PolygonBoardGame

# Run the build script
./build.sh

# Run the game
cd Builds/Linux
./PolygonBoardGame
```

### Method 2: Unity Editor
1. Open Unity Hub
2. Click "Open" and select the `PolygonBoardGame` folder
3. Unity will automatically import the project
4. Open the `MainGame` scene from `Assets/Scenes/`
5. Press Play to test in the editor
6. Use File → Build Settings to build for your platform

### Method 3: Command Line Build
```bash
# Replace with your Unity installation path
UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"

# Build for Linux
$UNITY_PATH -batchmode -quit -projectPath "$(pwd)" \
    -buildTarget Linux64 \
    -buildPath "Builds/Linux/PolygonBoardGame" \
    -executeMethod BuildScript.BuildFromCommandLine \
    -logFile "Builds/build.log"

# Build for Windows
$UNITY_PATH -batchmode -quit -projectPath "$(pwd)" \
    -buildTarget Win64 \
    -buildPath "Builds/Windows/PolygonBoardGame.exe" \
    -executeMethod BuildScript.BuildFromCommandLine \
    -logFile "Builds/build.log"

# Build for macOS
$UNITY_PATH -batchmode -quit -projectPath "$(pwd)" \
    -buildTarget OSXUniversal \
    -buildPath "Builds/Mac/PolygonBoardGame.app" \
    -executeMethod BuildScript.BuildFromCommandLine \
    -logFile "Builds/build.log"
```

## 🎯 Game Features

### Automatic Setup
The game includes a `GameSetup` script that automatically:
- Creates the polygon board with 20 spaces
- Spawns 4 players (1 human, 3 AI)
- Sets up camera and lighting
- Initializes all game systems
- Starts the game immediately

### Board Layout
- **20 Total Spaces**: Arranged in a polygon shape
- **4 Special Spaces**: Mini-game zones at positions 5, 10, 15, 20
- **16 Property Spaces**: Available for purchase
- **Dynamic Pricing**: Properties increase in value by position

### Player System
- **Human Player**: Red color, full UI control
- **AI Players**: Blue, Green, Yellow with different difficulty levels
- **Starting Money**: $1,500 per player
- **Starting Position**: All players begin at space 0

### Game Systems
- **🎵 Dynamic Music**: Adaptive soundtrack based on game state
- **🏆 Achievements**: 25+ achievements across 9 categories
- **🌐 Networking**: Support for online multiplayer
- **⚙️ Mod Support**: Extensible framework for custom content
- **🎨 Visual Effects**: Smooth animations and particle effects

## 🔧 Build Configuration

### Build Settings
The game is configured to build with:
- **Scene**: `Assets/Scenes/MainGame.unity`
- **Platform**: Standalone (Windows, macOS, Linux)
- **Architecture**: x64
- **Compression**: LZ4 for faster loading
- **Optimization**: Size and runtime optimization

### Build Targets
- **Linux**: `Builds/Linux/PolygonBoardGame`
- **Windows**: `Builds/Windows/PolygonBoardGame.exe`
- **macOS**: `Builds/Mac/PolygonBoardGame.app`

## 🎮 How to Play

### Starting the Game
1. Launch the executable
2. The game will automatically set up the board
3. Click "Start Game" to begin
4. The human player (red) goes first

### Gameplay
1. **Roll Dice**: Click the dice to move your player
2. **Land on Properties**: Choose to buy, sell, or challenge
3. **Challenge Mode**: Triggers random mini-games
4. **Win Condition**: Bankrupt all opponents or reach target score

### Mini-Games
- **Race to Finish**: Navigate obstacles to reach the end
- **Memory Match**: Match pairs of cards
- **Platform Jump**: Survive falling platforms
- **Color Clash**: Quick color matching game
- **Polygon Panic**: Collect items while avoiding hazards

### Controls
- **Mouse**: Click to interact with UI elements
- **WASD**: Move in mini-games
- **Space**: Jump in platform games
- **ESC**: Pause/menu
- **F11**: Toggle fullscreen

## 🐛 Troubleshooting

### Common Issues

#### "Unity not found" Error
- Ensure Unity 2021.3 LTS is installed
- Check Unity installation path in `build.sh`
- Update `UNITY_PATH` variable if needed

#### Build Failures
- Check `Builds/build.log` for detailed error messages
- Ensure all dependencies are installed
- Verify Unity modules for target platform

#### Game Won't Start
- Check system requirements
- Ensure all game files are present
- Run from command line to see error messages

#### Performance Issues
- Close other applications to free memory
- Lower game resolution in settings
- Disable visual effects if needed

### Build Script Debugging
```bash
# Enable verbose logging
export UNITY_LOG_LEVEL=DEBUG

# Run with detailed output
./build.sh 2>&1 | tee build_debug.log

# Check Unity process
ps aux | grep Unity
```

## 📁 Project Structure

```
PolygonBoardGame/
├── Assets/
│   ├── Scenes/
│   │   └── MainGame.unity          # Main game scene
│   ├── Scripts/
│   │   ├── GameManagement/         # Core game logic
│   │   ├── Player/                 # Player systems
│   │   ├── Board/                  # Board management
│   │   ├── MiniGames/              # Mini-game implementations
│   │   ├── UI/                     # User interface
│   │   ├── Audio/                  # Sound and music
│   │   ├── Achievements/           # Achievement system
│   │   ├── Networking/             # Multiplayer support
│   │   ├── Mods/                   # Mod system
│   │   └── Editor/                 # Build scripts
│   └── GameSetup.cs                # Auto-setup script
├── Builds/                         # Build output directory
├── build.sh                        # Linux/macOS build script
├── build.bat                       # Windows build script
└── README.md                       # Project documentation
```

## 🔄 Development Workflow

### Making Changes
1. Open project in Unity Editor
2. Make your changes
3. Test in Play Mode
4. Commit changes to git
5. Build using `./build.sh`
6. Test the build

### Adding New Features
1. Create new scripts in appropriate folders
2. Reference them in `GameSetup.cs` if needed
3. Update this guide if new dependencies are required
4. Test thoroughly before committing

### Mod Development
The game supports custom mods through the `ModSystem`:
- Create new mini-games by implementing `IMiniGame`
- Add custom board layouts
- Extend achievement system
- See `Assets/Scripts/Mods/` for examples

## 🚀 Deployment

### Distribution
1. Build for all target platforms
2. Test on each platform
3. Package builds with documentation
4. Upload to distribution platform

### System Requirements (End Users)
- **Windows**: Windows 10 x64
- **macOS**: macOS 10.15 or later
- **Linux**: Ubuntu 18.04 or equivalent
- **Memory**: 2GB RAM minimum
- **Storage**: 1GB free space

## 📞 Support

### Getting Help
- Check the [COMPILATION_GUIDE.md](COMPILATION_GUIDE.md) for Unity setup
- Review [CHANGELOG.md](CHANGELOG.md) for recent updates
- Check Unity Console for error messages
- Review build logs in `Builds/build.log`

### Reporting Issues
When reporting issues, include:
- Operating system and version
- Unity version used
- Build logs and error messages
- Steps to reproduce the issue

### Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

---

## 🎉 Ready to Play!

The Polygon Board Game is now ready to build and play! The auto-setup system ensures that everything works out of the box, and the comprehensive build system makes deployment easy across all platforms.

Enjoy your polygon-powered board game experience! 🎮✨