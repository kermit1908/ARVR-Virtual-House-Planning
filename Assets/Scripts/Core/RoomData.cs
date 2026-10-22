using System;
using System.Collections.Generic;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// Categorises a room by its intended purpose.
    /// </summary>
    public enum RoomType
    {
        /// <summary>A general-purpose living room.</summary>
        LivingRoom,

        /// <summary>A bedroom for sleeping.</summary>
        Bedroom,

        /// <summary>A kitchen for cooking.</summary>
        Kitchen,

        /// <summary>A bathroom with plumbing fixtures.</summary>
        Bathroom,

        /// <summary>A hallway or corridor connecting rooms.</summary>
        Hallway,

        /// <summary>A garage for vehicle storage.</summary>
        Garage,

        /// <summary>A user-defined custom room type.</summary>
        Custom
    }

    /// <summary>
    /// Serializable data describing a single room within the house plan,
    /// including its dimensions, position, walls, and material settings.
    /// </summary>
    [Serializable]
    public class RoomData
    {
        /// <summary>Unique identifier for this room.</summary>
        public string roomId;

        /// <summary>Human-readable display name (e.g. "Living Room").</summary>
        public string roomName;

        /// <summary>The functional category of this room.</summary>
        public RoomType roomType = RoomType.Custom;

        /// <summary>Width of the room in meters (X axis).</summary>
        public float width;

        /// <summary>Length of the room in meters (Z axis).</summary>
        public float length;

        /// <summary>Height of the room in meters (Y axis).</summary>
        public float height = 2.8f;

        /// <summary>Position of the room's origin on the 2D floor plan (meters).</summary>
        public Vector2Serializable position;

        /// <summary>Rotation of the room around the Y axis in degrees.</summary>
        public float rotation;

        /// <summary>Ordered list of wall segments that form this room's perimeter.</summary>
        public List<WallData> walls = new List<WallData>();

        /// <summary>Material identifier or name for the floor surface.</summary>
        public string floorMaterial = "default_wood";

        /// <summary>Hex colour string for the walls (e.g. "#FFFFFF").</summary>
        public string wallColor = "#FFFFFF";

        /// <summary>Hex colour string for the ceiling (e.g. "#F5F5F5").</summary>
        public string ceilingColor = "#F5F5F5";

        /// <summary>
        /// Calculates the floor area of the room in square meters.
        /// </summary>
        /// <returns>The area (width × length).</returns>
        public float GetArea()
        {
            return width * length;
        }

        /// <summary>
        /// Calculates the perimeter of the room in meters.
        /// </summary>
        /// <returns>The perimeter (2 × (width + length)).</returns>
        public float GetPerimeter()
        {
            return 2f * (width + length);
        }

        /// <summary>
        /// Validates all room data including dimensions and child wall data.
        /// </summary>
        /// <returns><c>true</c> if all data is valid; otherwise <c>false</c>.</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(roomId))
            {
                Debug.LogWarning("[RoomData] roomId is null or empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(roomName))
            {
                Debug.LogWarning($"[RoomData:{roomId}] roomName is null or empty.");
                isValid = false;
            }

            if (width <= 0f)
            {
                Debug.LogWarning($"[RoomData:{roomId}] width must be positive, got {width}.");
                isValid = false;
            }

            if (length <= 0f)
            {
                Debug.LogWarning($"[RoomData:{roomId}] length must be positive, got {length}.");
                isValid = false;
            }

            if (height <= 0f)
            {
                Debug.LogWarning($"[RoomData:{roomId}] height must be positive, got {height}.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(floorMaterial))
            {
                Debug.LogWarning($"[RoomData:{roomId}] floorMaterial is empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(wallColor))
            {
                Debug.LogWarning($"[RoomData:{roomId}] wallColor is empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(ceilingColor))
            {
                Debug.LogWarning($"[RoomData:{roomId}] ceilingColor is empty.");
                isValid = false;
            }

            if (walls != null)
            {
                foreach (var wall in walls)
                {
                    if (wall == null)
                    {
                        Debug.LogWarning($"[RoomData:{roomId}] Contains a null wall entry.");
                        isValid = false;
                        continue;
                    }

                    if (!wall.Validate())
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
    }
}
