using System;
using UnityEngine;
using UnityEngine.UI;
using HousePlanner.Core;
using HousePlanner.FloorPlan;
using HousePlanner.Persistence;

namespace HousePlanner.UI
{
    /// <summary>
    /// UI Controller for the 2D Floor Plan editor screen.
    /// Hosts editing tool selection, room property controls, dimensions display,
    /// grid snap toggles, and transitions to 3D and AR viewers.
    /// </summary>
    public class FloorPlanUI : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private FloorPlanController _floorPlanController;

        [Header("Tool Selection Buttons")]
        [SerializeField] private Button _btnToolSelect;
        [SerializeField] private Button _btnToolDrawRoom;
        [SerializeField] private Button _btnToolAddWall;
        [SerializeField] private Button _btnToolAddDoor;
        [SerializeField] private Button _btnToolAddWindow;

        [Header("Room Properties Panel")]
        [SerializeField] private GameObject _roomPropertiesPanel;
        [SerializeField] private InputField _txtRoomName;
        [SerializeField] private InputField _txtRoomWidth;
        [SerializeField] private InputField _txtRoomLength;
        [SerializeField] private Dropdown _ddFloorMaterial;
        [SerializeField] private Button _btnDeleteRoom;

        [Header("Toggles & View Options")]
        [SerializeField] private Toggle _toggleSnapGrid;
        [SerializeField] private Toggle _toggleShowMeasurements;

        [Header("Header Actions")]
        [SerializeField] private Button _btnSave;
        [SerializeField] private Button _btnView3D;
        [SerializeField] private Button _btnBackToMenu;
        [SerializeField] private Text _lblStatus;
        [SerializeField] private Text _lblProjectTitle;

        private RoomData _activeSelectedRoom;

        private void Start()
        {
            if (_floorPlanController == null)
            {
                _floorPlanController = FindFirstObjectByType<FloorPlanController>();
            }

            SetupListeners();
            UpdateProjectHeader();

            if (_roomPropertiesPanel != null)
            {
                _roomPropertiesPanel.SetActive(false);
            }

            SetStatus("Select a tool from the left toolbar to begin designing.");
        }

        private void SetupListeners()
        {
            if (_btnToolSelect != null) _btnToolSelect.onClick.AddListener(() => SetTool(FloorPlanController.EditingMode.Select, "Select & Move Mode: Click on any room or wall to edit."));
            if (_btnToolDrawRoom != null) _btnToolDrawRoom.onClick.AddListener(() => SetTool(FloorPlanController.EditingMode.PlaceRoom, "Draw Room Mode: Click and drag on the floor plan to size a room."));
            if (_btnToolAddWall != null) _btnToolAddWall.onClick.AddListener(() => SetTool(FloorPlanController.EditingMode.DrawWall, "Add Wall Mode: Click start and end points to draw a wall."));
            if (_btnToolAddDoor != null) _btnToolAddDoor.onClick.AddListener(() => SetTool(FloorPlanController.EditingMode.PlaceDoor, "Add Door Mode: Click on any wall to place a doorway."));
            if (_btnToolAddWindow != null) _btnToolAddWindow.onClick.AddListener(() => SetTool(FloorPlanController.EditingMode.PlaceWindow, "Add Window Mode: Click on any wall to place a window."));

            if (_toggleSnapGrid != null)
            {
                _toggleSnapGrid.onValueChanged.AddListener((val) =>
                {
                    if (NavigationManager.CurrentProject != null && NavigationManager.CurrentProject.floorPlanSettings != null)
                    {
                        NavigationManager.CurrentProject.floorPlanSettings.snapToGrid = val;
                    }
                });
            }

            if (_btnSave != null) _btnSave.onClick.AddListener(SaveCurrentProject);
            if (_btnView3D != null) _btnView3D.onClick.AddListener(OnView3DClicked);
            if (_btnBackToMenu != null) _btnBackToMenu.onClick.AddListener(OnBackClicked);

            if (_txtRoomName != null) _txtRoomName.onEndEdit.AddListener(OnRoomNameEdited);
            if (_txtRoomWidth != null) _txtRoomWidth.onEndEdit.AddListener(OnRoomWidthEdited);
            if (_txtRoomLength != null) _txtRoomLength.onEndEdit.AddListener(OnRoomLengthEdited);
            if (_btnDeleteRoom != null) _btnDeleteRoom.onClick.AddListener(OnDeleteRoomClicked);

            if (_floorPlanController != null)
            {
                _floorPlanController.OnRoomSelected += HandleRoomSelected;
            }
        }

