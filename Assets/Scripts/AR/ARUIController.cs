using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HousePlanner.AR
{
    /// <summary>
    /// Describes which content layer is visible in the AR preview.
    /// </summary>
    public enum ARViewMode
    {
        /// <summary>Display both the house structure and furniture.</summary>
        All,
        /// <summary>Display only furniture items.</summary>
        FurnitureOnly,
        /// <summary>Display only the house walls / floors / roof.</summary>
        HouseOnly
    }

    /// <summary>
    /// MonoBehaviour that drives the on-screen AR mode UI: status text,
    /// scanning instructions, placement controls, scale/rotation sliders,
    /// and mode toggles. Delegates all AR operations to
    /// <see cref="ARPlacementController"/>.
    /// </summary>
    public class ARUIController : MonoBehaviour
    {
        // ──────────────────────────── Inspector Fields ────────────────────────────

        [Header("References")]
        [SerializeField] private ARPlacementController placementController;
        [SerializeField] private ARHousePreview housePreview;

        [Header("UI Elements – Text")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text scaleLabel;

        [Header("UI Elements – Buttons")]
        [SerializeField] private Button tapToPlaceButton;
        [SerializeField] private Button rotateLeftButton;
        [SerializeField] private Button rotateRightButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button removeObjectButton;

        [Header("UI Elements – Controls")]
        [SerializeField] private Slider scaleSlider;
        [SerializeField] private Toggle viewModeToggle;

        [Header("UI Panels")]
        [SerializeField] private GameObject scanningPanel;
        [SerializeField] private GameObject placementPanel;
        [SerializeField] private GameObject controlsPanel;

        [Header("Settings")]
        [SerializeField] private float rotationStep = 15f;
        [SerializeField] private float minScale = 0.01f;
        [SerializeField] private float maxScale = 1.0f;

        // ──────────────────────────── Events ─────────────────────────────────────

        /// <summary>Raised when the user presses the Back button.</summary>
        public event Action OnBackRequested;

        // ──────────────────────────── Private State ──────────────────────────────

        private ARViewMode _currentViewMode = ARViewMode.All;

        // ──────────────────────────── Unity Lifecycle ────────────────────────────

        private void Awake()
        {
            BindButtons();
            ConfigureScaleSlider();
        }

        private void OnEnable()
        {
            if (placementController != null)
            {
                placementController.OnARStateChanged += HandleARStateChanged;
                placementController.OnStatusMessage  += HandleStatusMessage;
            }
        }

        private void OnDisable()
        {
            if (placementController != null)
            {
                placementController.OnARStateChanged -= HandleARStateChanged;
                placementController.OnStatusMessage  -= HandleStatusMessage;
            }
        }

        private void Update()
        {
            HandleTouchInput();
        }

        // ──────────────────────────── Public API ─────────────────────────────────

        /// <summary>
        /// Programmatically set the status text shown to the user.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void SetStatusText(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        /// <summary>
        /// Set the instruction banner text.
        /// </summary>
        /// <param name="message">Instruction string.</param>
        public void SetInstructionText(string message)
        {
            if (instructionText != null) instructionText.text = message;
        }

        /// <summary>
        /// Cycle through view modes: All → FurnitureOnly → HouseOnly → All.
        /// </summary>
        public void CycleViewMode()
        {
            _currentViewMode = _currentViewMode switch
            {
                ARViewMode.All           => ARViewMode.FurnitureOnly,
                ARViewMode.FurnitureOnly => ARViewMode.HouseOnly,
                ARViewMode.HouseOnly     => ARViewMode.All,
                _                        => ARViewMode.All
            };

            ApplyViewMode();
        }

        // ──────────────────────────── UI Callbacks ───────────────────────────────

        private void OnTapToPlaceClicked()
        {
            if (placementController != null)
            {
                placementController.OnTapToPlace();
            }
        }

        private void OnRotateLeft()
        {
            if (placementController != null)
            {
                placementController.RotateObject(-rotationStep);
            }
        }

        private void OnRotateRight()
        {
            if (placementController != null)
            {
                placementController.RotateObject(rotationStep);
            }
        }

        private void OnScaleChanged(float value)
        {
            float scale = Mathf.Lerp(minScale, maxScale, value);

            if (placementController != null)
            {
                placementController.ScaleObject(scale);
            }

            if (housePreview != null)
            {
                housePreview.SetScale(scale);
            }

            UpdateScaleLabel(scale);
        }

        private void OnResetClicked()
        {
            if (placementController != null)
            {
                placementController.ResetAR();
            }

            if (scaleSlider != null)
            {
                scaleSlider.value = scaleSlider.minValue;
            }
        }

        private void OnBackClicked()
        {
            OnBackRequested?.Invoke();
        }

        private void OnRemoveObjectClicked()
        {
            if (placementController != null)
            {
                placementController.RemoveObject();
            }
        }

        private void OnViewModeToggled(bool isOn)
        {
            CycleViewMode();
        }

        // ──────────────────────────── State Handling ─────────────────────────────

        private void HandleARStateChanged(ARState state)
        {
            switch (state)
            {
                case ARState.Initializing:
                    ShowPanel(scanningPanel);
                    HidePanel(placementPanel);
                    HidePanel(controlsPanel);
                    SetInstructionText("Initialising AR…");
                    break;

                case ARState.Scanning:
                    ShowPanel(scanningPanel);
                    HidePanel(placementPanel);
                    HidePanel(controlsPanel);
                    SetInstructionText("Move your phone slowly to scan the floor.");
                    break;

                case ARState.PlaneDetected:
                    ShowPanel(scanningPanel);
                    ShowPanel(placementPanel);
                    HidePanel(controlsPanel);
                    SetInstructionText("Tap to place your design on the detected surface.");
                    break;

                case ARState.ObjectPlaced:
                    HidePanel(scanningPanel);
                    HidePanel(placementPanel);
                    ShowPanel(controlsPanel);
                    SetInstructionText(string.Empty);
                    break;

                case ARState.Error:
                    ShowPanel(scanningPanel);
                    HidePanel(placementPanel);
                    HidePanel(controlsPanel);
                    SetInstructionText("Tracking lost. Move your device slowly.");
                    break;

                case ARState.Unsupported:
                    ShowPanel(scanningPanel);
                    HidePanel(placementPanel);
                    HidePanel(controlsPanel);
                    SetInstructionText("AR is not supported on this device.");
                    break;
            }
        }

        private void HandleStatusMessage(string message)
        {
            SetStatusText(message);
        }

        // ──────────────────────────── Touch Input ────────────────────────────────

        private void HandleTouchInput()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount == 0) return;
            var touch = Input.GetTouch(0);

            // Ignore touches over UI elements.
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (placementController == null) return;

            switch (placementController.CurrentState)
            {
                case ARState.PlaneDetected when touch.phase == TouchPhase.Began:
                    placementController.OnTapToPlace();
                    break;

                case ARState.ObjectPlaced when touch.phase == TouchPhase.Moved:
                    placementController.MoveObject(touch.position);
                    break;
            }
#endif
        }

        // ──────────────────────────── View Mode ──────────────────────────────────

        private void ApplyViewMode()
        {
            if (housePreview == null) return;

            switch (_currentViewMode)
            {
                case ARViewMode.All:
                    housePreview.ShowAll();
                    SetStatusText("Showing all");
                    break;
                case ARViewMode.FurnitureOnly:
                    housePreview.ShowFurnitureOnly();
                    SetStatusText("Furniture only");
                    break;
                case ARViewMode.HouseOnly:
                    housePreview.ShowHouseOnly();
                    SetStatusText("House structure only");
                    break;
            }
        }

        // ──────────────────────────── Helpers ────────────────────────────────────

        private void BindButtons()
        {
            if (tapToPlaceButton != null)    tapToPlaceButton.onClick.AddListener(OnTapToPlaceClicked);
            if (rotateLeftButton != null)    rotateLeftButton.onClick.AddListener(OnRotateLeft);
            if (rotateRightButton != null)   rotateRightButton.onClick.AddListener(OnRotateRight);
            if (resetButton != null)         resetButton.onClick.AddListener(OnResetClicked);
            if (backButton != null)          backButton.onClick.AddListener(OnBackClicked);
            if (removeObjectButton != null)  removeObjectButton.onClick.AddListener(OnRemoveObjectClicked);
            if (viewModeToggle != null)      viewModeToggle.onValueChanged.AddListener(OnViewModeToggled);
        }

        private void ConfigureScaleSlider()
        {
            if (scaleSlider == null) return;

            scaleSlider.minValue = 0f;
            scaleSlider.maxValue = 1f;
            scaleSlider.value    = 0f; // starts at minScale
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);

            UpdateScaleLabel(minScale);
        }

        private void UpdateScaleLabel(float scale)
        {
            if (scaleLabel == null) return;

            if (scale < 0.1f)
            {
                int ratio = Mathf.RoundToInt(1f / scale);
                scaleLabel.text = $"Scale 1:{ratio}";
            }
            else
            {
                scaleLabel.text = $"Scale {scale:F2}x";
            }
        }

        private static void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        private static void HidePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
