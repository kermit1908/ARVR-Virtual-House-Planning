using System;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// Categorises furniture by its functional group.
    /// </summary>
    public enum FurnitureCategory
    {
        Seating,
        Bed,
        Table,
        Storage,
        Kitchen,
        Bathroom,
        Lighting,
        Decorative,
        Door,
        Window,
        Electronics,
        Decor,
        Other
    }

    /// <summary>
    /// Serializable data for a single furniture instance placed within the house plan.
    /// </summary>
    [Serializable]
    public class FurnitureItemData
    {
        /// <summary>Unique instance identifier.</summary>
        public string instanceId;

        /// <summary>Catalogue entry identifier.</summary>
        public string catalogueId;

        /// <summary>Identifier of the room this item belongs to.</summary>
        public string roomId;

        /// <summary>Functional category.</summary>
        public FurnitureCategory category;

        /// <summary>World-space position in meters.</summary>
        public Vector3Serializable position;

        /// <summary>Euler rotation in degrees.</summary>
        public Vector3Serializable rotation;

        /// <summary>Scale multiplier (default 1,1,1).</summary>
        public Vector3Serializable scale = Vector3Serializable.One;

        /// <summary>Material / paint colour hex string.</summary>
        public string materialColor = "#FFFFFF";

        /// <summary>Display name.</summary>
        public string customName;

        /// <summary>2D footprint size in meters.</summary>
        public Vector2 footprintSize = new Vector2(1f, 1f);

        // --- Compatibility Aliases ---
        public string itemId
        {
            get => instanceId;
            set => instanceId = value;
        }

        public string itemName
        {
            get => customName;
            set => customName = value;
        }

        public string catalogId
        {
            get => catalogueId;
            set => catalogueId = value;
        }

        public string parentRoomId
        {
            get => roomId;
            set => roomId = value;
        }

        public string furnitureId
        {
            get => instanceId;
            set => instanceId = value;
        }

        public string furnitureName
        {
            get => customName;
            set => customName = value;
        }

        public float positionX
        {
            get => position.x;
            set => position = new Vector3Serializable(value, position.y, position.z);
        }

        public float positionY
        {
            get => position.z;
            set => position = new Vector3Serializable(position.x, position.y, value);
        }

        public float sizeX
        {
            get => scale.x;
            set => scale = new Vector3Serializable(value, scale.y, scale.z);
        }

        public float sizeY
        {
            get => scale.z;
            set => scale = new Vector3Serializable(scale.x, scale.y, value);
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(instanceId)) return false;
            return true;
        }
    }

    /// <summary>
    /// Alias class for backward compatibility with subagent code.
    /// </summary>
    [Serializable]
    public class FurnitureData : FurnitureItemData
    {
    }
}
