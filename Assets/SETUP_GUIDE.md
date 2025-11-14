# Unity Setup Guide

This guide explains how to set up the resource gathering game in Unity Editor.

## Prerequisites
- Unity 2021.3 or later
- Basic knowledge of Unity Editor

## Step-by-Step Setup

### 1. Create the Ground Plane
1. In Hierarchy, right-click → 3D Object → Plane
2. Scale it up (e.g., Scale: 10, 1, 10)
3. Position at (0, 0, 0)

### 2. Create Base Prefab
1. Create an empty GameObject: "Base"
2. Add visual representation:
   - Add child: Cube (Scale: 2, 1, 2)
   - Change material color to blue
3. Add Base.cs component
4. Create prefab: Drag "Base" to Prefabs folder

### 3. Create Drone Prefab
1. Create an empty GameObject: "Drone"
2. Add visual representation:
   - Add child: Capsule (Scale: 0.5, 0.5, 0.5)
   - Change material color to green
3. Add Drone.cs component
4. Add NavMeshAgent component:
   - Speed: 3.5
   - Radius: 0.5
   - Height: 1
5. Create prefab: Drag "Drone" to Prefabs folder

### 4. Create Resource Prefab
1. Create an empty GameObject: "Resource"
2. Add visual representation:
   - Add child: Sphere (Scale: 0.8, 0.8, 0.8)
   - Change material color to yellow/gold
3. Add Resource.cs component
4. Create prefab: Drag "Resource" to Prefabs folder

### 5. Setup Main Camera
1. Position camera for top-down view: (0, 30, -15)
2. Rotate: (60, 0, 0)

### 6. Create Minimap Camera
1. Create new camera: "MinimapCamera"
2. Position: (0, 50, 0)
3. Rotation: (90, 0, 0)
4. Set Viewport Rect: X=0.75, Y=0.75, W=0.25, H=0.25 (or use RenderTexture)

### 7. Setup UI Canvas
1. Create UI → Canvas
2. Set Canvas Scaler to "Scale With Screen Size"
3. Add Text elements:
   - Resource Counter (Top-left)
   - Drone Counter (Top-left, below resources)
   - Instructions (Bottom-center)
4. Add RawImage for minimap (Top-right)

### 8. Create GameManager
1. Create empty GameObject: "GameManager"
2. Add GameManager.cs component
3. Assign references:
   - Base: Base prefab from scene
   - Resource Prefab: Resource prefab
   - UI Manager: (next step)

### 9. Create UIManager
1. Create empty GameObject: "UIManager"
2. Add UIManager.cs component
3. Assign references:
   - Resource Text
   - Drone Text
   - Instructions Text
   - Minimap Image
   - Minimap Camera

### 10. Configure Base Object
1. Select Base object in scene
2. In Base component, assign:
   - Drone Prefab: Drone prefab from Prefabs folder
   - Build Cooldown: 3
   - Initial Drones: 3
   - Spawn Radius: 2

### 11. Bake NavMesh
1. Window → AI → Navigation
2. Select ground plane
3. Check "Navigation Static"
4. Go to Bake tab
5. Click "Bake"

### 12. Test the Game
1. Press Play
2. Verify:
   - 3 drones spawn
   - Resources spawn around the map
   - Drones automatically collect resources
   - UI updates with resource/drone counts
   - Press SPACE to build new drones

## Troubleshooting

**Drones not moving:**
- Ensure NavMesh is baked
- Check NavMeshAgent component is attached
- Verify ground is marked as Navigation Static

**Resources not spawning:**
- Check Resource Prefab is assigned in GameManager
- Verify Resource.cs component is on prefab

**UI not updating:**
- Check UIManager references are set
- Verify GameManager has UIManager reference

**Drones not collecting:**
- Check Drone's Search Radius (default: 20)
- Ensure resources are within range
- Verify Resource.IsAvailable returns true

## Customization

You can adjust parameters in Inspector:
- **Base**: Build cooldown, initial drones, spawn radius
- **Drone**: Search radius, collect range, move speed, carry capacity
- **Resource**: Min/max charges (10-50 by default)
- **GameManager**: Resource count, map size
