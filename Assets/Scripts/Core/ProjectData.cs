using System;
using System.Collections.Generic;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// Root data container for an entire house-planning project.
    /// Holds all rooms, furniture items, and floor plan settings.
    /// Designed for JSON serialisation via <see cref="JsonUtility"/>.
    /// </summary>
    [Serializable]
    public class ProjectData
    {
        // ------------------------------------------------------------------
        //  Constants
        // ------------------------------------------------------------------

        /// <summary>Current schema version for forward-compatibility checks.</summary>
        public const int CurrentSchemaVersion = 1;

        // ------------------------------------------------------------------
        //  Fields
        // ------------------------------------------------------------------

        /// <summary>Globally unique identifier for this project.</summary>
        public string projectId;

        /// <summary>Human-readable project name.</summary>
        public string projectName;

        /// <summary>
        /// ISO-8601 timestamp of when the project was first created.
        /// Stored as a string for <see cref="JsonUtility"/> compatibility.
        /// </summary>
        public string createdAt;

        /// <summary>
        /// ISO-8601 timestamp of the most recent modification.
        /// Stored as a string for <see cref="JsonUtility"/> compatibility.
        /// </summary>
        public string modifiedAt;

        /// <summary>
        /// Schema version number used for migration and compatibility.
        /// </summary>
        public int schemaVersion = CurrentSchemaVersion;

        /// <summary>All rooms in this project.</summary>
        public List<RoomData> rooms = new List<RoomData>();

        /// <summary>All placed furniture items in this project.</summary>
        public List<FurnitureItemData> furnitureItems = new List<FurnitureItemData>();

        /// <summary>Floor plan grid, snapping, and default dimension settings.</summary>
        public FloorPlanSettings floorPlanSettings = new FloorPlanSettings();

        // ------------------------------------------------------------------
        //  DateTime helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="createdAt"/> value as a <see cref="DateTime"/>.
        /// Returns <see cref="DateTime.MinValue"/> if parsing fails.
        /// </summary>
        public DateTime CreatedAtUtc
        {
            get
            {
                if (DateTime.TryParse(createdAt, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    return dt;
                }
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="modifiedAt"/> value as a <see cref="DateTime"/>.
        /// Returns <see cref="DateTime.MinValue"/> if parsing fails.
        /// </summary>
        public DateTime ModifiedAtUtc
        {
            get
            {
                if (DateTime.TryParse(modifiedAt, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    return dt;
                }
                return DateTime.MinValue;
            }
        }

        // ------------------------------------------------------------------
        //  Methods
        // ------------------------------------------------------------------

        /// <summary>
        /// Generates a new GUID string suitable for use as an identifier.
        /// </summary>
        /// <returns>A new GUID as a lowercase string without braces.</returns>
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// Creates a new <see cref="ProjectData"/> with sensible defaults and a
        /// freshly generated identifier.
        /// </summary>
        /// <param name="name">The human-readable project name.</param>
        /// <returns>A fully initialised default project.</returns>
        public static ProjectData CreateDefault(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Untitled Project";
            }

            string now = DateTime.UtcNow.ToString("O"); // ISO-8601 round-trip

            return new ProjectData
            {
                projectId = GenerateId(),
                projectName = name,
                createdAt = now,
                modifiedAt = now,
                schemaVersion = CurrentSchemaVersion,
                rooms = new List<RoomData>(),
                furnitureItems = new List<FurnitureItemData>(),
                floorPlanSettings = new FloorPlanSettings()
            };
        }

        /// <summary>
        /// Validates the entire project tree – project metadata, every room,
        /// every furniture item, and the floor plan settings.
        /// </summary>
        /// <returns><c>true</c> if the full project graph is valid; otherwise <c>false</c>.</returns>
        public bool Validate()
        {
            bool isValid = true;

            // --- Project-level fields ---
            if (string.IsNullOrWhiteSpace(projectId))
            {
                Debug.LogWarning("[ProjectData] projectId is null or empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(projectName))
            {
                Debug.LogWarning("[ProjectData] projectName is null or empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(createdAt))
            {
                Debug.LogWarning("[ProjectData] createdAt is null or empty.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(modifiedAt))
            {
                Debug.LogWarning("[ProjectData] modifiedAt is null or empty.");
                isValid = false;
            }

            if (schemaVersion < 1)
            {
                Debug.LogWarning($"[ProjectData] schemaVersion {schemaVersion} is invalid (must be >= 1).");
                isValid = false;
            }

            // --- Floor plan settings ---
            if (floorPlanSettings == null)
            {
                Debug.LogWarning("[ProjectData] floorPlanSettings is null.");
                isValid = false;
            }
            else if (!floorPlanSettings.Validate())
            {
                isValid = false;
            }

            // --- Rooms ---
            if (rooms == null)
            {
                Debug.LogWarning("[ProjectData] rooms list is null.");
                isValid = false;
            }
            else
            {
                var seenRoomIds = new HashSet<string>();
                foreach (var room in rooms)
                {
                    if (room == null)
                    {
                        Debug.LogWarning("[ProjectData] Contains a null room entry.");
                        isValid = false;
                        continue;
                    }

                    if (!room.Validate())
                    {
                        isValid = false;
                    }

                    if (!string.IsNullOrWhiteSpace(room.roomId) && !seenRoomIds.Add(room.roomId))
                    {
                        Debug.LogWarning($"[ProjectData] Duplicate roomId: {room.roomId}");
                        isValid = false;
                    }
                }
            }

            // --- Furniture items ---
            if (furnitureItems == null)
            {
                Debug.LogWarning("[ProjectData] furnitureItems list is null.");
                isValid = false;
            }
            else
            {
                var seenInstanceIds = new HashSet<string>();
                foreach (var item in furnitureItems)
                {
                    if (item == null)
                    {
                        Debug.LogWarning("[ProjectData] Contains a null furniture item entry.");
                        isValid = false;
                        continue;
                    }

                    if (!item.Validate())
                    {
                        isValid = false;
                    }

                    if (!string.IsNullOrWhiteSpace(item.instanceId) && !seenInstanceIds.Add(item.instanceId))
                    {
                        Debug.LogWarning($"[ProjectData] Duplicate furniture instanceId: {item.instanceId}");
                        isValid = false;
                    }
                }
            }

            // --- Stamp modified time on successful validation ---
            if (isValid)
            {
                modifiedAt = DateTime.UtcNow.ToString("O");
            }

            return isValid;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            int roomCount = rooms?.Count ?? 0;
            int furnitureCount = furnitureItems?.Count ?? 0;
            return $"Project '{projectName}' (v{schemaVersion}) – {roomCount} rooms, {furnitureCount} items";
        }
    }
}
