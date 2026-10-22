using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Furniture
{
    /// <summary>
    /// Provides a hard-coded default set of furniture catalogue entries.
    /// Use this at runtime when no ScriptableObject catalogue asset exists.
    /// </summary>
    public static class DefaultFurnitureCatalogue
    {
        /// <summary>
        /// Creates and returns the full list of default furniture entries.
        /// </summary>
        /// <returns>A list of 20 pre-configured <see cref="FurnitureCatalogueEntry"/> items.</returns>
        public static List<FurnitureCatalogueEntry> CreateDefaultEntries()
        {
            return new List<FurnitureCatalogueEntry>
            {
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Sofa",
                    displayName = "Sofa",
                    category = FurnitureCategory.Seating,
                    defaultScale = new Vector3(2.0f, 0.9f, 0.8f),
                    description = "Three-seat sofa with armrests.",
                    iconName = "icon_sofa"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Armchair",
                    displayName = "Armchair",
                    category = FurnitureCategory.Seating,
                    defaultScale = new Vector3(0.9f, 0.9f, 0.8f),
                    description = "Single armchair.",
                    iconName = "icon_armchair"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Bed_Single",
                    displayName = "Single Bed",
                    category = FurnitureCategory.Bed,
                    defaultScale = new Vector3(1.0f, 2.0f, 0.5f),
                    description = "Standard single bed with headboard.",
                    iconName = "icon_bed_single"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Bed_Double",
                    displayName = "Double Bed",
                    category = FurnitureCategory.Bed,
                    defaultScale = new Vector3(1.6f, 2.0f, 0.5f),
                    description = "Standard double bed with headboard.",
                    iconName = "icon_bed_double"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "DiningTable",
                    displayName = "Dining Table",
                    category = FurnitureCategory.Table,
                    defaultScale = new Vector3(1.5f, 0.9f, 0.75f),
                    description = "Rectangular dining table for 4–6 people.",
                    iconName = "icon_dining_table"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "DiningChair",
                    displayName = "Dining Chair",
                    category = FurnitureCategory.Seating,
                    defaultScale = new Vector3(0.45f, 0.45f, 0.9f),
                    description = "Standard dining chair.",
                    iconName = "icon_dining_chair"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Desk",
                    displayName = "Desk",
                    category = FurnitureCategory.Table,
                    defaultScale = new Vector3(1.2f, 0.6f, 0.75f),
                    description = "Office or study desk.",
                    iconName = "icon_desk"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Wardrobe",
                    displayName = "Wardrobe",
                    category = FurnitureCategory.Storage,
                    defaultScale = new Vector3(1.2f, 0.6f, 2.0f),
                    description = "Tall freestanding wardrobe.",
                    iconName = "icon_wardrobe"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Bookshelf",
                    displayName = "Bookshelf",
                    category = FurnitureCategory.Storage,
                    defaultScale = new Vector3(0.8f, 0.3f, 1.8f),
                    description = "Open bookshelf with multiple shelves.",
                    iconName = "icon_bookshelf"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "KitchenCounter",
                    displayName = "Kitchen Counter",
                    category = FurnitureCategory.Kitchen,
                    defaultScale = new Vector3(2.0f, 0.6f, 0.9f),
                    description = "Kitchen countertop unit.",
                    iconName = "icon_kitchen_counter"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Refrigerator",
                    displayName = "Refrigerator",
                    category = FurnitureCategory.Kitchen,
                    defaultScale = new Vector3(0.7f, 0.7f, 1.8f),
                    description = "Full-size refrigerator.",
                    iconName = "icon_refrigerator"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Sink",
                    displayName = "Sink",
                    category = FurnitureCategory.Kitchen,
                    defaultScale = new Vector3(0.6f, 0.5f, 0.85f),
                    description = "Kitchen or utility sink.",
                    iconName = "icon_sink"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Toilet",
                    displayName = "Toilet",
                    category = FurnitureCategory.Bathroom,
                    defaultScale = new Vector3(0.4f, 0.65f, 0.4f),
                    description = "Standard toilet.",
                    iconName = "icon_toilet"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Bathtub",
                    displayName = "Bathtub",
                    category = FurnitureCategory.Bathroom,
                    defaultScale = new Vector3(1.7f, 0.75f, 0.6f),
                    description = "Full-length bathtub.",
                    iconName = "icon_bathtub"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "CeilingLight",
                    displayName = "Ceiling Light",
                    category = FurnitureCategory.Lighting,
                    defaultScale = new Vector3(0.4f, 0.4f, 0.15f),
                    description = "Flush-mount ceiling light fixture.",
                    iconName = "icon_ceiling_light"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "FloorLamp",
                    displayName = "Floor Lamp",
                    category = FurnitureCategory.Lighting,
                    defaultScale = new Vector3(0.3f, 0.3f, 1.5f),
                    description = "Tall standing floor lamp.",
                    iconName = "icon_floor_lamp"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "TV",
                    displayName = "Television",
                    category = FurnitureCategory.Electronics,
                    defaultScale = new Vector3(1.2f, 0.1f, 0.7f),
                    description = "Flat-screen television.",
                    iconName = "icon_tv"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "CoffeeTable",
                    displayName = "Coffee Table",
                    category = FurnitureCategory.Table,
                    defaultScale = new Vector3(1.0f, 0.6f, 0.45f),
                    description = "Low rectangular coffee table.",
                    iconName = "icon_coffee_table"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Nightstand",
                    displayName = "Nightstand",
                    category = FurnitureCategory.Storage,
                    defaultScale = new Vector3(0.45f, 0.4f, 0.55f),
                    description = "Bedside nightstand with drawer.",
                    iconName = "icon_nightstand"
                },
                new FurnitureCatalogueEntry
                {
                    catalogueId = "Rug",
                    displayName = "Rug",
                    category = FurnitureCategory.Decor,
                    defaultScale = new Vector3(2.0f, 1.5f, 0.02f),
                    description = "Decorative floor rug.",
                    iconName = "icon_rug"
                }
            };
        }

        /// <summary>
        /// Creates a runtime <see cref="FurnitureCatalogue"/> ScriptableObject instance
        /// populated with all default entries.
        /// </summary>
        /// <returns>A new <see cref="FurnitureCatalogue"/> with default entries loaded.</returns>
        public static FurnitureCatalogue CreateRuntimeCatalogue()
        {
            var catalogue = ScriptableObject.CreateInstance<FurnitureCatalogue>();
            catalogue.PopulateFromList(CreateDefaultEntries());
            return catalogue;
        }
    }
}
