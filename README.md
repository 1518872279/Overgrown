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

### 2. Leaf Prefab Setup
1. Create a new sprite GameObject for a leaf
2. Add the `IvySway` script
3. Drag this GameObject to your Project panel to create a prefab
4. Delete the instance from your scene

### 3. Ivy Root Prefab Setup
1. Create an empty GameObject
2. Add the `IvyNode` component
3. Assign the ivy segment prefab to the `branchPrefab` field
4. Assign the leaf prefab to the `leafPrefab` field
5. Configure the density controls:
   - `branchChance` - Base probability for side branches
   - `branchDecay` - How quickly branching reduces with depth
   - `maxBranchSpawns` - Maximum branches per node
   - `leafSpawnChance` - Probability for spawning leaves
   - `maxLeavesPerBranch` - Maximum leaves per branch
6. Set up an AnimationCurve in the `healthCurve` field (matching your XP curve)
7. Drag this GameObject to your Project panel to create a prefab

### 4. Growth Manager Setup
1. Create an empty GameObject at the center of your scene
2. Add the `GrowthManager` component
3. Assign the ivy root prefab you created to the `ivyRootPrefab` field
4. Adjust the parameters:
   - `spawnRadius` - Distance from center to spawn points
   - `rootCount` - Number of ivy roots to spawn

### 5. Growth Controller Setupx
1. Create an empty GameObject named "GrowthController"
2. Add the `GrowthController` script
3. Configure the `speedMultiplier` parameter (1.0 is normal speed, >1 is faster, <1 is slower)
4. This allows you to dynamically control ivy growth speed during gameplay

### 6. Center Area Protection Setup
1. Create an empty GameObject at the center of your scene
2. Add a `CircleCollider2D` component (set Is Trigger to true)
3. Set the radius to define the protected area
4. Add the `CenterAreaProtection` script

### 7. Player Setup
1. Create a new GameObject for your player character
2. Add a `Rigidbody2D` component (set to Kinematic if you want)
3. Add a `CircleCollider2D` component and set Is Trigger to true
4. Add the `PlayerMovement` component
5. Add the `PlayerStats` component
6. Add the `IvyCutterCollision` component

### 8. UI Setup
1. Import TextMeshPro into your project (Window > Package Manager > TextMeshPro)
2. Create a Canvas (GameObject > UI > Canvas)
3. Add two TextMeshPro Text elements to the Canvas:
   - Level Text (top-left corner)
   - Finesse Text (below Level Text)
4. Create an empty GameObject as a child of the Canvas
5. Add the `PlayerUI` script to this GameObject
6. Assign the Level Text and Finesse Text references in the Inspector

### 9. Camera Setup
1. Select your Main Camera
2. Add the `CameraFollow` component
3. Assign your player as the `target`
4. Set the `offset` (typically (0,0,-10) for 2D)
5. Add the `CameraShake` component (no additional setup needed)

### 10. XP Manager Setup
1. Create an empty GameObject
2. Add the `XPManager` component
3. Configure the `xpCurve` AnimationCurve in the Inspector:
   - X-axis represents level
   - Y-axis represents XP required for that level

### 11. Game Initializer Setup (Important)
1. Create an empty GameObject named "GameInitializer"
2. Add the `GameInitializer` script
3. Assign references to your XPManager and PlayerStats
4. Make sure this GameObject is active in the scene before gameplay starts
5. This ensures proper initialization order and prevents null reference errors

### 12. Item System (Optional)
1. Right-click in your Project panel
2. Select Create > Items > CutterItem
3. Configure the item properties in the Inspector
4. Create item pickup prefabs that use these ScriptableObjects

## Script Overview