        private void UpdateProjectHeader()
        {
            if (_lblProjectTitle != null && NavigationManager.CurrentProject != null)
            {
                _lblProjectTitle.text = NavigationManager.CurrentProject.projectName;
            }
        }

        private void SetTool(FloorPlanController.EditingMode mode, string statusMessage)
        {
            if (_floorPlanController != null)
            {
                _floorPlanController.SetMode(mode);
            }
            SetStatus(statusMessage);
        }

        public void SetStatus(string message)
        {
            if (_lblStatus != null)
            {
                _lblStatus.text = message;
            }
        }

        private void HandleRoomSelected(RoomData room)
        {
            _activeSelectedRoom = room;

            if (_roomPropertiesPanel == null) return;

            if (room == null)
            {
                _roomPropertiesPanel.SetActive(false);
                return;
            }

            _roomPropertiesPanel.SetActive(true);
            if (_txtRoomName != null) _txtRoomName.text = room.roomName;
            if (_txtRoomWidth != null) _txtRoomWidth.text = room.width.ToString("F1");
            if (_txtRoomLength != null) _txtRoomLength.text = room.length.ToString("F1");
        }

        private void OnRoomNameEdited(string newName)
        {
            if (_activeSelectedRoom != null && !string.IsNullOrWhiteSpace(newName))
            {
                _activeSelectedRoom.roomName = newName;
                if (_floorPlanController != null) _floorPlanController.UpdateRoom(_activeSelectedRoom);
            }
        }

        private void OnRoomWidthEdited(string text)
        {
            if (_activeSelectedRoom != null && float.TryParse(text, out float w) && w > 0.5f)
            {
                _activeSelectedRoom.width = w;
                if (_floorPlanController != null) _floorPlanController.UpdateRoom(_activeSelectedRoom);
            }
        }

        private void OnRoomLengthEdited(string text)
        {
            if (_activeSelectedRoom != null && float.TryParse(text, out float l) && l > 0.5f)
            {
                _activeSelectedRoom.length = l;
                if (_floorPlanController != null) _floorPlanController.UpdateRoom(_activeSelectedRoom);
            }
        }

        private void OnDeleteRoomClicked()
        {
            if (_activeSelectedRoom == null || _floorPlanController == null) return;

            DialogUI.ShowConfirmation("Delete Room", 
                $"Are you sure you want to delete \"{_activeSelectedRoom.roomName}\"?", () =>
            {
                _floorPlanController.RemoveRoom(_activeSelectedRoom.roomId);
                _activeSelectedRoom = null;
                _roomPropertiesPanel.SetActive(false);
            });
        }

        public void SaveCurrentProject()
        {
            if (NavigationManager.CurrentProject == null || ProjectPersistenceService.Instance == null) return;

            bool success = ProjectPersistenceService.Instance.SaveProject(NavigationManager.CurrentProject);
            if (success)
            {
                SetStatus($"✓ Project '{NavigationManager.CurrentProject.projectName}' saved successfully.");
                DialogUI.ShowMessage("Saved", "Project saved to local storage.");
            }
            else
            {
                DialogUI.ShowError("Save Failed", "Could not write project file to storage.");
            }
        }

        public void OnView3DClicked()
        {
            SaveCurrentProject();
            NavigationManager.Instance.GoToHouseViewer();
        }

        public void OnBackClicked()
        {
            SaveCurrentProject();
            NavigationManager.Instance.GoToMainMenu();
        }
    }
}
