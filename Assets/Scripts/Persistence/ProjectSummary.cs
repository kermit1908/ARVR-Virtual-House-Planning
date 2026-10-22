using System;

namespace HousePlanner.Persistence
{
    /// <summary>
    /// Lightweight summary of a project used for listing projects
    /// without loading the full project data.
    /// </summary>
    [Serializable]
    public class ProjectSummary
    {
        /// <summary>Unique identifier for the project.</summary>
        public string projectId;

        /// <summary>Human-readable project name.</summary>
        public string projectName;

        /// <summary>ISO 8601 timestamp of the last modification.</summary>
        public string modifiedAtIso;

        /// <summary>Number of rooms in the project.</summary>
        public int roomCount;

        /// <summary>Total number of furniture items across all rooms.</summary>
        public int furnitureCount;

        /// <summary>
        /// Gets the last-modified date as a <see cref="DateTime"/>.
        /// Falls back to <see cref="DateTime.MinValue"/> when the stored string cannot be parsed.
        /// </summary>
        public DateTime ModifiedAt
        {
            get
            {
                if (DateTime.TryParse(modifiedAtIso, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out DateTime result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ProjectSummary"/> with the given values.
        /// </summary>
        /// <param name="projectId">Unique project identifier.</param>
        /// <param name="projectName">Display name of the project.</param>
        /// <param name="modifiedAt">Last-modified timestamp.</param>
        /// <param name="roomCount">Number of rooms.</param>
        /// <param name="furnitureCount">Total furniture items.</param>
        /// <returns>A populated <see cref="ProjectSummary"/>.</returns>
        public static ProjectSummary Create(string projectId, string projectName,
            DateTime modifiedAt, int roomCount, int furnitureCount)
        {
            return new ProjectSummary
            {
                projectId = projectId,
                projectName = projectName,
                modifiedAtIso = modifiedAt.ToString("o"),
                roomCount = roomCount,
                furnitureCount = furnitureCount
            };
        }

        public override string ToString()
        {
            return $"[ProjectSummary] {projectName} ({projectId}) – " +
                   $"{roomCount} rooms, {furnitureCount} furniture, modified {modifiedAtIso}";
        }
    }
}
