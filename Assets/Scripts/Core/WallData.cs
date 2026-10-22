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
        /// <summary>A standard door opening.</summary>
        Door,

        /// <summary>A window opening.</summary>
        Window
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
        /// Normalised position along the wall (0 = start, 1 = end)
        /// indicating the centre of the opening.
        /// </summary>
        public float position;

        /// <summary>Width of the opening in meters.</summary>
        public float width;

        /// <summary>Height of the opening in meters.</summary>
        public float height;

        /// <summary>
        /// Elevation of the bottom edge of the opening from the floor in meters.
        /// Typically 0 for doors and ~0.9 m for windows.
        /// </summary>
        public float elevation;

        /// <summary>
        /// Validates the opening data.
        /// </summary>
        /// <returns><c>true</c> if valid; otherwise <c>false</c>.</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[WallOpeningData] id is null or empty.");
                isValid = false;
            }

            if (position < 0f || position > 1f)
            {
                Debug.LogWarning($"[WallOpeningData:{id}] position {position} is outside 0–1 range.");
                isValid = false;
            }

            if (width <= 0f)
            {
                Debug.LogWarning($"[WallOpeningData:{id}] width must be positive.");
                isValid = false;
            }

            if (height <= 0f)
            {
                Debug.LogWarning($"[WallOpeningData:{id}] height must be positive.");
                isValid = false;
            }

            if (elevation < 0f)
            {
                Debug.LogWarning($"[WallOpeningData:{id}] elevation must be non-negative.");
                isValid = false;
            }

            return isValid;
        }
    }

    /// <summary>
    /// Data describing a single wall segment within a room, including any
    /// door or window openings cut into it.
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

        /// <summary>Material / paint colour of the wall as a hex string (e.g. "#FFFFFF").</summary>
        public string materialColor = "#FFFFFF";

        /// <summary>List of openings (doors and windows) in this wall.</summary>
        public List<WallOpeningData> openings = new List<WallOpeningData>();

        /// <summary>
        /// Calculates the length of the wall in meters.
        /// </summary>
        /// <returns>The Euclidean distance between start and end points.</returns>
        public float GetLength()
        {
            Vector2 start = startPoint;
            Vector2 end = endPoint;
            return Vector2.Distance(start, end);
        }

        /// <summary>
        /// Returns the normalised direction vector from the start point to the end point.
        /// </summary>
        /// <returns>A unit <see cref="Vector2"/> pointing from start to end,
        /// or <see cref="Vector2.zero"/> if the wall has zero length.</returns>
        public Vector2 GetDirection()
        {
            Vector2 start = startPoint;
            Vector2 end = endPoint;
            Vector2 dir = end - start;
            return dir.sqrMagnitude > Mathf.Epsilon ? dir.normalized : Vector2.zero;
        }

        /// <summary>
        /// Validates the wall data and all contained openings.
        /// </summary>
        /// <returns><c>true</c> if all data is valid; otherwise <c>false</c>.</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(wallId))
            {
                Debug.LogWarning("[WallData] wallId is null or empty.");
                isValid = false;
            }

            if (GetLength() < Mathf.Epsilon)
            {
                Debug.LogWarning($"[WallData:{wallId}] Wall has zero length (start == end).");
                isValid = false;
            }

            if (height <= 0f)
            {
                Debug.LogWarning($"[WallData:{wallId}] height must be positive.");
                isValid = false;
            }

            if (thickness <= 0f)
            {
                Debug.LogWarning($"[WallData:{wallId}] thickness must be positive.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(materialColor))
            {
                Debug.LogWarning($"[WallData:{wallId}] materialColor is empty.");
                isValid = false;
            }

            if (openings != null)
            {
                foreach (var opening in openings)
                {
                    if (opening == null)
                    {
                        Debug.LogWarning($"[WallData:{wallId}] Contains a null opening entry.");
                        isValid = false;
                        continue;
                    }

                    if (!opening.Validate())
                    {
                        isValid = false;
                    }

                    // Check that the opening fits within the wall height
                    if (opening.elevation + opening.height > height)
                    {
                        Debug.LogWarning(
                            $"[WallData:{wallId}] Opening '{opening.id}' exceeds wall height " +
                            $"(elevation {opening.elevation} + height {opening.height} > {height}).");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }
}
