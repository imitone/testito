# Polygon Board Game Unity Project

A Unity-based board game similar to Pummel Party with Monopoly-style mechanics, featuring polygon-style graphics and mini-games.

## 🎮 Game Features

- **Board Game Mechanics**: Similar to Monopoly with properties to buy, sell, and trade
- **Mini-Games**: 5 different mini-games that players can challenge each other to
- **Polygon Art Style**: Clean, modern polygon-based visuals
- **Multiplayer**: Support for 2-4 players (local multiplayer)
- **Property Management**: Buy, sell, and challenge system for properties
- **Dynamic Gameplay**: Random events and mini-game challenges

## 🎯 Mini-Games Included

1. **Race to the Finish** - First player to reach the end wins
2. **Memory Match** - Match pairs of cards to score points
3. **Platform Jump** - Jump on moving platforms and avoid falling
4. **Color Clash** - Stand on the correct color when time runs out
5. **Polygon Panic** - Collect polygons to score points

## 📁 Project Structure

```
PolygonBoardGame/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManagement/
│   │   │   └── GameManager.cs
│   │   ├── BoardGame/
│   │   │   ├── BoardManager.cs
│   │   │   └── BoardSpace.cs
│   │   ├── Players/
│   │   │   └── Player.cs
│   │   ├── MiniGames/
│   │   │   ├── MiniGameManager.cs
│   │   │   ├── MiniGameBase.cs
│   │   │   ├── RaceGame.cs
│   │   │   ├── MemoryGame.cs
│   │   │   ├── PlatformGame.cs
│   │   │   ├── ColorGame.cs
│   │   │   └── PolygonGame.cs
│   │   └── UI/
│   │       ├── UI_GameManager.cs
│   │       ├── UI_MiniGameAnnouncement.cs
│   │       └── UI_DiceRoller.cs
│   ├── Scenes/
│   ├── Materials/
│   ├── Prefabs/
│   ├── UI/
│   └── Audio/
```

## 🚀 Setup Instructions

### 1. Unity Setup
1. Create a new Unity 3D project
2. Copy all scripts from the `Assets/Scripts/` folder into your Unity project
3. Set up the scene with the required GameObjects (see Scene Setup below)

### 2. Scene Setup

Create a new scene called "MainGame" and add the following GameObjects:

#### Main Game Objects:
- **GameManager** (Empty GameObject)
  - Add `GameManager.cs` script
  - Configure player count, starting money, etc.

- **BoardManager** (Empty GameObject)
  - Add `BoardManager.cs` script
  - Create a prefab for board spaces

- **MiniGameManager** (Empty GameObject)
  - Add `MiniGameManager.cs` script

- **UI_Canvas** (Canvas)
  - Add `UI_GameManager.cs` script
  - Create UI panels for different game states

#### Board Space Prefab:
Create a prefab with:
- Cube or custom polygon mesh
- `BoardSpace.cs` script
- Collider for player interaction
- Text components for property name and price

#### Player Prefab:
Create a prefab with:
- Capsule or custom character model
- `Player.cs` script
- Rigidbody for movement
- Collider for board interaction

### 3. UI Setup

Create the following UI panels:

#### Main Menu Panel:
- Title text
- Start game button
- Settings button

#### Game HUD:
- Current player display
- Player information panels
- Game state indicator
- Roll dice button

#### Property Decision Panel:
- Property name and details
- Buy/Sell/Challenge buttons
- Property owner information

#### Mini-Game UI:
- Game announcement panel
- Countdown timer
- Score display

### 4. Materials and Visuals

Create materials for:
- Board spaces (default, owned, special)
- Player colors
- Mini-game objects
- UI elements

Use bright, clean colors to achieve the polygon art style:
- Primary colors: Blues, greens, reds
- Clean gradients and flat shading
- Minimal shadows and textures

## 🎮 How to Play

### Board Game Phase:
1. Players take turns rolling dice
2. Move around the board based on dice roll
3. When landing on a property, choose:
   - **Buy**: Purchase the property if available
   - **Sell**: Sell owned properties
   - **Challenge**: Start a mini-game

### Mini-Game Phase:
1. Random mini-game is selected
2. All players participate
3. Winner receives bonus money
4. Return to board game

### Winning:
- Game continues for a set number of rounds
- Player with the highest net worth wins

## 🎯 Game Controls

### Board Game:
- **Space**: Roll dice (Player 1)
- **Mouse**: Click on UI buttons
- **Arrow Keys**: Navigate menus

### Mini-Games:
- **Space**: Jump (Platform Game)
- **Mouse**: Click cards (Memory Game)
- **Arrow Keys**: Move character (Color Clash, Polygon Panic)

## 🔧 Customization

### Adding New Mini-Games:
1. Create a new class inheriting from `MiniGameBase`
2. Implement required abstract methods:
   - `SetupGame()`
   - `OnGameStart()`
   - `HandleGameLogic()`
   - `CheckWinConditions()`
   - `OnGameEnd()`
3. Add to `MiniGameManager` creation methods

### Modifying Board Layout:
- Adjust `boardSize` in `BoardManager`
- Modify `GetSpacePosition()` for different board shapes
- Configure space types in `ConfigureSpace()`

### UI Customization:
- Modify UI prefabs and layouts
- Adjust colors and fonts
- Add animations and effects

## 🎨 Art Style Guidelines

### Polygon Style:
- Use low-poly 3D models
- Flat colors with minimal gradients
- Clean geometric shapes
- Bright, vibrant color palette

### UI Design:
- Flat design with subtle shadows
- Clear typography
- Consistent color scheme
- Smooth animations

## 🐛 Troubleshooting

### Common Issues:
1. **Players not moving**: Check BoardManager setup and board space positions
2. **UI not responding**: Verify Canvas and EventSystem setup
3. **Mini-games not starting**: Check MiniGameManager references
4. **Collisions not working**: Verify Collider and Rigidbody settings

### Performance Tips:
- Use object pooling for frequently created objects
- Optimize materials and textures
- Limit particle effects during mini-games
- Use LOD (Level of Detail) for complex models

## 🚧 Future Enhancements

### Planned Features:
- Online multiplayer support
- More mini-games
- Customizable board layouts
- Power-ups and special items
- Tournament mode
- Achievements and unlockables

### Technical Improvements:
- Save/Load game state
- Replay system
- Analytics integration
- Mobile platform support

## 📝 License

This project is created for educational purposes. Feel free to modify and use as needed.

## 🤝 Contributing

Feel free to contribute by:
- Adding new mini-games
- Improving UI/UX
- Optimizing performance
- Adding new features
- Bug fixes and improvements

---

Enjoy your polygon-style board game adventure! 🎮✨