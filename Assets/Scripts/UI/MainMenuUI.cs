using System;
using UnityEngine;
using UnityEngine.UI;
using HousePlanner.Core;
using HousePlanner.Persistence;
using HousePlanner.AR;

namespace HousePlanner.UI
{
    /// <summary>
    /// UI Controller for the application Landing / Home screen.
    /// Manages top-level navigation, project creation/opening, and feature entry points.
    /// Auto-binds UI buttons and handles seamless scene transitions.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Top Navigation Buttons")]
        [SerializeField] private Button _btnCreateNew;
        [SerializeField] private Button _btnOpenExisting;
        [SerializeField] private Button _btnRoomPlanner;
        [SerializeField] private Button _btn3DViewer;
        [SerializeField] private Button _btnARPreview;
        [SerializeField] private Button _btnSavedDesigns;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnHelp;

        [Header("Panels")]
        [SerializeField] private GameObject _projectListPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _helpPanel;

        [Header("Status Labels")]
        [SerializeField] private Text _lblCurrentProject;
        [SerializeField] private Text _lblAppVersion;
        [SerializeField] private Text _lblARStatus;

        private void Awake()
        {
            AutoFindReferences();
        }

        private void Start()
        {
            SetupButtonListeners();
            UpdateLabels();
            CheckARAvailability();
        }

