using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Furniture
{
    /// <summary>
    /// A single entry in the furniture catalogue describing one type of furniture.
    /// </summary>
    [Serializable]
    public class FurnitureCatalogueEntry
    {
        /// <summary>Unique catalogue identifier (e.g. "Sofa", "Bed_Single").</summary>
        public string catalogueId;

        /// <summary>Human-readable display name (e.g. "Double Bed").</summary>
        public string displayName;

        /// <summary>Functional category for filtering.</summary>
        public FurnitureCategory category;

        /// <summary>Default scale in meters (width x height x depth).</summary>
        public Vector3 defaultScale;

        /// <summary>Short description of the furniture item.</summary>
        public string description;

        /// <summary>Name of the icon resource for UI display.</summary>
        public string iconName;
    }

    /// <summary>
    /// ScriptableObject-based catalogue that holds all available furniture entries.
    /// Create via Assets > Create > HousePlanner > Furniture Catalogue in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "FurnitureCatalogue", menuName = "HousePlanner/Furniture Catalogue")]
    public class FurnitureCatalogue : ScriptableObject
    {
        /// <summary>All furniture entries in this catalogue.</summary>
        [SerializeField]
        private List<FurnitureCatalogueEntry> entries = new List<FurnitureCatalogueEntry>();

        /// <summary>
        /// Provides read-only access to all catalogue entries.
        /// </summary>
        public IReadOnlyList<FurnitureCatalogueEntry> Entries => entries;

        /// <summary>
        /// Returns all entries that match the specified <see cref="FurnitureCategory"/>.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>A list of matching entries. Empty if none found.</returns>
        public List<FurnitureCatalogueEntry> GetByCategory(FurnitureCategory category)
        {
            var results = new List<FurnitureCatalogueEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].category == category)
                {
                    results.Add(entries[i]);
                }
            }
            return results;
        }

        /// <summary>
        /// Returns the catalogue entry with the specified ID, or null if not found.
        /// </summary>
        /// <param name="catalogueId">The unique catalogue identifier to look up.</param>
        /// <returns>The matching <see cref="FurnitureCatalogueEntry"/>, or null.</returns>
        public FurnitureCatalogueEntry GetById(string catalogueId)
        {
            if (string.IsNullOrEmpty(catalogueId))
            {
                Debug.LogWarning("[FurnitureCatalogue] GetById called with null or empty catalogueId.");
                return null;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].catalogueId == catalogueId)
                {
                    return entries[i];
                }
            }

            Debug.LogWarning($"[FurnitureCatalogue] No entry found for catalogueId '{catalogueId}'.");
            return null;
        }

        /// <summary>
        /// Populates the catalogue with the given list of entries, replacing any existing data.
        /// Useful for runtime initialization from <see cref="DefaultFurnitureCatalogue"/>.
        /// </summary>
        /// <param name="newEntries">The entries to populate the catalogue with.</param>
        public void PopulateFromList(List<FurnitureCatalogueEntry> newEntries)
        {
            if (newEntries == null)
            {
                Debug.LogError("[FurnitureCatalogue] Cannot populate from a null list.");
                return;
            }

            entries = new List<FurnitureCatalogueEntry>(newEntries);
            Debug.Log($"[FurnitureCatalogue] Populated with {entries.Count} entries.");
        }

        /// <summary>
        /// Returns all unique categories present in the catalogue.
        /// </summary>
        /// <returns>A list of distinct <see cref="FurnitureCategory"/> values.</returns>
        public List<FurnitureCategory> GetAvailableCategories()
        {
            var categories = new HashSet<FurnitureCategory>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null)
                {
                    categories.Add(entries[i].category);
                }
            }
            return categories.ToList();
        }

        /// <summary>
        /// Returns the total number of entries in the catalogue.
        /// </summary>
        public int Count => entries.Count;
    }
}
