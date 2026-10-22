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

        private void Start()
        {
            SetupButtonListeners();
            UpdateLabels();
            CheckARAvailability();
        }

        private void SetupButtonListeners()
        {
            if (_btnCreateNew != null) _btnCreateNew.onClick.AddListener(OnCreateNewProjectClicked);
            if (_btnOpenExisting != null) _btnOpenExisting.onClick.AddListener(OnOpenExistingClicked);
            if (_btnRoomPlanner != null) _btnRoomPlanner.onClick.AddListener(OnRoomPlannerClicked);
            if (_btn3DViewer != null) _btn3DViewer.onClick.AddListener(On3DViewerClicked);
            if (_btnARPreview != null) _btnARPreview.onClick.AddListener(OnARPreviewClicked);
            if (_btnSavedDesigns != null) _btnSavedDesigns.onClick.AddListener(OnSavedDesignsClicked);
            if (_btnSettings != null) _btnSettings.onClick.AddListener(OnSettingsClicked);
            if (_btnHelp != null) _btnHelp.onClick.AddListener(OnHelpClicked);
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
            DialogUI.ShowInputDialog("Create New Project", "My Dream Home", (name) =>
            {
                if (string.IsNullOrWhiteSpace(name)) name = "Untitled Project";
                ProjectData newProject = ProjectData.CreateDefault(name);

                // Save initial
                if (ProjectPersistenceService.Instance != null)
                {
                    ProjectPersistenceService.Instance.SaveProject(newProject);
                }

                NavigationManager.Instance.SetActiveProject(newProject);
                UpdateLabels();
                NavigationManager.Instance.GoToFloorPlanner();
            });
        }

        public void OnOpenExistingClicked()
        {
            if (_projectListPanel != null)
            {
                _projectListPanel.SetActive(true);
            }
            else
            {
                // Toggle saved designs view
                OnSavedDesignsClicked();
            }
        }

        public void OnRoomPlannerClicked()
        {
            EnsureActiveProject();
            NavigationManager.Instance.GoToFloorPlanner();
        }

        public void On3DViewerClicked()
        {
            EnsureActiveProject();
            NavigationManager.Instance.GoToHouseViewer();
        }

        public void OnARPreviewClicked()
        {
            EnsureActiveProject();

            // Capability check before entering AR
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
                DialogUI.ShowMessage("House Planner Help", 
                    "• 2D Planner: Drag to draw rooms, add walls, doors, and windows.\n" +
                    "• 3D Viewer: Orbit with right-mouse / touch, toggle walls/ceilings, view furniture.\n" +
                    "• AR Preview: Scan floor, tap to place life-size or miniature house model on phone.\n" +
                    "• Storage: Projects auto-save locally to disk in JSON format.");
            }
        }

        private void EnsureActiveProject()
        {
            if (NavigationManager.CurrentProject == null)
            {
                NavigationManager.CurrentProject = SampleProjectGenerator.CreateSampleProject();
            }
        }
    }
}
