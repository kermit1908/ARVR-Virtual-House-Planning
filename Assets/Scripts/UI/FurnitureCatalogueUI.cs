using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HousePlanner.Core;
using HousePlanner.Furniture;

namespace HousePlanner.UI
{
    /// <summary>
    /// UI Controller for the furniture catalogue panel.
    /// Provides category filtering tabs, search bar, item cards with dimensions,
    /// and starts placement mode via <see cref="FurniturePlacementController"/>.
    /// </summary>
    public class FurnitureCatalogueUI : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform _itemsGridContainer;
        [SerializeField] private GameObject _itemCardTemplate;
        [SerializeField] private InputField _searchInputField;

        [Header("Category Buttons")]
        [SerializeField] private Button _btnCatAll;
        [SerializeField] private Button _btnCatSeating;
        [SerializeField] private Button _btnCatBeds;
        [SerializeField] private Button _btnCatTables;
        [SerializeField] private Button _btnCatStorage;
        [SerializeField] private Button _btnCatKitchen;
        [SerializeField] private Button _btnCatBathroom;
        [SerializeField] private Button _btnCatLighting;

        [Header("Header Actions")]
        [SerializeField] private Button _btnClose;

        private List<FurnitureCatalogueEntry> _allEntries = new List<FurnitureCatalogueEntry>();
        private FurnitureCategory? _selectedCategory = null;
        private string _searchFilter = "";

        private void Start()
        {
            _allEntries = DefaultFurnitureCatalogue.CreateDefaultEntries();

            SetupCategoryButtons();
            if (_searchInputField != null) _searchInputField.onValueChanged.AddListener(OnSearchChanged);
            if (_btnClose != null) _btnClose.onClick.AddListener(() => gameObject.SetActive(false));

            if (_itemCardTemplate != null) _itemCardTemplate.SetActive(false);

            PopulateItems();
        }

        private void SetupCategoryButtons()
        {
            if (_btnCatAll != null) _btnCatAll.onClick.AddListener(() => SelectCategory(null));
            if (_btnCatSeating != null) _btnCatSeating.onClick.AddListener(() => SelectCategory(FurnitureCategory.Seating));
            if (_btnCatBeds != null) _btnCatBeds.onClick.AddListener(() => SelectCategory(FurnitureCategory.Bed));
            if (_btnCatTables != null) _btnCatTables.onClick.AddListener(() => SelectCategory(FurnitureCategory.Table));
            if (_btnCatStorage != null) _btnCatStorage.onClick.AddListener(() => SelectCategory(FurnitureCategory.Storage));
            if (_btnCatKitchen != null) _btnCatKitchen.onClick.AddListener(() => SelectCategory(FurnitureCategory.Kitchen));
            if (_btnCatBathroom != null) _btnCatBathroom.onClick.AddListener(() => SelectCategory(FurnitureCategory.Bathroom));
            if (_btnCatLighting != null) _btnCatLighting.onClick.AddListener(() => SelectCategory(FurnitureCategory.Lighting));
        }

        private void SelectCategory(FurnitureCategory? cat)
        {
            _selectedCategory = cat;
            PopulateItems();
        }

        private void OnSearchChanged(string query)
        {
            _searchFilter = query.Trim().ToLowerInvariant();
            PopulateItems();
        }

        private void PopulateItems()
        {
            if (_itemsGridContainer == null) return;

            // Clear old cards
            for (int i = _itemsGridContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = _itemsGridContainer.GetChild(i);
                if (child.gameObject != _itemCardTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var entry in _allEntries)
            {
                // Category filter
                if (_selectedCategory.HasValue && entry.category != _selectedCategory.Value)
                {
                    continue;
                }

                // Search query filter
                if (!string.IsNullOrEmpty(_searchFilter))
                {
                    if (!entry.displayName.ToLowerInvariant().Contains(_searchFilter) &&
                        !entry.description.ToLowerInvariant().Contains(_searchFilter))
                    {
                        continue;
                    }
                }

                CreateCard(entry);
            }
        }

        private void CreateCard(FurnitureCatalogueEntry entry)
        {
            GameObject card = _itemCardTemplate != null 
                ? Instantiate(_itemCardTemplate, _itemsGridContainer) 
                : new GameObject($"Card_{entry.catalogueId}");

            card.SetActive(true);

            Text title = card.transform.Find("Title")?.GetComponent<Text>();
            if (title == null) title = card.GetComponentInChildren<Text>();
            if (title != null)
            {
                title.text = entry.displayName;
            }

            Text dims = card.transform.Find("Dimensions")?.GetComponent<Text>();
            if (dims != null)
            {
                dims.text = $"{entry.defaultScale.x:F1}m × {entry.defaultScale.z:F1}m × {entry.defaultScale.y:F1}m";
            }

            Button btnSelect = card.GetComponent<Button>();
            if (btnSelect != null)
            {
                btnSelect.onClick.AddListener(() =>
                {
                    if (FurniturePlacementController.Instance != null)
                    {
                        FurniturePlacementController.Instance.BeginPlacement(entry.catalogueId);
                        gameObject.SetActive(false); // Close panel to allow placement
                    }
                });
            }
        }
    }
}
