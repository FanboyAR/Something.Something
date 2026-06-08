# Procedurally Generated 3D Maze Game — Game Design

Working Title
=============

**The Cube**

Premise
-------

The player controls a cube (or guides a cube) through an endless series of procedurally generated 3D maze facilities. Each maze is unique and contains puzzles, hazards, and alternate routes. The objective is to reach the exit while overcoming obstacles that become increasingly complex.

Core Gameplay
-------------

Main Loop

1. Enter a generated maze.
2. Explore and locate the exit.
3. Solve puzzles to unlock pathways.
4. Avoid hazards and enemies.
5. Reach the exit.
6. Generate a larger, more difficult maze.
7. Repeat.

Perspective Options

- First-Person: Similar to Portal or Escape Simulator. Most immersive.
- Third-Person: Camera follows behind the cube. Easier to see puzzle elements.

Recommendation: First-person works best if the player is a human guiding the cube. Third-person works best if the player actually controls the cube itself.

Maze Generation
---------------

Structure

The maze is generated from modular rooms. Instead of generating individual walls, generate room pieces:

- Corridors
- Junctions
- Dead ends
- Puzzle rooms
- Vertical shafts
- Large chambers

Generation Process

Generate Main Path -> Generate Branches -> Place Exit -> Place Puzzle Rooms -> Place Hazards -> Place Collectibles -> Validate Completion

The system always guarantees at least one solution.

Room Types

- Standard Corridor
- Puzzle Room
- Challenge Room
- Treasure Room
- Elevator Room
- Observation Room

Multi-Level Mazes
-----------------

Players navigate up and down using elevators, lifts, moving platforms, and gravity tubes.

Puzzle Systems
--------------

- Pushable Cubes
- Energy Cores
- Laser Reflection
- Weight Platforms
- Circuit Puzzles

Hazards
-------

- Spinning Blades
- Laser Grids
- Crushing Walls
- Falling Floors
- Security Drones

Dynamic Events
--------------

- Power Outage
- Maze Reconfiguration
- Security Alert
- Flooding

Progression
-----------

Each completed maze increases difficulty and size.

Themes
------

- Research Facility
- Abandoned Factory
- Underground Bunker
- Alien Structure
- Digital Simulation

Replayability
-------------

- Random Seeds (players can share seeds)
- Daily Challenge
- Endless Mode

Unique Twist
------------

The maze builds itself around the player as they move. Unexplored sections do not exist yet. This allows infinite mazes and smaller memory usage while keeping unpredictability.

Unity Implementation
--------------------

Core Systems to implement in Unity:

- `MazeGenerator` — Generates room layout and handles seed logic.
- `RoomManager` — Spawns and recycles room prefabs.
- `PuzzleManager` — Manages puzzle lifecycle and interactions.
- `HazardManager` — Spawns and configures hazards.
- `SaveSystem` — Persistent data storage for seeds, progress, and settings.
- `GameManager` — Overall flow (start run, end run, difficulty scaling).

Notes & Next Steps
------------------

- Project scaffold is created; open the folder in Unity to let the Editor generate full `ProjectSettings/` and `Library/`.
- I added skeleton C# scripts under `Assets/Scripts/` for the main systems. These are minimal and ready to expand.
- Recommended immediate tasks: choose perspective, create room prefabs, and implement the streaming generator prototype that spawns rooms as the player moves.

