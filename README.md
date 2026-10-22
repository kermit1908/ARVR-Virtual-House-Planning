# AR/VR House Planning and Interior Design System

An interactive 3D, 2D, and Augmented Reality architectural design application built with **Unity 6 LTS**, **C#**, and **AR Foundation (ARCore)** for macOS (Apple Silicon native) and Android devices.

Designed for ₹0 budget using exclusively free, open-source, and legally permissible tooling.

---

## Table of Contents
1. [Project Overview](#project-overview)
2. [Key Features & Modules](#key-features--modules)
3. [Technology Stack & Package Compatibility](#technology-stack--package-compatibility)
4. [Prerequisites](#prerequisites)
5. [Installation & Setup on macOS (Apple Silicon)](#installation--setup-on-macos-apple-silicon)
6. [Automatic Scene Setup in Unity](#automatic-scene-setup-in-unity)
7. [Operating the Application](#operating-the-application)
8. [Android AR Build & Deployment Guide](#android-ar-build--deployment-guide)
9. [Optional Backend Service](#optional-backend-service)
10. [Project Architecture & Directory Structure](#project-architecture--directory-structure)
11. [Testing & Quality Assurance](#testing--quality-assurance)
12. [Troubleshooting & Known Limitations](#troubleshooting--known-limitations)

---

## 1. Project Overview
The **AR/VR House Planning System** enables users to design residential floor plans in 2D, immediately generate and explore realistic procedural 3D models of their home designs, decorate rooms using a 20-piece starter furniture catalogue with full transform and material customization, and visualize both miniature and 1:1 room-scale versions in the physical world via Augmented Reality.

---

## 2. Key Features & Modules

### Module A: Home Screen & Project Management
- Polished landing UI with quick navigation to all design modules.
- Create new projects, load existing designs, rename, and delete with confirmation dialogs.
- Real-time indicator displaying active project metadata and AR device readiness.

### Module B: 2D Interactive Floor Planner
- Top-down orthographic CAD-style floor canvas with pan, pinch/scroll zoom, and grid overlay.
- Drag-and-drop room sizing with automatic 4-wall perimeter generation and dimensions in meters.
- Custom wall placement, resizing, and opening cuts (doors and windows).
- Wall hit-testing, tolerance snapping, and dimensional measurements display.

### Module C: Procedural 3D House Generation
- Generates 3D house geometry at runtime from `ProjectData` without requiring paid 3D asset packs.
- Procedural mesh builder for floors, ceilings, doorframes, windowframes, and subdivided walls with cutouts.
- **Interior Viewing Mode**: Automatically removes ceiling obstructions and applies cutaway visibility so interior spaces are easily inspected without camera clipping.
- Tri-mode camera: **Orbit** (turntable inspection), **Fly** (WASD free camera), and **Walk** (1.7m human eye-level interior walk).

### Module D & E: Furniture Catalogue & Interior Customization
- Built-in 20-piece procedural starter catalogue (Sofas, Armchairs, Beds, Dining Tables, Chairs, Desks, Wardrobes, Bookshelves, Kitchen Counters, Refrigerators, Sinks, Toilets, Bathtubs, Lamps, TVs, Nightstands, Rugs).
- Click-to-place onto detected floor surfaces with real-time ghost preview and mouse-wheel rotation.
- Selected Object Inspector: Move, rotate (45° steps), uniform scale slider (0.2x–5.0x), duplicate, delete, and quick color swatch swapper.
- Runtime floor material switcher (Hardwood, Ceramic Tile, Marble, Carpet, Concrete).

### Module F & G: Android Augmented Reality (ARCore)
- Built on **Unity AR Foundation 6.1** + **ARCore XR Plugin 6.1**.
- Hardware capability checking and camera runtime permission management.
- Live camera background passthrough with real-time horizontal plane detection.
- Animated placement reticle indicator aligned to plane surface normals.
- Tap-to-place with AR Raycasting and AR Anchor attachment.
- Dual visualization modes:
  - **Miniature Mode (1:20 scale)**: Place a tabletop architectural model to inspect the whole home.
  - **Room-Scale Mode (1:1 scale)**: Inspect individual furniture items and rooms at true physical scale.

### Module H: Persistent Save & Load System
- Fully offline JSON serialization stored in `Application.persistentDataPath/Projects/`.
- Portable schema versioning (`schemaVersion: 1`) with forward-compatibility upgrader.
- Bundled sample project (`SampleProject.json`) featuring a 4-room L-shaped modern home ready to open immediately.

### Module I: Backend Integration (Optional)
- FastAPI (Python) + SQLite local REST server (`Backend/main.py`) for multi-device sync or cloud backup.
- Complete CRUD API endpoints (`/api/projects`, `/api/health`).
- Unity `BackendApiClient` with automatic fallback to offline mode when server is unavailable.

---

## 3. Technology Stack & Package Compatibility

| Component | Target Version | Compatibility Notes |
|---|---|---|
| **Unity Editor** | Unity 6 LTS (`6000.0.x` or `6000.3.x` Apple Silicon ARM64) | Native M1/M2/M3/M4 performance |
| **AR Foundation** | `com.unity.xr.arfoundation` 6.1.0 | Core XR subsystem management |
| **ARCore XR Plugin** | `com.unity.xr.arcore` 6.1.0 | Must match AR Foundation major/minor |
| **XR Core Utils** | `com.unity.xr.core-utils` 2.3.0 | XROrigin component support |
| **Graphics API** | Vulkan (preferred) / OpenGLES3 | Linear Color Space recommended |
| **Input System** | `com.unity.inputsystem` 1.8.2 | Touch & mouse unified handling |
| **UI Framework** | Unity uGUI (`com.unity.ugui` 2.0.0) | Canvas & EventSystem |
| **Backend** | Python 3.9+ / FastAPI / SQLite | Zero external dependencies |

---

## 4. Prerequisites
- **Computer**: MacBook with Apple Silicon (M1/M2/M3/M4) running macOS Monterey 12.0 or newer.
- **Unity Hub**: Free official download.
- **Code Editor**: Visual Studio Code (free) or standard text editor.
- **Android Phone** (for AR testing): Android 7.0+ (API level 24+) supporting Google Play Services for AR (ARCore).
- **USB Cable**: For device deployment and debugging.

---

## 5. Installation & Setup on macOS (Apple Silicon)

### Step 1: Install Unity Hub
Download and install Unity Hub from the official Unity website:
[https://unity.com/download](https://unity.com/download)

### Step 2: Install Unity 6 LTS (Apple Silicon ARM64)
1. Open **Unity Hub**.
2. Navigate to **Installs** → **Install Editor**.
3. Select **Unity 6 LTS** (e.g. `6000.0.x`).
4. **Important**: Under Install Modules, ensure the following are selected:
   - **Android Build Support**
   - **Android SDK & NDK Tools**
   - **OpenJDK**
5. Complete installation.

### Step 3: Open Project in Unity
1. In Unity Hub, click **Add** → **Add project from disk**.
2. Select `/Users/nikhil/Downloads/Mini_Project`.
3. Unity will open using the native Apple Silicon editor and resolve all packages listed in `Packages/manifest.json`.

---

## 6. Automatic Scene Setup in Unity

You do not need to manually create GameObjects, attach components, or build UI trees by hand.

Once the project opens in Unity:
1. Locate the top menu bar item: **House Planner**
2. Click **House Planner → 1. Setup All Scenes & Hierarchy**
   - This automatically creates all 4 scenes (`MainMenu.unity`, `FloorPlanner.unity`, `HouseViewer.unity`, `ARView.unity`).
   - Configures Cameras, Directional Lights, UI Canvases, EventSystems, Controllers, ARSession, and XROrigin.
   - Registers all 4 scenes in **Build Settings**.
3. Click **House Planner → 2. Create Default Materials & Assets**
4. Click **House Planner → Validate Project Health** to verify that all systems are operational.

---

## 7. Operating the Application

### 2D Floor Planner
- **Draw Room**: Click tool, then click & drag anywhere on the canvas to define room dimensions.
- **Add Wall**: Click tool, click starting point, click ending point.
- **Add Door / Window**: Click tool, click on any existing wall segment to place an opening.
- **Select / Move**: Click any room to open its properties panel on the right (modify width, length, name, or delete).
- **Zoom & Pan**: Use mouse wheel to zoom in/out; middle-click drag to pan the viewport.

### 3D House Viewer
- **Orbit Camera**: Right-click drag (or single-finger drag on mobile) to orbit around the house.
- **Fly Camera**: Press `WASD` to move, `E`/`Q` for elevation, right-click drag to look around.
- **Walk Camera**: First-person interior inspection locked at 1.7m eye-level.
- **Furniture Placement**: Click **Furniture Catalogue** → select an item (e.g., Sofa, Bed) → hover over floor → click to place.
- **Inspector Panel**: Click placed furniture to rotate, scale with the slider, or change colors.

### Augmented Reality Mode
1. Ensure you are in a well-lit room.
2. Slowly move the phone camera across the floor to detect horizontal surfaces.
3. The green reticle indicator will appear on detected floor planes.
4. Tap the screen to place the miniature (1:20) or 1:1 house model.
5. Use the on-screen slider to scale and rotate buttons to orient the design.

---

## 8. Android AR Build & Deployment Guide

### Configure Project Settings
1. Open **File → Build Settings...**
2. Select **Android** platform and click **Switch Platform**.
3. Click **Player Settings...** (bottom left):
   - **Company Name**: `MyCompany`
   - **Product Name**: `ARHousePlanner`
   - **Package Name**: `com.mycompany.arhouseplanner`
   - **Minimum API Level**: `Android 7.0 'Nougat' (API Level 24)`
   - **Target API Level**: `Android 14 (API Level 34)` or `Automatic`
   - **Scripting Backend**: `IL2CPP`
   - **Target Architectures**: Check `ARM64` (uncheck ARMv7)
4. Go to **XR Plug-in Management** (left sidebar):
   - Switch to the **Android** tab (robot icon).
   - Check **Google ARCore**.

### Connect Android Phone & Build
1. On your Android phone, enable **Developer Options**:
   - Go to **Settings → About Phone** → tap **Build Number** 7 times.
   - Go to **Settings → Developer Options** → enable **USB Debugging**.
2. Connect your phone to your Mac via USB cable.
3. Accept the RSA debugging prompt on your phone screen.
4. In Unity, open **File → Build Settings...**
5. Select your connected phone under **Run Device**.
6. Click **Build And Run** (choose a filename like `ARHousePlanner.apk`).
7. Unity compiles and installs the application directly onto your Android device.

---

## 9. Optional Backend Service

To start the optional local FastAPI backend:
```bash
cd /Users/nikhil/Downloads/Mini_Project/Backend
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --reload --port 8000
```
Interactive API docs are available at: `http://localhost:8000/docs`.

---

## 10. Project Architecture & Directory Structure

```
Mini_Project/
├── Assets/
│   ├── Scenes/               # MainMenu, FloorPlanner, HouseViewer, ARView
│   ├── Scripts/
│   │   ├── Core/             # Data models (ProjectData, RoomData, WallData, FurnitureItemData)
│   │   ├── FloorPlan/        # 2D planner controller, renderer, camera, wall/room tools
│   │   ├── HouseGeneration/  # Procedural mesh builders, material manager, 3D camera
│   │   ├── Furniture/        # Catalogue, procedural models, placement controller, handles
│   │   ├── AR/               # ARCore controller, capability checker, placement indicator
│   │   ├── Persistence/      # JSON serializer, schema upgrader, sample project generator
│   │   ├── Backend/          # UnityWebRequest REST client
│   │   ├── UI/               # Menus, inspectors, dialogues, navigation
│   │   └── Editor/           # Automated scene generator & project validator
│   ├── Resources/            # SampleProject.json
│   ├── Tests/EditMode/       # NUnit tests for core geometry and data logic
│   └── Packages/             # Package manifest configuring AR Foundation 6.1
├── Backend/                  # FastAPI + SQLite local server
├── README.md                 # Complete system guide
├── IMPLEMENTATION_STATUS.md  # Detailed module completion report
└── TESTING.md                # QA test cases and validation report
```

---

## 11. Testing & Quality Assurance
Run automated tests inside Unity:
1. Open **Window → General → Test Runner**.
2. Select **EditMode** tab.
3. Click **Run All** to execute tests in `CoreDataTests`, `PersistenceTests`, and `GeometryTests`.

---

## 12. Troubleshooting & Known Limitations
- **Camera Feed Black in AR**: Ensure camera permission was granted when prompted on the phone. Check that Google Play Services for AR (ARCore) is updated from Google Play Store.
- **Vulkan Shimmering**: If Vulkan exhibits rendering artifacts on older Mali GPU devices, switch Graphics API to OpenGLES3 in Player Settings.
- **Room Scale in Small Spaces**: Full-scale (1:1) house models require large outdoor or warehouse environments; use **Miniature Mode (1:20)** for desktop and tabletop AR exploration.
