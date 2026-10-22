using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Persistence
{
    /// <summary>
    /// Singleton MonoBehaviour that manages save / load / delete / import / export
    /// operations for house-planning projects. Files are stored as JSON under
    /// <c>Application.persistentDataPath/Projects/</c>.
    /// </summary>
    public class ProjectPersistenceService : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Singleton
        // -----------------------------------------------------------------------

        private static ProjectPersistenceService _instance;

        /// <summary>Global singleton accessor.</summary>
        public static ProjectPersistenceService Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning(
                        "[ProjectPersistenceService] Instance requested before Awake. " +
                        "Make sure the GameObject exists in the scene.");
                }
                return _instance;
            }
        }

        // -----------------------------------------------------------------------
        // Events
        // -----------------------------------------------------------------------

        /// <summary>Raised after a project is successfully saved. Carries the project ID.</summary>
        public event Action<string> OnProjectSaved;

        /// <summary>Raised after a project is successfully loaded. Carries the loaded data.</summary>
        public event Action<ProjectData> OnProjectLoaded;

        /// <summary>Raised after a project is deleted. Carries the deleted project ID.</summary>
        public event Action<string> OnProjectDeleted;

        // -----------------------------------------------------------------------
        // Private state
        // -----------------------------------------------------------------------

        private string _projectsDirectory;

        // -----------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ProjectPersistenceService] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _projectsDirectory = Path.Combine(Application.persistentDataPath, "Projects");
            EnsureDirectoryExists();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Serializes the given <see cref="ProjectData"/> to JSON and writes it to disk.
        /// The file is named <c>{projectId}.json</c>.
        /// </summary>
        /// <param name="project">The project to save. Must not be <c>null</c>.</param>
        /// <returns><c>true</c> if the save succeeded; otherwise <c>false</c>.</returns>
        public bool SaveProject(ProjectData project)
        {
            if (project == null)
            {
                Debug.LogError("[ProjectPersistenceService] Cannot save a null project.");
                return false;
            }

            if (string.IsNullOrEmpty(project.projectId))
            {
                Debug.LogError("[ProjectPersistenceService] Project has no projectId. Aborting save.");
                return false;
            }

            try
            {
                // Stamp metadata before writing.
                project.schemaVersion = SchemaVersionUpgrader.CurrentVersion;
                project.modifiedAtIso = DateTime.UtcNow.ToString("o");

                string json = JsonUtility.ToJson(project, true);
                string filePath = GetProjectFilePath(project.projectId);

                EnsureDirectoryExists();
                File.WriteAllText(filePath, json);

                Debug.Log($"[ProjectPersistenceService] Saved project '{project.projectName}' → {filePath}");
                OnProjectSaved?.Invoke(project.projectId);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Failed to save project '{project.projectId}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a project from disk by its unique identifier.
        /// Applies schema upgrades if the file was written by an older version.
        /// </summary>
        /// <param name="projectId">The unique project identifier.</param>
        /// <returns>The deserialized <see cref="ProjectData"/>, or <c>null</c> on failure.</returns>
        public ProjectData LoadProject(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                Debug.LogError("[ProjectPersistenceService] Cannot load project – projectId is null/empty.");
                return null;
            }

            string filePath = GetProjectFilePath(projectId);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[ProjectPersistenceService] Project file not found: {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);

                // Schema upgrade pass.
                string upgradedJson = SchemaVersionUpgrader.UpgradeIfNeeded(json);
                if (upgradedJson == null)
                {
                    Debug.LogError(
                        $"[ProjectPersistenceService] Schema upgrade failed for project '{projectId}'.");
                    return null;
                }

                ProjectData project = JsonUtility.FromJson<ProjectData>(upgradedJson);

                if (project == null)
                {
                    Debug.LogError(
                        $"[ProjectPersistenceService] Deserialization returned null for project '{projectId}'.");
                    return null;
                }

                // Basic validation.
                if (string.IsNullOrEmpty(project.projectId))
                {
                    Debug.LogWarning(
                        "[ProjectPersistenceService] Loaded project has empty projectId – patching.");
                    project.projectId = projectId;
                }

                // If the file was upgraded on disk, re-save at the current version.
                if (upgradedJson != json)
                {
                    File.WriteAllText(filePath, upgradedJson);
                    Debug.Log($"[ProjectPersistenceService] Re-saved upgraded project '{projectId}'.");
                }

                Debug.Log(
                    $"[ProjectPersistenceService] Loaded project '{project.projectName}' " +
                    $"(v{project.schemaVersion}).");

                OnProjectLoaded?.Invoke(project);
                return project;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Failed to load project '{projectId}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Scans the projects directory and returns lightweight summaries of every
        /// saved project without fully deserializing each file.
        /// </summary>
        /// <returns>A list of <see cref="ProjectSummary"/> instances.</returns>
        public List<ProjectSummary> LoadAllProjectSummaries()
        {
            List<ProjectSummary> summaries = new List<ProjectSummary>();

            EnsureDirectoryExists();

            string[] files;
            try
            {
                files = Directory.GetFiles(_projectsDirectory, "*.json");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Failed to enumerate project files: {ex.Message}");
                return summaries;
            }

            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    ProjectData project = JsonUtility.FromJson<ProjectData>(json);

                    if (project == null)
                    {
                        Debug.LogWarning($"[ProjectPersistenceService] Skipping invalid file: {file}");
                        continue;
                    }

                    int furnitureCount = 0;
                    int roomCount = 0;

                    if (project.rooms != null)
                    {
                        roomCount = project.rooms.Count;
                        foreach (RoomData room in project.rooms)
                        {
                            if (room.furniture != null)
                            {
                                furnitureCount += room.furniture.Count;
                            }
                        }
                    }

                    DateTime modifiedAt = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(project.modifiedAtIso))
                    {
                        DateTime.TryParse(project.modifiedAtIso, null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out modifiedAt);
                    }

                    summaries.Add(ProjectSummary.Create(
                        project.projectId,
                        project.projectName,
                        modifiedAt,
                        roomCount,
                        furnitureCount));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[ProjectPersistenceService] Could not read summary from '{file}': {ex.Message}");
                }
            }

            Debug.Log($"[ProjectPersistenceService] Loaded {summaries.Count} project summary(ies).");
            return summaries;
        }

        /// <summary>
        /// Permanently deletes a project file from disk.
        /// </summary>
        /// <param name="projectId">The project to delete.</param>
        /// <returns><c>true</c> if the file was deleted; <c>false</c> otherwise.</returns>
        public bool DeleteProject(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                Debug.LogError("[ProjectPersistenceService] Cannot delete – projectId is null/empty.");
                return false;
            }

            string filePath = GetProjectFilePath(projectId);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning(
                    $"[ProjectPersistenceService] Cannot delete – file not found: {filePath}");
                return false;
            }

            try
            {
                File.Delete(filePath);
                Debug.Log($"[ProjectPersistenceService] Deleted project '{projectId}'.");
                OnProjectDeleted?.Invoke(projectId);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Failed to delete project '{projectId}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks whether a saved project file exists on disk.
        /// </summary>
        /// <param name="projectId">The project identifier to check.</param>
        /// <returns><c>true</c> if the file exists.</returns>
        public bool ProjectExists(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return false;
            }
            return File.Exists(GetProjectFilePath(projectId));
        }

        /// <summary>
        /// Copies a project file to the specified export path.
        /// </summary>
        /// <param name="projectId">The project to export.</param>
        /// <param name="exportPath">Absolute destination file path.</param>
        /// <returns><c>true</c> if the export succeeded.</returns>
        public bool ExportProject(string projectId, string exportPath)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                Debug.LogError("[ProjectPersistenceService] Cannot export – projectId is null/empty.");
                return false;
            }

            if (string.IsNullOrEmpty(exportPath))
            {
                Debug.LogError("[ProjectPersistenceService] Cannot export – exportPath is null/empty.");
                return false;
            }

            string sourcePath = GetProjectFilePath(projectId);

            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning(
                    $"[ProjectPersistenceService] Cannot export – source file not found: {sourcePath}");
                return false;
            }

            try
            {
                string exportDir = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir);
                }

                File.Copy(sourcePath, exportPath, overwrite: true);
                Debug.Log(
                    $"[ProjectPersistenceService] Exported project '{projectId}' → {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Failed to export project '{projectId}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Imports a project file from an external path into the projects directory.
        /// The file is validated, schema-upgraded, and stored under its original project ID.
        /// </summary>
        /// <param name="importPath">Absolute path to the <c>.json</c> project file.</param>
        /// <returns>The imported <see cref="ProjectData"/>, or <c>null</c> on failure.</returns>
        public ProjectData ImportProject(string importPath)
        {
            if (string.IsNullOrEmpty(importPath))
            {
                Debug.LogError("[ProjectPersistenceService] Cannot import – importPath is null/empty.");
                return null;
            }

            if (!File.Exists(importPath))
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Cannot import – file not found: {importPath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(importPath);

                string upgradedJson = SchemaVersionUpgrader.UpgradeIfNeeded(json);
                if (upgradedJson == null)
                {
                    Debug.LogError(
                        "[ProjectPersistenceService] Schema upgrade failed during import.");
                    return null;
                }

                ProjectData project = JsonUtility.FromJson<ProjectData>(upgradedJson);

                if (project == null)
                {
                    Debug.LogError(
                        "[ProjectPersistenceService] Deserialization failed during import.");
                    return null;
                }

                // Assign a new ID if the project would collide with an existing one.
                if (string.IsNullOrEmpty(project.projectId))
                {
                    project.projectId = Guid.NewGuid().ToString();
                }
                else if (ProjectExists(project.projectId))
                {
                    string oldId = project.projectId;
                    project.projectId = Guid.NewGuid().ToString();
                    Debug.Log(
                        $"[ProjectPersistenceService] Imported project ID collision " +
                        $"'{oldId}' → new ID '{project.projectId}'.");
                }

                // Persist the imported project.
                if (!SaveProject(project))
                {
                    Debug.LogError("[ProjectPersistenceService] Save after import failed.");
                    return null;
                }

                Debug.Log(
                    $"[ProjectPersistenceService] Imported project '{project.projectName}' " +
                    $"({project.projectId}) from {importPath}");
                return project;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ProjectPersistenceService] Failed to import from '{importPath}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns the absolute file path where the given project is (or would be) stored.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>Absolute file path ending in <c>.json</c>.</returns>
        public string GetProjectFilePath(string projectId)
        {
            return Path.Combine(_projectsDirectory, $"{projectId}.json");
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates the projects directory if it does not already exist.
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_projectsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_projectsDirectory);
                    Debug.Log(
                        $"[ProjectPersistenceService] Created projects directory: {_projectsDirectory}");
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[ProjectPersistenceService] Failed to create directory: {ex.Message}");
                }
            }
        }
    }
}
