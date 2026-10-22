using System;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// Categorises furniture by its functional group.
    /// </summary>
    public enum FurnitureCategory
    {
        /// <summary>Chairs, sofas, and other seating.</summary>
        Seating,

        /// <summary>Beds, mattresses, and sleeping furniture.</summary>
        Bed,

        /// <summary>Tables, desks, and countertops.</summary>
        Table,

        /// <summary>Wardrobes, shelves, cabinets, and storage units.</summary>
        Storage,

        /// <summary>Kitchen-specific fixtures and appliances.</summary>
        Kitchen,

        /// <summary>Bathroom-specific fixtures such as sinks, toilets, and tubs.</summary>
        Bathroom,

        /// <summary>Lamps, ceiling lights, and other light fixtures.</summary>
        Lighting,

        /// <summary>Decorative items such as plants, artwork, and rugs.</summary>
        Decorative,

        /// <summary>Door elements that can be placed as furniture items.</summary>
        Door,

        /// <summary>Window elements that can be placed as furniture items.</summary>
        Window
    }

    /// <summary>
    /// Serializable data for a single furniture instance placed within the house plan.
    /// References a catalogue entry via <see cref="catalogueId"/> and is optionally
    /// assigned to a room via <see cref="roomId"/>.
    /// </summary>
    [Serializable]
    public class FurnitureItemData
    {
        /// <summary>Unique instance identifier for this placed furniture item.</summary>
        public string instanceId;

        /// <summary>
        /// Identifier referencing the furniture catalogue entry that defines
        /// the 3D model, default dimensions, and metadata.
        /// </summary>
        public string catalogueId;

        /// <summary>
        /// Identifier of the room this item belongs to.
        /// Empty or null if the item is unassigned / floating.
        /// </summary>
        public string roomId;

        /// <summary>Functional category of this furniture item.</summary>
        public FurnitureCategory category;

        /// <summary>World-space position of the item in meters.</summary>
        public Vector3Serializable position;

        /// <summary>Euler rotation of the item in degrees.</summary>
        public Vector3Serializable rotation;

        /// <summary>Scale multiplier of the item (default 1,1,1).</summary>
        public Vector3Serializable scale = Vector3Serializable.One;

        /// <summary>Material / paint colour as a hex string (e.g. "#8B4513").</summary>
        public string materialColor = "#FFFFFF";

        /// <summary>Optional user-defined display name for this item.</summary>
        public string customName;

        /// <summary>
        /// Validates the furniture item data.
        /// </summary>
        /// <returns><c>true</c> if all required fields are valid; otherwise <c>false</c>.</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                Debug.LogWarning("[FurnitureItemData] instanceId is null or empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(catalogueId))
            {
                Debug.LogWarning($"[FurnitureItemData:{instanceId}] catalogueId is null or empty.");
                isValid = false;
            }

            // roomId is intentionally nullable – unassigned items are allowed.

            Vector3 s = scale;
            if (s.x <= 0f || s.y <= 0f || s.z <= 0f)
            {
                Debug.LogWarning(
                    $"[FurnitureItemData:{instanceId}] Scale components must be positive, got {scale}.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(materialColor))
            {
                Debug.LogWarning($"[FurnitureItemData:{instanceId}] materialColor is empty.");
                isValid = false;
            }

            return isValid;
        }
    }
}
