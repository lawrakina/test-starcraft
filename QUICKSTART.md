# Quick Start Guide

Get the resource gathering game running in 5 minutes!

## Prerequisites
- Unity 2021.3 or later installed
- Basic Unity Editor knowledge

## 1. Open Project
```bash
# Clone repository (if not already cloned)
git clone https://github.com/lawrakina/test-starcraft.git
cd test-starcraft

# Open in Unity Hub
# Unity Hub → Add → Select this folder
```

## 2. Quick Scene Setup

### Create Prefabs (2 minutes)

**Base Prefab:**
```
1. Create Empty GameObject → Name: "Base"
2. Add child Cube → Scale: (2, 1, 2)
3. Change material to blue
4. Add Base.cs component to parent
5. Drag to Assets/Prefabs/ to create prefab
```

**Drone Prefab:**
```
1. Create Empty GameObject → Name: "Drone"
2. Add child Capsule → Scale: (0.5, 0.5, 0.5)
3. Change material to green
4. Add Drone.cs component to parent
5. Add NavMeshAgent component (optional but recommended)
6. Drag to Assets/Prefabs/ to create prefab
```

**Resource Prefab:**
```
1. Create Empty GameObject → Name: "Resource"
2. Add child Sphere → Scale: (0.8, 0.8, 0.8)
3. Change material to yellow
4. Add Resource.cs component to parent
5. Drag to Assets/Prefabs/ to create prefab
```

### Setup Scene (2 minutes)

**Ground:**
```
1. Create 3D Object → Plane
2. Scale: (10, 1, 10)
3. Position: (0, 0, 0)
```

**Base:**
```
1. Drag Base prefab to scene
2. Position: (0, 0.5, 0)
3. In Inspector → Drone Prefab: Select Drone prefab
```

**Cameras:**
```
1. Main Camera: Position (0, 30, -15), Rotation (60, 0, 0)
2. Optional: Add CameraController.cs for pan/zoom
```

**UI:**
```
1. Create UI → Canvas
2. Add 3 Text elements (Resource Count, Drone Count, Instructions)
3. Position in corners
```

**Game Manager:**
```
1. Create Empty → Name: "GameManager"
2. Add GameManager.cs
3. Assign:
   - Base: Base object from scene
   - Resource Prefab: Resource prefab
   - Resource Count: 15
   - Map Size: 40
```

**UI Manager:**
```
1. Create Empty → Name: "UIManager"  
2. Add UIManager.cs
3. Assign text references from Canvas
4. Drag UIManager to GameManager's UI Manager field
```

## 3. Optional: Bake NavMesh (1 minute)
```
1. Select Ground plane
2. Check "Navigation Static" in Inspector
3. Window → AI → Navigation
4. Bake tab → Click "Bake"
```

## 4. Play!
Press Play ▶️ and watch:
- 3 drones spawn
- Drones search for resources
- Resources are collected
- Resource counter increases
- Press SPACE to build more drones

## Troubleshooting

**Nothing happens?**
- Check GameManager has Base and Resource Prefab assigned
- Check Base has Drone Prefab assigned

**Drones don't move?**
- Either: Bake NavMesh OR remove NavMeshAgent component (fallback mode works)

**No resources spawn?**
- Check GameManager has Resource Prefab assigned
- Check Resource.cs is on the prefab

**Can't build drones?**
- Press SPACE (3 second cooldown between builds)
- Check Base component has Drone Prefab set

## Alternative: Minimal Test Setup

Want to test even faster? Use simple primitives without prefabs:

```
1. Create Plane (ground)
2. Create Cube → Add Base.cs
3. In Base component: 
   - Uncheck everything
   - Add 3 drones manually in scene
4. Create 3 Capsules → Add Drone.cs to each
   - Set Home Base reference to Base object
5. Create 5 Spheres → Add Resource.cs to each
6. Press Play!
```

## Next Steps

- Read `SETUP_GUIDE.md` for detailed setup
- Read `ARCHITECTURE.md` to understand the code
- Customize parameters in Inspector
- Add DroneVisualizer.cs to drones to see states

## Controls

- **SPACE**: Build new drone (3s cooldown)
- **WASD/Arrows**: Pan camera (if CameraController added)
- **Mouse Wheel**: Zoom camera (if CameraController added)

## Common Customizations

Edit values in Inspector:

**Base:**
- Build Cooldown: Speed of drone production
- Initial Drones: Starting drone count
- Spawn Radius: Drone spawn spread

**Drone:**
- Search Radius: How far drones look for resources (default 20)
- Move Speed: Drone movement speed (default 3.5)
- Carry Capacity: Resources per trip (default 5)

**Resource:**
- Min/Max Charges: Resource value range (default 10-50)

**GameManager:**
- Resource Count: Number of resources on map (default 15)
- Map Size: Spawn area radius (default 40)

---

Need help? Check the documentation files or the comments in the scripts!
