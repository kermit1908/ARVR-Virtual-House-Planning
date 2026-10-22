#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HousePlanner.Core;
using HousePlanner.FloorPlan;
using HousePlanner.HouseGeneration;
using HousePlanner.Furniture;
using HousePlanner.AR;
using HousePlanner.UI;
using HousePlanner.Persistence;
using HousePlanner.Backend;

namespace HousePlanner.Editor
{
    /// <summary>
    /// Automated Project and Scene setup utility for the AR/VR House Planning project.
    /// Provides top-level editor menu commands under "House Planner/" to automatically create
    /// all required scenes, hierarchy objects, camera configurations, lighting, and managers.
    /// </summary>
    public static class HousePlannerSetup
    {
        private const string ScenesDir = "Assets/Scenes";

        [MenuItem("House Planner/1. Setup All Scenes & Hierarchy", false, 1)]
        public static void SetupAllScenes()
        {
            EnsureDirectories();
            SetupMainMenuScene();
            SetupFloorPlannerScene();
            SetupHouseViewerScene();
            SetupARScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("House Planner Setup", 
                "All 4 scenes (MainMenu, FloorPlanner, HouseViewer, ARView) have been successfully generated and configured in Build Settings!", 
                "OK");
        }

        [MenuItem("House Planner/2. Create Default Materials & Assets", false, 2)]
        public static void CreateDefaultMaterials()
        {
            EnsureDirectories();
            MaterialManager.Instance.CreateFloorMaterial("wood");
            MaterialManager.Instance.CreateFloorMaterial("tile");
            MaterialManager.Instance.CreateWallMaterial(new Color(0.9f, 0.9f, 0.9f));
            MaterialManager.Instance.CreateCeilingMaterial();
            Debug.Log("[HousePlannerSetup] Default materials initialized.");
        }

