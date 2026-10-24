using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using HousePlanner.Core;
using HousePlanner.Persistence;

namespace HousePlanner.UI
{
    /// <summary>
    /// Global navigation and project session context manager.
    /// Preserves active project state across scene transitions and screen panels.
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        private static NavigationManager _instance;
        public static NavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NavigationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("NavigationManager");
                        _instance = go.AddComponent<NavigationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // Active project carried across scenes
        public static ProjectData CurrentProject { get; set; }

        public const string SceneMainMenu = "MainMenu";
        public const string SceneFloorPlanner = "FloorPlanner";
        public const string SceneHouseViewer = "HouseViewer";
        public const string SceneARView = "ARView";

        public event Action<string> OnScreenChanged;
        public event Action<ProjectData> OnCurrentProjectChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure sample project exists if none loaded
            if (CurrentProject == null)
            {
                CurrentProject = SampleProjectGenerator.CreateSampleProject();
            }
        }

        public void SetActiveProject(ProjectData project)
        {
            CurrentProject = project;
            OnCurrentProjectChanged?.Invoke(project);
        }

        public void GoToMainMenu()
        {
            LoadSceneSafely(SceneMainMenu);
        }

        public void GoToFloorPlanner()
        {
            LoadSceneSafely(SceneFloorPlanner);
        }

        public void GoToHouseViewer()
        {
            LoadSceneSafely(SceneHouseViewer);
        }

        public void GoToARView()
        {
            LoadSceneSafely(SceneARView);
        }

        private void LoadSceneSafely(string sceneName)
        {
            Debug.Log($"[NavigationManager] Transitioning to scene: '{sceneName}'...");
            try
            {
                SceneManager.LoadScene(sceneName);
                OnScreenChanged?.Invoke(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NavigationManager] Direct load failed for '{sceneName}': {ex.Message}. Trying with Assets/Scenes/ path...");
                try
                {
                    SceneManager.LoadScene($"Assets/Scenes/{sceneName}.unity");
                    OnScreenChanged?.Invoke(sceneName);
                }
                catch (Exception ex2)
                {
                    Debug.LogError($"[NavigationManager] Scene load failed: {ex2.Message}");
                    OnScreenChanged?.Invoke(sceneName);
                }
            }
        }
    }
}
