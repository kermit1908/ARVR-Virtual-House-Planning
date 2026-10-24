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
    /// Automatically builds clean, interactive controls if missing.
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

        private void Awake()
        {
            if (_houseGenerator == null) _houseGenerator = FindObjectOfType<HouseGenerator>();
            if (_viewCamera == null) _viewCamera = FindObjectOfType<HouseViewCamera>();

            AutoFindOrBuildUI();
        }

        private void Start()
        {
            SetupListeners();
            InitializeHouse();
            UpdateLabels();
        }

        private void AutoFindOrBuildUI()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("orbit")) _btnOrbitMode = b;
                else if (n.Contains("fly")) _btnFlyMode = b;
                else if (n.Contains("walk")) _btnWalkMode = b;
                else if (n.Contains("regenerate")) _btnRegenerate = b;
                else if (n.Contains("catalogue") || n.Contains("furniture")) _btnFurnitureCatalogue = b;
                else if (n.Contains("screenshot")) _btnScreenshot = b;
                else if (n.Contains("planner")) _btnBackToPlanner = b;
                else if (n.Contains("menu")) _btnMainMenu = b;
            }

            if (_btnOrbitMode == null)
            {
                BuildRuntimeToolbar();
            }
        }

        private void BuildRuntimeToolbar()
        {
            Font font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Top Bar
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(transform, false);
            Image topImg = topBar.AddComponent<Image>();
            topImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            RectTransform topRt = topBar.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0, 1);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.pivot = new Vector2(0.5f, 1);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0, 56);

            _btnBackToPlanner = CreateButton(topBar.transform, "BtnPlanner", "◀ 2D Planner", new Vector2(90, -28), new Vector2(140, 38), font);
            _btnMainMenu = CreateButton(topBar.transform, "BtnMenu", "🏠 Main Menu", new Vector2(245, -28), new Vector2(140, 38), font);
            _lblProjectName = CreateText(topBar.transform, "Title", "3D HOUSE VIEWER", 20, TextAnchor.MiddleLeft, new Vector2(430, -28), new Vector2(300, 38), font);
            _btnRegenerate = CreateButton(topBar.transform, "BtnRegen", "🔄 Regenerate", new Vector2(-220, -28), new Vector2(140, 38), font, new Vector2(1, 1));
            _btnScreenshot = CreateButton(topBar.transform, "BtnShot", "📸 Screenshot", new Vector2(-70, -28), new Vector2(140, 38), font, new Vector2(1, 1));

            // Left Camera Mode Bar
            GameObject modeBar = new GameObject("ModeBar");
            modeBar.transform.SetParent(transform, false);
            Image modeImg = modeBar.AddComponent<Image>();
            modeImg.color = new Color(0.15f, 0.17f, 0.22f, 0.95f);
            RectTransform modeRt = modeBar.GetComponent<RectTransform>();
            modeRt.anchorMin = new Vector2(0, 0);
            modeRt.anchorMax = new Vector2(0, 1);
            modeRt.pivot = new Vector2(0, 0.5f);
            modeRt.anchoredPosition = new Vector2(0, -28);
            modeRt.sizeDelta = new Vector2(160, -56);

            _btnOrbitMode = CreateButton(modeBar.transform, "BtnOrbit", "🔄 Orbit Mode", new Vector2(80, -40), new Vector2(140, 42), font);
            _btnFlyMode = CreateButton(modeBar.transform, "BtnFly", "✈️ Fly Mode", new Vector2(80, -90), new Vector2(140, 42), font);
            _btnWalkMode = CreateButton(modeBar.transform, "BtnWalk", "🚶 Walk (1.7m)", new Vector2(80, -140), new Vector2(140, 42), font);

            // Right Toggle Bar
            GameObject toggleBar = new GameObject("ToggleBar");
            toggleBar.transform.SetParent(transform, false);
            Image togImg = toggleBar.AddComponent<Image>();
            togImg.color = new Color(0.15f, 0.17f, 0.22f, 0.95f);
            RectTransform togRt = toggleBar.GetComponent<RectTransform>();
            togRt.anchorMin = new Vector2(1, 0);
            togRt.anchorMax = new Vector2(1, 1);
            togRt.pivot = new Vector2(1, 0.5f);
            togRt.anchoredPosition = new Vector2(0, -28);
            togRt.sizeDelta = new Vector2(160, -56);

            _btnFurnitureCatalogue = CreateButton(toggleBar.transform, "BtnCatalogue", "🛋️ Furniture", new Vector2(-80, -40), new Vector2(140, 42), font, new Vector2(1, 1));
            
            // Mode Label
            _lblCurrentMode = CreateText(modeBar.transform, "LblMode", "Mode: Orbit", 13, TextAnchor.MiddleCenter, new Vector2(80, -200), new Vector2(140, 30), font);
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Font font, Vector2? anchor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.24f, 0.35f, 0.52f, 1f);
            Button btn = go.AddComponent<Button>();
            RectTransform rt = go.GetComponent<RectTransform>();
            if (anchor.HasValue)
            {
                rt.anchorMin = anchor.Value;
                rt.anchorMax = anchor.Value;
            }
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            CreateText(go.transform, "Text", label, 14, TextAnchor.MiddleCenter, Vector2.zero, size, font);
            return btn;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 pos, Vector2 size, Font font)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = Color.white;
            txt.font = font;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return txt;
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
