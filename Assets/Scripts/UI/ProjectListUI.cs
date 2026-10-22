using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HousePlanner.Core;
using HousePlanner.Persistence;

namespace HousePlanner.UI
{
    /// <summary>
    /// UI Controller for browsing, opening, renaming, and deleting saved house planning projects.
    /// Interacts directly with <see cref="ProjectPersistenceService"/>.
    /// </summary>
    public class ProjectListUI : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemTemplate;
        [SerializeField] private GameObject _emptyStateText;

        [Header("Buttons")]
        [SerializeField] private Button _btnRefresh;
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnLoadSample;

        private void Start()
        {
            if (_btnRefresh != null) _btnRefresh.onClick.AddListener(RefreshList);
            if (_btnClose != null) _btnClose.onClick.AddListener(() => gameObject.SetActive(false));
            if (_btnLoadSample != null) _btnLoadSample.onClick.AddListener(OnLoadSampleClicked);

            if (_itemTemplate != null) _itemTemplate.SetActive(false);
            RefreshList();
        }

        private void OnEnable()
        {
            RefreshList();
        }

        public void RefreshList()
        {
            if (_itemsContainer == null) return;

            // Clear old items (except template)
            for (int i = _itemsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = _itemsContainer.GetChild(i);
                if (child.gameObject != _itemTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            if (ProjectPersistenceService.Instance == null)
            {
                if (_emptyStateText != null) _emptyStateText.SetActive(true);
                return;
            }

            List<ProjectSummary> summaries = ProjectPersistenceService.Instance.LoadAllProjectSummaries();

            if (summaries == null || summaries.Count == 0)
            {
                if (_emptyStateText != null) _emptyStateText.SetActive(true);
                return;
            }

            if (_emptyStateText != null) _emptyStateText.SetActive(false);

            foreach (var summary in summaries)
            {
                CreateItemRow(summary);
            }
        }

        private void CreateItemRow(ProjectSummary summary)
        {
            GameObject row = _itemTemplate != null 
                ? Instantiate(_itemTemplate, _itemsContainer) 
                : new GameObject($"Project_{summary.projectId}");

            row.SetActive(true);

            // Populate text
            Text titleText = row.transform.Find("TitleText")?.GetComponent<Text>();
            if (titleText == null) titleText = row.GetComponentInChildren<Text>();
            if (titleText != null)
            {
                titleText.text = $"{summary.projectName} ({summary.roomCount} rooms, {summary.furnitureCount} items)";
            }

            Text dateText = row.transform.Find("DateText")?.GetComponent<Text>();
            if (dateText != null)
            {
                dateText.text = summary.modifiedAt;
            }

            // Wire buttons
            Button btnOpen = row.transform.Find("BtnOpen")?.GetComponent<Button>();
            if (btnOpen != null)
            {
                btnOpen.onClick.AddListener(() => OpenProject(summary.projectId));
            }

            Button btnRename = row.transform.Find("BtnRename")?.GetComponent<Button>();
            if (btnRename != null)
            {
                btnRename.onClick.AddListener(() => RenameProject(summary));
            }

            Button btnDelete = row.transform.Find("BtnDelete")?.GetComponent<Button>();
            if (btnDelete != null)
            {
                btnDelete.onClick.AddListener(() => DeleteProject(summary));
            }
        }

        private void OpenProject(string projectId)
        {
            if (ProjectPersistenceService.Instance == null) return;

            ProjectData loaded = ProjectPersistenceService.Instance.LoadProject(projectId);
            if (loaded != null)
            {
                NavigationManager.Instance.SetActiveProject(loaded);
                gameObject.SetActive(false);
                NavigationManager.Instance.GoToFloorPlanner();
            }
            else
            {
                DialogUI.ShowError("Load Failed", "Could not load the specified project file.");
            }
        }

        private void RenameProject(ProjectSummary summary)
        {
            DialogUI.ShowInputDialog("Rename Project", summary.projectName, (newName) =>
            {
                if (string.IsNullOrWhiteSpace(newName)) return;
                ProjectData data = ProjectPersistenceService.Instance.LoadProject(summary.projectId);
                if (data != null)
                {
                    data.projectName = newName;
                    data.modifiedAtIso = DateTime.UtcNow.ToString("o");
                    ProjectPersistenceService.Instance.SaveProject(data);
                    RefreshList();
                }
            });
        }

        private void DeleteProject(ProjectSummary summary)
        {
            DialogUI.ShowConfirmation("Delete Project", 
                $"Are you sure you want to permanently delete \"{summary.projectName}\"?", () =>
            {
                ProjectPersistenceService.Instance.DeleteProject(summary.projectId);
                RefreshList();
            });
        }

        private void OnLoadSampleClicked()
        {
            ProjectData sample = SampleProjectGenerator.CreateSampleProject();
            ProjectPersistenceService.Instance.SaveProject(sample);
            NavigationManager.Instance.SetActiveProject(sample);
            RefreshList();
            DialogUI.ShowMessage("Sample Loaded", "Sample Home Design loaded and saved to your designs.");
        }
    }
}
