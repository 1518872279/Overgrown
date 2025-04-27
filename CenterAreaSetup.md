# Center Area Protection Setup Guide

Follow these steps to properly set up the center area protection system:

## 1. Set Up GameManager

1. Create an empty GameObject named "GameManager" in your scene
2. Add the `GameManager` script to it
3. Keep it at position (0,0,0) for simplicity

## 2. Create Game Over UI

1. In your Canvas, create a new Panel named "GameOverPanel"
2. Set its visibility to off initially
3. Add the following UI elements to the panel:
   - Text - "GAME OVER" (large font)
   - Text - Description (for displaying the reason)
   - Text - Score (for displaying level/XP)
   - Button - "Restart"
   - Button - "Quit"
4. Add the `GameOverUI` script to the panel
5. Assign the UI elements to the appropriate fields in the inspector
6. Assign the GameOverPanel to the GameManager's `gameOverPanel` field

## 3. Set Up Center Area

1. Create an empty GameObject named "CenterArea" at the center of your play area
2. Add a `CircleCollider2D` component
   - Set "Is Trigger" to true
   - Set the radius to define your protected area (e.g., 1.5)
3. Add the `CenterAreaProtection` script
4. Configure the settings:
   - Set `segmentsToTriggerGameOver` (default is 1)
   - Enable `showProtectionArea` to visualize the area
   - Adjust colors as needed

## 4. Check IvySegment Tags

1. Make sure your ivy segment prefab has the tag "IvySegment"
2. If not, add this tag in the Tags & Layers settings
3. Assign the tag to your ivy segment prefab

## 5. Testing

1. Play the game and ensure ivy is growing
2. When ivy touches the center area, it should:
   - Change color to show intrusion
   - Trigger game over when enough segments enter
   - Display the game over UI
   - Allow restart or quit

## Troubleshooting

If the center area protection isn't working:

1. **Check Tags**: Ensure ivy segments have the "IvySegment" tag
2. **Check Colliders**: Make sure both the ivy segments and center area have colliders
3. **Check Is Trigger**: Both colliders should have "Is Trigger" enabled
4. **Check Physics 2D Settings**: Make sure the layers can collide in the Physics2D settings
5. **Debug with Gizmos**: Use the Scene view and turn on Gizmos to see colliders
6. **Add Debug Logs**: Add temporary Debug.Log statements to OnTriggerEnter2D to verify collisions

## Advanced Configuration

- Increase `segmentsToTriggerGameOver` to make the game more forgiving
- Adjust the center area color and opacity for better visibility
- Consider adding audio feedback when ivy enters the protected area 