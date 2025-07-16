# 🎮 Polygon Board Game
### A Unity-based Multiplayer Board Game with Mini-Games

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Mac%20%7C%20Linux-lightgrey.svg)](https://unity3d.com/)

A modern, polygon-style board game inspired by **Monopoly** and **Pummel Party**. Features property management, AI opponents, and exciting mini-games with stunning low-poly visuals.

![Game Preview](https://via.placeholder.com/800x400/4a90e2/ffffff?text=Polygon+Board+Game+Preview)

## ✨ Features

### 🏠 **Board Game Mechanics**
- **Monopoly-Style Gameplay**: Buy, sell, and manage properties
- **Dynamic Property System**: 3 actions per property - Buy, Sell, or Challenge
- **AI Opponents**: 3 difficulty levels with unique strategies
- **Turn-Based Multiplayer**: 2-4 players (local multiplayer)
- **Bankruptcy System**: Realistic financial consequences
- **Statistics Tracking**: Detailed player performance metrics

### 🎯 **Mini-Games Collection**
1. **🏁 Race to the Finish** - Speed competition with boost mechanics
2. **🧠 Memory Match** - Card matching with strategic scoring
3. **🦘 Platform Jump** - Survive on moving platforms
4. **🎨 Color Clash** - Quick reaction color matching
5. **🔷 Polygon Panic** - Collect geometric shapes for points

### 🎨 **Visual & Audio**
- **Low-Poly Art Style**: Clean, modern polygon graphics
- **Particle Effects**: Money, movement, and victory celebrations
- **Dynamic Camera**: Smooth transitions and player focus
- **Spatial Audio**: Immersive sound design
- **Animated UI**: Smooth transitions and feedback

### 🤖 **AI System**
- **Adaptive AI**: Three difficulty levels with different strategies
- **Personality-Based Decisions**: Risk tolerance and aggressiveness
- **Smart Property Management**: AI evaluates deals strategically
- **Mini-Game Participation**: AI competes in all mini-games

## 🚀 Quick Start

### Prerequisites
- Unity 2021.3 LTS or newer
- Git (for cloning)

### Installation
1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/PolygonBoardGame.git
   cd PolygonBoardGame
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" and select the `PolygonBoardGame` folder
   - Open the project

3. **Auto-Setup**
   - The game includes an auto-setup system
   - Create an empty GameObject and add the `GameSetup.cs` script
   - Run the scene - everything will be configured automatically

4. **Play**
   - Press Play in Unity
   - Enjoy the game!

## 🎮 How to Play

### Board Phase
1. **Roll Dice**: Click the dice button to move your player
2. **Land on Properties**: Choose your action when landing on cities:
   - **🏪 Buy**: Purchase the property if available
   - **💰 Sell**: Sell your owned properties for half price
   - **⚡ Challenge**: Start a mini-game for bonus rewards

### Mini-Game Phase
- **Automatic Selection**: Random mini-game chosen
- **All Players Participate**: Everyone competes simultaneously
- **Winner Rewards**: Bonus money for the victor
- **Return to Board**: Continue the board game

### Victory Conditions
- **Reach $5,000 net worth** OR
- **Last player standing** after others go bankrupt
- **20 rounds maximum** (richest player wins)

## 🎯 Controls

### Board Game
- **Mouse**: Click UI buttons and interact with properties
- **Keyboard**: 
  - `Space`: Roll dice (Player 1)
  - `B`: Board view (camera)
  - `M`: Mini-game view (camera)
  - `Scroll`: Zoom in/out

### Mini-Games
- **Race Game**: `Space` for boost
- **Memory Game**: `Mouse` to click cards
- **Platform Game**: `Space` to jump
- **Color Clash**: `Arrow Keys` to move
- **Polygon Panic**: `Arrow Keys` to collect

## 🔧 Project Structure

```
PolygonBoardGame/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManagement/        # Core game logic
│   │   ├── BoardGame/            # Board and property system
│   │   ├── Players/              # Player mechanics and AI
│   │   ├── MiniGames/           # All mini-game implementations
│   │   ├── UI/                  # User interface system
│   │   ├── Audio/               # Audio management
│   │   ├── Camera/              # Camera control system
│   │   └── GameSetup/           # Auto-setup utilities
│   ├── Scenes/                  # Game scenes
│   ├── Materials/               # Visual materials
│   ├── Prefabs/                 # Game object prefabs
│   └── UI/                      # UI assets
├── README.md
├── LICENSE
└── .gitignore
```

## 🎨 Art Style Guide

### Polygon Design Principles
- **Low-Poly Models**: Clean geometric shapes
- **Bright Colors**: Vibrant, saturated palette
- **Flat Shading**: Minimal lighting complexity
- **Smooth Animations**: Fluid movement and transitions

### Color Palette
- **Primary**: Blues (#4a90e2), Greens (#7ed321), Reds (#d0021b)
- **Secondary**: Yellows (#f5a623), Purples (#9013fe), Oranges (#f8e71c)
- **Neutrals**: Grays (#9b9b9b), Whites (#ffffff), Blacks (#000000)

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/AmazingFeature`
3. **Commit changes**: `git commit -m 'Add AmazingFeature'`
4. **Push to branch**: `git push origin feature/AmazingFeature`
5. **Open a Pull Request**

### Adding New Mini-Games
1. Extend `MiniGameBase` class
2. Implement required abstract methods
3. Add to `MiniGameManager` creation system
4. Test thoroughly with AI players

## 📊 Game Statistics

The game tracks comprehensive statistics:
- **Properties bought/sold**
- **Mini-games won/played**
- **Money earned/spent**
- **Win rates and performance**
- **Game duration and rounds**

## 🛠️ Technical Details

### Architecture
- **Singleton Pattern**: Core managers (GameManager, AudioManager)
- **Event System**: Decoupled communication between systems
- **State Machine**: Clean game state management
- **Component-Based**: Modular, reusable components

### Performance Optimizations
- **Object Pooling**: Reused game objects
- **Efficient UI Updates**: Minimal UI refresh calls
- **Optimized Particle Systems**: Controlled emission rates
- **Smart Camera Culling**: Optimized rendering

## 🔮 Future Enhancements

### Planned Features
- **🌐 Online Multiplayer**: Network play with friends
- **📱 Mobile Support**: Touch controls and optimization
- **🎵 Custom Music**: Dynamic soundtrack system
- **🏆 Achievements**: Unlock system and rewards
- **⚙️ Mod Support**: Custom mini-games and boards

### Technical Improvements
- **💾 Save/Load**: Game state persistence
- **📊 Analytics**: Performance tracking
- **🎬 Replay System**: Record and playback games
- **🔧 Level Editor**: Create custom boards

## 🐛 Known Issues

- AI decision-making could be more sophisticated
- Some UI animations may stutter on older hardware
- Audio clips are placeholders (need actual sound effects)
- Camera transitions might be jarring in certain scenarios

## 📋 Requirements

### Minimum System Requirements
- **OS**: Windows 10, macOS 10.14, Ubuntu 18.04
- **Processor**: Intel i5-4590 / AMD FX 8350
- **Memory**: 4 GB RAM
- **Graphics**: DirectX 11 compatible
- **Storage**: 500 MB available space

### Recommended
- **OS**: Windows 11, macOS 12.0, Ubuntu 20.04
- **Processor**: Intel i7-8700K / AMD Ryzen 5 3600
- **Memory**: 8 GB RAM
- **Graphics**: Dedicated GPU with 2GB VRAM
- **Storage**: 1 GB available space

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Credits

### Development Team
- **Game Design**: Inspired by classic board games
- **Programming**: Unity C# implementation
- **Art Style**: Low-poly polygon aesthetic
- **Audio**: Spatial sound design

### Special Thanks
- Unity Technologies for the excellent game engine
- The board game community for inspiration
- Beta testers and feedback providers

## 📞 Support

Having issues? Need help?

- **📧 Email**: support@polygonboardgame.com
- **🐛 Bug Reports**: [GitHub Issues](https://github.com/yourusername/PolygonBoardGame/issues)
- **💬 Discord**: [Join our community](https://discord.gg/polygonboardgame)
- **📚 Documentation**: [Full docs](https://github.com/yourusername/PolygonBoardGame/wiki)

## 🎉 Changelog

### Version 1.0.0 (Current)
- ✅ Complete board game implementation
- ✅ 5 fully functional mini-games
- ✅ AI opponents with 3 difficulty levels
- ✅ Polygon art style with particle effects
- ✅ Audio system with spatial sound
- ✅ Dynamic camera system
- ✅ Statistics tracking
- ✅ Auto-setup system

---

**Ready to Play?** Download now and experience the future of board gaming! 🎮✨

Made with ❤️ and Unity