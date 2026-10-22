using UnityEngine;
using UnityEngine.XR.ARFoundation;
using HousePlanner.Core;
using HousePlanner.HouseGeneration;

namespace HousePlanner.AR
{
    /// <summary>
    /// Scale presets for the AR house preview.
    /// </summary>
    public enum ScaleMode
    {
        /// <summary>1:20 miniature (default). Fits on a table.</summary>
        Miniature,
        /// <summary>1:1 full-size walkable model.</summary>
        RoomScale,
        /// <summary>User-defined custom scale factor.</summary>
        Custom
    }

    /// <summary>
    /// Generates and manages a 3D preview of a saved house design for
    /// placement in AR.  Works together with <see cref="ARPlacementController"/>
    /// and <see cref="HouseGenerator"/>.
    /// </summary>
    public class ARHousePreview : MonoBehaviour
    {
        // ──────────────────────────── Constants ──────────────────────────────────

        /// <summary>Default miniature scale factor (1:20).</summary>
        private const float MiniatureScale = 0.05f;

        /// <summary>Full-size (1:1) scale factor.</summary>
        private const float RoomScaleValue = 1.0f;

        // ──────────────────────────── Inspector Fields ────────────────────────────

        [Header("Generation")]
        [SerializeField] private HouseGenerator houseGenerator;
        [SerializeField] private Material defaultWallMaterial;
        [SerializeField] private Material defaultFloorMaterial;

        [Header("Scale Settings")]
        [SerializeField] private ScaleMode initialScaleMode = ScaleMode.Miniature;
        [SerializeField] private float customScale = MiniatureScale;

        // ──────────────────────────── Properties ─────────────────────────────────

        /// <summary>The root <see cref="GameObject"/> of the generated house.</summary>
        public GameObject HouseRoot => _houseRoot;

        /// <summary>Current scale mode.</summary>
        public ScaleMode CurrentScaleMode => _scaleMode;

        /// <summary>Current absolute scale factor.</summary>
        public float CurrentScale => _currentScale;

        // ──────────────────────────── Private State ──────────────────────────────

        private GameObject _houseRoot;
        private GameObject _wallsContainer;
        private GameObject _furnitureContainer;
        private ScaleMode _scaleMode;
        private float _currentScale;
        private ARAnchor _parentAnchor;

        // ──────────────────────────── Unity Lifecycle ────────────────────────────

        private void Awake()
        {
            _scaleMode = initialScaleMode;
            _currentScale = ScaleForMode(_scaleMode);
        }

        // ──────────────────────────── Public API ─────────────────────────────────

        /// <summary>
        /// Generate the 3D house model from a saved <see cref="ProjectData"/>
        /// and prepare it for AR placement.
        /// </summary>
        /// <param name="project">The project data to visualise.</param>
        public void LoadDesignForAR(ProjectData project)
        {
            if (project == null)
            {
                Debug.LogWarning("[ARHousePreview] ProjectData is null – nothing to load.");
                return;
            }

            // Destroy previous preview if one exists.
            ClearPreview();

            // Create a root container.
            _houseRoot = new GameObject("AR_HousePreview");
            _houseRoot.transform.SetParent(transform, false);

            // Generate walls / structure.
            _wallsContainer = new GameObject("Walls");
            _wallsContainer.transform.SetParent(_houseRoot.transform, false);

            // Generate furniture.
            _furnitureContainer = new GameObject("Furniture");
            _furnitureContainer.transform.SetParent(_houseRoot.transform, false);

            if (houseGenerator != null)
            {
                houseGenerator.GenerateFromProject(project, _wallsContainer.transform,
                                                   _furnitureContainer.transform);
            }
            else
            {
                Debug.LogWarning("[ARHousePreview] HouseGenerator reference missing. " +
                                 "Assign it in the Inspector.");
            }

            // Apply initial scale.
            ApplyScale();

            Debug.Log($"[ARHousePreview] Design loaded – scale mode: {_scaleMode}, " +
                      $"factor: {_currentScale:F4}");
        }

        /// <summary>
        /// Set an absolute scale factor for the preview.
        /// Automatically switches <see cref="CurrentScaleMode"/> to
        /// <see cref="ScaleMode.Custom"/>.
        /// </summary>
        /// <param name="scale">Uniform scale factor (clamped 0.001 – 10).</param>
        public void SetScale(float scale)
        {
            _scaleMode = ScaleMode.Custom;
            _currentScale = Mathf.Clamp(scale, 0.001f, 10f);
            customScale = _currentScale;
            ApplyScale();
        }

        /// <summary>
        /// Set the scale mode and apply the corresponding factor.
        /// </summary>
        /// <param name="mode">The desired scale mode.</param>
        public void SetScaleMode(ScaleMode mode)
        {
            _scaleMode = mode;
            _currentScale = ScaleForMode(mode);
            ApplyScale();
        }

        /// <summary>
        /// Rotate the house preview around the world-up (Y) axis.
        /// </summary>
        /// <param name="yRotation">Degrees to rotate.</param>
        public void SetRotation(float yRotation)
        {
            if (_houseRoot == null) return;

            var euler = _houseRoot.transform.localEulerAngles;
            euler.y = yRotation;
            _houseRoot.transform.localEulerAngles = euler;
        }

        /// <summary>
        /// Parent the preview to an <see cref="ARAnchor"/> so it stays
        /// locked in world-space.
        /// </summary>
        /// <param name="anchor">The AR anchor to attach to.</param>
        public void AttachToAnchor(ARAnchor anchor)
        {
            if (anchor == null)
            {
                Debug.LogWarning("[ARHousePreview] Anchor is null.");
                return;
            }

            _parentAnchor = anchor;

            if (_houseRoot != null)
            {
                _houseRoot.transform.SetParent(anchor.transform, true);
            }
        }

        /// <summary>Show only furniture; hide walls / structure.</summary>
        public void ShowFurnitureOnly()
        {
            SetContainerActive(_wallsContainer, false);
            SetContainerActive(_furnitureContainer, true);
        }

        /// <summary>Show only walls / structure; hide furniture.</summary>
        public void ShowHouseOnly()
        {
            SetContainerActive(_wallsContainer, true);
            SetContainerActive(_furnitureContainer, false);
        }

        /// <summary>Show both walls and furniture.</summary>
        public void ShowAll()
        {
            SetContainerActive(_wallsContainer, true);
            SetContainerActive(_furnitureContainer, true);
        }

        /// <summary>
        /// Destroy the current preview and free resources.
        /// </summary>
        public void ClearPreview()
        {
            if (_houseRoot != null)
            {
                Destroy(_houseRoot);
                _houseRoot = null;
                _wallsContainer = null;
                _furnitureContainer = null;
            }
        }

        // ──────────────────────────── Private Helpers ────────────────────────────

        private float ScaleForMode(ScaleMode mode)
        {
            return mode switch
            {
                ScaleMode.Miniature  => MiniatureScale,
                ScaleMode.RoomScale  => RoomScaleValue,
                ScaleMode.Custom     => customScale,
                _                    => MiniatureScale
            };
        }

        private void ApplyScale()
        {
            if (_houseRoot == null) return;
            _houseRoot.transform.localScale = Vector3.one * _currentScale;
        }

        private static void SetContainerActive(GameObject container, bool active)
        {
            if (container != null)
            {
                container.SetActive(active);
            }
        }
    }
}
