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
    /// Auto-constructs clean toolbar and status controls if missing.
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

        private void Awake()
        {
            if (_floorPlanController == null)
            {
                _floorPlanController = FindObjectOfType<FloorPlanController>();
            }

            AutoFindOrBuildUI();
        }

        private void Start()
        {
            SetupListeners();
            UpdateProjectHeader();

            if (_roomPropertiesPanel != null)
            {
                _roomPropertiesPanel.SetActive(false);
            }

            SetStatus("Select 'Draw Room' to draw rooms on the grid, or '3D View' to view the house.");
        }

        private void AutoFindOrBuildUI()
        {
            // 1. Try to find existing buttons
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("select")) _btnToolSelect = b;
                else if (n.Contains("drawroom") || n.Contains("room")) _btnToolDrawRoom = b;
                else if (n.Contains("addwall") || n.Contains("wall")) _btnToolAddWall = b;
                else if (n.Contains("adddoor") || n.Contains("door")) _btnToolAddDoor = b;
                else if (n.Contains("addwindow") || n.Contains("window")) _btnToolAddWindow = b;
                else if (n.Contains("save")) _btnSave = b;
                else if (n.Contains("view3d") || n.Contains("3d")) _btnView3D = b;
                else if (n.Contains("back") || n.Contains("menu")) _btnBackToMenu = b;
                else if (n.Contains("delete")) _btnDeleteRoom = b;
            }

            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                string n = t.gameObject.name.ToLower();
                if (n.Contains("status")) _lblStatus = t;
                else if (n.Contains("title") || n.Contains("project")) _lblProjectTitle = t;
            }

            // 2. Build UI dynamically if buttons are missing
            if (_btnToolDrawRoom == null)
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

            _btnBackToMenu = CreateButton(topBar.transform, "BtnBack", "◀ Main Menu", new Vector2(90, -28), new Vector2(140, 38), font);
            _lblProjectTitle = CreateText(topBar.transform, "Title", "2D FLOOR PLANNER", 20, TextAnchor.MiddleLeft, new Vector2(280, -28), new Vector2(300, 38), font);
            _btnSave = CreateButton(topBar.transform, "BtnSave", "💾 Save Project", new Vector2(-220, -28), new Vector2(140, 38), font, new Vector2(1, 1));
            _btnView3D = CreateButton(topBar.transform, "BtnView3D", "🏠 3D House View", new Vector2(-70, -28), new Vector2(140, 38), font, new Vector2(1, 1));

            // Left Toolbar
            GameObject toolBar = new GameObject("LeftToolbar");
            toolBar.transform.SetParent(transform, false);
            Image toolImg = toolBar.AddComponent<Image>();
            toolImg.color = new Color(0.15f, 0.17f, 0.22f, 0.95f);
            RectTransform toolRt = toolBar.GetComponent<RectTransform>();
            toolRt.anchorMin = new Vector2(0, 0);
            toolRt.anchorMax = new Vector2(0, 1);
            toolRt.pivot = new Vector2(0, 0.5f);
            toolRt.anchoredPosition = new Vector2(0, -28);
            toolRt.sizeDelta = new Vector2(160, -56);

            _btnToolSelect = CreateButton(toolBar.transform, "BtnSelect", "👆 Select / Move", new Vector2(80, -40), new Vector2(140, 42), font);
            _btnToolDrawRoom = CreateButton(toolBar.transform, "BtnDrawRoom", "📐 Draw Room", new Vector2(80, -90), new Vector2(140, 42), font);
            _btnToolAddWall = CreateButton(toolBar.transform, "BtnAddWall", "🧱 Add Wall", new Vector2(80, -140), new Vector2(140, 42), font);
            _btnToolAddDoor = CreateButton(toolBar.transform, "BtnAddDoor", "🚪 Add Door", new Vector2(80, -190), new Vector2(140, 42), font);
            _btnToolAddWindow = CreateButton(toolBar.transform, "BtnAddWindow", "🪟 Add Window", new Vector2(80, -240), new Vector2(140, 42), font);

            // Bottom Status Bar
            GameObject bottomBar = new GameObject("BottomBar");
            bottomBar.transform.SetParent(transform, false);
            Image btmImg = bottomBar.AddComponent<Image>();
            btmImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            RectTransform btmRt = bottomBar.GetComponent<RectTransform>();
            btmRt.anchorMin = new Vector2(0, 0);
            btmRt.anchorMax = new Vector2(1, 0);
            btmRt.pivot = new Vector2(0.5f, 0);
            btmRt.anchoredPosition = Vector2.zero;
            btmRt.sizeDelta = new Vector2(0, 36);

            _lblStatus = CreateText(bottomBar.transform, "LblStatus", "Ready", 14, TextAnchor.MiddleLeft, new Vector2(180, 18), new Vector2(800, 30), font);
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
                if (_roomPropertiesPanel != null) _roomPropertiesPanel.SetActive(false);
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
