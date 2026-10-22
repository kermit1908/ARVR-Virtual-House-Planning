using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using HousePlanner.Core;

namespace HousePlanner.AR
{
    /// <summary>
    /// Possible states of the AR system lifecycle.
    /// </summary>
    public enum ARState
    {
        /// <summary>AR session is starting up.</summary>
        Initializing,
        /// <summary>Actively scanning for planes.</summary>
        Scanning,
        /// <summary>At least one plane has been detected.</summary>
        PlaneDetected,
        /// <summary>An object has been placed in the scene.</summary>
        ObjectPlaced,
        /// <summary>A recoverable error has occurred.</summary>
        Error,
        /// <summary>The device does not support AR.</summary>
        Unsupported
    }

    /// <summary>
    /// Main AR controller that manages plane detection, object placement,
    /// manipulation (move / rotate / scale), and session lifecycle.
    /// Requires AR Foundation 5.x+ components on the same GameObject hierarchy.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARPlacementController : MonoBehaviour
    {
        // ──────────────────────────── Inspector Fields ────────────────────────────

        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARAnchorManager anchorManager;

        [Header("Placement")]
        [SerializeField] private PlacementIndicator placementIndicator;
        [SerializeField] private Camera arCamera;

        [Header("Defaults")]
        [SerializeField] private float defaultScale = 0.05f;
        [SerializeField] private LayerMask placedObjectLayer;

        // ──────────────────────────── Events ─────────────────────────────────────

        /// <summary>Raised whenever the AR state changes.</summary>
        public event Action<ARState> OnARStateChanged;

        /// <summary>Raised to relay human-readable status messages to the UI.</summary>
        public event Action<string> OnStatusMessage;

        /// <summary>Raised when an object is successfully placed.</summary>
        public event Action<GameObject> OnObjectPlaced;

        /// <summary>Raised when a placed object is removed.</summary>
        public event Action OnObjectRemoved;

        // ──────────────────────────── Properties ─────────────────────────────────

        /// <summary>Current state of the AR system.</summary>
        public ARState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState == value) return;
                _currentState = value;
                OnARStateChanged?.Invoke(_currentState);
            }
        }

        /// <summary>The most recently placed AR object, if any.</summary>
        public GameObject ActiveObject => _activeObject;

        /// <summary>True when a valid placement pose is available.</summary>
        public bool HasValidPlacement => _hasValidPlacement;

        // ──────────────────────────── Private State ──────────────────────────────

        private ARState _currentState = ARState.Initializing;
        private readonly List<ARRaycastHit> _raycastHits = new List<ARRaycastHit>();
        private readonly List<GameObject> _placedObjects = new List<GameObject>();
        private GameObject _activeObject;
        private ARAnchor _activeAnchor;
        private Pose _currentPlacementPose;
        private bool _hasValidPlacement;
        private bool _isInitialized;

        // ──────────────────────────── Unity Lifecycle ────────────────────────────

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            if (arSession != null)
            {
                ARSession.stateChanged += OnARSessionStateChanged;
            }

            if (planeManager != null)
            {
                planeManager.trackablesChanged += OnPlanesChanged;
            }
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= OnARSessionStateChanged;

            if (planeManager != null)
            {
                planeManager.trackablesChanged -= OnPlanesChanged;
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;

            if (_currentState == ARState.Scanning || _currentState == ARState.PlaneDetected)
            {
                UpdatePlacementIndicator();
            }
        }

        // ──────────────────────────── Public API ─────────────────────────────────

        /// <summary>
        /// Initialise the AR session – checks availability, requests permissions,
        /// and begins scanning when ready.
        /// </summary>
        public async void Initialize()
        {
#if UNITY_ANDROID || UNITY_IOS
            CurrentState = ARState.Initializing;
            EmitStatus("Checking AR support…");

            var availability = await ARSession.CheckAvailability();

            if (availability.status == SessionAvailabilityStatus.Supported)
            {
                _isInitialized = true;
                StartScanning();
            }
            else if (availability.status == SessionAvailabilityStatus.NeedsInstall)
            {
                EmitStatus("AR software update required. Installing…");
                var install = await ARSession.Install();
                if (install.status == SessionInstallationStatus.Success)
                {
                    _isInitialized = true;
                    StartScanning();
                }
                else
                {
                    CurrentState = ARState.Unsupported;
                    EmitStatus("Failed to install AR software.");
                }
            }
            else
            {
                CurrentState = ARState.Unsupported;
                EmitStatus("AR is not supported on this device.");
            }
#else
            CurrentState = ARState.Unsupported;
            EmitStatus("AR is only supported on Android and iOS.");
#endif
        }

        /// <summary>
        /// Called by the UI when the user taps to place.  Uses the current
        /// placement indicator pose.
        /// </summary>
        public void OnTapToPlace()
        {
            if (!_hasValidPlacement)
            {
                EmitStatus("No valid surface detected. Keep scanning.");
                return;
            }

            // Default: place an empty container the caller can populate.
            PlaceObjectInternal(null);
        }

        /// <summary>
        /// Instantiate <paramref name="prefab"/> at the current placement pose
        /// and anchor it to the detected plane.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <returns>The instantiated <see cref="GameObject"/>, or null on failure.</returns>
        public GameObject PlaceObject(GameObject prefab)
        {
            if (!_hasValidPlacement)
            {
                EmitStatus("No valid surface detected.");
                return null;
            }

            return PlaceObjectInternal(prefab);
        }

        /// <summary>
        /// Move the currently active placed object to a new screen position
        /// via AR raycast.
        /// </summary>
        /// <param name="screenPos">Screen-space touch position.</param>
        public void MoveObject(Vector2 screenPos)
        {
            if (_activeObject == null)
            {
                EmitStatus("No object selected to move.");
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            if (raycastManager != null &&
                raycastManager.Raycast(screenPos, _raycastHits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = _raycastHits[0].pose;
                _activeObject.transform.position = hitPose.position;
                EmitStatus("Object moved.");
            }
#endif
        }

        /// <summary>
        /// Rotate the active placed object around the world-up axis.
        /// </summary>
        /// <param name="degrees">Rotation in degrees (positive = clockwise when viewed from above).</param>
        public void RotateObject(float degrees)
        {
            if (_activeObject == null)
            {
                EmitStatus("No object selected to rotate.");
                return;
            }

            _activeObject.transform.Rotate(Vector3.up, degrees, Space.World);
        }

        /// <summary>
        /// Uniformly scale the active placed object.
        /// </summary>
        /// <param name="scaleFactor">Absolute scale multiplier (e.g. 0.05 for 1:20).</param>
        public void ScaleObject(float scaleFactor)
        {
            if (_activeObject == null)
            {
                EmitStatus("No object selected to scale.");
                return;
            }

            scaleFactor = Mathf.Clamp(scaleFactor, 0.001f, 10f);
            _activeObject.transform.localScale = Vector3.one * scaleFactor;
        }

        /// <summary>
        /// Destroy the active placed object and its anchor.
        /// </summary>
        public void RemoveObject()
        {
            if (_activeObject == null) return;

            _placedObjects.Remove(_activeObject);
            Destroy(_activeObject);
            _activeObject = null;

            if (_activeAnchor != null)
            {
                Destroy(_activeAnchor.gameObject);
                _activeAnchor = null;
            }

            CurrentState = ARState.PlaneDetected;
            EmitStatus("Object removed.");
            OnObjectRemoved?.Invoke();
        }

        /// <summary>
        /// Clear every placed object and restart scanning.
        /// </summary>
        public void ResetAR()
        {
            foreach (var obj in _placedObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _placedObjects.Clear();
            _activeObject = null;
            _activeAnchor = null;

            if (arSession != null)
            {
                arSession.Reset();
            }

            _hasValidPlacement = false;
            ShowPlacementIndicator(false);
            StartScanning();
            EmitStatus("AR session reset. Scan a surface to begin.");
        }

        /// <summary>
        /// Show or hide the placement reticle.
        /// </summary>
        /// <param name="show">True to show, false to hide.</param>
        public void ShowPlacementIndicator(bool show)
        {
            if (placementIndicator != null)
            {
                placementIndicator.SetVisible(show);
            }
        }

        /// <summary>
        /// Set a specific object as the active selection.
        /// </summary>
        /// <param name="obj">The placed object to select.</param>
        public void SetActiveObject(GameObject obj)
        {
            _activeObject = obj;
        }

        // ──────────────────────────── Private Helpers ────────────────────────────

        private void CacheComponents()
        {
            if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
            if (arSession == null) arSession = FindFirstObjectByType<ARSession>();
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
            if (anchorManager == null) anchorManager = GetComponent<ARAnchorManager>();
            if (arCamera == null) arCamera = Camera.main;
        }

        private void StartScanning()
        {
            CurrentState = ARState.Scanning;
            EmitStatus("Move your device slowly to scan the floor.");

            if (planeManager != null)
            {
                planeManager.enabled = true;
            }

            ShowPlacementIndicator(false);
        }

        private void UpdatePlacementIndicator()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (arCamera == null || raycastManager == null) return;

            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            if (raycastManager.Raycast(screenCenter, _raycastHits, TrackableType.PlaneWithinPolygon))
            {
                _currentPlacementPose = _raycastHits[0].pose;
                _hasValidPlacement = true;

                if (placementIndicator != null)
                {
                    placementIndicator.UpdatePose(_currentPlacementPose);
                    placementIndicator.SetValid(true);
                    placementIndicator.SetVisible(true);
                }

                if (_currentState == ARState.Scanning)
                {
                    CurrentState = ARState.PlaneDetected;
                    EmitStatus("Surface detected! Tap to place your design.");
                }
            }
            else
            {
                _hasValidPlacement = false;
                if (placementIndicator != null)
                {
                    placementIndicator.SetValid(false);
                }
            }
#endif
        }

        private GameObject PlaceObjectInternal(GameObject prefab)
        {
#if UNITY_ANDROID || UNITY_IOS
            // Create anchor at placement pose.
            var anchorGO = new GameObject("AR_Anchor");
            anchorGO.transform.SetPositionAndRotation(
                _currentPlacementPose.position,
                _currentPlacementPose.rotation);

            _activeAnchor = anchorGO.AddComponent<ARAnchor>();

            GameObject placed;
            if (prefab != null)
            {
                placed = Instantiate(prefab, _currentPlacementPose.position,
                                     _currentPlacementPose.rotation);
            }
            else
            {
                placed = new GameObject("AR_PlacedObject");
                placed.transform.SetPositionAndRotation(
                    _currentPlacementPose.position,
                    _currentPlacementPose.rotation);
            }

            placed.transform.SetParent(_activeAnchor.transform, true);
            placed.transform.localScale = Vector3.one * defaultScale;

            _activeObject = placed;
            _placedObjects.Add(placed);

            ShowPlacementIndicator(false);
            CurrentState = ARState.ObjectPlaced;
            EmitStatus("Object placed. Use controls to adjust.");
            OnObjectPlaced?.Invoke(placed);

            return placed;
#else
            EmitStatus("AR placement is not available on this platform.");
            return null;
#endif
        }

        // ──────────────────────────── AR Callbacks ───────────────────────────────

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            switch (args.state)
            {
                case ARSessionState.Unsupported:
                    CurrentState = ARState.Unsupported;
                    EmitStatus("AR is not supported on this device.");
                    break;

                case ARSessionState.NeedsInstall:
                    EmitStatus("AR software needs to be installed.");
                    break;

                case ARSessionState.Installing:
                    EmitStatus("Installing AR software…");
                    break;

                case ARSessionState.Ready:
                    if (_currentState == ARState.Initializing)
                    {
                        StartScanning();
                    }
                    break;

                case ARSessionState.SessionInitializing:
                    EmitStatus("AR session initialising…");
                    break;

                case ARSessionState.SessionTracking:
                    if (_currentState == ARState.Error)
                    {
                        StartScanning();
                        EmitStatus("Tracking recovered.");
                    }
                    break;

                case ARSessionState.None:
                case ARSessionState.CheckingAvailability:
                default:
                    break;
            }
        }

        private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
        {
            if (args.added != null && args.added.Count > 0 && _currentState == ARState.Scanning)
            {
                CurrentState = ARState.PlaneDetected;
                EmitStatus("Surface detected! Tap to place your design.");
            }
        }

        private void EmitStatus(string message)
        {
            OnStatusMessage?.Invoke(message);
            Debug.Log($"[ARPlacementController] {message}");
        }
    }
}
