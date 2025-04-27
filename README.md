# Overgrown - Ivy Growth & Cutting System

A procedural ivy growth and cutting game with rogue-lite progression.

## Setup Instructions

### 1. Ivy Segment Setup
1. Create a new sprite GameObject in your scene
2. Add a `BoxCollider2D` component (set Is Trigger to true)
3. Set its Tag to "IvySegment" (create this tag if it doesn't exist)
4. Add the `IvySegment` script
5. Drag this GameObject to your Project panel to create a prefab
6. Delete the instance from your scene

### 2. Root Spawner Setup
1. Create an empty GameObject in your scene
2. Add the `IvyNode` component
3. Assign the ivy segment prefab you created to the `segmentPrefab` field
4. Adjust the other parameters as desired:
   - `growthInterval` - Time between growth steps (seconds)
   - `segmentLength` - Length of each segment
   - `branchChance` - Probability of branching (0-1)
   - `maxDepth` - Maximum recursion depth of growth

### 3. Player Setup
1. Create a new GameObject for your player character
2. Add a `Rigidbody2D` component (set to Kinematic if you want)
3. Add a `CircleCollider2D` component and set Is Trigger to true
4. Add the `PlayerMovement` component
5. Add the `PlayerStats` component
6. Add the `IvyCutterCollision` component

### 4. Camera Setup
1. Select your Main Camera
2. Add the `CameraFollow` component
3. Assign your player as the `target`
4. Set the `offset` (typically (0,0,-10) for 2D)
5. Add the `CameraShake` component (no additional setup needed)

### 5. XP Manager Setup
1. Create an empty GameObject
2. Add the `XPManager` component
3. Configure the `xpCurve` AnimationCurve in the Inspector:
   - X-axis represents level
   - Y-axis represents XP required for that level

### 6. Game Initializer Setup (Important)
1. Create an empty GameObject named "GameInitializer"
2. Add the `GameInitializer` script
3. Assign references to your XPManager and PlayerStats
4. Make sure this GameObject is active in the scene before gameplay starts
5. This ensures proper initialization order and prevents null reference errors

### 7. Item System (Optional)
1. Right-click in your Project panel
2. Select Create > Items > CutterItem
3. Configure the item properties in the Inspector
4. Create item pickup prefabs that use these ScriptableObjects

## Script Overview

- **IvyNode.cs** - Controls procedural ivy growth and branching
- **IvySway.cs** - Adds organic movement to ivy segments
- **IvySegment.cs** - Adds health to ivy segments and handles damage
- **PlayerStats.cs** - Tracks player level-based stats (finesse and radius)
- **IvyCutterCollision.cs** - Applies continuous damage to ivy on contact
- **XPManager.cs** - Manages XP, levels, and upgrade points
- **GameInitializer.cs** - Ensures proper initialization order of game systems
- **CutterItem.cs** - Defines item pickups for stat boosts
- **PlayerMovement.cs** - Smooth 2D character movement
- **CameraFollow.cs** - Camera that follows the player
- **CameraShake.cs** - Screen shake for impactful feedback

## Performance Tips

- Limit `maxDepth` on your IvyNode to prevent excessive growth
- Consider object pooling for ivy segments and particles
- Turn off ivy segments that are far from the player
- Batch ivy segments with static batching when possible
- Aim for fewer, larger cuts rather than many small ones

## Controls

- WASD or Arrow Keys - Move the player
- Player will automatically damage ivy on contact based on finesse

## Progression System

The game features a simple rogue-lite progression system:
1. Destroy ivy segments to gain XP
2. Level up to increase your finesse (cutting power) and radius
3. Higher levels allow you to cut through ivy faster and across a larger area

## Troubleshooting

- If you encounter a NullReferenceException in IvyCutterCollision, make sure:
  - The PlayerStats component is on the same GameObject as the IvyCutterCollision
  - The GameInitializer is properly set up with references
  - The execution order ensures XPManager is initialized before PlayerStats
- Check that all required tags ("IvySegment") have been created
- For debugging, try adding Debug.Log statements to verify component initialization

Enjoy your Overgrown game! 