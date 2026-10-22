using System;
using System.Collections.Generic;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// The type of opening that can be placed in a wall.
    /// </summary>
    public enum OpeningType
    {
        Door,
        Window,
        Archway,
        SlidingDoor
    }

    /// <summary>
    /// Data describing a single opening (door or window) within a wall.
    /// </summary>
    [Serializable]
    public class WallOpeningData
    {
        /// <summary>Unique identifier for this opening.</summary>
        public string id;

        /// <summary>Whether this opening is a door or a window.</summary>
        public OpeningType openingType;

        /// <summary>
        /// Normalised position along the wall (0 = start, 1 = end).
        /// </summary>
        public float position;

        /// <summary>Width of the opening in meters.</summary>
        public float width = 0.9f;

        /// <summary>Height of the opening in meters.</summary>
        public float height = 2.1f;

        /// <summary>Elevation of the bottom edge of the opening from the floor in meters.</summary>
        public float elevation = 0f;

        /// <summary>Parent wall identifier.</summary>
        public string parentWallId;

        // --- Compatibility Aliases ---
        public string openingId
        {
            get => id;
            set => id = value;
        }

        public string wallId
        {
            get => parentWallId;
            set => parentWallId = value;
        }

        public float positionAlongWall
        {
            get => position;
            set => position = value;
        }

        public float normalizedPosition
        {
            get => position;
            set => position = value;
        }

        public float sillHeight
        {
            get => elevation;
            set => elevation = value;
        }

        public float openingWidth
        {
            get => width;
            set => width = value;
        }

        public float openingHeight
        {
            get => height;
            set => height = value;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            if (position < 0f || position > 1f) return false;
            if (width <= 0f || height <= 0f) return false;
            return true;
        }
    }

    /// <summary>
    /// Alias class for backward compatibility with subagent code.
    /// </summary>
    [Serializable]
    public class OpeningData : WallOpeningData
    {
    }

    /// <summary>
    /// Data describing a single wall segment within a room.
    /// </summary>
    [Serializable]
    public class WallData
    {
        /// <summary>Unique identifier for this wall.</summary>
        public string wallId;

        /// <summary>Start point of the wall on the 2D floor plan (meters).</summary>
        public Vector2Serializable startPoint;

        /// <summary>End point of the wall on the 2D floor plan (meters).</summary>
        public Vector2Serializable endPoint;

        /// <summary>Height of the wall in meters.</summary>
        public float height = 2.8f;

        /// <summary>Thickness of the wall in meters.</summary>
        public float thickness = 0.15f;

        /// <summary>Material / paint colour of the wall as a hex string.</summary>
        public string materialColor = "#FFFFFF";

        /// <summary>Material identifier for the wall surface.</summary>
        public string materialId;

        /// <summary>Parent room identifier.</summary>
        public string parentRoomId;

        /// <summary>List of openings (doors and windows) in this wall.</summary>
        public List<WallOpeningData> openings = new List<WallOpeningData>();

        // --- Compatibility Aliases ---
        public float Length => GetLength();

        public Color color
        {
            get
            {
                if (ColorUtility.TryParseHtmlString(materialColor, out Color c))
                    return c;
                return Color.white;
            }
            set => materialColor = "#" + ColorUtility.ToHtmlStringRGBA(value);
        }

        /// <summary>
        /// Calculates the length of the wall in meters.
        /// </summary>
        public float GetLength()
        {
            Vector2 start = startPoint;
            Vector2 end = endPoint;
            return Vector2.Distance(start, end);
        }

        /// <summary>
        /// Returns the normalised direction vector from start to end.
        /// </summary>
        public Vector2 GetDirection()
        {
            Vector2 start = startPoint;
            Vector2 end = endPoint;
            Vector2 dir = end - start;
            return dir.sqrMagnitude > Mathf.Epsilon ? dir.normalized : Vector2.zero;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(wallId)) return false;
            if (GetLength() < Mathf.Epsilon) return false;
            if (height <= 0f || thickness <= 0f) return false;
            return true;
        }
    }
}
