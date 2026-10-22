using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.HouseGeneration
{
    /// <summary>
    /// Generates a complete 3D representation of a house from <see cref="ProjectData"/>.
    /// Constructs procedural meshes for floors, walls with door/window openings, and ceilings,
    /// applies appropriate materials, and supports visibility modes (cutaway, interior view).
    /// </summary>
    public class HouseGenerator : MonoBehaviour
    {
        private static HouseGenerator _instance;
        public static HouseGenerator Instance => _instance;

        [Header("Hierarchy")]
        [SerializeField] private Transform _houseRoot;

        [Header("Visibility Settings")]
        [SerializeField] private bool _showWalls = true;
        [SerializeField] private bool _showFloors = true;
        [SerializeField] private bool _showCeilings = false;
        [SerializeField] private bool _interiorViewMode = true;

        [Header("Materials")]
        [SerializeField] private MaterialManager _materialManager;

        private ProjectData _currentProject;
        private readonly List<GameObject> _wallObjects = new List<GameObject>();
        private readonly List<GameObject> _floorObjects = new List<GameObject>();
        private readonly List<GameObject> _ceilingObjects = new List<GameObject>();
        private readonly List<GameObject> _openingFrameObjects = new List<GameObject>();

        public ProjectData CurrentProject => _currentProject;
        public bool ShowWalls
        {
            get => _showWalls;
            set { _showWalls = value; UpdateVisibility(); }
        }
        public bool ShowFloors
        {
            get => _showFloors;
            set { _showFloors = value; UpdateVisibility(); }
        }
        public bool ShowCeilings
        {
            get => _showCeilings;
            set { _showCeilings = value; UpdateVisibility(); }
        }
        public bool InteriorViewMode
        {
            get => _interiorViewMode;
            set { _interiorViewMode = value; UpdateVisibility(); }
        }

        public event Action OnHouseGenerated;
        public event Action OnHouseCleared;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (_houseRoot == null)
            {
                GameObject rootGo = new GameObject("GeneratedHouse");
                rootGo.transform.SetParent(transform);
                _houseRoot = rootGo.transform;
            }

            if (_materialManager == null)
            {
                _materialManager = MaterialManager.Instance;
            }
        }

        /// <summary>
        /// Clears any currently generated house geometry.
        /// </summary>
        public void ClearGeneratedHouse()
        {
            if (_houseRoot != null)
            {
                // Destroy all children of _houseRoot
                for (int i = _houseRoot.childCount - 1; i >= 0; i--)
                {
                    Destroy(_houseRoot.GetChild(i).gameObject);
                }
            }

            _wallObjects.Clear();
            _floorObjects.Clear();
            _ceilingObjects.Clear();
            _openingFrameObjects.Clear();

            OnHouseCleared?.Invoke();
        }

        /// <summary>
        /// Builds 3D house geometry for the provided project data.
        /// </summary>
        public void GenerateHouse(ProjectData project)
        {
            if (project == null)
            {
                Debug.LogWarning("[HouseGenerator] Cannot generate house from null project.");
                return;
            }

            ClearGeneratedHouse();
            _currentProject = project;

            if (_houseRoot == null)
            {
                GameObject rootGo = new GameObject("GeneratedHouse");
                rootGo.transform.SetParent(transform);
                _houseRoot = rootGo.transform;
            }

            if (_materialManager == null)
            {
                _materialManager = MaterialManager.Instance;
            }

            // Generate each room
            if (project.rooms != null)
            {
                foreach (RoomData room in project.rooms)
                {
                    GenerateRoom(room);
                }
            }

            UpdateVisibility();
            OnHouseGenerated?.Invoke();
        }

        /// <summary>
        /// Rebuilds the current house.
        /// </summary>
        public void RegenerateHouse()
        {
            if (_currentProject != null)
            {
                GenerateHouse(_currentProject);
            }
        }

        private void GenerateRoom(RoomData room)
        {
            GameObject roomRoot = new GameObject($"Room_{room.roomName}_{room.roomId}");
            roomRoot.transform.SetParent(_houseRoot, false);
            roomRoot.transform.position = new Vector3(room.position.x, 0f, room.position.y);
            roomRoot.transform.rotation = Quaternion.Euler(0f, room.rotation, 0f);

            // 1. Floor
            GenerateFloor(room, roomRoot.transform);

            // 2. Ceiling
            GenerateCeiling(room, roomRoot.transform);

            // 3. Walls
            if (room.walls != null && room.walls.Count > 0)
            {
                foreach (WallData wall in room.walls)
                {
                    GenerateWall(wall, roomRoot.transform, room);
                }
            }
            else
            {
                // Auto-generate 4 perimeter walls from room dimensions if walls list is empty
                GeneratePerimeterWalls(room, roomRoot.transform);
            }
        }

        private void GenerateFloor(RoomData room, Transform parent)
        {
            GameObject floorGo = new GameObject("Floor");
            floorGo.transform.SetParent(parent, false);
            floorGo.layer = LayerMask.NameToLayer("Default");

            MeshFilter mf = floorGo.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateFloor(room.width, room.length);

            MeshRenderer mr = floorGo.AddComponent<MeshRenderer>();
            mr.material = _materialManager.CreateFloorMaterial(room.floorMaterial);

            // Add floor collider for raycasting
            MeshCollider mc = floorGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;

            _floorObjects.Add(floorGo);
        }

        private void GenerateCeiling(RoomData room, Transform parent)
        {
            GameObject ceilingGo = new GameObject("Ceiling");
            ceilingGo.transform.SetParent(parent, false);
            ceilingGo.transform.localPosition = new Vector3(0f, room.height, 0f);

            MeshFilter mf = ceilingGo.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateCeiling(room.width, room.length);

            MeshRenderer mr = ceilingGo.AddComponent<MeshRenderer>();
            Color ceilColor = Color.white;
            if (!string.IsNullOrEmpty(room.ceilingColor))
            {
                ColorUtility.TryParseHtmlString(room.ceilingColor, out ceilColor);
            }
            mr.material = _materialManager.GetOrCreateMaterial($"ceiling_{room.roomId}", ceilColor);

            _ceilingObjects.Add(ceilingGo);
        }

        private void GenerateWall(WallData wall, Transform parent, RoomData parentRoom)
        {
            float length = wall.GetLength();
            if (length < 0.05f) return;

            GameObject wallGo = new GameObject($"Wall_{wall.wallId}");
            wallGo.transform.SetParent(parent, false);

            Vector2 start = wall.startPoint;
            Vector2 end = wall.endPoint;
            Vector2 dir = wall.GetDirection();
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Position at wall start point
            wallGo.transform.localPosition = new Vector3(start.x, 0f, start.y);
            // Rotate along wall vector (Y rotation)
            wallGo.transform.localRotation = Quaternion.Euler(0f, -angle, 0f);

            MeshFilter mf = wallGo.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateWall(length, wall.height, wall.thickness, wall.openings);

            MeshRenderer mr = wallGo.AddComponent<MeshRenderer>();
            Color wallColor = Color.white;
            if (!string.IsNullOrEmpty(wall.materialColor))
            {
                ColorUtility.TryParseHtmlString(wall.materialColor, out wallColor);
            }
            else if (!string.IsNullOrEmpty(parentRoom.wallColor))
            {
                ColorUtility.TryParseHtmlString(parentRoom.wallColor, out wallColor);
            }
            mr.material = _materialManager.CreateWallMaterial(wallColor);

            // Add collider for collision
            MeshCollider mc = wallGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;

            _wallObjects.Add(wallGo);

            // Generate door/window frames if any openings exist
            if (wall.openings != null)
            {
                foreach (var op in wall.openings)
                {
                    GenerateOpeningFrames(op, wallGo.transform, length, wall.thickness);
                }
            }
        }

        private void GenerateOpeningFrames(WallOpeningData op, Transform wallTransform, float wallLength, float wallThickness)
        {
            GameObject frameGo = new GameObject($"Frame_{op.openingType}_{op.id}");
            frameGo.transform.SetParent(wallTransform, false);

            float posX = op.position * wallLength;
            frameGo.transform.localPosition = new Vector3(posX - (op.width * 0.5f), op.elevation, 0f);

            MeshFilter mf = frameGo.AddComponent<MeshFilter>();
            if (op.openingType == OpeningType.Door)
            {
                mf.sharedMesh = ProceduralMeshBuilder.CreateDoorFrame(op.width, op.height, wallThickness * 1.05f);
            }
            else
            {
                mf.sharedMesh = ProceduralMeshBuilder.CreateWindowFrame(op.width, op.height, wallThickness * 1.05f);
            }

            MeshRenderer mr = frameGo.AddComponent<MeshRenderer>();
            Color frameColor = op.openingType == OpeningType.Door ? new Color(0.4f, 0.25f, 0.15f) : new Color(0.85f, 0.85f, 0.9f);
            mr.material = _materialManager.GetOrCreateMaterial($"frame_{op.openingType}", frameColor);

            _openingFrameObjects.Add(frameGo);
        }

        private void GeneratePerimeterWalls(RoomData room, Transform parent)
        {
            float w = room.width;
            float l = room.length;
            float h = room.height;
            float t = 0.15f;

            // South wall (along X at Z=0)
            CreateWallSegment(parent, new Vector3(0, 0, 0), Quaternion.identity, w, h, t, "Wall_South", room.wallColor);
            // North wall (along X at Z=l)
            CreateWallSegment(parent, new Vector3(0, 0, l), Quaternion.identity, w, h, t, "Wall_North", room.wallColor);
            // West wall (along Z at X=0)
            CreateWallSegment(parent, new Vector3(0, 0, 0), Quaternion.Euler(0, -90, 0), l, h, t, "Wall_West", room.wallColor);
            // East wall (along Z at X=w)
            CreateWallSegment(parent, new Vector3(w, 0, 0), Quaternion.Euler(0, -90, 0), l, h, t, "Wall_East", room.wallColor);
        }

        private void CreateWallSegment(Transform parent, Vector3 localPos, Quaternion localRot, float width, float height, float thickness, string name, string colorHex)
        {
            GameObject wallGo = new GameObject(name);
            wallGo.transform.SetParent(parent, false);
            wallGo.transform.localPosition = localPos;
            wallGo.transform.localRotation = localRot;

            MeshFilter mf = wallGo.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateWall(width, height, thickness, null);

            MeshRenderer mr = wallGo.AddComponent<MeshRenderer>();
            Color wallColor = Color.white;
            if (!string.IsNullOrEmpty(colorHex))
            {
                ColorUtility.TryParseHtmlString(colorHex, out wallColor);
            }
            mr.material = _materialManager.CreateWallMaterial(wallColor);

            MeshCollider mc = wallGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;

            _wallObjects.Add(wallGo);
        }

        private void UpdateVisibility()
        {
            foreach (var w in _wallObjects)
            {
                if (w != null) w.SetActive(_showWalls);
            }

            foreach (var f in _floorObjects)
            {
                if (f != null) f.SetActive(_showFloors);
            }

            foreach (var c in _ceilingObjects)
            {
                if (c != null)
                {
                    // Ceilings hidden if interior view mode is on
                    c.SetActive(_showCeilings && !_interiorViewMode);
                }
            }

            foreach (var fr in _openingFrameObjects)
            {
                if (fr != null) fr.SetActive(_showWalls);
            }
        }
    }
}
