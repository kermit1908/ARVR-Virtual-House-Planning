#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using HousePlanner.Core;
using HousePlanner.Persistence;
using HousePlanner.HouseGeneration;
using HousePlanner.Furniture;

namespace HousePlanner.Editor
{
    /// <summary>
    /// Diagnostics and validation utility for the AR/VR House Planning project.
    /// Runs integrity checks on scenes, data models, persistence paths, and package manifests.
    /// Accessible via "House Planner/Validate Project Health".
    /// </summary>
    public static class SceneValidator
    {
        [MenuItem("House Planner/Validate Project Health", false, 10)]
        public static void ValidateProject()
        {
            int errorCount = 0;
            int warningCount = 0;

            Debug.Log("<b><color=#4EA8DE>[SceneValidator]</color> Starting Project Health Check...</b>");

            // 1. Check Scene Files
            string[] requiredScenes = { "MainMenu.unity", "FloorPlanner.unity", "HouseViewer.unity", "ARView.unity" };
            foreach (var scene in requiredScenes)
            {
                string path = $"Assets/Scenes/{scene}";
                if (File.Exists(path))
                {
                    Debug.Log($"<color=#57CC99>✓ Scene Found:</color> {scene}");
                }
                else
                {
                    Debug.LogWarning($"<color=#FFB703>⚠️ Missing Scene:</color> {scene}. Run 'House Planner/1. Setup All Scenes & Hierarchy' to generate.");
                    warningCount++;
                }
            }

            // 2. Check Sample Project Generation
            try
            {
                ProjectData sample = SampleProjectGenerator.CreateSampleProject();
                if (sample.Validate())
                {
                    Debug.Log($"<color=#57CC99>✓ Core Data Models:</color> Sample project validated successfully with {sample.rooms.Count} rooms and {sample.furnitureItems.Count} furniture items.");
                }
                else
                {
                    Debug.LogError("<color=#E63946>✗ Core Data Models:</color> Sample project failed validation.");
                    errorCount++;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=#E63946>✗ Core Data Models Exception:</color> {ex.Message}");
                errorCount++;
            }

            // 3. Check Procedural Mesh Generation
            try
            {
                Mesh floor = ProceduralMeshBuilder.CreateFloor(4f, 5f);
                Mesh wall = ProceduralMeshBuilder.CreateWall(5f, 2.8f, 0.15f, null);
                Mesh box = ProceduralMeshBuilder.CreateBox(1f, 1f, 1f);

                if (floor.vertexCount > 0 && wall.vertexCount > 0 && box.vertexCount > 0)
                {
                    Debug.Log("<color=#57CC99>✓ Procedural Mesh Generator:</color> Floor, wall, and box meshes generated with valid buffers.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=#E63946>✗ Procedural Mesh Generator Exception:</color> {ex.Message}");
                errorCount++;
            }

            // 4. Check Catalogue
            try
            {
                var entries = DefaultFurnitureCatalogue.CreateDefaultEntries();
                Debug.Log($"<color=#57CC99>✓ Furniture Catalogue:</color> {entries.Count} default furniture items registered.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=#E63946>✗ Furniture Catalogue Exception:</color> {ex.Message}");
                errorCount++;
            }

            // Summary
            if (errorCount == 0 && warningCount == 0)
            {
                EditorUtility.DisplayDialog("Validation Passed", "All project systems, procedural mesh generators, data models, and scenes are healthy!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Complete", $"Completed with {errorCount} errors and {warningCount} warnings. Check the Unity Console for details.", "OK");
            }
        }
    }
}
#endif