- **GrowthManager.cs** - Spawns multiple ivy roots around the perimeter
- **GrowthController.cs** - Dynamic control of ivy growth speed
- **IvyNode.cs** - Controls procedural ivy growth with advanced density controls
- **IvySegment.cs** - Adds scalable health and XP drops
- **IvySway.cs** - Adds organic movement to leaves
- **CenterAreaProtection.cs** - Detects when ivy covers the center (game over)
- **PlayerStats.cs** - Tracks player level-based stats (finesse and radius)
- **IvyCutterCollision.cs** - Applies continuous damage to ivy on contact
- **XPManager.cs** - Manages XP, levels, and upgrade points
- **PlayerUI.cs** - Displays player level and finesse on the UI using TextMeshPro
- **GameInitializer.cs** - Ensures proper initialization order of game systems
- **CutterItem.cs** - Defines item pickups for stat boosts
- **PlayerMovement.cs** - Smooth 2D character movement
- **CameraFollow.cs** - Camera that follows the player
- **CameraShake.cs** - Screen shake for impactful feedback

## Dynamic Growth Speed Control

The game now features a dynamic growth speed control system:
1. The `GrowthController` script provides a global speed multiplier for all ivy growth
2. IvyNode instances check the multiplier when calculating their growth intervals
3. This allows for gameplay mechanics like:
   - Gradually increasing difficulty by speeding up growth over time
   - Power-ups that temporarily slow ivy growth
   - Game events that trigger rapid growth bursts
   - Difficulty settings that adjust growth speed
4. Modify the `speedMultiplier` value at runtime to see immediate effects

## Player UI System

The game now includes a TextMeshPro-based UI system:
1. The `PlayerUI` script displays player's current level and finesse
2. Uses TextMeshPro for crisp, scalable text at any resolution
3. This helps players track their progression
4. The UI automatically updates when player stats change
5. Easy to extend with additional UI elements for more game mechanics

## Advanced Density Controls

The game features a sophisticated growth control system:
1. **Branch Decay** - Branching probability decreases with depth using `branchChance * pow(branchDecay, depth)`
2. **Controlled Side Branches** - Limits the maximum number of branches per node with `maxBranchSpawns`
3. **Probabilistic Leaf Spawning** - Leaves only appear based on `leafSpawnChance`
4. **Leaf Density Limits** - Controls maximum leaves per branch with `maxLeavesPerBranch`
5. **Continuous Growth** - Uses coroutines with dynamic interval adjustment

These controls allow you to fine-tune the visual density and difficulty progression of the ivy.

## Health Scaling System

The game features a dynamic difficulty scaling system:
1. Set up an AnimationCurve in both the IvyNode's `healthCurve` and XPManager's `xpCurve`
2. The ivy health increases with generation (depth)
3. The ivy segment health is determined by `healthCurve.Evaluate(depth)`
4. This allows difficulty to scale alongside player progression

## Game Over Condition

The game now includes a game over condition:
1. When ivy segments reach the protected center area
2. The CenterAreaProtection script detects this and triggers game over
3. This creates a defense objective for the player

## Performance Tips

- Limit `maxDepth` on your IvyNode to prevent excessive growth
- Adjust `branchDecay` to control exponential growth
- Use `maxBranchSpawns` to prevent excessive branch calculation
- Lower `leafSpawnChance` for better performance
- Consider object pooling for branches and leaves
- Turn off ivy segments that are far from the player
- Batch ivy segments with static batching when possible
- Increase `growthInterval` or decrease `speedMultiplier` if performance drops

## Controls

- WASD or Arrow Keys - Move the player
- Player will automatically damage ivy on contact based on finesse

## Progression System

The game features a simple rogue-lite progression system:
1. Destroy ivy segments to gain XP
2. Level up to increase your finesse (cutting power) and radius
3. Higher levels allow you to cut through ivy faster and across a larger area
4. Current level and finesse values are displayed on the UI

## Troubleshooting

- If you encounter a NullReferenceException in IvyCutterCollision, make sure:
  - The PlayerStats component is on the same GameObject as the IvyCutterCollision
  - The GameInitializer is properly set up with references
  - The execution order ensures XPManager is initialized before PlayerStats
- If ivy growth is too dense, increase `branchDecay` or decrease `maxBranchSpawns`
- If leaves appear in the wrong position, check the rotation values and parent-child relationships
- If growth speed is uneven, ensure GrowthController is properly assigned
- For UI issues, check that TextMeshPro components are properly assigned to PlayerUI
- For debugging, try adding Debug.Log statements to verify component initialization

Enjoy your Overgrown game! 