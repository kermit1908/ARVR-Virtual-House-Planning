using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HousePlanner.Core;
using HousePlanner.Persistence;

namespace HousePlanner.Backend
{
    /// <summary>
    /// HTTP Client for connecting Unity to the optional FastAPI backend server.
    /// Provides coroutine-based REST API calls with timeouts, JSON serialisation,
    /// and fallback to offline mode when the server is unreachable.
    /// </summary>
    public class BackendApiClient : MonoBehaviour
    {
        private static BackendApiClient _instance;
        public static BackendApiClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BackendApiClient>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("BackendApiClient");
                        _instance = go.AddComponent<BackendApiClient>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private string _baseUrl = "http://localhost:8000/api";
        [SerializeField] private float _timeoutSeconds = 5f;

        [Header("Runtime Status")]
        [SerializeField] private bool _isBackendAvailable = false;

        public bool IsBackendAvailable => _isBackendAvailable;
        public string BaseUrl
        {
            get => _baseUrl;
            set => _baseUrl = value;
        }

        public event Action<bool> OnBackendStatusChanged;
        public event Action<string> OnBackendError;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CheckHealth();
        }

        public void CheckHealth(Action<bool> onComplete = null)
        {
            StartCoroutine(CheckHealthRoutine(onComplete));
        }

        private IEnumerator CheckHealthRoutine(Action<bool> onComplete)
        {
            using (UnityWebRequest req = UnityWebRequest.Get($"{_baseUrl}/health"))
            {
                req.timeout = Mathf.RoundToInt(_timeoutSeconds);
                yield return req.SendWebRequest();

                bool available = req.result == UnityWebRequest.Result.Success;
                if (_isBackendAvailable != available)
                {
                    _isBackendAvailable = available;
                    OnBackendStatusChanged?.Invoke(available);
                }
                onComplete?.Invoke(available);
            }
        }

        public void CreateProject(ProjectData project, Action<bool> callback)
        {
            StartCoroutine(PostProjectRoutine(project, callback));
        }

        private IEnumerator PostProjectRoutine(ProjectData project, Action<bool> callback)
        {
            string json = JsonUtility.ToJson(project);
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest req = new UnityWebRequest($"{_baseUrl}/projects", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = Mathf.RoundToInt(_timeoutSeconds);

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    OnBackendError?.Invoke($"POST /projects error: {req.error}");
                    callback?.Invoke(false);
                }
            }
        }

        public void GetProject(string projectId, Action<ProjectData> callback)
        {
            StartCoroutine(GetProjectRoutine(projectId, callback));
        }

        private IEnumerator GetProjectRoutine(string projectId, Action<ProjectData> callback)
        {
            using (UnityWebRequest req = UnityWebRequest.Get($"{_baseUrl}/projects/{projectId}"))
            {
                req.timeout = Mathf.RoundToInt(_timeoutSeconds);
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        ProjectData data = JsonUtility.FromJson<ProjectData>(req.downloadHandler.text);
                        callback?.Invoke(data);
                    }
                    catch (Exception ex)
                    {
                        OnBackendError?.Invoke($"Parse error: {ex.Message}");
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    OnBackendError?.Invoke($"GET /projects/{projectId} error: {req.error}");
                    callback?.Invoke(null);
                }
            }
        }

        public void UpdateProject(ProjectData project, Action<bool> callback)
        {
            StartCoroutine(PutProjectRoutine(project, callback));
        }

        private IEnumerator PutProjectRoutine(ProjectData project, Action<bool> callback)
        {
            string json = JsonUtility.ToJson(project);
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest req = new UnityWebRequest($"{_baseUrl}/projects/{project.projectId}", "PUT"))
            {
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = Mathf.RoundToInt(_timeoutSeconds);

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    OnBackendError?.Invoke($"PUT /projects error: {req.error}");
                    callback?.Invoke(false);
                }
            }
        }

        public void DeleteProject(string projectId, Action<bool> callback)
        {
            StartCoroutine(DeleteProjectRoutine(projectId, callback));
        }

        private IEnumerator DeleteProjectRoutine(string projectId, Action<bool> callback)
        {
            using (UnityWebRequest req = UnityWebRequest.Delete($"{_baseUrl}/projects/{projectId}"))
            {
                req.timeout = Mathf.RoundToInt(_timeoutSeconds);
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    OnBackendError?.Invoke($"DELETE /projects error: {req.error}");
                    callback?.Invoke(false);
                }
            }
        }
    }
}
