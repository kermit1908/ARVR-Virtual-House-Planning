# QA Testing & Verification Guide: AR/VR House Planning System

This document outlines the testing protocols, automated test suites, and manual verification plans for the AR/VR House Planning system.

---

## 1. Automated Test Suite (Unity Test Framework)

All unit tests are located in `Assets/Tests/EditMode/` and use NUnit through the Unity Test Framework.

### Test Categories

| Test Class | Focus Area | Assertions |
|---|---|---|
| `CoreDataTests.cs` | Data Models & Logic | Validates ID generation, area computation, perimeter calculation, rejection of negative/zero dimensions, vector serialisation conversions, and furniture data models. |
| `PersistenceTests.cs` | Storage & Schema | Validates JSON roundtrip serialization/deserialization, ensures room/furniture counts match after load, verifies schema version preservation, and tests `SchemaVersionUpgrader`. |
| `GeometryTests.cs` | Procedural Meshes | Validates vertex and triangle count on floor quads, verifies wall mesh dimensions, verifies subdivided quads created for door/window openings, and tests ceiling mesh generation. |

### How to Run Automated Tests
1. In the Unity Editor menu bar, navigate to **Window → General → Test Runner**.
2. Select the **EditMode** tab.
3. Click **Run All** (or select individual test fixtures).
4. Verify all tests pass with green checkmarks.

---

## 2. Manual Test Procedures

### Test Plan 1: 2D Floor Planner
1. Launch the application and click **Create New Project** → name it `"Test Villa"`.
2. Select **Draw Room** tool.
3. Click and drag on the grid to create a rectangular room (e.g. 6m × 5m).
   - *Expected Result*: Room rectangle appears with 4 perimeter walls, area label, and dimension lines.
4. Select **Add Door** tool and click along the bottom wall.
   - *Expected Result*: Door opening is placed and indicated on the wall.
5. Select **Add Window** tool and click along the right wall.
   - *Expected Result*: Window opening is placed.
6. Click **Save** button.
   - *Expected Result*: Green status message confirms successful save to local storage.

### Test Plan 2: 3D House Viewer & Cameras
1. In the Floor Planner, click **View 3D**.
   - *Expected Result*: Procedural 3D house is generated instantly showing floors, walls with cutouts, and door/window frames.
2. Verify **Orbit Mode**:
   - Right-click drag: camera orbits smoothly around house pivot.
   - Mouse scroll: camera zooms in and out within clamped boundaries (1.5m to 35m).
3. Switch to **Fly Mode**:
   - Press `W`, `A`, `S`, `D` and use right-click mouse look to fly freely inside rooms.
   - Press `E` to ascend, `Q` to descend.
4. Switch to **Walk Mode**:
   - Verify camera is constrained to 1.7m eye-level.
5. Toggle **Interior View Mode**:
   - Ceilings are hidden, allowing unobstructed top-down viewing of the interior.

### Test Plan 3: Furniture Placement & Customization
1. In the 3D Viewer, click **Furniture Catalogue**.
2. Switch category tabs (Seating, Beds, Kitchen, etc.) and verify items load.
3. Select **Sofa**:
   - Ghost preview follows mouse cursor across the floor plane.
   - Mouse wheel rotates ghost preview in 15° increments.
   - Click to place on floor.
4. Click on the placed sofa:
   - Object Inspector panel opens on the right.
   - Test rotation buttons (±45°).
   - Adjust scale slider from 0.5x to 2.0x.
   - Click the Blue color swatch: Sofa cushion color updates immediately.
5. Click **Duplicate**:
   - A copy of the sofa spawns offset by 0.5m with identical properties.
6. Click **Delete**:
   - Sofa is removed from scene and project data.

### Test Plan 4: Persistent Save & Re-open
1. Exit to **Main Menu**.
2. Click **Open Existing Project**.
   - Verify `"Test Villa"` appears in the list with correct room and furniture counts.
3. Click **Open**:
   - Project loads into the Floor Planner with all room boundaries and walls intact.
4. Switch to 3D Viewer:
   - Placed furniture items and custom colors appear exactly as previously configured.

### Test Plan 5: Android Augmented Reality (Physical Device)
1. Build and install the APK on a compatible ARCore Android phone.
2. Launch the app and tap **AR Room Preview**.
3. Accept the Camera runtime permission prompt when requested.
4. Point phone towards a flat floor area and move slowly in a circular motion:
   - *Expected Result*: Yellow/white feature dots and a green circular reticle appear on the floor.
5. Tap the reticle:
   - *Expected Result*: Miniature (1:20 scale) house model spawns anchored firmly to the floor.
6. Walk around the virtual house:
   - *Expected Result*: Model remains stable and does not drift.
7. Use on-screen rotation buttons to turn the house.
8. Use scale slider to expand to 1:1 scale for room immersion.
9. Tap **Reset Placement** to clear and place again.

---

## 3. Edge Cases & Error Handling

| Scenario | Tested Handling |
|---|---|
| Non-AR Device Launched into AR | `ARCapabilityChecker` detects lack of ARCore support and displays a friendly dialog suggesting 3D Viewer instead of crashing. |
| Camera Permission Denied | Application catches missing permission, shows guidance message, and stays in menu. |
| Negative or Zero Room Dimensions | `RoomData.Validate()` logs warning and prevents corrupted project saving. |
| Wall Start equals End (0 Length) | `WallData.Validate()` rejects degenerate wall segments. |
| Backend Server Offline | `BackendApiClient` times out gracefully after 5s and falls back to local JSON storage with zero user disruption. |
| Corrupted JSON Project File | `ProjectPersistenceService` catches deserialization exceptions, logs error, and returns null safely. |
