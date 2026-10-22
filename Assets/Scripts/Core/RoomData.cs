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
        LivingRoom,
        Bedroom,
        Kitchen,
        Bathroom,
        Hallway,
        Garage,
        DiningRoom,
        Office,
        Storage,
        Balcony,
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

        /// <summary>Length of the room in meters (Z axis or floorplan Y axis).</summary>
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

        /// <summary>Floor height / elevation in meters.</summary>
        public float floorHeight = 0f;

        /// <summary>2D floor plan fill color.</summary>
        public Color fillColor = new Color(0.9f, 0.9f, 0.85f, 0.4f);

        /// <summary>Furniture placed in this room.</summary>
        public List<FurnitureItemData> furniture = new List<FurnitureItemData>();

        /// <summary>Direct openings list (if stored per-room).</summary>
        public List<WallOpeningData> openings = new List<WallOpeningData>();

        // --- Compatibility Properties ---
        public Vector2 size
        {
            get => new Vector2(width, length);
            set
            {
                width = value.x;
                length = value.y;
            }
        }

        public float ceilingHeight
        {
            get => height;
            set => height = value;
        }

        public float positionX
        {
            get => position.x;
            set => position = new Vector2Serializable(value, position.y);
        }

        public float positionY
        {
            get => position.y;
            set => position = new Vector2Serializable(position.x, value);
        }

        public string floorMaterialId
        {
            get => floorMaterial;
            set => floorMaterial = value;
        }

        /// <summary>
        /// Calculates the floor area of the room in square meters.
        /// </summary>
        public float GetArea()
        {
            return width * length;
        }

        /// <summary>
        /// Calculates the perimeter of the room in meters.
        /// </summary>
        public float GetPerimeter()
        {
            return 2f * (width + length);
        }

        /// <summary>
        /// Validates all room data including dimensions and child wall data.
        /// </summary>
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

            return isValid;
        }
    }
}
