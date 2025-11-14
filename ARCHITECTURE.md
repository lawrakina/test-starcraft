# Architecture Documentation

## Overview
This document describes the architecture and implementation of the Unity resource gathering game with basic economy mechanics.

## Core Systems

### 1. Base System (`Base.cs`)
**Responsibilities:**
- Builds drone units
- Receives and stores resources from drones
- Manages drone lifecycle

**Key Features:**
- Initial spawn of 3 drones on start
- Build new drones on SPACE key press (3 second cooldown)
- Tracks total resources collected
- Maintains list of active drones
- Spawn drones in radius around base

**Public API:**
```csharp
void DepositResource(int amount)  // Called by drones to deposit resources
Vector3 Position                   // Base position for drone navigation
int TotalResources                 // Total resources collected
int DroneCount                     // Active drone count
```

### 2. Drone System (`Drone.cs`)
**Responsibilities:**
- Autonomous resource gathering using FSM AI
- Navigation to resources and base
- Resource collection and delivery

**FSM States:**
1. **Idle**: Waiting before next action
2. **SearchResource**: Finding nearest available resource within search radius
3. **MoveToResource**: Navigating to target resource
4. **CollectResource**: Collecting resources (0.5s per unit, max 5 units)
5. **ReturnToBase**: Delivering collected resources to base

**State Transitions:**
```
Idle → SearchResource (after 1s)
SearchResource → MoveToResource (resource found)
SearchResource → Idle (no resource found after 2s)
MoveToResource → CollectResource (arrived at resource)
MoveToResource → SearchResource (resource depleted/unavailable)
CollectResource → ReturnToBase (capacity full or resource depleted)
CollectResource → SearchResource (resource depleted, no cargo)
ReturnToBase → SearchResource (delivered resources to base)
```

**Navigation:**
- Primary: Unity NavMeshAgent for pathfinding
- Fallback: Simple direct movement if NavMesh unavailable

**Parameters:**
- Search Radius: 20 units
- Collect Range: 1 unit
- Move Speed: 3.5 units/s
- Carry Capacity: 5 resources

### 3. Resource System (`Resource.cs`)
**Responsibilities:**
- Represents harvestable resource nodes
- Tracks remaining charges
- Self-deactivates when depleted

**Key Features:**
- Random charge count: 10-50 units (set on Start)
- Can be collected 1 unit at a time
- Becomes inactive when depleted

**Public API:**
```csharp
bool Collect(int amount = 1)  // Collect resources, returns success
bool IsAvailable              // Check if resource has charges
Vector3 Position              // Resource location
int GetCharges()              // Get remaining charges
```

### 4. Game Manager System (`GameManager.cs`)
**Responsibilities:**
- Game initialization and coordination
- Resource spawning
- UI updates coordination

**Key Features:**
- Singleton pattern for global access
- Spawns 15 resources on start within 40 unit radius
- Maintains minimum 5 unit distance from base
- Coordinates UI updates

**Parameters:**
- Resource Count: 15
- Map Size: 40 units radius
- Min Distance from Base: 5 units

### 5. UI System (`UIManager.cs`)
**Responsibilities:**
- Display resource and drone counts
- Show game instructions
- Manage minimap rendering

**UI Elements:**
- Resource counter text
- Drone counter text
- Instructions text
- Minimap (RenderTexture from minimap camera)

**Minimap Setup:**
- 256x256 RenderTexture
- Top-down camera view
- Can be positioned anywhere on screen

## Helper Components

### DroneVisualizer (`DroneVisualizer.cs`)
**Purpose:** Debug visualization of drone states

**Features:**
- Color-coded drone states:
  - Gray: Idle
  - Yellow: Searching
  - Cyan: Moving to resource
  - Green: Collecting
  - Blue: Returning to base
- On-screen debug text showing state and cargo
- Optional component (not required for core functionality)

### ResourceSpawner (`ResourceSpawner.cs`)
**Purpose:** Utility for spawning resources with validation

**Features:**
- Configurable spawn count and radius
- Distance validation from base
- Parent organization
- Gizmo visualization in editor
- Runtime spawn/clear methods

### CameraController (`CameraController.cs`)
**Purpose:** Camera navigation for better game view

**Features:**
- WASD/Arrow key panning
- Mouse scroll wheel zoom
- Configurable pan limits and zoom range
- Reset to starting position

## Data Flow

### Resource Collection Flow
```
1. Drone (SearchResource) → FindNearestResource()
2. Drone (MoveToResource) → Navigate to Resource.Position
3. Drone (CollectResource) → Resource.Collect(1)
4. Resource → Decrease charges, deactivate if depleted
5. Drone (ReturnToBase) → Navigate to Base.Position
6. Drone → Base.DepositResource(carriedResources)
7. Base → Update totalResources
8. Base → GameManager.UpdateUI()
9. GameManager → UIManager.UpdateResourceCount()
```

### Drone Building Flow
```
1. User presses SPACE
2. Base checks cooldown (3 seconds)
3. Base.BuildDrone() → Instantiate drone prefab
4. Drone.Initialize(base) → Set home base reference
5. Drone starts in SearchResource state
6. Base → GameManager.UpdateUI()
7. GameManager → UIManager.UpdateDroneCount()
```

## Design Decisions

### Why FSM for Drone AI?
- Simple and predictable behavior
- Easy to debug and extend
- Clear state transitions
- Suitable for the scope of this test project

### Why NavMesh with Fallback?
- NavMesh provides proper pathfinding around obstacles
- Fallback ensures game works without NavMesh baking
- Flexibility for different scene setups

### Why Singleton GameManager?
- Single source of truth for game state
- Easy access from any component
- Simple initialization coordination

### Resource Charges (10-50)
- Provides variety in resource node value
- Creates interesting decision-making (when to switch resources)
- Prevents all resources from depleting at same time

### Carry Capacity (5 units)
- Balances trip frequency vs efficiency
- Forces drones to make multiple trips
- Creates visible activity in the game

## Extension Points

The architecture supports easy extensions:

1. **New Drone States**: Add to enum and implement Update method
2. **Different Resource Types**: Subclass Resource with different behaviors
3. **Base Upgrades**: Add upgrade system to Base class
4. **Multiple Bases**: GameManager can manage list of bases
5. **Drone Specialization**: Create Drone subclasses for different roles
6. **Combat System**: Add attack/defense states to FSM
7. **Research System**: Technology tree in GameManager
8. **Save/Load**: Serialize game state from GameManager

## Performance Considerations

- Drones use FindObjectsOfType for resource search (acceptable for ~15 resources)
- For larger games, implement spatial partitioning or resource registry
- NavMesh queries are efficient but can be expensive with many agents
- UI updates are event-driven (not every frame)

## Testing in Unity Editor

See `SETUP_GUIDE.md` for complete setup instructions.

**Quick verification checklist:**
1. ✓ Drones spawn at start (3 drones)
2. ✓ Resources spawn around map (15 resources)
3. ✓ Drones find and move to resources
4. ✓ Resource charges decrease during collection
5. ✓ Drones return to base when full
6. ✓ Resource counter increases
7. ✓ SPACE builds new drone (with cooldown)
8. ✓ Minimap shows scene overview

## Known Limitations

1. **No obstacle avoidance** without NavMesh (fallback mode)
2. **No resource reservation** - multiple drones may target same resource
3. **No drone-drone collision avoidance** (can be added with NavMeshAgent)
4. **Fixed resource spawn** - no dynamic spawning during gameplay
5. **No save/load** functionality
6. **No multiplayer** support

These are intentional simplifications for a test project scope.
