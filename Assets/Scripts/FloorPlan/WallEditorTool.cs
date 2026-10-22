using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.FloorPlan
{
    /// <summary>
    /// Tool for editing walls in the floor plan: selecting walls, dragging
    /// endpoints to resize, adding/removing openings (doors/windows), and
    /// modifying wall properties such as height, thickness, and color.
    /// </summary>
    public class WallEditorTool : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField, Tooltip("The floor plan controller that owns this tool.")]
        private FloorPlanController _controller;

        [Header("Hit Testing")]
        [SerializeField, Tooltip("Distance tolerance for selecting a wall (meters).")]
        private float _wallSelectTolerance = 0.3f;

        [SerializeField, Tooltip("Distance tolerance for grabbing a wall endpoint (meters).")]
        private float _endpointGrabTolerance = 0.25f;

        [Header("Openings")]
        [SerializeField, Tooltip("Default door width in meters.")]
        private float _defaultDoorWidth = 0.9f;

        [SerializeField, Tooltip("Default door height in meters.")]
        private float _defaultDoorHeight = 2.1f;

        [SerializeField, Tooltip("Default window width in meters.")]
        private float _defaultWindowWidth = 1.2f;

        [SerializeField, Tooltip("Default window height in meters.")]
        private float _defaultWindowHeight = 1.0f;

        [SerializeField, Tooltip("Default window sill height from floor (meters).")]
        private float _defaultWindowSillHeight = 0.9f;

        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Color for highlighting the hovered wall.")]
        private Color _hoverColor = new Color(0.4f, 0.7f, 1f, 0.6f);

        [SerializeField, Tooltip("Color for endpoint grab handles.")]
        private Color _handleColor = new Color(1f, 0.6f, 0.2f, 1f);

        [SerializeField, Tooltip("Radius of endpoint grab handles (meters).")]
        private float _handleRadius = 0.15f;

        #endregion

        #region Public Events

        /// <summary>Raised when a wall is selected.</summary>
        public event Action<WallData> OnWallSelected;

        /// <summary>Raised when a wall is deselected.</summary>
        public event Action OnWallDeselected;

        /// <summary>Raised when a wall's geometry is modified (endpoint dragged).</summary>
        public event Action<WallData> OnWallModified;

        /// <summary>Raised when an opening is added to a wall.</summary>
        public event Action<WallOpeningData, WallData> OnOpeningAdded;

        /// <summary>Raised when an opening is removed from a wall.</summary>
        public event Action<WallOpeningData, WallData> OnOpeningRemoved;

        /// <summary>Raised when wall properties are changed.</summary>
        public event Action<WallData> OnWallPropertiesChanged;

        #endregion

        #region Public Properties

        /// <summary>Whether the tool is currently active.</summary>
        public bool IsActive { get; private set; }

        /// <summary>The currently selected wall, or null.</summary>
        public WallData SelectedWall { get; private set; }

        /// <summary>The wall currently under the cursor, or null.</summary>
        public WallData HoveredWall { get; private set; }

        #endregion

        #region Private Fields

        private enum DragTarget { None, StartPoint, EndPoint }
        private DragTarget _currentDragTarget = DragTarget.None;
        private Vector2 _dragOffset;
        private WallData _draggingWall;
        private Vector2 _originalStartPoint;
        private Vector2 _originalEndPoint;
        private Material _glMaterial;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponentInParent<FloorPlanController>();
            }

            CreateGLMaterial();
        }

        private void Update()
        {
            if (!IsActive) return;

            HandleHover();
            HandleInput();
        }

        private void OnRenderObject()
        {
            if (!IsActive) return;
            DrawFeedback();
        }

        private void OnDestroy()
        {
            if (_glMaterial != null)
            {
                Destroy(_glMaterial);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Activates the wall editor tool.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            Debug.Log("[WallEditorTool] Activated.");
        }

        /// <summary>
        /// Deactivates the wall editor tool, cancelling any in-progress operations.
        /// </summary>
        public void Deactivate()
        {
            CancelDrag();
            DeselectWall();
            IsActive = false;
            HoveredWall = null;
            Debug.Log("[WallEditorTool] Deactivated.");
        }

        /// <summary>
        /// Selects a wall for editing.
        /// </summary>
        /// <param name="wall">The wall to select.</param>
        public void SelectWall(WallData wall)
        {
            if (wall == null)
            {
                DeselectWall();
                return;
            }

            SelectedWall = wall;
            _controller.SelectWall(wall);
            Debug.Log($"[WallEditorTool] Wall selected: {wall.wallId} (length: {wall.Length:F2}m)");
            OnWallSelected?.Invoke(wall);
        }

        /// <summary>
        /// Deselects the currently selected wall.
        /// </summary>
        public void DeselectWall()
        {
            if (SelectedWall == null) return;

            SelectedWall = null;
            _controller.SelectWall(null);
            OnWallDeselected?.Invoke();
        }

        /// <summary>
        /// Adds a door opening to the specified wall at the given position along the wall.
        /// </summary>
        /// <param name="wall">The wall to add the door to.</param>
        /// <param name="positionAlongWall">Normalized position (0-1) along the wall.</param>
        /// <returns>The created opening data, or null if validation failed.</returns>
        public WallOpeningData AddDoor(WallData wall, float positionAlongWall)
        {
            return AddOpening(wall, positionAlongWall, OpeningType.Door,
                              _defaultDoorWidth, _defaultDoorHeight, 0f);
        }

        /// <summary>
        /// Adds a window opening to the specified wall at the given position along the wall.
        /// </summary>
        /// <param name="wall">The wall to add the window to.</param>
        /// <param name="positionAlongWall">Normalized position (0-1) along the wall.</param>
        /// <returns>The created opening data, or null if validation failed.</returns>
        public WallOpeningData AddWindow(WallData wall, float positionAlongWall)
        {
            return AddOpening(wall, positionAlongWall, OpeningType.Window,
                              _defaultWindowWidth, _defaultWindowHeight, _defaultWindowSillHeight);
        }

        /// <summary>
        /// Adds an opening of the specified type to a wall.
        /// </summary>
        /// <param name="wall">Target wall.</param>
        /// <param name="positionAlongWall">Normalized position (0-1).</param>
        /// <param name="type">Type of opening.</param>
        /// <param name="width">Width in meters.</param>
        /// <param name="height">Height in meters.</param>
        /// <param name="sillHeight">Sill height from floor in meters.</param>
        /// <returns>The created opening, or null.</returns>
        public WallOpeningData AddOpening(WallData wall, float positionAlongWall,
                                           OpeningType type, float width,
                                           float height, float sillHeight)
        {
            if (wall == null)
            {
                Debug.LogError("[WallEditorTool] Cannot add opening: wall is null.");
                return null;
            }

            // Validate that the opening fits on the wall.
            float wallLength = wall.Length;
            float halfNormalized = (width / wallLength) * 0.5f;
            float minPos = halfNormalized;
            float maxPos = 1f - halfNormalized;

            if (positionAlongWall < minPos || positionAlongWall > maxPos)
            {
                Debug.LogWarning($"[WallEditorTool] Opening does not fit at position {positionAlongWall:F2}. " +
                                 $"Valid range: [{minPos:F2}, {maxPos:F2}].");
                return null;
            }

            // Check for overlap with existing openings.
            if (HasOverlap(wall, positionAlongWall, width))
            {
                Debug.LogWarning("[WallEditorTool] Opening overlaps with an existing opening.");
                return null;
            }

            WallOpeningData opening = new WallOpeningData
            {
                openingId = Guid.NewGuid().ToString(),
                openingType = type,
                positionAlongWall = positionAlongWall,
                width = width,
                height = height,
                sillHeight = sillHeight,
                parentWallId = wall.wallId
            };

            wall.openings.Add(opening);
            Debug.Log($"[WallEditorTool] {type} added to wall {wall.wallId} at position {positionAlongWall:F2}.");
            OnOpeningAdded?.Invoke(opening, wall);
            return opening;
        }

        /// <summary>
        /// Removes an opening from a wall.
        /// </summary>
        /// <param name="wall">The wall containing the opening.</param>
        /// <param name="openingId">The ID of the opening to remove.</param>
        /// <returns>True if the opening was found and removed.</returns>
        public bool RemoveOpening(WallData wall, string openingId)
        {
            if (wall == null || string.IsNullOrEmpty(openingId))
            {
                Debug.LogError("[WallEditorTool] Cannot remove opening: invalid parameters.");
                return false;
            }

            WallOpeningData opening = wall.openings.Find(o => o.openingId == openingId);
            if (opening == null)
            {
                Debug.LogWarning($"[WallEditorTool] Opening not found: {openingId}");
                return false;
            }

            wall.openings.Remove(opening);
            Debug.Log($"[WallEditorTool] Opening removed: {openingId} from wall {wall.wallId}.");
            OnOpeningRemoved?.Invoke(opening, wall);
            return true;
        }

        /// <summary>
        /// Changes a wall's height.
        /// </summary>
        /// <param name="wall">The wall to modify.</param>
        /// <param name="newHeight">New height in meters.</param>
        public void SetWallHeight(WallData wall, float newHeight)
        {
            if (wall == null) return;

            newHeight = Mathf.Max(0.5f, newHeight);
            wall.height = newHeight;
            Debug.Log($"[WallEditorTool] Wall {wall.wallId} height set to {newHeight:F2}m.");
            OnWallPropertiesChanged?.Invoke(wall);
        }

        /// <summary>
        /// Changes a wall's thickness.
        /// </summary>
        /// <param name="wall">The wall to modify.</param>
        /// <param name="newThickness">New thickness in meters.</param>
        public void SetWallThickness(WallData wall, float newThickness)
        {
            if (wall == null) return;

            newThickness = Mathf.Clamp(newThickness, 0.05f, 1f);
            wall.thickness = newThickness;
            Debug.Log($"[WallEditorTool] Wall {wall.wallId} thickness set to {newThickness:F2}m.");
            OnWallPropertiesChanged?.Invoke(wall);
        }

        /// <summary>
        /// Changes a wall's color.
        /// </summary>
        /// <param name="wall">The wall to modify.</param>
        /// <param name="newColor">The new color.</param>
        public void SetWallColor(WallData wall, Color newColor)
        {
            if (wall == null) return;

            wall.color = newColor;
            OnWallPropertiesChanged?.Invoke(wall);
        }

        #endregion

        #region Input Handling

        private void HandleHover()
        {
            if (_currentDragTarget != DragTarget.None) return;

            Vector2 floorPos = _controller.ScreenToFloorPlan(Input.mousePosition);
            HoveredWall = _controller.GetWallAtPosition(floorPos, _wallSelectTolerance);
        }

        private void HandleInput()
        {
            // Escape to deselect / deactivate.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentDragTarget != DragTarget.None)
                {
                    CancelDrag();
                }
                else if (SelectedWall != null)
                {
                    DeselectWall();
                }
                else
                {
                    Deactivate();
                    _controller.SetMode(FloorPlanController.EditingMode.Select);
                }
                return;
            }

            Vector2 floorPos = _controller.ScreenToFloorPlan(Input.mousePosition);

            // Left-click: select wall or start endpoint drag.
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseDown(floorPos);
            }
            else if (Input.GetMouseButton(0) && _currentDragTarget != DragTarget.None)
            {
                HandleMouseDrag(floorPos);
            }
            else if (Input.GetMouseButtonUp(0) && _currentDragTarget != DragTarget.None)
            {
                HandleMouseUp(floorPos);
            }

            // Delete key to remove selected opening or wall.
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                // Could implement opening removal here if an opening is selected.
            }
        }

        private void HandleMouseDown(Vector2 floorPos)
        {
            // Check if clicking near an endpoint of the selected wall.
            if (SelectedWall != null)
            {
                float distToStart = Vector2.Distance(floorPos, SelectedWall.startPoint);
                float distToEnd = Vector2.Distance(floorPos, SelectedWall.endPoint);

                if (distToStart <= _endpointGrabTolerance)
                {
                    StartDrag(SelectedWall, DragTarget.StartPoint, floorPos);
                    return;
                }

                if (distToEnd <= _endpointGrabTolerance)
                {
                    StartDrag(SelectedWall, DragTarget.EndPoint, floorPos);
                    return;
                }
            }

            // Try selecting a wall.
            WallData wall = _controller.GetWallAtPosition(floorPos, _wallSelectTolerance);
            if (wall != null)
            {
                SelectWall(wall);
            }
            else
            {
                DeselectWall();
            }
        }

        private void HandleMouseDrag(Vector2 floorPos)
        {
            if (_draggingWall == null) return;

            Vector2 snapped = _controller.SnapToGrid
                ? _controller.SnapPosition(floorPos)
                : floorPos;

            if (_currentDragTarget == DragTarget.StartPoint)
            {
                _draggingWall.startPoint = snapped;
            }
            else if (_currentDragTarget == DragTarget.EndPoint)
            {
                _draggingWall.endPoint = snapped;
            }

            // Update the parent room's position and size if walls define the boundary.
            UpdateParentRoomGeometry(_draggingWall);
        }

        private void HandleMouseUp(Vector2 floorPos)
        {
            if (_draggingWall == null) return;

            // Validate minimum wall length.
            if (_draggingWall.Length < 0.1f)
            {
                Debug.LogWarning("[WallEditorTool] Wall too short; reverting drag.");
                _draggingWall.startPoint = _originalStartPoint;
                _draggingWall.endPoint = _originalEndPoint;
                UpdateParentRoomGeometry(_draggingWall);
            }
            else
            {
                Debug.Log($"[WallEditorTool] Wall endpoint moved. New length: {_draggingWall.Length:F2}m.");
                OnWallModified?.Invoke(_draggingWall);
            }

            _currentDragTarget = DragTarget.None;
            _draggingWall = null;
        }

        #endregion

        #region Drag Operations

        private void StartDrag(WallData wall, DragTarget target, Vector2 floorPos)
        {
            _draggingWall = wall;
            _currentDragTarget = target;
            _originalStartPoint = wall.startPoint;
            _originalEndPoint = wall.endPoint;

            Debug.Log($"[WallEditorTool] Dragging {target} of wall {wall.wallId}.");
        }

        private void CancelDrag()
        {
            if (_draggingWall != null)
            {
                _draggingWall.startPoint = _originalStartPoint;
                _draggingWall.endPoint = _originalEndPoint;
                UpdateParentRoomGeometry(_draggingWall);
                Debug.Log("[WallEditorTool] Drag cancelled; wall reverted.");
            }

            _currentDragTarget = DragTarget.None;
            _draggingWall = null;
        }

        #endregion

        #region Room Geometry Update

        /// <summary>
        /// Recalculates the parent room's position and size from its wall endpoints.
        /// Called after a wall endpoint is dragged.
        /// </summary>
        private void UpdateParentRoomGeometry(WallData wall)
        {
            if (_controller.CurrentProject == null || string.IsNullOrEmpty(wall.parentRoomId))
                return;

            RoomData room = _controller.CurrentProject.rooms.Find(
                r => r.roomId == wall.parentRoomId);
            if (room == null || room.walls.Count == 0) return;

            // Compute bounding box from all wall endpoints.
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (WallData w in room.walls)
            {
                minX = Mathf.Min(minX, w.startPoint.x, w.endPoint.x);
                minY = Mathf.Min(minY, w.startPoint.y, w.endPoint.y);
                maxX = Mathf.Max(maxX, w.startPoint.x, w.endPoint.x);
                maxY = Mathf.Max(maxY, w.startPoint.y, w.endPoint.y);
            }

            room.position = new Vector2(minX, minY);
            room.size = new Vector2(maxX - minX, maxY - minY);
        }

        #endregion

        #region Overlap Detection

        /// <summary>
        /// Checks whether a new opening at the given position would overlap
        /// any existing openings on the wall.
        /// </summary>
        private bool HasOverlap(WallData wall, float positionNorm, float width)
        {
            float wallLength = wall.Length;
            if (wallLength <= 0f) return true;

            float halfNorm = (width / wallLength) * 0.5f;
            float newMin = positionNorm - halfNorm;
            float newMax = positionNorm + halfNorm;

            foreach (WallOpeningData existing in wall.openings)
            {
                float existHalf = (existing.width / wallLength) * 0.5f;
                float existMin = existing.positionAlongWall - existHalf;
                float existMax = existing.positionAlongWall + existHalf;

                // Check interval overlap.
                if (newMin < existMax && newMax > existMin)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Visual Feedback

        private void CreateGLMaterial()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogError("[WallEditorTool] Could not find GL shader.");
                return;
            }

            _glMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _glMaterial.SetInt("_ZWrite", 0);
        }

        private void DrawFeedback()
        {
            if (_glMaterial == null) return;

            // Draw hover highlight.
            if (HoveredWall != null && HoveredWall != SelectedWall)
            {
                DrawWallHighlight(HoveredWall, _hoverColor);
            }

            // Draw endpoint handles for the selected wall.
            if (SelectedWall != null)
            {
                DrawHandle(SelectedWall.startPoint, _handleColor);
                DrawHandle(SelectedWall.endPoint, _handleColor);
            }
        }

        private void DrawWallHighlight(WallData wall, Color color)
        {
            Vector2 dir = (wall.endPoint - wall.startPoint).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x) * (wall.thickness * 0.5f + 0.05f);

            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.QUADS);
            GL.Color(color);

            GL.Vertex3(wall.startPoint.x + perp.x, 0.009f, wall.startPoint.y + perp.y);
            GL.Vertex3(wall.startPoint.x - perp.x, 0.009f, wall.startPoint.y - perp.y);
            GL.Vertex3(wall.endPoint.x - perp.x, 0.009f, wall.endPoint.y - perp.y);
            GL.Vertex3(wall.endPoint.x + perp.x, 0.009f, wall.endPoint.y + perp.y);

            GL.End();
            GL.PopMatrix();
        }

        private void DrawHandle(Vector2 position, Color color)
        {
            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.QUADS);
            GL.Color(color);

            float r = _handleRadius;
            GL.Vertex3(position.x - r, 0.01f, position.y - r);
            GL.Vertex3(position.x + r, 0.01f, position.y - r);
            GL.Vertex3(position.x + r, 0.01f, position.y + r);
            GL.Vertex3(position.x - r, 0.01f, position.y + r);

            GL.End();
            GL.PopMatrix();
        }

        #endregion
    }
}
