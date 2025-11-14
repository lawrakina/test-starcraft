# test-starcraft

Unity resource gathering game with basic economy mechanics.

## Features

### Base System
- Builds units (drones)
- Accepts resources from drones
- Press SPACE to build a new drone (3 second cooldown)
- Starts with 3 drones

### Drone System
- FSM (Finite State Machine) AI with states:
  - **Idle**: Waiting state
  - **SearchResource**: Looking for nearby resources
  - **MoveToResource**: Moving towards target resource
  - **CollectResource**: Gathering resources (up to 5 units)
  - **ReturnToBase**: Bringing resources back to base
- Navigation system using Unity NavMesh (with fallback movement)
- Search radius: 20 units
- Carry capacity: 5 resources per trip

### Resource System
- Scattered randomly on map (15 resources by default)
- Each resource has 10-50 charges (random)
- Resources become depleted when charges reach 0

### UI System
- Resource counter: Shows total collected resources
- Drone counter: Shows active drone count
- Instructions: Controls display
- Minimap: Top-down view of the game area

## Project Structure

```
Assets/
├── Scripts/
│   ├── Base.cs          - Base building and resource management
│   ├── Drone.cs         - Drone FSM AI and navigation
│   ├── Resource.cs      - Resource node with charges
│   ├── GameManager.cs   - Game initialization and coordination
│   └── UIManager.cs     - UI updates and minimap
├── Scenes/
│   └── MainScene.unity  - Main game scene
└── Prefabs/             - Game object prefabs (create in Unity Editor)
```

## Setup in Unity Editor

1. Open the project in Unity (2021.3 or later)
2. Create prefabs:
   - Base prefab with Base.cs component
   - Drone prefab with Drone.cs component and NavMeshAgent
   - Resource prefab with Resource.cs component
3. Set up the scene:
   - Add a plane for the ground
   - Place the Base object
   - Create a Camera for minimap
   - Set up UI Canvas with Text elements and RawImage for minimap
4. Configure GameManager with prefab references
5. Bake NavMesh for navigation

## Controls

- **SPACE**: Build a new drone (cooldown: 3 seconds)