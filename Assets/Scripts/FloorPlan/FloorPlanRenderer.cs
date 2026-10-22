using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.FloorPlan
{
    /// <summary>
    /// Renders the 2D floor plan using Unity's GL immediate-mode drawing API
    /// and optional LineRenderers. Handles grid, rooms, walls, openings,
    /// measurements, and furniture footprints.
    /// </summary>
    [RequireComponent(typeof(FloorPlanController))]
    public class FloorPlanRenderer : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Colors")]
        [SerializeField, Tooltip("Color for grid lines.")]
        private Color _gridColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);

        [SerializeField, Tooltip("Color for major grid lines (every 5 cells).")]
        private Color _gridMajorColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);

        [SerializeField, Tooltip("Default wall color.")]
        private Color _wallColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        [SerializeField, Tooltip("Color for selected objects.")]
        private Color _selectedColor = new Color(0.2f, 0.5f, 1f, 1f);

        [SerializeField, Tooltip("Color for door opening symbols.")]
        private Color _doorColor = new Color(0.55f, 0.35f, 0.15f, 1f);

        [SerializeField, Tooltip("Color for window opening symbols.")]
        private Color _windowColor = new Color(0.3f, 0.85f, 0.9f, 1f);

        [SerializeField, Tooltip("Color for measurement dimension lines.")]
        private Color _measurementColor = new Color(0.9f, 0.2f, 0.2f, 0.8f);

        [SerializeField, Tooltip("Color for furniture footprints.")]
        private Color _furnitureColor = new Color(0.5f, 0.4f, 0.3f, 0.6f);

        [Header("Drawing Settings")]
        [SerializeField, Tooltip("Default wall draw thickness in world units.")]
        private float _wallDrawThickness = 0.15f;

        [SerializeField, Tooltip("Opening symbol arc segment count.")]
        private int _doorArcSegments = 16;

        [Header("Measurement Labels")]
        [SerializeField, Tooltip("Prefab for measurement text labels (TextMesh or TMP).")]
        private GameObject _measurementLabelPrefab;

        [SerializeField, Tooltip("Offset from wall for dimension lines (meters).")]
        private float _dimensionOffset = 0.4f;

        #endregion

        #region Private Fields

        private Material _glMaterial;
        private FloorPlanController _controller;
        private readonly List<GameObject> _activeMeasurementLabels = new List<GameObject>();
        private readonly List<GameObject> _measurementLabelPool = new List<GameObject>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _controller = GetComponent<FloorPlanController>();
            CreateGLMaterial();
        }

        private void OnRenderObject()
        {
            if (_controller == null || _controller.CurrentProject == null) return;

            ProjectData project = _controller.CurrentProject;

            // Calculate visible area from the camera.
            Camera cam = Camera.current;
            if (cam == null) return;

            Rect visibleArea = GetVisibleArea(cam);

            // Draw in order: grid → room fills → walls → openings → furniture → measurements.
            DrawGrid(_controller.GridSize, visibleArea);

            foreach (RoomData room in project.rooms)
            {
                bool isSelected = _controller.SelectedRoom != null
                                  && _controller.SelectedRoom.roomId == room.roomId;
                DrawRoom(room, isSelected);
            }
        }

        private void OnDestroy()
        {
            if (_glMaterial != null)
            {
                Destroy(_glMaterial);
            }

            foreach (GameObject label in _measurementLabelPool)
            {
                if (label != null) Destroy(label);
            }
        }

        #endregion

        #region GL Material

        private void CreateGLMaterial()
        {
            // Use a simple unlit shader for GL drawing.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogError("[FloorPlanRenderer] Could not find 'Hidden/Internal-Colored' shader.");
                return;
            }

            _glMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _glMaterial.SetInt("_ZWrite", 0);
        }

        #endregion

        #region Grid Drawing

        /// <summary>
        /// Draws a grid of lines across the visible area.
        /// </summary>
        /// <param name="gridSize">The spacing between grid lines in meters.</param>
        /// <param name="visibleArea">The currently visible rectangle in floor plan space.</param>
        public void DrawGrid(float gridSize, Rect visibleArea)
        {
            if (_glMaterial == null || gridSize <= 0f) return;

            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);

            float startX = Mathf.Floor(visibleArea.xMin / gridSize) * gridSize;
            float endX = Mathf.Ceil(visibleArea.xMax / gridSize) * gridSize;
            float startZ = Mathf.Floor(visibleArea.yMin / gridSize) * gridSize;
            float endZ = Mathf.Ceil(visibleArea.yMax / gridSize) * gridSize;

            // Vertical lines
            for (float x = startX; x <= endX; x += gridSize)
            {
                bool isMajor = Mathf.Abs(x % (gridSize * 5f)) < 0.01f;
                GL.Color(isMajor ? _gridMajorColor : _gridColor);
                GL.Vertex3(x, 0.001f, visibleArea.yMin);
                GL.Vertex3(x, 0.001f, visibleArea.yMax);
            }

            // Horizontal lines
            for (float z = startZ; z <= endZ; z += gridSize)
            {
                bool isMajor = Mathf.Abs(z % (gridSize * 5f)) < 0.01f;
                GL.Color(isMajor ? _gridMajorColor : _gridColor);
                GL.Vertex3(visibleArea.xMin, 0.001f, z);
                GL.Vertex3(visibleArea.xMax, 0.001f, z);
            }

            GL.End();
            GL.PopMatrix();
        }

        #endregion

        #region Room Drawing

        /// <summary>
        /// Draws a room as a filled rectangle with its walls, openings,
        /// furniture, and measurements.
        /// </summary>
        /// <param name="room">The room data to draw.</param>
        /// <param name="isSelected">Whether this room is currently selected.</param>
        public void DrawRoom(RoomData room, bool isSelected)
        {
            if (room == null || _glMaterial == null) return;

            // Draw filled rectangle.
            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            GL.Begin(GL.QUADS);
            Color fill = isSelected
                ? new Color(_selectedColor.r, _selectedColor.g, _selectedColor.b, 0.15f)
                : room.fillColor;
            GL.Color(fill);

            Vector2 p = room.position;
            Vector2 s = room.size;
            GL.Vertex3(p.x, 0.002f, p.y);
            GL.Vertex3(p.x + s.x, 0.002f, p.y);
            GL.Vertex3(p.x + s.x, 0.002f, p.y + s.y);
            GL.Vertex3(p.x, 0.002f, p.y + s.y);
            GL.End();
            GL.PopMatrix();

            // Draw walls.
            foreach (WallData wall in room.walls)
            {
                bool wallSelected = _controller.SelectedWall != null
                                    && _controller.SelectedWall.wallId == wall.wallId;
                DrawWall(wall, isSelected || wallSelected);

                // Draw openings on this wall.
                foreach (WallOpeningData opening in wall.openings)
                {
                    if (opening.openingType == OpeningType.Door)
                        DrawDoorOpening(opening, wall);
                    else
                        DrawWindowOpening(opening, wall);
                }
            }

            // Furniture footprints.
            foreach (FurnitureItemData item in room.furniture)
            {
                DrawFurnitureFootprint(item);
            }

            // Measurements when selected.
            if (isSelected)
            {
                DrawMeasurements(room);
            }
        }

        #endregion

        #region Wall Drawing

        /// <summary>
        /// Draws a wall as a thick line between its start and end points.
        /// </summary>
        /// <param name="wall">The wall data to draw.</param>
        /// <param name="isSelected">Whether this wall is currently selected.</param>
        public void DrawWall(WallData wall, bool isSelected)
        {
            if (wall == null || _glMaterial == null) return;

            Color color = isSelected ? _selectedColor : _wallColor;
            float thickness = wall.thickness > 0f ? wall.thickness : _wallDrawThickness;

            DrawThickLine(wall.startPoint, wall.endPoint, thickness, color, 0.003f);
        }

        #endregion

        #region Opening Drawing

        /// <summary>
        /// Draws a door opening symbol on a wall: a gap in the wall with a swing arc.
        /// </summary>
        /// <param name="opening">The opening data.</param>
        /// <param name="parentWall">The wall this opening belongs to.</param>
        public void DrawDoorOpening(WallOpeningData opening, WallData parentWall)
        {
            if (opening == null || parentWall == null || _glMaterial == null) return;

            Vector2 wallDir = (parentWall.endPoint - parentWall.startPoint).normalized;
            Vector2 wallPerp = new Vector2(-wallDir.y, wallDir.x);
            float wallLength = parentWall.Length;

            // Position along wall.
            float centerDist = opening.positionAlongWall * wallLength;
            Vector2 center = parentWall.startPoint + wallDir * centerDist;
            float halfWidth = opening.width * 0.5f;

            Vector2 doorStart = center - wallDir * halfWidth;
            Vector2 doorEnd = center + wallDir * halfWidth;

            // Draw the door opening gap (two short lines to indicate break).
            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);

            GL.Color(_doorColor);

            // Hinge side marker.
            GL.Vertex3(doorStart.x, 0.005f, doorStart.y);
            GL.Vertex3(doorStart.x + wallPerp.x * opening.width,
                       0.005f,
                       doorStart.y + wallPerp.y * opening.width);

            GL.End();

            // Draw arc.
            GL.Begin(GL.LINES);
            GL.Color(_doorColor);
            for (int i = 0; i < _doorArcSegments; i++)
            {
                float a0 = (float)i / _doorArcSegments * Mathf.PI * 0.5f;
                float a1 = (float)(i + 1) / _doorArcSegments * Mathf.PI * 0.5f;

                Vector2 p0 = doorStart + new Vector2(
                    Mathf.Cos(a0) * wallDir.x - Mathf.Sin(a0) * wallPerp.x,
                    Mathf.Cos(a0) * wallDir.y - Mathf.Sin(a0) * wallPerp.y
                ) * opening.width;

                Vector2 p1 = doorStart + new Vector2(
                    Mathf.Cos(a1) * wallDir.x - Mathf.Sin(a1) * wallPerp.x,
                    Mathf.Cos(a1) * wallDir.y - Mathf.Sin(a1) * wallPerp.y
                ) * opening.width;

                GL.Vertex3(p0.x, 0.005f, p0.y);
                GL.Vertex3(p1.x, 0.005f, p1.y);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a window opening symbol on a wall: a gap with parallel lines.
        /// </summary>
        /// <param name="opening">The opening data.</param>
        /// <param name="parentWall">The wall this opening belongs to.</param>
        public void DrawWindowOpening(WallOpeningData opening, WallData parentWall)
        {
            if (opening == null || parentWall == null || _glMaterial == null) return;

            Vector2 wallDir = (parentWall.endPoint - parentWall.startPoint).normalized;
            Vector2 wallPerp = new Vector2(-wallDir.y, wallDir.x);
            float wallLength = parentWall.Length;

            float centerDist = opening.positionAlongWall * wallLength;
            Vector2 center = parentWall.startPoint + wallDir * centerDist;
            float halfWidth = opening.width * 0.5f;
            float halfThick = parentWall.thickness * 0.5f;

            Vector2 wStart = center - wallDir * halfWidth;
            Vector2 wEnd = center + wallDir * halfWidth;

            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);

            GL.Color(_windowColor);

            // Three parallel lines across the opening to indicate glass.
            for (int line = -1; line <= 1; line++)
            {
                Vector2 offset = wallPerp * (halfThick * line * 0.8f);
                GL.Vertex3(wStart.x + offset.x, 0.005f, wStart.y + offset.y);
                GL.Vertex3(wEnd.x + offset.x, 0.005f, wEnd.y + offset.y);
            }

            GL.End();
            GL.PopMatrix();
        }

        #endregion

        #region Measurement Drawing

        /// <summary>
        /// Draws dimension lines with measurement labels for each wall of a room.
        /// </summary>
        /// <param name="room">The room whose measurements to display.</param>
        public void DrawMeasurements(RoomData room)
        {
            if (room == null || _glMaterial == null) return;

            RecycleLabels();

            foreach (WallData wall in room.walls)
            {
                Vector2 wallDir = (wall.endPoint - wall.startPoint).normalized;
                Vector2 wallPerp = new Vector2(-wallDir.y, wallDir.x);
                float wallLength = wall.Length;

                // Offset the dimension line from the wall.
                Vector2 offsetStart = wall.startPoint + wallPerp * _dimensionOffset;
                Vector2 offsetEnd = wall.endPoint + wallPerp * _dimensionOffset;

                // Draw the dimension line.
                _glMaterial.SetPass(0);
                GL.PushMatrix();
                GL.MultMatrix(Matrix4x4.identity);
                GL.Begin(GL.LINES);
                GL.Color(_measurementColor);

                // Main dimension line.
                GL.Vertex3(offsetStart.x, 0.006f, offsetStart.y);
                GL.Vertex3(offsetEnd.x, 0.006f, offsetEnd.y);

                // Extension lines (ticks).
                Vector2 tickDir = wallPerp * 0.1f;
                GL.Vertex3(offsetStart.x - tickDir.x, 0.006f, offsetStart.y - tickDir.y);
                GL.Vertex3(offsetStart.x + tickDir.x, 0.006f, offsetStart.y + tickDir.y);

                GL.Vertex3(offsetEnd.x - tickDir.x, 0.006f, offsetEnd.y - tickDir.y);
                GL.Vertex3(offsetEnd.x + tickDir.x, 0.006f, offsetEnd.y + tickDir.y);

                GL.End();
                GL.PopMatrix();

                // Place label at midpoint.
                Vector2 midpoint = (offsetStart + offsetEnd) * 0.5f;
                string label = $"{wallLength:F2} m";
                PlaceMeasurementLabel(midpoint, label);
            }
        }

        #endregion

        #region Furniture Drawing

        /// <summary>
        /// Draws a top-down rectangular footprint for a furniture item.
        /// </summary>
        /// <param name="item">The furniture item data.</param>
        public void DrawFurnitureFootprint(FurnitureItemData item)
        {
            if (item == null || _glMaterial == null) return;

            Vector2 halfSize = item.footprintSize * 0.5f;
            float rad = item.rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            // Calculate rotated corners.
            Vector2[] localCorners = new Vector2[]
            {
                new Vector2(-halfSize.x, -halfSize.y),
                new Vector2( halfSize.x, -halfSize.y),
                new Vector2( halfSize.x,  halfSize.y),
                new Vector2(-halfSize.x,  halfSize.y)
            };

            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            // Filled quad.
            GL.Begin(GL.QUADS);
            GL.Color(_furnitureColor);
            for (int i = 0; i < 4; i++)
            {
                Vector2 rotated = new Vector2(
                    localCorners[i].x * cos - localCorners[i].y * sin,
                    localCorners[i].x * sin + localCorners[i].y * cos
                );
                Vector2 world = item.position + rotated;
                GL.Vertex3(world.x, 0.004f, world.y);
            }
            GL.End();

            // Outline.
            GL.Begin(GL.LINES);
            GL.Color(new Color(_furnitureColor.r * 0.6f, _furnitureColor.g * 0.6f,
                               _furnitureColor.b * 0.6f, 1f));
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                Vector2 r0 = new Vector2(
                    localCorners[i].x * cos - localCorners[i].y * sin,
                    localCorners[i].x * sin + localCorners[i].y * cos
                ) + item.position;
                Vector2 r1 = new Vector2(
                    localCorners[next].x * cos - localCorners[next].y * sin,
                    localCorners[next].x * sin + localCorners[next].y * cos
                ) + item.position;
                GL.Vertex3(r0.x, 0.004f, r0.y);
                GL.Vertex3(r1.x, 0.004f, r1.y);
            }
            GL.End();

            GL.PopMatrix();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Draws a thick line using two triangles (quad) between two points.
        /// </summary>
        private void DrawThickLine(Vector2 start, Vector2 end, float thickness, Color color, float yHeight)
        {
            if (_glMaterial == null) return;

            Vector2 dir = (end - start).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

            _glMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.QUADS);
            GL.Color(color);

            GL.Vertex3(start.x + perp.x, yHeight, start.y + perp.y);
            GL.Vertex3(start.x - perp.x, yHeight, start.y - perp.y);
            GL.Vertex3(end.x - perp.x, yHeight, end.y - perp.y);
            GL.Vertex3(end.x + perp.x, yHeight, end.y + perp.y);

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Computes the floor-plan rectangle currently visible by the given camera.
        /// </summary>
        private Rect GetVisibleArea(Camera cam)
        {
            if (cam == null) return new Rect(-10, -10, 20, 20);

            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            Vector3 camPos = cam.transform.position;

            return new Rect(
                camPos.x - width * 0.5f,
                camPos.z - height * 0.5f,
                width,
                height
            );
        }

        /// <summary>
        /// Gets or creates a measurement label from the pool.
        /// </summary>
        private void PlaceMeasurementLabel(Vector2 position, string text)
        {
            GameObject label = GetPooledLabel();
            if (label == null) return;

            label.transform.position = new Vector3(position.x, 0.01f, position.y);
            label.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            TextMesh tm = label.GetComponent<TextMesh>();
            if (tm != null)
            {
                tm.text = text;
                tm.color = _measurementColor;
                tm.characterSize = 0.05f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.fontSize = 36;
            }

            label.SetActive(true);
            _activeMeasurementLabels.Add(label);
        }

        private GameObject GetPooledLabel()
        {
            // Try to reuse from pool.
            foreach (GameObject pooled in _measurementLabelPool)
            {
                if (pooled != null && !pooled.activeSelf)
                {
                    return pooled;
                }
            }

            // Create new.
            GameObject label;
            if (_measurementLabelPrefab != null)
            {
                label = Instantiate(_measurementLabelPrefab, transform);
            }
            else
            {
                label = new GameObject("MeasurementLabel");
                label.transform.SetParent(transform);
                label.AddComponent<TextMesh>();
            }

            label.SetActive(false);
            _measurementLabelPool.Add(label);
            return label;
        }

        private void RecycleLabels()
        {
            foreach (GameObject label in _activeMeasurementLabels)
            {
                if (label != null)
                {
                    label.SetActive(false);
                }
            }
            _activeMeasurementLabels.Clear();
        }

        #endregion
    }
}