        private void AutoFindReferences()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("createnew") || n == "btncreate") _btnCreateNew = b;
                else if (n.Contains("openexisting") || n == "btnopen") _btnOpenExisting = b;
                else if (n.Contains("roomplanner") || n == "btnplanner") _btnRoomPlanner = b;
                else if (n.Contains("3dviewer") || n == "btnviewer") _btn3DViewer = b;
                else if (n.Contains("arpreview") || n == "btnar") _btnARPreview = b;
                else if (n.Contains("saveddesigns") || n == "btnsaved") _btnSavedDesigns = b;
                else if (n.Contains("settings")) _btnSettings = b;
                else if (n.Contains("help")) _btnHelp = b;
            }

            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                string n = t.gameObject.name.ToLower();
                if (n.Contains("version")) _lblAppVersion = t;
                else if (n.Contains("currentproject") || n.Contains("active")) _lblCurrentProject = t;
                else if (n.Contains("arstatus")) _lblARStatus = t;
            }
        }

        private void SetupButtonListeners()
        {
            if (_btnCreateNew != null)
            {
                _btnCreateNew.onClick.RemoveAllListeners();
                _btnCreateNew.onClick.AddListener(OnCreateNewProjectClicked);
            }
            if (_btnOpenExisting != null)
            {
                _btnOpenExisting.onClick.RemoveAllListeners();
                _btnOpenExisting.onClick.AddListener(OnOpenExistingClicked);
            }
            if (_btnRoomPlanner != null)
            {
                _btnRoomPlanner.onClick.RemoveAllListeners();
                _btnRoomPlanner.onClick.AddListener(OnRoomPlannerClicked);
            }
            if (_btn3DViewer != null)
            {
                _btn3DViewer.onClick.RemoveAllListeners();
                _btn3DViewer.onClick.AddListener(On3DViewerClicked);
            }
            if (_btnARPreview != null)
            {
                _btnARPreview.onClick.RemoveAllListeners();
                _btnARPreview.onClick.AddListener(OnARPreviewClicked);
            }
            if (_btnSavedDesigns != null)
            {
                _btnSavedDesigns.onClick.RemoveAllListeners();
                _btnSavedDesigns.onClick.AddListener(OnSavedDesignsClicked);
            }
            if (_btnSettings != null)
            {
                _btnSettings.onClick.RemoveAllListeners();
                _btnSettings.onClick.AddListener(OnSettingsClicked);
            }
            if (_btnHelp != null)
            {
                _btnHelp.onClick.RemoveAllListeners();
                _btnHelp.onClick.AddListener(OnHelpClicked);
            }
        }

        private void UpdateLabels()
        {
            if (_lblAppVersion != null)
            {
                _lblAppVersion.text = "AR/VR House Planner v1.0.0 (Unity 6 LTS)";
            }

            if (_lblCurrentProject != null)
            {
                ProjectData current = NavigationManager.CurrentProject;
                _lblCurrentProject.text = current != null ? $"Active: {current.projectName}" : "No Active Project";
            }
        }

        private void CheckARAvailability()
        {
            if (_lblARStatus != null)
            {
                var status = ARCapabilityChecker.CheckARSupport();
                _lblARStatus.text = status == ARSupportStatus.Supported 
                    ? "✓ AR Ready" 
                    : $"AR: {status}";
            }
        }

        public void OnCreateNewProjectClicked()
        {
            Debug.Log("[MainMenuUI] Create New Project clicked. Opening 2D Floor Planner...");
            ProjectData newProject = ProjectData.CreateDefault("New House Project");
            if (ProjectPersistenceService.Instance != null)
            {
                ProjectPersistenceService.Instance.SaveProject(newProject);
            }
            NavigationManager.Instance.SetActiveProject(newProject);
            UpdateLabels();
            NavigationManager.Instance.GoToFloorPlanner();
        }

        public void OnOpenExistingClicked()
        {
            Debug.Log("[MainMenuUI] Open Existing Project clicked. Loading Sample Project...");
            ProjectData projectToOpen = null;
            if (ProjectPersistenceService.Instance != null)
            {
                var summaries = ProjectPersistenceService.Instance.LoadAllProjectSummaries();
                if (summaries != null && summaries.Count > 0)
                {
                    projectToOpen = ProjectPersistenceService.Instance.LoadProject(summaries[0].projectId);
                }
            }

            if (projectToOpen == null)
            {
                projectToOpen = SampleProjectGenerator.CreateSampleProject();
                if (ProjectPersistenceService.Instance != null)
                {
                    ProjectPersistenceService.Instance.SaveProject(projectToOpen);
                }
            }

            NavigationManager.Instance.SetActiveProject(projectToOpen);
            UpdateLabels();
            NavigationManager.Instance.GoToHouseViewer();
        }

        public void OnRoomPlannerClicked()
        {
            Debug.Log("[MainMenuUI] 2D Room Planner clicked. Loading FloorPlanner scene...");
            EnsureActiveProject();
            NavigationManager.Instance.GoToFloorPlanner();
        }

        public void On3DViewerClicked()
        {
            Debug.Log("[MainMenuUI] 3D House Viewer clicked. Loading HouseViewer scene...");
            EnsureActiveProject();
            NavigationManager.Instance.GoToHouseViewer();
        }

        public void OnARPreviewClicked()
        {
            Debug.Log("[MainMenuUI] AR Preview clicked.");
            EnsureActiveProject();

            var status = ARCapabilityChecker.CheckARSupport();
            if (status == ARSupportStatus.Unsupported && Application.platform == RuntimePlatform.Android)
            {
                DialogUI.ShowError("AR Not Supported", 
                    "This device does not meet the Google ARCore hardware or software requirements.\n\nYou can still use the 3D Viewer mode!");
                return;
            }

            NavigationManager.Instance.GoToARView();
        }

        public void OnSavedDesignsClicked()
        {
            if (_projectListPanel != null)
            {
                _projectListPanel.SetActive(!_projectListPanel.activeSelf);
            }
            else
            {
                OnOpenExistingClicked();
            }
        }

        public void OnSettingsClicked()
        {
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(!_settingsPanel.activeSelf);
            }
        }

        public void OnHelpClicked()
        {
            if (_helpPanel != null)
            {
                _helpPanel.SetActive(!_helpPanel.activeSelf);
            }
            else
            {
                DialogUI.ShowMessage("House Planner Quick Guide", 
                    "• 2D Room Planner: Draw rooms, drag walls to resize, add doors & windows.\n" +
                    "• 3D House Viewer: Inspect procedural 3D model, walk inside, orbit, place furniture.\n" +
                    "• AR Room Preview: Project floor plan life-size or miniature on detected real-world surfaces.\n" +
                    "• Save & Load: Designs save to local storage in JSON format.");
            }
        }

        private void EnsureActiveProject()
        {
            if (NavigationManager.CurrentProject == null)
            {
                NavigationManager.CurrentProject = SampleProjectGenerator.CreateSampleProject();
                if (ProjectPersistenceService.Instance != null)
                {
                    ProjectPersistenceService.Instance.SaveProject(NavigationManager.CurrentProject);
                }
            }
        }
    }
}
