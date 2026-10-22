using System.IO;
using UnityEngine;
using UnityEngine.UI;
using HousePlanner.Core;
using HousePlanner.HouseGeneration;
using HousePlanner.Furniture;

namespace HousePlanner.UI
{
    /// <summary>
    /// UI Controller for the 3D House Viewer screen.
    /// Manages camera view mode switching (Orbit, Fly, Walk), wall/floor/ceiling visibility,
    /// interior view mode toggle, opening the furniture catalogue, taking screenshots, and returning to 2D floor planner.
    /// </summary>
    public class HouseViewerUI : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private HouseGenerator _houseGenerator;
        [SerializeField] private HouseViewCamera _viewCamera;

        [Header("Camera Mode Buttons")]
        [SerializeField] private Button _btnOrbitMode;
        [SerializeField] private Button _btnFlyMode;
        [SerializeField] private Button _btnWalkMode;

        [Header("Visibility Toggles")]
        [SerializeField] private Toggle _toggleWalls;
        [SerializeField] private Toggle _toggleFloors;
        [SerializeField] private Toggle _toggleCeilings;
        [SerializeField] private Toggle _toggleInteriorView;

        [Header("Actions")]
        [SerializeField] private Button _btnRegenerate;
        [SerializeField] private Button _btnFurnitureCatalogue;
        [SerializeField] private Button _btnScreenshot;
        [SerializeField] private Button _btnBackToPlanner;
        [SerializeField] private Button _btnMainMenu;

        [Header("Sub-Panels")]
        [SerializeField] private GameObject _furnitureCataloguePanel;

        [Header("Labels")]
        [SerializeField] private Text _lblProjectName;
        [SerializeField] private Text _lblCurrentMode;

        private void Start()
        {
            if (_houseGenerator == null) _houseGenerator = FindFirstObjectByType<HouseGenerator>();
            if (_viewCamera == null) _viewCamera = FindFirstObjectByType<HouseViewCamera>();

            SetupListeners();
            InitializeHouse();
            UpdateLabels();
        }

        private void InitializeHouse()
        {
            if (_houseGenerator != null && NavigationManager.CurrentProject != null)
            {
                _houseGenerator.GenerateHouse(NavigationManager.CurrentProject);
            }

            if (FurniturePlacementController.Instance != null && NavigationManager.CurrentProject != null)
            {
                FurniturePlacementController.Instance.BindProject(NavigationManager.CurrentProject);
            }
        }

        private void SetupListeners()
        {
            if (_btnOrbitMode != null) _btnOrbitMode.onClick.AddListener(() => SetCameraMode(CameraViewMode.Orbit));
            if (_btnFlyMode != null) _btnFlyMode.onClick.AddListener(() => SetCameraMode(CameraViewMode.Fly));
            if (_btnWalkMode != null) _btnWalkMode.onClick.AddListener(() => SetCameraMode(CameraViewMode.Walk));

            if (_toggleWalls != null)
            {
                _toggleWalls.isOn = true;
                _toggleWalls.onValueChanged.AddListener((val) => { if (_houseGenerator != null) _houseGenerator.ShowWalls = val; });
            }

            if (_toggleFloors != null)
            {
                _toggleFloors.isOn = true;
                _toggleFloors.onValueChanged.AddListener((val) => { if (_houseGenerator != null) _houseGenerator.ShowFloors = val; });
            }

            if (_toggleCeilings != null)
            {
                _toggleCeilings.isOn = false;
                _toggleCeilings.onValueChanged.AddListener((val) => { if (_houseGenerator != null) _houseGenerator.ShowCeilings = val; });
            }

            if (_toggleInteriorView != null)
            {
                _toggleInteriorView.isOn = true;
                _toggleInteriorView.onValueChanged.AddListener((val) => { if (_houseGenerator != null) _houseGenerator.InteriorViewMode = val; });
            }

            if (_btnRegenerate != null) _btnRegenerate.onClick.AddListener(OnRegenerateClicked);
            if (_btnFurnitureCatalogue != null) _btnFurnitureCatalogue.onClick.AddListener(ToggleFurnitureCatalogue);
            if (_btnScreenshot != null) _btnScreenshot.onClick.AddListener(TakeScreenshot);
            if (_btnBackToPlanner != null) _btnBackToPlanner.onClick.AddListener(OnBackToPlannerClicked);
            if (_btnMainMenu != null) _btnMainMenu.onClick.AddListener(OnMainMenuClicked);
        }

        private void UpdateLabels()
        {
            if (_lblProjectName != null && NavigationManager.CurrentProject != null)
            {
                _lblProjectName.text = NavigationManager.CurrentProject.projectName;
            }
            if (_lblCurrentMode != null && _viewCamera != null)
            {
                _lblCurrentMode.text = $"Mode: {_viewCamera.Mode}";
            }
        }

        private void SetCameraMode(CameraViewMode mode)
        {
            if (_viewCamera != null)
            {
                _viewCamera.Mode = mode;
                UpdateLabels();
            }
        }

        private void OnRegenerateClicked()
        {
            if (_houseGenerator != null && NavigationManager.CurrentProject != null)
            {
                _houseGenerator.GenerateHouse(NavigationManager.CurrentProject);
            }
        }

        private void ToggleFurnitureCatalogue()
        {
            if (_furnitureCataloguePanel != null)
            {
                _furnitureCataloguePanel.SetActive(!_furnitureCataloguePanel.activeSelf);
            }
        }

        private void TakeScreenshot()
        {
            string folder = Application.persistentDataPath + "/Screenshots";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string path = $"{folder}/House_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(path);
            DialogUI.ShowMessage("Screenshot Saved", $"Saved to:\n{path}");
        }

        private void OnBackToPlannerClicked()
        {
            NavigationManager.Instance.GoToFloorPlanner();
        }

        private void OnMainMenuClicked()
        {
            NavigationManager.Instance.GoToMainMenu();
        }
    }
}
