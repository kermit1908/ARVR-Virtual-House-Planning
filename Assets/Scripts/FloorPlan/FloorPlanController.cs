using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.FloorPlan
{
    /// <summary>
    /// Central controller for the 2D floor plan editing interface.
    /// Manages project data, editing modes, selection state, and coordinates
    /// between tools, renderer, and camera.
    /// </summary>
    public class FloorPlanController : MonoBehaviour
    {
        #region Editing Mode

        /// <summary>
        /// Enumerates the available editing modes in the floor plan editor.
        /// </summary>
        public enum EditingMode
        {
            /// <summary>Select and inspect existing objects.</summary>
            Select,
            /// <summary>Draw new wall segments.</summary>
            DrawWall,
            /// <summary>Place a new room rectangle.</summary>
            PlaceRoom,
            /// <summary>Place a door opening on a wall.</summary>
            PlaceDoor,
            /// <summary>Place a window opening on a wall.</summary>
            PlaceWindow,
            /// <summary>Move an existing object.</summary>
            MoveObject,
            /// <summary>Resize an existing object.</summary>
            ResizeObject
        }

        #endregion

        #region Events

        /// <summary>Raised when a room is selected. Passes the selected RoomData (null if deselected).</summary>
        public event Action<RoomData> OnRoomSelected;

        /// <summary>Raised when a new room is created and added to the project.</summary>
        public event Action<RoomData> OnRoomCreated;

        /// <summary>Raised when an existing room's data is modified.</summary>
        public event Action<RoomData> OnRoomModified;

        /// <summary>Raised when a wall is selected. Passes the selected WallData (null if deselected).</summary>
        public event Action<WallData> OnWallSelected;

        /// <summary>Raised when the editing mode changes. Passes the new mode.</summary>
        public event Action<EditingMode> OnModeChanged;

        /// <summary>Raised before a room is removed, allowing listeners to cancel (return false).</summary>
        public event Func<RoomData, bool> OnRoomRemoveConfirm;

        /// <summary>Raised after a room is removed.</summary>
        public event Action<string> OnRoomRemoved;

        /// <summary>Raised when any object is selected by ID.</summary>
        public event Action<string> OnObjectSelected;

        /// <summary>Raised when all selections are cleared.</summary>
        public event Action OnSelectionCleared;

        #endregion

        #region Serialized Fields

        [Header("References")]
        [SerializeField, Tooltip("Camera used for the 2D top-down floor plan view.")]
        private Camera _floorPlanCamera;

        [SerializeField, Tooltip("Floor plan renderer component.")]
        private FloorPlanRenderer _renderer;

        [SerializeField, Tooltip("Floor plan camera controller.")]
        private FloorPlanCamera _cameraController;

        [SerializeField, Tooltip("Room creation tool.")]
        private RoomCreationTool _roomCreationTool;

        [SerializeField, Tooltip("Wall editor tool.")]
        private WallEditorTool _wallEditorTool;

        [Header("Grid Settings")]
        [SerializeField, Tooltip("Size of each grid cell in meters.")]
        private float _gridSize = 0.5f;

        [SerializeField, Tooltip("Whether to snap operations to the grid.")]
        private bool _snapToGrid = true;

        [Header("UI References")]
        [SerializeField, Tooltip("Root canvas for floor plan UI overlays.")]
        private Canvas _uiCanvas;

        [SerializeField, Tooltip("Panel displaying properties of the selected object.")]
        private GameObject _propertiesPanel;

        [SerializeField, Tooltip("Text element showing the current editing mode.")]
        private UnityEngine.UI.Text _modeLabel;

        [SerializeField, Tooltip("Text element showing cursor coordinates.")]
        private UnityEngine.UI.Text _coordinatesLabel;

        [Header("Undo / Redo")]
        [SerializeField, Tooltip("Maximum number of undo steps to retain.")]
        private int _maxUndoSteps = 50;

        #endregion

        #region Public Properties

        /// <summary>The project currently being edited.</summary>
        public ProjectData CurrentProject { get; private set; }

        /// <summary>The current editing mode.</summary>
        public EditingMode CurrentMode { get; private set; } = EditingMode.Select;

        /// <summary>The currently selected room, or null.</summary>
        public RoomData SelectedRoom { get; private set; }

        /// <summary>The currently selected wall, or null.</summary>
        public WallData SelectedWall { get; private set; }

        /// <summary>The ID of the currently selected generic object, or null.</summary>
        public string SelectedObjectId { get; private set; }

        /// <summary>Grid cell size in meters.</summary>
        public float GridSize => _gridSize;

        /// <summary>Whether grid snapping is enabled.</summary>
        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => _snapToGrid = value;
        }

        /// <summary>Whether an undo operation is available.</summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Whether a redo operation is available.</summary>
        public bool CanRedo => _redoStack.Count > 0;

        #endregion

        #region Private Fields

        private readonly Stack<IEditorCommand> _undoStack = new Stack<IEditorCommand>();
        private readonly Stack<IEditorCommand> _redoStack = new Stack<IEditorCommand>();
        private int _roomCounter;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_floorPlanCamera == null)
            {
                _floorPlanCamera = Camera.main;
                Debug.LogWarning("[FloorPlanController] No camera assigned; falling back to Camera.main.");
            }

            if (_floorPlanCamera != null)
            {
                _floorPlanCamera.orthographic = true;
            }
        }

        private void Start()
        {
            if (CurrentProject == null)
            {
                Debug.Log("[FloorPlanController] No project loaded. Creating default project.");
                LoadProject(CreateDefaultProject());
            }
        }

        private void Update()
        {
            UpdateCoordinatesLabel();
            HandleKeyboardShortcuts();
        }

        #endregion

        #region Project Management

        /// <summary>
        /// Loads a project into the editor, resetting all state.
        /// </summary>
        /// <param name="project">The project data to load.</param>
        public void LoadProject(ProjectData project)
        {
            if (project == null)
            {
                Debug.LogError("[FloorPlanController] Cannot load a null project.");
                return;
            }

            CurrentProject = project;
            _roomCounter = project.rooms.Count;
            DeselectAll();
            ClearCommandHistory();

            if (_cameraController != null)
            {
                _cameraController.FocusOnAll();
            }

            Debug.Log($"[FloorPlanController] Loaded project '{project.projectName}' with {project.rooms.Count} rooms.");
        }

        /// <summary>
        /// Creates a default empty project.
        /// </summary>
        /// <returns>A new <see cref="ProjectData"/> with default settings.</returns>
        public ProjectData CreateDefaultProject()
        {
            return new ProjectData
            {
                projectId = Guid.NewGuid().ToString(),
                projectName = "New House Plan",
                createdAt = DateTime.UtcNow.ToString("o"),
                modifiedAt = DateTime.UtcNow.ToString("o"),
                rooms = new List<RoomData>()
            };
        }

        #endregion

        #region Coordinate Conversion

        /// <summary>
        /// Converts a screen-space position to floor plan coordinates (meters).
        /// </summary>
        /// <param name="screenPos">Screen position in pixels.</param>
        /// <returns>Position in floor plan space (meters).</returns>
        public Vector2 ScreenToFloorPlan(Vector2 screenPos)
        {
            if (_floorPlanCamera == null)
            {
                Debug.LogError("[FloorPlanController] Cannot convert coordinates: no camera assigned.");
                return Vector2.zero;
            }

            Vector3 worldPos = _floorPlanCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, _floorPlanCamera.nearClipPlane));

            // Floor plan uses X/Z; camera looks down Y.
            return new Vector2(worldPos.x, worldPos.z);
        }

        /// <summary>
        /// Converts a floor plan position (meters) to world space.
        /// </summary>
        /// <param name="planPos">Position in floor plan coordinates.</param>
        /// <returns>World-space position at ground level (Y=0).</returns>
        public Vector3 FloorPlanToWorld(Vector2 planPos)
        {
            return new Vector3(planPos.x, 0f, planPos.y);
        }

        /// <summary>
        /// Snaps a floor plan position to the nearest grid point if snapping is enabled.
        /// </summary>
        /// <param name="position">The raw position.</param>
        /// <returns>The snapped (or original) position.</returns>
        public Vector2 SnapPosition(Vector2 position)
        {
            if (!_snapToGrid || _gridSize <= 0f)
                return position;

            return new Vector2(
                Mathf.Round(position.x / _gridSize) * _gridSize,
                Mathf.Round(position.y / _gridSize) * _gridSize
            );
        }

        #endregion

        #region Editing Mode

        /// <summary>
        /// Sets the current editing mode.
        /// </summary>
        /// <param name="mode">The mode to switch to.</param>
        public void SetMode(EditingMode mode)
        {
            if (CurrentMode == mode) return;

            EditingMode previous = CurrentMode;
            CurrentMode = mode;

            // Deselect when switching modes.
            if (mode != EditingMode.Select)
            {
                DeselectAll();
            }

            if (_modeLabel != null)
            {
                _modeLabel.text = mode.ToString();
            }

            Debug.Log($"[FloorPlanController] Mode changed: {previous} → {mode}");
            OnModeChanged?.Invoke(mode);
        }

        #endregion

        #region Room Management

        /// <summary>
        /// Adds a room to the current project.
        /// </summary>
        /// <param name="room">The room data to add.</param>
        public void AddRoom(RoomData room)
        {
            if (room == null)
            {
                Debug.LogError("[FloorPlanController] Cannot add a null room.");
                return;
            }

            if (CurrentProject == null)
            {
                Debug.LogError("[FloorPlanController] No project loaded.");
                return;
            }

            if (string.IsNullOrEmpty(room.roomId))
            {
                room.roomId = Guid.NewGuid().ToString();
            }

            var command = new AddRoomCommand(this, room);
            ExecuteCommand(command);

            Debug.Log($"[FloorPlanController] Room added: '{room.roomName}' ({room.roomId})");
            OnRoomCreated?.Invoke(room);
            MarkProjectModified();
        }

        /// <summary>
        /// Removes a room from the current project by ID after confirmation.
        /// </summary>
        /// <param name="roomId">The unique identifier of the room to remove.</param>
        /// <returns>True if the room was removed; false if not found or cancelled.</returns>
        public bool RemoveRoom(string roomId)
        {
            if (CurrentProject == null)
            {
                Debug.LogError("[FloorPlanController] No project loaded.");
                return false;
            }

            RoomData room = CurrentProject.rooms.Find(r => r.roomId == roomId);
            if (room == null)
            {
                Debug.LogWarning($"[FloorPlanController] Room not found: {roomId}");
                return false;
            }

            // Confirmation gate.
            if (OnRoomRemoveConfirm != null && !OnRoomRemoveConfirm.Invoke(room))
            {
                Debug.Log($"[FloorPlanController] Room removal cancelled by listener: '{room.roomName}'");
                return false;
            }

            var command = new RemoveRoomCommand(this, room);
            ExecuteCommand(command);

            if (SelectedRoom != null && SelectedRoom.roomId == roomId)
            {
                DeselectAll();
            }

            Debug.Log($"[FloorPlanController] Room removed: '{room.roomName}' ({roomId})");
            OnRoomRemoved?.Invoke(roomId);
            MarkProjectModified();
            return true;
        }

        /// <summary>
        /// Updates an existing room's data in the project.
        /// </summary>
        /// <param name="room">The updated room data. Must have a matching roomId.</param>
        public void UpdateRoom(RoomData room)
        {
            if (room == null)
            {
                Debug.LogError("[FloorPlanController] Cannot update with null room data.");
                return;
            }

            if (CurrentProject == null)
            {
                Debug.LogError("[FloorPlanController] No project loaded.");
                return;
            }

            int index = CurrentProject.rooms.FindIndex(r => r.roomId == room.roomId);
            if (index < 0)
            {
                Debug.LogWarning($"[FloorPlanController] Room not found for update: {room.roomId}");
                return;
            }

            var command = new UpdateRoomCommand(this, CurrentProject.rooms[index], room);
            ExecuteCommand(command);

            Debug.Log($"[FloorPlanController] Room updated: '{room.roomName}' ({room.roomId})");
            OnRoomModified?.Invoke(room);
            MarkProjectModified();
        }

        /// <summary>
        /// Generates a default room name based on the internal room counter.
        /// </summary>
        /// <returns>A name like "Room 1", "Room 2", etc.</returns>
        public string GenerateRoomName()
        {
            _roomCounter++;
            return $"Room {_roomCounter}";
        }

        #endregion

        #region Selection

        /// <summary>
        /// Selects a generic object by its ID. Determines the object type and
        /// delegates to the appropriate selection handler.
        /// </summary>
        /// <param name="objectId">The unique identifier of the object to select.</param>
        public void SelectObject(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                DeselectAll();
                return;
            }

            if (CurrentProject == null) return;

            // Try rooms first.
            RoomData room = CurrentProject.rooms.Find(r => r.roomId == objectId);
            if (room != null)
            {
                SelectRoom(room);
                return;
            }

            // Try walls.
            foreach (RoomData r in CurrentProject.rooms)
            {
                WallData wall = r.walls.Find(w => w.wallId == objectId);
                if (wall != null)
                {
                    SelectWall(wall);
                    return;
                }
            }

            // Generic fallback.
            SelectedObjectId = objectId;
            OnObjectSelected?.Invoke(objectId);
            Debug.Log($"[FloorPlanController] Object selected: {objectId}");
        }

        /// <summary>
        /// Selects a room and raises the appropriate event.
        /// </summary>
        /// <param name="room">The room to select.</param>
        public void SelectRoom(RoomData room)
        {
            SelectedRoom = room;
            SelectedWall = null;
            SelectedObjectId = room?.roomId;

            if (_propertiesPanel != null)
            {
                _propertiesPanel.SetActive(room != null);
            }

            Debug.Log($"[FloorPlanController] Room selected: '{room?.roomName}'");
            OnRoomSelected?.Invoke(room);
        }

        /// <summary>
        /// Selects a wall and raises the appropriate event.
        /// </summary>
        /// <param name="wall">The wall to select.</param>
        public void SelectWall(WallData wall)
        {
            SelectedWall = wall;
            SelectedObjectId = wall?.wallId;

            Debug.Log($"[FloorPlanController] Wall selected: {wall?.wallId}");
            OnWallSelected?.Invoke(wall);
        }

        /// <summary>
        /// Clears all current selections.
        /// </summary>
        public void DeselectAll()
        {
            bool hadSelection = SelectedRoom != null || SelectedWall != null
                                || !string.IsNullOrEmpty(SelectedObjectId);

            SelectedRoom = null;
            SelectedWall = null;
            SelectedObjectId = null;

            if (_propertiesPanel != null)
            {
                _propertiesPanel.SetActive(false);
            }

            if (hadSelection)
            {
                OnRoomSelected?.Invoke(null);
                OnWallSelected?.Invoke(null);
                OnSelectionCleared?.Invoke();
            }
        }

        #endregion

        #region Undo / Redo

        /// <summary>
        /// Executes a command and pushes it onto the undo stack.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void ExecuteCommand(IEditorCommand command)
        {
            if (command == null) return;

            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // new action invalidates the redo history

            // Enforce maximum stack depth.
            if (_undoStack.Count > _maxUndoSteps)
            {
                // Stack doesn't support removing from bottom easily,
                // so we accept slightly exceeding the limit until GC.
            }
        }

        /// <summary>
        /// Undoes the most recent command.
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count == 0)
            {
                Debug.Log("[FloorPlanController] Nothing to undo.");
                return;
            }

            IEditorCommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            Debug.Log($"[FloorPlanController] Undo: {command.Description}");
            MarkProjectModified();
        }

        /// <summary>
        /// Redoes the most recently undone command.
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count == 0)
            {
                Debug.Log("[FloorPlanController] Nothing to redo.");
                return;
            }

            IEditorCommand command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            Debug.Log($"[FloorPlanController] Redo: {command.Description}");
            MarkProjectModified();
        }

        /// <summary>
        /// Clears both undo and redo history.
        /// </summary>
        public void ClearCommandHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        #endregion

        #region Private Helpers

        private void MarkProjectModified()
        {
            if (CurrentProject != null)
            {
                CurrentProject.modifiedAt = DateTime.UtcNow.ToString("o");
            }
        }

        private void UpdateCoordinatesLabel()
        {
            if (_coordinatesLabel == null || _floorPlanCamera == null) return;

            Vector2 planPos = ScreenToFloorPlan(Input.mousePosition);
            _coordinatesLabel.text = $"({planPos.x:F2}, {planPos.y:F2}) m";
        }

        private void HandleKeyboardShortcuts()
        {
            // Ctrl/Cmd + Z = Undo, Ctrl/Cmd + Y = Redo
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                        || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

            if (ctrl && Input.GetKeyDown(KeyCode.Z))
            {
                // Shift+Z for redo on Mac
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    Redo();
                else
                    Undo();
            }
            else if (ctrl && Input.GetKeyDown(KeyCode.Y))
            {
                Redo();
            }

            // Escape to return to Select mode
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetMode(EditingMode.Select);
                DeselectAll();
            }

            // Delete to remove selected room
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (SelectedRoom != null)
                {
                    RemoveRoom(SelectedRoom.roomId);
                }
            }
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Finds the room at a given floor plan position.
        /// </summary>
        /// <param name="planPos">Position in floor plan coordinates (meters).</param>
        /// <returns>The topmost room at that position, or null.</returns>
        public RoomData GetRoomAtPosition(Vector2 planPos)
        {
            if (CurrentProject == null) return null;

            // Iterate in reverse so later-added rooms (drawn on top) are found first.
            for (int i = CurrentProject.rooms.Count - 1; i >= 0; i--)
            {
                RoomData room = CurrentProject.rooms[i];
                Rect roomRect = new Rect(room.position, room.size);
                if (roomRect.Contains(planPos))
                {
                    return room;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the wall closest to a given floor plan position within a tolerance.
        /// </summary>
        /// <param name="planPos">Position in floor plan coordinates (meters).</param>
        /// <param name="tolerance">Maximum distance from the wall line in meters.</param>
        /// <returns>The closest wall within tolerance, or null.</returns>
        public WallData GetWallAtPosition(Vector2 planPos, float tolerance = 0.3f)
        {
            if (CurrentProject == null) return null;

            WallData closest = null;
            float closestDist = tolerance;

            foreach (RoomData room in CurrentProject.rooms)
            {
                foreach (WallData wall in room.walls)
                {
                    float dist = DistanceToSegment(planPos, wall.startPoint, wall.endPoint);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = wall;
                    }
                }
            }

            return closest;
        }

        /// <summary>
        /// Computes the minimum distance from a point to a line segment.
        /// </summary>
        private static float DistanceToSegment(Vector2 point, Vector2 segA, Vector2 segB)
        {
            Vector2 ab = segB - segA;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < Mathf.Epsilon)
                return Vector2.Distance(point, segA);

            float t = Mathf.Clamp01(Vector2.Dot(point - segA, ab) / lengthSq);
            Vector2 projection = segA + t * ab;
            return Vector2.Distance(point, projection);
        }

        #endregion
    }

    #region Command Pattern (Undo/Redo)

    /// <summary>
    /// Interface for undoable editor commands.
    /// </summary>
    public interface IEditorCommand
    {
        /// <summary>Human-readable description of this command.</summary>
        string Description { get; }

        /// <summary>Executes or re-executes the command.</summary>
        void Execute();

        /// <summary>Reverses the command.</summary>
        void Undo();
    }

    /// <summary>
    /// Command to add a room to the project.
    /// </summary>
    public class AddRoomCommand : IEditorCommand
    {
        private readonly FloorPlanController _controller;
        private readonly RoomData _room;

        /// <inheritdoc/>
        public string Description => $"Add Room '{_room.roomName}'";

        /// <summary>
        /// Creates a new <see cref="AddRoomCommand"/>.
        /// </summary>
        public AddRoomCommand(FloorPlanController controller, RoomData room)
        {
            _controller = controller;
            _room = room;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            if (!_controller.CurrentProject.rooms.Contains(_room))
            {
                _controller.CurrentProject.rooms.Add(_room);
            }
        }

        /// <inheritdoc/>
        public void Undo()
        {
            _controller.CurrentProject.rooms.Remove(_room);
        }
    }

    /// <summary>
    /// Command to remove a room from the project.
    /// </summary>
    public class RemoveRoomCommand : IEditorCommand
    {
        private readonly FloorPlanController _controller;
        private readonly RoomData _room;
        private int _previousIndex;

        /// <inheritdoc/>
        public string Description => $"Remove Room '{_room.roomName}'";

        /// <summary>
        /// Creates a new <see cref="RemoveRoomCommand"/>.
        /// </summary>
        public RemoveRoomCommand(FloorPlanController controller, RoomData room)
        {
            _controller = controller;
            _room = room;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            _previousIndex = _controller.CurrentProject.rooms.IndexOf(_room);
            _controller.CurrentProject.rooms.Remove(_room);
        }

        /// <inheritdoc/>
        public void Undo()
        {
            int insertAt = Mathf.Clamp(_previousIndex, 0, _controller.CurrentProject.rooms.Count);
            _controller.CurrentProject.rooms.Insert(insertAt, _room);
        }
    }

    /// <summary>
    /// Command to update a room's data, storing the old state for undo.
    /// </summary>
    public class UpdateRoomCommand : IEditorCommand
    {
        private readonly FloorPlanController _controller;
        private readonly RoomData _oldData;
        private readonly RoomData _newData;

        /// <inheritdoc/>
        public string Description => $"Update Room '{_newData.roomName}'";

        /// <summary>
        /// Creates a new <see cref="UpdateRoomCommand"/>.
        /// </summary>
        public UpdateRoomCommand(FloorPlanController controller, RoomData oldData, RoomData newData)
        {
            _controller = controller;
            // Deep-copy the old state snapshot.
            _oldData = JsonUtility.FromJson<RoomData>(JsonUtility.ToJson(oldData));
            _newData = newData;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            ReplaceRoom(_newData);
        }

        /// <inheritdoc/>
        public void Undo()
        {
            ReplaceRoom(_oldData);
        }

        private void ReplaceRoom(RoomData data)
        {
            int index = _controller.CurrentProject.rooms.FindIndex(r => r.roomId == data.roomId);
            if (index >= 0)
            {
                _controller.CurrentProject.rooms[index] = data;
            }
        }
    }

    #endregion
}
