using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Furniture
{
    public enum PlacementState
    {
        Idle,
        PlacingNew,
        DraggingSelected,
        RotatingSelected
    }

    /// <summary>
    /// Controls placement, manipulation, and editing of furniture in 3D and floor views.
    /// Manages raycasting against floors, grid snapping, and coordinates with UI inspector.
    /// </summary>
    public class FurniturePlacementController : MonoBehaviour
    {
        private static FurniturePlacementController _instance;
        public static FurniturePlacementController Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private float _gridSize = 0.5f;
        [SerializeField] private bool _snapToGrid = false;
        [SerializeField] private LayerMask _floorLayerMask = ~0;

        [Header("Runtime State")]
        [SerializeField] private PlacementState _state = PlacementState.Idle;
        private FurnitureHandle _selectedHandle;
        private GameObject _ghostObject;
        private string _pendingCatalogueId;
        private Camera _mainCamera;
        private ProjectData _currentProject;
        private readonly List<FurnitureHandle> _activeHandles = new List<FurnitureHandle>();

        public PlacementState State => _state;
        public FurnitureHandle SelectedHandle => _selectedHandle;
        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => _snapToGrid = value;
        }

        public event Action<FurnitureHandle> OnFurnitureSelected;
        public event Action<FurnitureHandle> OnFurniturePlaced;
        public event Action<FurnitureHandle> OnFurnitureModified;
        public event Action<string> OnFurnitureDeleted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        public void BindProject(ProjectData project)
        {
            _currentProject = project;
            ClearAllPlaced();

            if (_currentProject != null && _currentProject.furnitureItems != null)
            {
                foreach (var item in _currentProject.furnitureItems)
                {
                    SpawnFurnitureFromData(item);
                }
            }
        }

        public void ClearAllPlaced()
        {
            DeselectCurrent();
            foreach (var h in _activeHandles)
            {
                if (h != null)
                {
                    Destroy(h.gameObject);
                }
            }
            _activeHandles.Clear();
        }

        /// <summary>
        /// Begins placement mode for a specific furniture catalogue ID.
        /// </summary>
        public void BeginPlacement(string catalogueId)
        {
            CancelPlacement();
            DeselectCurrent();

            _pendingCatalogueId = catalogueId;
            _state = PlacementState.PlacingNew;

            _ghostObject = ProceduralFurnitureGenerator.GenerateFurniture(catalogueId, Vector3.one, new Color(1f, 1f, 1f, 0.5f));
            _ghostObject.name = $"Ghost_{catalogueId}";
            Collider col = _ghostObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        public void CancelPlacement()
        {
            if (_ghostObject != null)
            {
                Destroy(_ghostObject);
                _ghostObject = null;
            }
            _state = PlacementState.Idle;
            _pendingCatalogueId = null;
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            // Keyboard shortcuts
            if (_selectedHandle != null)
            {
                if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                {
                    DeleteSelected();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RotateSelected(45f);
                }
                if (Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)))
                {
                    DuplicateSelected();
                }
            }

            if (_state == PlacementState.PlacingNew && _ghostObject != null)
            {
                UpdateGhostPosition();

                if (Input.GetMouseButtonDown(0))
                {
                    ConfirmPlacement();
                }
                else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelPlacement();
                }
            }
            else if (_state == PlacementState.Idle && Input.GetMouseButtonDown(0))
            {
                HandleSelectionClick();
            }
        }

        private void UpdateGhostPosition()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _floorLayerMask))
            {
                Vector3 target = hit.point;
                if (_snapToGrid)
                {
                    target = SnapPoint(target);
                }
                _ghostObject.transform.position = target;
            }
            else
            {
                // Fallback to ground plane Y=0
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 target = ray.GetPoint(enter);
                    if (_snapToGrid)
                    {
                        target = SnapPoint(target);
                    }
                    _ghostObject.transform.position = target;
                }
            }

            // Rotate preview with mouse wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _ghostObject.transform.Rotate(Vector3.up, scroll > 0 ? 15f : -15f);
            }
        }

        private void ConfirmPlacement()
        {
            if (_ghostObject == null || string.IsNullOrEmpty(_pendingCatalogueId)) return;

            Vector3 finalPos = _ghostObject.transform.position;
            Vector3 finalRot = _ghostObject.transform.eulerAngles;
            Vector3 finalScale = _ghostObject.transform.localScale;

            FurnitureItemData data = new FurnitureItemData
            {
                instanceId = Guid.NewGuid().ToString(),
                catalogueId = _pendingCatalogueId,
                customName = _pendingCatalogueId,
                position = finalPos,
                rotation = finalRot,
                scale = finalScale,
                materialColor = "#FFFFFF"
            };

            if (_currentProject != null)
            {
                if (_currentProject.furnitureItems == null)
                {
                    _currentProject.furnitureItems = new List<FurnitureItemData>();
                }
                _currentProject.furnitureItems.Add(data);
            }

            CancelPlacement();

            FurnitureHandle spawned = SpawnFurnitureFromData(data);
            SelectHandle(spawned);
            OnFurniturePlaced?.Invoke(spawned);
        }

        private FurnitureHandle SpawnFurnitureFromData(FurnitureItemData data)
        {
            Color tint = Color.white;
            if (!string.IsNullOrEmpty(data.materialColor))
            {
                ColorUtility.TryParseHtmlString(data.materialColor, out tint);
            }

            GameObject go = ProceduralFurnitureGenerator.GenerateFurniture(data.catalogueId, data.scale, tint);
            go.transform.position = data.position;
            go.transform.rotation = Quaternion.Euler(data.rotation);
            go.name = $"{data.catalogueId}_{data.instanceId.Substring(0, 8)}";

            FurnitureHandle handle = go.AddComponent<FurnitureHandle>();
            handle.Initialize(data);
            handle.OnSelected += SelectHandle;
            _activeHandles.Add(handle);

            return handle;
        }

        private void HandleSelectionClick()
        {
            // Raycast against furniture
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                FurnitureHandle handle = hit.collider.GetComponentInParent<FurnitureHandle>();
                if (handle != null)
                {
                    SelectHandle(handle);
                    return;
                }
            }

            // Clicked empty space - deselect
            DeselectCurrent();
        }

        public void SelectHandle(FurnitureHandle handle)
        {
            if (_selectedHandle == handle) return;

            DeselectCurrent();
            _selectedHandle = handle;
            if (_selectedHandle != null)
            {
                _selectedHandle.SetSelected(true);
            }
            OnFurnitureSelected?.Invoke(_selectedHandle);
        }

        public void DeselectCurrent()
        {
            if (_selectedHandle != null)
            {
                _selectedHandle.SetSelected(false);
                _selectedHandle = null;
                OnFurnitureSelected?.Invoke(null);
            }
        }

        public void RotateSelected(float angleDelta)
        {
            if (_selectedHandle == null) return;
            _selectedHandle.transform.Rotate(Vector3.up, angleDelta, Space.World);
            _selectedHandle.SyncToData();
            OnFurnitureModified?.Invoke(_selectedHandle);
        }

        public void ScaleSelected(float scaleMultiplier)
        {
            if (_selectedHandle == null) return;
            Vector3 current = _selectedHandle.transform.localScale;
            Vector3 newScale = current * scaleMultiplier;
            newScale = new Vector3(
                Mathf.Clamp(newScale.x, 0.2f, 5f),
                Mathf.Clamp(newScale.y, 0.2f, 5f),
                Mathf.Clamp(newScale.z, 0.2f, 5f)
            );
            _selectedHandle.transform.localScale = newScale;
            _selectedHandle.SyncToData();
            OnFurnitureModified?.Invoke(_selectedHandle);
        }

        public void SetSelectedColor(Color color)
        {
            if (_selectedHandle == null || _selectedHandle.Data == null) return;
            _selectedHandle.Data.materialColor = "#" + ColorUtility.ToHtmlStringRGBA(color);
            
            Renderer[] renderers = _selectedHandle.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = color;
                }
            }
            OnFurnitureModified?.Invoke(_selectedHandle);
        }

        public void DuplicateSelected()
        {
            if (_selectedHandle == null || _selectedHandle.Data == null) return;

            FurnitureItemData original = _selectedHandle.Data;
            Vector3 newPos = (Vector3)original.position + new Vector3(0.5f, 0f, 0.5f);

            FurnitureItemData copy = new FurnitureItemData
            {
                instanceId = Guid.NewGuid().ToString(),
                catalogueId = original.catalogueId,
                customName = $"{original.customName} (Copy)",
                position = newPos,
                rotation = original.rotation,
                scale = original.scale,
                materialColor = original.materialColor
            };

            if (_currentProject != null)
            {
                _currentProject.furnitureItems.Add(copy);
            }

            FurnitureHandle spawned = SpawnFurnitureFromData(copy);
            SelectHandle(spawned);
            OnFurniturePlaced?.Invoke(spawned);
        }

        public void DeleteSelected()
        {
            if (_selectedHandle == null) return;

            string id = _selectedHandle.Data != null ? _selectedHandle.Data.instanceId : "";
            if (_currentProject != null && _selectedHandle.Data != null)
            {
                _currentProject.furnitureItems.Remove(_selectedHandle.Data);
            }

            _activeHandles.Remove(_selectedHandle);
            Destroy(_selectedHandle.gameObject);
            _selectedHandle = null;

            OnFurnitureDeleted?.Invoke(id);
            OnFurnitureSelected?.Invoke(null);
        }

        private Vector3 SnapPoint(Vector3 point)
        {
            float x = Mathf.Round(point.x / _gridSize) * _gridSize;
            float z = Mathf.Round(point.z / _gridSize) * _gridSize;
            return new Vector3(x, point.y, z);
        }
    }
}
