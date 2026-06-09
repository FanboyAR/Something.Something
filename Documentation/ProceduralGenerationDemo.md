# Procedural Generation Demo Scene

This scene showcases the procedural maze generation system of the game.

## What's Included

- **ProceduralGenerationDemo.cs** - Main demo controller script with features to manage and regenerate the maze
- **ProceduralGenerationDemoSetup.cs** - Editor helper script for quick scene setup
- **ProceduralGenerationDemo.unity** - Pre-configured demo scene

## Setup Instructions

### Quick Setup Method (Recommended)

1. Open the **ProceduralGenerationDemo** scene in Unity
2. Create an empty GameObject in the scene (if not already present)
3. Add the **ProceduralGenerationDemoSetup** script to it
4. In the Inspector, right-click on the script component and select **Setup Demo Scene**
5. The script will automatically:
   - Create/configure GameManager
   - Create/configure MazeGenerator
   - Create/configure Player with PlayerController and Camera
   - Create FPSCamera on the camera holder
   - Setup ground plane and lighting
   - Create ProceduralGenerationDemo controller

### Manual Setup Method

If the automated setup doesn't work for any reason:

1. Create the following GameObjects in your scene:
   - `GameManager` - Add GameManager component
   - `MazeGenerator` - Add MazeGenerator component
   - `Player` - Add PlayerController and CharacterController components
     - Child: `CameraHolder` - Add Camera and FPSCamera components
   - `Directional Light` - Add Light component (type: Directional)
   - `Ground` - Add a plane or cube for reference

2. Link references:
   - GameManager → MazeGenerator reference
   - PlayerController → CameraHolder transform reference
   - ProceduralGenerationDemo → All above components

3. Add ProceduralGenerationDemo component to manage the demo

## Controls

When the demo is running:

- **WASD** - Move forward/backward/left/right
- **Mouse** - Look around (vertical and horizontal rotation)
- **Space** - Jump
- **Shift** - Run (faster movement)
- **R** - Regenerate maze with a new random seed
- **H** - Display help text in console

## Demo Settings

The ProceduralGenerationDemo script has the following settings:

- **Auto Start Generation** - Whether to start generating on play
- **Demo Seed** - Fixed seed for reproducible generation (set to 0 for random)

## Features

### Procedural Generation
- The MazeGenerator creates a unique maze layout each time
- Uses seed-based generation for reproducibility
- Seeds are logged to console for reference

### Controllable Regeneration
- Press **R** during gameplay to generate a new maze with a different seed
- Each regeneration creates a completely unique layout

### Player Control
- First-person camera perspective
- Full 3D movement with gravity and jumping
- Mouse sensitivity adjustable in PlayerController settings

## Extending the Demo

To enhance the demo:

1. **Add Room Generation** - Implement the `GenerateAroundPlayer()` method in MazeGenerator to create rooms dynamically around the player
2. **Add Hazards** - Use HazardManager to spawn obstacles and challenges
3. **Add Puzzles** - Use PuzzleManager to add interactive elements
4. **Add NPCs** - Spawn enemies or interactive characters in the generated maze
5. **Add UI** - Create a UI overlay showing current seed, room count, etc.

## Troubleshooting

### Scene doesn't load properly
- Make sure all script references are assigned in the Inspector
- Check that the namespace is `TheCube` (all scripts use this namespace)
- Verify that MazeGenerator is referenced by GameManager

### Player can't move
- Ensure PlayerController has a CharacterController component on the same GameObject
- Check that the CameraHolder transform is assigned in PlayerController
- Verify Input Manager has WASD and mouse axis configured

### No directional light visible
- Add a Directional Light GameObject to the scene
- Adjust its rotation to desired angle (default is 50, -27, 0 degrees)
- Set intensity to 1 or higher

### Console shows errors about missing components
- Re-run the ProceduralGenerationDemoSetup script
- Or manually add all required components as described in Manual Setup

## Performance Tips

- For large mazes, consider using object pooling for room generation
- Use LOD (Level of Detail) for distant maze sections
- Consider implementing frustum culling for better performance
- Monitor memory usage when generating large procedural layouts

## Future Enhancements

Potential improvements to the demo:

1. Real-time maze visualization with minimap
2. Performance metrics display (generation time, FPS, memory)
3. Multiple generation algorithms to choose from
4. Difficulty settings that affect maze complexity
5. Save/load functionality for favorite mazes
6. Multiplayer support with network generation
7. Mobile controls support
8. VR support for immersive maze exploration
