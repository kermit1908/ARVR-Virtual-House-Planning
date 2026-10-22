using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.FloorPlan
{
    /// <summary>
    /// Dedicated camera controller for the 2D floor plan view.
    /// Provides orthographic top-down viewing with zoom (scroll wheel / pinch),
    /// pan (middle mouse / right-click / two-finger drag), smooth interpolation,
    /// and the ability to focus on individual rooms or the entire project.
    /// </summary>
    public class FloorPlanCamera : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Camera Reference")]
        [SerializeField, Tooltip("The camera to control. Defaults to Camera.main if unset.")]
        private Camera _camera;

        [Header("Zoom Settings")]
        [SerializeField, Tooltip("Minimum orthographic size (maximum zoom in).")]
        private float _minZoom = 1f;

        [SerializeField, Tooltip("Maximum orthographic size (maximum zoom out).")]
        private float _maxZoom = 50f;

        [SerializeField, Tooltip("Zoom sensitivity for scroll wheel.")]
        private float _zoomSpeed = 2f;

        [Header("Pan Settings")]
        [SerializeField, Tooltip("Pan speed multiplier.")]
        private float _panSpeed = 1f;

        [Header("Smoothing")]
        [SerializeField, Tooltip("Smoothing factor for zoom transitions (higher = snappier).")]
        private float _zoomSmoothSpeed = 10f;

        [SerializeField, Tooltip("Smoothing factor for pan transitions (higher = snappier).")]
        private float _panSmoothSpeed = 10f;

        [Header("Bounds")]
        [SerializeField, Tooltip("Enable clamping camera position to bounds.")]
        private bool _clampToBounds = true;

        [SerializeField, Tooltip("Minimum X position of the camera.")]
        private float _boundsMinX = -50f;

        [SerializeField, Tooltip("Maximum X position of the camera.")]
        private float _boundsMaxX = 50f;

        [SerializeField, Tooltip("Minimum Z position of the camera.")]
        private float _boundsMinZ = -50f;

        [SerializeField, Tooltip("Maximum Z position of the camera.")]
        private float _boundsMaxZ = 50f;

        [Header("Focus Settings")]
        [SerializeField, Tooltip("Padding around focused content (meters).")]
        private float _focusPadding = 2f;

        [Header("Controller Reference")]
        [SerializeField, Tooltip("Floor plan controller for accessing project data.")]
        private FloorPlanController _controller;

        #endregion

        #region Private Fields

        private float _targetZoom;
        private Vector3 _targetPosition;
        private bool _isPanning;
        private Vector3 _panStartMouseWorld;

        // Touch support.
        private float _initialPinchDistance;
        private float _initialPinchZoom;
        private Vector2 _previousTouchMidpoint;

        #endregion

        #region Public Properties

        /// <summary>Current orthographic zoom size.</summary>
        public float CurrentZoom => _camera != null ? _camera.orthographicSize : _targetZoom;

        /// <summary>Current camera world position (X, Z used as floor plan coordinates).</summary>
        public Vector3 CurrentPosition => _targetPosition;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    Debug.LogError("[FloorPlanCamera] No camera found. Assign one in the Inspector.");
                    return;
                }
            }

            // Ensure orthographic and top-down orientation.
            _camera.orthographic = true;
            _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            _targetZoom = _camera.orthographicSize;
            _targetPosition = _camera.transform.position;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            HandleScrollZoom();
            HandleTouchInput();
            HandlePan();
            ApplySmoothing();
        }

        #endregion

        #region Zoom

        private void HandleScrollZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            // Zoom towards the cursor position.
            _targetZoom -= scroll * _zoomSpeed * (_targetZoom * 0.1f);
            _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        /// <summary>
        /// Programmatically sets the zoom level.
        /// </summary>
        /// <param name="orthographicSize">The desired orthographic size.</param>
        /// <param name="instant">If true, applies immediately without smoothing.</param>
        public void SetZoom(float orthographicSize, bool instant = false)
        {
            _targetZoom = Mathf.Clamp(orthographicSize, _minZoom, _maxZoom);
            if (instant && _camera != null)
            {
                _camera.orthographicSize = _targetZoom;
            }
        }

        #endregion

        #region Pan

        private void HandlePan()
        {
            // Middle mouse or right-click drag.
            bool panButton = Input.GetMouseButton(2) || Input.GetMouseButton(1);

            if (panButton && !_isPanning)
            {
                // Start panning.
                _isPanning = true;
                _panStartMouseWorld = GetMouseWorldPosition();
            }
            else if (panButton && _isPanning)
            {
                // Continue panning: offset by difference.
                Vector3 currentMouseWorld = GetMouseWorldPosition();
                Vector3 delta = _panStartMouseWorld - currentMouseWorld;
                _targetPosition += new Vector3(delta.x, 0f, delta.z);
                ClampPosition();
            }
            else if (!panButton)
            {
                _isPanning = false;
            }
        }

        /// <summary>
        /// Programmatically sets the camera's target position.
        /// </summary>
        /// <param name="worldPosition">World position to pan to.</param>
        /// <param name="instant">If true, applies immediately without smoothing.</param>
        public void SetPosition(Vector3 worldPosition, bool instant = false)
        {
            _targetPosition = new Vector3(worldPosition.x, _targetPosition.y, worldPosition.z);
            ClampPosition();

            if (instant && _camera != null)
            {
                Vector3 pos = _camera.transform.position;
                _camera.transform.position = new Vector3(_targetPosition.x, pos.y, _targetPosition.z);
            }
        }

        #endregion

        #region Touch Input

        private void HandleTouchInput()
        {
            if (Input.touchCount < 2) return;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // Pinch to zoom.
            float currentPinchDist = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _initialPinchDistance = currentPinchDist;
                _initialPinchZoom = _targetZoom;
                _previousTouchMidpoint = (t0.position + t1.position) * 0.5f;
            }
            else if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                // Zoom.
                if (_initialPinchDistance > 0.01f)
                {
                    float ratio = _initialPinchDistance / currentPinchDist;
                    _targetZoom = Mathf.Clamp(_initialPinchZoom * ratio, _minZoom, _maxZoom);
                }

                // Two-finger pan.
                Vector2 currentMidpoint = (t0.position + t1.position) * 0.5f;
                Vector2 delta = currentMidpoint - _previousTouchMidpoint;
                _previousTouchMidpoint = currentMidpoint;

                float worldUnitsPerPixel = (_camera.orthographicSize * 2f) / Screen.height;
                _targetPosition -= new Vector3(
                    delta.x * worldUnitsPerPixel * _panSpeed,
                    0f,
                    delta.y * worldUnitsPerPixel * _panSpeed
                );

                ClampPosition();
            }
        }

        #endregion

        #region Focus Methods

        /// <summary>
        /// Smoothly centers and zooms the camera to frame a specific room.
        /// </summary>
        /// <param name="room">The room to focus on.</param>
        public void FocusOnRoom(RoomData room)
        {
            if (room == null)
            {
                Debug.LogWarning("[FloorPlanCamera] Cannot focus: room is null.");
                return;
            }

            Vector2 center = room.position + room.size * 0.5f;
            _targetPosition = new Vector3(center.x, _targetPosition.y, center.y);

            // Zoom to fit the room with padding.
            float requiredHeight = room.size.y + _focusPadding * 2f;
            float requiredWidth = room.size.x + _focusPadding * 2f;
            float aspect = _camera != null ? _camera.aspect : 1.78f;
            float zoomForHeight = requiredHeight * 0.5f;
            float zoomForWidth = (requiredWidth * 0.5f) / aspect;
            _targetZoom = Mathf.Clamp(Mathf.Max(zoomForHeight, zoomForWidth), _minZoom, _maxZoom);

            ClampPosition();
            Debug.Log($"[FloorPlanCamera] Focusing on room '{room.roomName}' at ({center.x:F1}, {center.y:F1}).");
        }

        /// <summary>
        /// Smoothly centers and zooms the camera to fit all rooms in the project.
        /// </summary>
        public void FocusOnAll()
        {
            if (_controller == null || _controller.CurrentProject == null
                || _controller.CurrentProject.rooms.Count == 0)
            {
                // Default view at origin.
                _targetPosition = new Vector3(0f, _targetPosition.y, 0f);
                _targetZoom = 10f;
                Debug.Log("[FloorPlanCamera] No rooms to focus on; resetting to default view.");
                return;
            }

            // Compute bounding box of all rooms.
            float minX = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxZ = float.MinValue;

            foreach (RoomData room in _controller.CurrentProject.rooms)
            {
                minX = Mathf.Min(minX, room.position.x);
                minZ = Mathf.Min(minZ, room.position.y);
                maxX = Mathf.Max(maxX, room.position.x + room.size.x);
                maxZ = Mathf.Max(maxZ, room.position.y + room.size.y);
            }

            float centerX = (minX + maxX) * 0.5f;
            float centerZ = (minZ + maxZ) * 0.5f;
            _targetPosition = new Vector3(centerX, _targetPosition.y, centerZ);

            float spanX = (maxX - minX) + _focusPadding * 2f;
            float spanZ = (maxZ - minZ) + _focusPadding * 2f;
            float aspect = _camera != null ? _camera.aspect : 1.78f;
            float zoomForHeight = spanZ * 0.5f;
            float zoomForWidth = (spanX * 0.5f) / aspect;
            _targetZoom = Mathf.Clamp(Mathf.Max(zoomForHeight, zoomForWidth), _minZoom, _maxZoom);

            ClampPosition();
            Debug.Log($"[FloorPlanCamera] Focusing on all rooms. Center: ({centerX:F1}, {centerZ:F1}), Zoom: {_targetZoom:F1}.");
        }

        #endregion

        #region Smoothing & Clamping

        private void ApplySmoothing()
        {
            if (_camera == null) return;

            // Smooth zoom.
            float currentZoom = _camera.orthographicSize;
            _camera.orthographicSize = Mathf.Lerp(currentZoom, _targetZoom,
                                                   Time.deltaTime * _zoomSmoothSpeed);

            // Smooth pan.
            Vector3 currentPos = _camera.transform.position;
            float newX = Mathf.Lerp(currentPos.x, _targetPosition.x,
                                    Time.deltaTime * _panSmoothSpeed);
            float newZ = Mathf.Lerp(currentPos.z, _targetPosition.z,
                                    Time.deltaTime * _panSmoothSpeed);
            _camera.transform.position = new Vector3(newX, currentPos.y, newZ);
        }

        private void ClampPosition()
        {
            if (!_clampToBounds) return;

            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _boundsMinX, _boundsMaxX);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, _boundsMinZ, _boundsMaxZ);
        }

        private Vector3 GetMouseWorldPosition()
        {
            if (_camera == null) return Vector3.zero;

            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = _camera.transform.position.y; // distance from camera
            return _camera.ScreenToWorldPoint(mouseScreen);
        }

        #endregion
    }
}