        [MenuItem("House Planner/3. Create Sample Project File", false, 3)]
        public static void CreateSampleProjectFile()
        {
            ProjectData sample = SampleProjectGenerator.CreateSampleProject();
            string json = JsonUtility.ToJson(sample, true);
            string path = Path.Combine(Application.dataPath, "Resources/SampleProject.json");
            
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"[HousePlannerSetup] Sample project created at {path}");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(ScenesDir)) Directory.CreateDirectory(ScenesDir);
            if (!Directory.Exists("Assets/Materials")) Directory.CreateDirectory("Assets/Materials");
            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
            if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
        }

        private static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            // 1. Camera & Lighting
            CreateMainCamera(new Vector3(0, 1, -10), Quaternion.identity, true);
            CreateDirectionalLight();

            // 2. Event System
            CreateEventSystem();

            // 3. Managers
            GameObject managers = new GameObject("AppManagers");
            managers.AddComponent<NavigationManager>();
            managers.AddComponent<ProjectPersistenceService>();
            managers.AddComponent<BackendApiClient>();

            // 4. UI Canvas
            GameObject canvasGo = CreateCanvas("MainMenuCanvas");
            MainMenuUI menuUI = canvasGo.AddComponent<MainMenuUI>();
            canvasGo.AddComponent<DialogUI>();

            // Main Menu Panels & Buttons
            GameObject panel = CreateUIPanel(canvasGo.transform, "MenuPanel", new Color(0.12f, 0.12f, 0.15f, 0.95f));
            CreateUIText(panel.transform, "Title", "AR/VR HOUSE PLANNER", 32, TextAnchor.MiddleCenter, new Vector2(0, 180), new Vector2(600, 60));

            CreateUIButton(panel.transform, "BtnCreateNew", "Create New Project", new Vector2(0, 90), () => {});
            CreateUIButton(panel.transform, "BtnOpenExisting", "Open Existing Project", new Vector2(0, 30), () => {});
            CreateUIButton(panel.transform, "BtnRoomPlanner", "2D Room Planner", new Vector2(0, -30), () => {});
            CreateUIButton(panel.transform, "Btn3DViewer", "3D House Viewer", new Vector2(0, -90), () => {});
            CreateUIButton(panel.transform, "BtnARPreview", "AR Room Preview", new Vector2(0, -150), () => {});
            CreateUIButton(panel.transform, "BtnHelp", "Help & Guide", new Vector2(0, -210), () => {});

            string scenePath = $"{ScenesDir}/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[HousePlannerSetup] Created scene: {scenePath}");
        }

        private static void SetupFloorPlannerScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FloorPlanner";

            // 1. Orthographic Camera for Top-Down 2D
            GameObject camGo = new GameObject("FloorPlanCamera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.transform.position = new Vector3(5f, 20f, 5f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.2f, 0.24f);
            camGo.AddComponent<FloorPlanCamera>();

            // 2. Event System & Lighting
            CreateEventSystem();
            CreateDirectionalLight();

            // 3. Controllers
            GameObject controllers = new GameObject("FloorPlanControllers");
            var fpc = controllers.AddComponent<FloorPlanController>();
            controllers.AddComponent<FloorPlanRenderer>();
            controllers.AddComponent<RoomCreationTool>();
            controllers.AddComponent<WallEditorTool>();

            // 4. UI Canvas
            GameObject canvasGo = CreateCanvas("FloorPlanCanvas");
            canvasGo.AddComponent<FloorPlanUI>();
            canvasGo.AddComponent<DialogUI>();

            string scenePath = $"{ScenesDir}/FloorPlanner.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[HousePlannerSetup] Created scene: {scenePath}");
        }

        private static void SetupHouseViewerScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HouseViewer";

            // 1. Orbit / Free Camera
            GameObject camGo = CreateMainCamera(new Vector3(5, 8, -8), Quaternion.Euler(35, 0, 0), false);
            camGo.AddComponent<HouseViewCamera>();

            // 2. Lighting
            CreateDirectionalLight();
            CreateEventSystem();

            // 3. House Generation & Furniture Controllers
            GameObject houseGen = new GameObject("HouseGenerator");
            houseGen.AddComponent<MaterialManager>();
            houseGen.AddComponent<HouseGenerator>();

            GameObject furnitureCtrl = new GameObject("FurniturePlacementController");
            furnitureCtrl.AddComponent<FurniturePlacementController>();

            // 4. UI Canvas
            GameObject canvasGo = CreateCanvas("HouseViewerCanvas");
            canvasGo.AddComponent<HouseViewerUI>();
            canvasGo.AddComponent<FurnitureCatalogueUI>();
            canvasGo.AddComponent<InspectorPanelUI>();
            canvasGo.AddComponent<DialogUI>();

            string scenePath = $"{ScenesDir}/HouseViewer.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[HousePlannerSetup] Created scene: {scenePath}");
        }

        private static void SetupARScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ARView";

            // 1. Event System & Lighting
            CreateEventSystem();
            CreateDirectionalLight();

            // 2. AR Foundation Session & Origin Hierarchy
            GameObject arSessionGo = new GameObject("AR Session");
            arSessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();

            GameObject xrOriginGo = new GameObject("XR Origin");
            xrOriginGo.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            xrOriginGo.AddComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>();
            xrOriginGo.AddComponent<UnityEngine.XR.ARFoundation.ARRaycastManager>();
            xrOriginGo.AddComponent<UnityEngine.XR.ARFoundation.ARAnchorManager>();

            GameObject cameraGo = new GameObject("AR Camera");
            cameraGo.transform.SetParent(xrOriginGo.transform, false);
            Camera arCam = cameraGo.AddComponent<Camera>();
            arCam.clearFlags = CameraClearFlags.SolidColor;
            arCam.backgroundColor = Color.black;
            cameraGo.AddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
            cameraGo.AddComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();

            // Link camera to XROrigin
            var origin = xrOriginGo.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null) origin.Camera = arCam;

            // 3. AR Controllers
            GameObject arCtrlGo = new GameObject("ARPlacementController");
            arCtrlGo.AddComponent<ARPlacementController>();
            arCtrlGo.AddComponent<ARHousePreview>();

            // 4. AR Placement Indicator Reticle
            GameObject reticle = new GameObject("PlacementIndicator");
            reticle.AddComponent<PlacementIndicator>();

            // 5. AR UI Canvas
            GameObject canvasGo = CreateCanvas("ARViewCanvas");
            canvasGo.AddComponent<ARUIController>();
            canvasGo.AddComponent<DialogUI>();

            string scenePath = $"{ScenesDir}/ARView.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[HousePlannerSetup] Created scene: {scenePath}");
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesDir}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/FloorPlanner.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/HouseViewer.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/ARView.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[HousePlannerSetup] Build Settings updated with all 4 scenes.");
        }

        private static GameObject CreateMainCamera(Vector3 position, Quaternion rotation, bool solidColor)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = solidColor ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            camGo.transform.position = position;
            camGo.transform.rotation = rotation;
            camGo.AddComponent<AudioListener>();
            return camGo;
        }

        private static GameObject CreateDirectionalLight()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.96f, 0.9f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            return lightGo;
        }

        private static void CreateEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasGo = new GameObject(name);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image img = panel.AddComponent<Image>();
            img.color = color;
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return panel;
        }

        private static void CreateUIText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 size)
        {
            GameObject textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            Text t = textGo.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = Color.white;
            t.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform rt = textGo.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        private static void CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPos, System.Action onClick)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            Image img = btnGo.AddComponent<Image>();
            img.color = new Color(0.24f, 0.35f, 0.5f, 1f);
            Button btn = btnGo.AddComponent<Button>();
            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(320, 48);

            CreateUIText(btnGo.transform, "Label", label, 18, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(320, 48));
        }
    }
}
#endif
