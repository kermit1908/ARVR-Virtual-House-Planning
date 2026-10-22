using System;
using UnityEngine;

namespace HousePlanner.Persistence
{
    /// <summary>
    /// Handles forward-compatible schema version upgrades for serialized project JSON.
    /// When the data format changes in future releases, sequential upgrade methods are
    /// added here so that older project files can still be loaded.
    /// </summary>
    public static class SchemaVersionUpgrader
    {
        /// <summary>
        /// The current schema version that new projects are saved with.
        /// Increment this whenever <see cref="HousePlanner.Core.ProjectData"/> gains
        /// breaking serialization changes, and add a corresponding upgrade method.
        /// </summary>
        public static readonly int CurrentVersion = 1;

        /// <summary>
        /// Lightweight wrapper used only for peeking at the schema version inside a
        /// JSON string without deserializing the entire project.
        /// </summary>
        [Serializable]
        private class VersionPeek
        {
            public int schemaVersion;
        }

        /// <summary>
        /// Inspects the <c>schemaVersion</c> field in the provided JSON and applies
        /// sequential upgrade transformations until it matches <see cref="CurrentVersion"/>.
        /// </summary>
        /// <param name="json">Raw JSON string of a serialized project.</param>
        /// <returns>
        /// The (potentially upgraded) JSON string at <see cref="CurrentVersion"/>,
        /// or <c>null</c> if the version is from the future and cannot be handled.
        /// </returns>
        public static string UpgradeIfNeeded(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[SchemaVersionUpgrader] Cannot upgrade null or empty JSON.");
                return null;
            }

            VersionPeek peek;
            try
            {
                peek = JsonUtility.FromJson<VersionPeek>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SchemaVersionUpgrader] Failed to peek schema version: {ex.Message}");
                return null;
            }

            int version = peek.schemaVersion;

            // Future version – we cannot safely downgrade.
            if (version > CurrentVersion)
            {
                Debug.LogWarning(
                    $"[SchemaVersionUpgrader] Project has schema version {version} " +
                    $"which is newer than the current version {CurrentVersion}. " +
                    "This build cannot safely load it. Please update to a newer version of the app.");
                return null;
            }

            // Already up-to-date.
            if (version == CurrentVersion)
            {
                return json;
            }

            // Sequential upgrades.
            string upgraded = json;

            if (version < 1)
            {
                upgraded = UpgradeFrom0To1(upgraded);
                version = 1;
            }

            // -------------------------------------------------------------------
            // Future upgrade steps go here:
            //   if (version < 2) { upgraded = UpgradeFrom1To2(upgraded); version = 2; }
            //   if (version < 3) { upgraded = UpgradeFrom2To3(upgraded); version = 3; }
            // -------------------------------------------------------------------

            Debug.Log($"[SchemaVersionUpgrader] Upgraded project from schema v{peek.schemaVersion} to v{version}.");
            return upgraded;
        }

        /// <summary>
        /// Returns the schema version found in the JSON without performing any upgrade.
        /// Returns <c>-1</c> if the version cannot be determined.
        /// </summary>
        /// <param name="json">Raw JSON string.</param>
        /// <returns>The schema version, or <c>-1</c> on failure.</returns>
        public static int PeekVersion(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return -1;
            }

            try
            {
                VersionPeek peek = JsonUtility.FromJson<VersionPeek>(json);
                return peek.schemaVersion;
            }
            catch
            {
                return -1;
            }
        }

        // -----------------------------------------------------------------------
        // Upgrade helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Upgrades a legacy version-0 (pre-versioned) project to version 1.
        /// Version 0 projects simply lack a <c>schemaVersion</c> field; we inject it.
        /// </summary>
        private static string UpgradeFrom0To1(string json)
        {
            // A version-0 project is identical in shape to version 1 but is missing
            // the schemaVersion field (defaults to 0 in deserialization).
            // Simply inject the field so it round-trips correctly.
            // We do a lightweight string manipulation to avoid a full deserialize/re-serialize
            // that could drop unknown future fields.
            if (json.TrimStart().StartsWith("{"))
            {
                // Insert schemaVersion right after the opening brace.
                int braceIndex = json.IndexOf('{');
                json = json.Insert(braceIndex + 1, "\"schemaVersion\":1,");
            }

            Debug.Log("[SchemaVersionUpgrader] Applied upgrade v0 → v1.");
            return json;
        }
    }
}
