using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.FloorPlan
{
    /// <summary>
    /// Interactive tool for creating rectangular rooms by click-and-drag.
    /// Handles pointer input, preview rendering, grid snapping, validation,
    /// and automatic wall generation for the new room.
    /// </summary>
    public class RoomCreationTool : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField, Tooltip("The floor plan controller that owns this tool.")]
        private FloorPlanController _controller;

        [Header("Constraints")]
        [SerializeField, Tooltip("Minimum room width in meters.")]
        private float _minimumWidth = 1f;

        [SerializeField, Tooltip("Minimum room depth in meters.")]
        private float _minimumDepth = 1f;

        [Header("Defaults")]
        [SerializeField, Tooltip("Default room type for new rooms.")]
        private RoomType _defaultRoomType = RoomType.LivingRoom;

        [SerializeField, Tooltip("Default ceiling height for new rooms (meters).")]
        private float _defaultCeilingHeight = 2.8f;

        [SerializeField, Tooltip("Default wall thickness for new rooms (meters).")]
        private float _defaultWallThickness = 0.15f;

        [Header("Preview")]
        [SerializeField, Tooltip("Color of the room preview rectangle during drag.")]
        private Color _previewFillColor = new Color(0.3f, 0.7f, 1f, 0.15f);

        [SerializeField, Tooltip("Color of the preview outline.")]
        private Color _previewOutlineColor = new Color(0.3f, 0.7f, 1f, 0.8f);

        [SerializeField, Tooltip("Color when the room size is invalid (too small).")]
        private Color _previewInvalidColor = new Color(1f, 0.3f, 0.3f, 0.5f);

        #endregion

        #region Public Events

        /// <summary>Raised when room creation starts (user begins dragging).</summary>
        public event Action OnCreationStarted;

        /// <summary>Raised when room creation completes successfully.</summary>
        public event Action<RoomData> OnCreationCompleted;

        /// <summary>Raised when room creation is cancelled (e.g., invalid size, Escape key).</summary>
        public event Action OnCreationCancelled;

        #endregion

        #region Public Properties

        /// <summary>Whether the tool is currently active and accepting input.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether the user is currently dragging to define a room.</summary>
        public bool IsDragging { get; private set; }

        #endregion

        #region Private Fields

        private Vector2 _dragStartFloorPlan;
        private Vector2 _dragCurrentFloorPlan;
        private Material _previewMaterial;
        private bool _currentSizeValid;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponentInParent<FloorPlanController>();
            }

            CreatePreviewMaterial();
        }

        private void Update()
        {
            if (!IsActive) return;

            HandleInput();
        }

        private void OnRenderObject()
        {
            if (!IsActive || !IsDragging) return;
            DrawPreview();
        }

        private void OnDestroy()
        {
            if (_previewMaterial != null)
            {
                Destroy(_previewMaterial);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Begins room creation mode. The tool will start listening for pointer input
        /// to define a new room rectangle.
        /// </summary>
        public void StartCreation()
        {
            if (_controller == null)
            {
                Debug.LogError("[RoomCreationTool] Cannot start: no FloorPlanController assigned.");
                return;
            }

            IsActive = true;
            IsDragging = false;
            _controller.SetMode(FloorPlanController.EditingMode.PlaceRoom);
            Debug.Log("[RoomCreationTool] Room creation mode activated. Click and drag to define a room.");
        }

        /// <summary>
        /// Cancels the current room creation and deactivates the tool.
        /// </summary>
        public void CancelCreation()
        {
            bool wasDragging = IsDragging;
            IsActive = false;
            IsDragging = false;

            if (wasDragging)
            {
                Debug.Log("[RoomCreationTool] Room creation cancelled.");
                OnCreationCancelled?.Invoke();
            }
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// Handles pointer down events to begin defining the room rectangle.
        /// </summary>
        /// <param name="floorPlanPos">Position in floor plan coordinates.</param>
        public void OnPointerDown(Vector2 floorPlanPos)
        {
            if (!IsActive) return;

            Vector2 snapped = _controller.SnapToGrid
                ? _controller.SnapPosition(floorPlanPos)
                : floorPlanPos;

            _dragStartFloorPlan = snapped;
            _dragCurrentFloorPlan = snapped;
            IsDragging = true;

            Debug.Log($"[RoomCreationTool] Drag started at ({snapped.x:F2}, {snapped.y:F2})");
            OnCreationStarted?.Invoke();
        }

        /// <summary>
        /// Handles pointer move events to update the room preview rectangle.
        /// </summary>
        /// <param name="floorPlanPos">Current position in floor plan coordinates.</param>
        public void OnPointerMove(Vector2 floorPlanPos)
        {
            if (!IsActive || !IsDragging) return;

            _dragCurrentFloorPlan = _controller.SnapToGrid
                ? _controller.SnapPosition(floorPlanPos)
                : floorPlanPos;

            ValidateCurrentSize();
        }

        /// <summary>
        /// Handles pointer up events to finalize room creation.
        /// </summary>
        /// <param name="floorPlanPos">Final position in floor plan coordinates.</param>
        public void OnPointerUp(Vector2 floorPlanPos)
        {
            if (!IsActive || !IsDragging) return;

            _dragCurrentFloorPlan = _controller.SnapToGrid
                ? _controller.SnapPosition(floorPlanPos)
                : floorPlanPos;

            IsDragging = false;

            if (!ValidateCurrentSize())
            {
                Debug.LogWarning($"[RoomCreationTool] Room too small. Minimum: {_minimumWidth}m × {_minimumDepth}m.");
                OnCreationCancelled?.Invoke();
                return;
            }

            RoomData room = CreateRoomFromDrag();
            if (room != null)
            {
                _controller.AddRoom(room);
                Debug.Log($"[RoomCreationTool] Room created: '{room.roomName}' " +
                          $"at ({room.position.x:F2}, {room.position.y:F2}), " +
                          $"size ({room.size.x:F2} × {room.size.y:F2})");
                OnCreationCompleted?.Invoke(room);
            }

            // Stay active for creating more rooms.
        }

        private void HandleInput()
        {
            // Escape to cancel.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelCreation();
                _controller.SetMode(FloorPlanController.EditingMode.Select);
                return;
            }

            // Mouse / touch input.
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 floorPos = _controller.ScreenToFloorPlan(Input.mousePosition);
                OnPointerDown(floorPos);
            }
            else if (Input.GetMouseButton(0) && IsDragging)
            {
                Vector2 floorPos = _controller.ScreenToFloorPlan(Input.mousePosition);
                OnPointerMove(floorPos);
            }
            else if (Input.GetMouseButtonUp(0) && IsDragging)
            {
                Vector2 floorPos = _controller.ScreenToFloorPlan(Input.mousePosition);
                OnPointerUp(floorPos);
            }
        }

        #endregion

        #region Room Creation

        private RoomData CreateRoomFromDrag()
        {
            GetNormalizedRect(out Vector2 position, out Vector2 size);

            RoomData room = new RoomData
            {
                roomId = Guid.NewGuid().ToString(),
                roomName = _controller.GenerateRoomName(),
                roomType = _defaultRoomType,
                position = position,
                size = size,
                ceilingHeight = _defaultCeilingHeight,
                floorHeight = 0f,
                fillColor = GetRoomTypeColor(_defaultRoomType),
                walls = new List<WallData>(),
                furniture = new List<FurnitureItemData>()
            };

            // Generate 4 walls for the rectangular room.
            GenerateWalls(room);

            return room;
        }

        /// <summary>
        /// Generates four walls for a rectangular room, one for each side.
        /// Walls are created clockwise starting from the bottom edge.
        /// </summary>
        /// <param name="room">The room to generate walls for.</param>
        private void GenerateWalls(RoomData room)
        {
            Vector2 p = room.position;
            Vector2 s = room.size;

            // Corner points: bottom-left, bottom-right, top-right, top-left.
            Vector2 bl = p;
            Vector2 br = new Vector2(p.x + s.x, p.y);
            Vector2 tr = new Vector2(p.x + s.x, p.y + s.y);
            Vector2 tl = new Vector2(p.x, p.y + s.y);

            string[] wallNames = { "Bottom", "Right", "Top", "Left" };
            Vector2[][] wallPoints = new Vector2[][]
            {
                new[] { bl, br },  // Bottom
                new[] { br, tr },  // Right
                new[] { tr, tl },  // Top
                new[] { tl, bl }   // Left
            };

            for (int i = 0; i < 4; i++)
            {
                WallData wall = new WallData
                {
                    wallId = Guid.NewGuid().ToString(),
                    startPoint = wallPoints[i][0],
                    endPoint = wallPoints[i][1],
                    height = room.ceilingHeight,
                    thickness = _defaultWallThickness,
                    parentRoomId = room.roomId,
                    openings = new List<WallOpeningData>()
                };

                room.walls.Add(wall);
            }

            Debug.Log($"[RoomCreationTool] Generated {room.walls.Count} walls for room '{room.roomName}'.");
        }

        #endregion

        #region Validation

        private bool ValidateCurrentSize()
        {
            GetNormalizedRect(out Vector2 _, out Vector2 size);
            _currentSizeValid = size.x >= _minimumWidth && size.y >= _minimumDepth;
            return _currentSizeValid;
        }

        private void GetNormalizedRect(out Vector2 position, out Vector2 size)
        {
            float minX = Mathf.Min(_dragStartFloorPlan.x, _dragCurrentFloorPlan.x);
            float minY = Mathf.Min(_dragStartFloorPlan.y, _dragCurrentFloorPlan.y);
            float maxX = Mathf.Max(_dragStartFloorPlan.x, _dragCurrentFloorPlan.x);
            float maxY = Mathf.Max(_dragStartFloorPlan.y, _dragCurrentFloorPlan.y);

            position = new Vector2(minX, minY);
            size = new Vector2(maxX - minX, maxY - minY);
        }

        #endregion

        #region Preview Drawing

        private void CreatePreviewMaterial()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogError("[RoomCreationTool] Could not find preview shader.");
                return;
            }

            _previewMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _previewMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _previewMaterial.SetInt("_ZWrite", 0);
        }

        private void DrawPreview()
        {
            if (_previewMaterial == null) return;

            GetNormalizedRect(out Vector2 pos, out Vector2 size);

            Color fillColor = _currentSizeValid ? _previewFillColor : _previewInvalidColor;
            Color outlineColor = _currentSizeValid ? _previewOutlineColor : _previewInvalidColor;

            _previewMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            // Filled preview quad.
            GL.Begin(GL.QUADS);
            GL.Color(fillColor);
            GL.Vertex3(pos.x, 0.007f, pos.y);
            GL.Vertex3(pos.x + size.x, 0.007f, pos.y);
            GL.Vertex3(pos.x + size.x, 0.007f, pos.y + size.y);
            GL.Vertex3(pos.x, 0.007f, pos.y + size.y);
            GL.End();

            // Outline.
            GL.Begin(GL.LINES);
            GL.Color(outlineColor);

            // Bottom
            GL.Vertex3(pos.x, 0.008f, pos.y);
            GL.Vertex3(pos.x + size.x, 0.008f, pos.y);
            // Right
            GL.Vertex3(pos.x + size.x, 0.008f, pos.y);
            GL.Vertex3(pos.x + size.x, 0.008f, pos.y + size.y);
            // Top
            GL.Vertex3(pos.x + size.x, 0.008f, pos.y + size.y);
            GL.Vertex3(pos.x, 0.008f, pos.y + size.y);
            // Left
            GL.Vertex3(pos.x, 0.008f, pos.y + size.y);
            GL.Vertex3(pos.x, 0.008f, pos.y);

            GL.End();
            GL.PopMatrix();
        }

        #endregion

        #region Utility

        /// <summary>
        /// Returns a default fill color based on room type.
        /// </summary>
        private Color GetRoomTypeColor(RoomType type)
        {
            switch (type)
            {
                case RoomType.LivingRoom: return new Color(0.9f, 0.92f, 0.85f, 0.4f);
                case RoomType.Bedroom:    return new Color(0.85f, 0.88f, 0.95f, 0.4f);
                case RoomType.Kitchen:    return new Color(0.95f, 0.90f, 0.80f, 0.4f);
                case RoomType.Bathroom:   return new Color(0.80f, 0.92f, 0.95f, 0.4f);
                case RoomType.DiningRoom: return new Color(0.92f, 0.88f, 0.82f, 0.4f);
                case RoomType.Office:     return new Color(0.88f, 0.90f, 0.88f, 0.4f);
                case RoomType.Hallway:    return new Color(0.90f, 0.90f, 0.90f, 0.4f);
                case RoomType.Storage:    return new Color(0.85f, 0.85f, 0.82f, 0.4f);
                case RoomType.Garage:     return new Color(0.80f, 0.80f, 0.80f, 0.4f);
                case RoomType.Balcony:    return new Color(0.85f, 0.95f, 0.85f, 0.4f);
                default:                  return new Color(0.90f, 0.90f, 0.85f, 0.4f);
            }
        }

        #endregion
    }
}
