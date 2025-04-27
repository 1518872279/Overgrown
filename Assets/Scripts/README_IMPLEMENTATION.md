# Ivy Growth & Push-Back System Implementation Guide

This guide provides instructions for setting up the ivy growth and push-back mechanics in your Unity project.

## 1. Component Setup

### GrowthManager
- Attach to a GameObject at the center of your play area
- Configure:
  - `ivyRootPrefab`: A prefab with the `IvyNode` component attached
  - `rootCount`: Number of root points around the perimeter (default: 8)
  - `spawnRadius`: Distance from center to spawn roots (default: 10f)
  - `centerRadius`: Protected center area radius (default: 1f)

### GrowthController
- Controls the global growth speed of all ivy
- Configure:
  - `tickInterval`: Base time between growth cycles (default: 0.5f)
  - `speedMultiplier`: Multiplier to increase/decrease growth speed (default: 1f)

### Player Setup
1. Attach `PlayerStats` to your player GameObject
   - Configure base finesse, finesse per level, push force, etc.
2. Attach `IvyCutterCollision` to the same GameObject
   - Ensure a CircleCollider2D is present (will be auto-added if missing)
3. Make sure `XPManager` is present in your scene

### Branch/Ivy Prefabs
1. Create a root prefab with:
   - `IvyNode` component configured with:
     - Branch prefab
     - Leaf prefab (optional)
     - Growth parameters
   - Set appropriate tags ("IvySegment")

2. Create a branch segment prefab with:
   - `IvySegment` component for health tracking
   - Collider for collision detection
   - Appropriate visuals

## 2. Protected Center Area

1. Add a GameObject with:
   - `CircleCollider2D` (set as trigger)
   - `CenterAreaProtection` component
   - Assign a `GameOverHandler` to trigger game end

## 3. System Integration Notes

### Player vs. Ivy Interaction
- Player damages branches by staying near them
- When branch health depletes, it's pushed back (not destroyed)
- Branches reset health after knockback and continue growing

### XP and Progression
- Configure `XPManager` with your desired XP curve
- Player stats (finesse, push force) increase with level

### Performance Optimization
- Consider using object pooling for branches and leaves
- Spatial partitioning can improve collision performance
- Set appropriate `maxActiveSegments` in IvyNode to limit total active branches

## 4. Required Components Checklist

- [x] GrowthManager.cs
- [x] GrowthController.cs
- [x] IvyNode.cs (for individual branch node behaviors)
- [x] IvyCutterCollision.cs (for player interaction)
- [x] PlayerStats.cs (for player progression)
- [x] XPManager.cs (for XP accumulation and leveling)
- [x] CenterAreaProtection.cs (for center area breach detection)
- [x] Supporting scripts: PlayerMovement, CameraFollow, etc.

## 5. Compatibility Notes

This system is designed to work with a roguelite progression system where:
- The player pushes back encroaching ivy
- The ivy constantly grows toward the center
- The player gains experience and becomes more effective
- Game ends if ivy reaches the protected center

---

For any questions or issues, refer to the detailed script documentation or contact the development team. 