
# Waypoint System

### Description

This waypoint system lets game objects follow a path easily. With customizable parameters and support for instructions, it’s simple to set up and flexible to use.

---

### How to Use

1. Create a GameObject and attach the `WaypointAgent` component to it.  
2. Create another GameObject and attach the `Waypoint` component.  
3. Create a second waypoint, then set the first one’s `next` field to reference it. Also set the second waypoint’s `previous` field.  
4. A line should now appear between the two waypoints.  
5. Repeat step 3 to create a longer path.  
6. You can tweak line color, thickness, and the waypoint sphere size using the inspector.  
7. In the `WaypointAgent` GameObject, assign one of the waypoints to the `pathToFollow` field — this will be the starting point.  
8. If all offset options (`OffsetX`, `OffsetY`, `OffsetZ`) are false, the agent will snap directly onto the path. If one is true (e.g., `OffsetY`), the agent will follow the path while preserving its initial Y position.  
9. Adjust the `speed`, and enable `joinTrack` or `FollowWaypoint` depending on the behavior you want.  
10. Press Play — the agent should now follow the path.

---

### How to Use Curves

1. Add an `ICurve` component to a GameObject that already has a `Waypoint` component.  
2. This creates a curved path between the current and next waypoint.  
3. Adjust curve parameters as needed. To change smoothness, use the `CurveResolution` field in the waypoint.  
4. For **Bezier** and **CatmullRom** curves, you’ll need one extra waypoint **before** the curved waypoint and one **after** its `next`.

---

### How to Use Instructions

1. Check the `Instruction` script to see what each instruction type does.  
2. Look at the provided example script for how to use them in code.  
3. Add instructions to the agent’s list and call `ExecuteNextInstruction()` to start.

---

### Note

You can subscribe to the `OnObjectiveReached` event to know when the next instruction starts executing.  
Keep in mind: this event will also trigger at the beginning if `ExecuteFirstInstructionOnStart` is true and the instruction list is not empty.

Please put the script `WaypointEditorTool` in the editor folder. It enables you to make a chain of waypoints based on the distance between each other.

The demo scene is using unlit materials, to be able to show it on any rendering pipeline.

### Use Cases

Here are a few practical examples of how you can use the waypoint system:

- **Patrolling Guards**  
  Create a looped path and use `GoToInstructionIndex` to make the agent patrol endlessly.

- **Cutscene Movement**  
  Use `TravelSomeSeconds`, `ChangeSpeed`, and `WaitSeconds` to create timed movement between points in a cutscene.

- **Platform Pathing**  
  Attach the waypoint agent to a platform object and move it along a set of waypoints, optionally preserving its Y or Z position.

- **Cinematic Flythrough**  
  Use curves to create a smooth camera movement through waypoints with no abrupt turns.

- **Following NPCs**  
  Set multiple agents to start at different positions and follow the same path with offsets for group movement.