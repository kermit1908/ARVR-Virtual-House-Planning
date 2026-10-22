using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;
using HousePlanner.HouseGeneration;

namespace HousePlanner.Furniture
{
    /// <summary>
    /// Generates recognizable 3D placeholder models procedurally for any furniture catalogue item.
    /// Combines primitives (boxes, cylinders, cushions) into identifiable furniture shapes
    /// with appropriate colors and colliders for selection and placement.
    /// </summary>
    public static class ProceduralFurnitureGenerator
    {
        /// <summary>
        /// Generates a complete furniture GameObject for a given catalogue item.
        /// </summary>
        public static GameObject GenerateFurniture(string catalogueId, Vector3 customScale, Color tintColor)
        {
            GameObject root = new GameObject($"Furniture_{catalogueId}");
            
            switch (catalogueId)
            {
                case "Sofa":
                    BuildSofa(root, tintColor);
                    break;
                case "Armchair":
                    BuildArmchair(root, tintColor);
                    break;
                case "Bed_Single":
                    BuildBed(root, 1.0f, 2.0f, 0.5f, tintColor);
                    break;
                case "Bed_Double":
                    BuildBed(root, 1.6f, 2.0f, 0.5f, tintColor);
                    break;
                case "DiningTable":
                    BuildTable(root, 1.5f, 0.9f, 0.75f, tintColor);
                    break;
                case "CoffeeTable":
                    BuildTable(root, 1.0f, 0.6f, 0.45f, tintColor);
                    break;
                case "Desk":
                    BuildDesk(root, tintColor);
                    break;
                case "DiningChair":
                    BuildChair(root, tintColor);
                    break;
                case "Wardrobe":
                    BuildWardrobe(root, tintColor);
                    break;
                case "Bookshelf":
                    BuildBookshelf(root, tintColor);
                    break;
                case "KitchenCounter":
                    BuildKitchenCounter(root, tintColor);
                    break;
                case "Refrigerator":
                    BuildRefrigerator(root, tintColor);
                    break;
                case "Sink":
                    BuildSink(root, tintColor);
                    break;
                case "Toilet":
                    BuildToilet(root, tintColor);
                    break;
                case "Bathtub":
                    BuildBathtub(root, tintColor);
                    break;
                case "FloorLamp":
                    BuildFloorLamp(root, tintColor);
                    break;
                case "CeilingLight":
                    BuildCeilingLight(root, tintColor);
                    break;
                case "TV":
                    BuildTV(root, tintColor);
                    break;
                case "Nightstand":
                    BuildNightstand(root, tintColor);
                    break;
                case "Rug":
                    BuildRug(root, tintColor);
                    break;
                default:
                    BuildDefaultBox(root, customScale, tintColor);
                    break;
            }

            // Ensure root has an overall BoxCollider for interaction
            Bounds combinedBounds = CalculateBounds(root);
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = root.transform.InverseTransformPoint(combinedBounds.center);
            col.size = combinedBounds.size;

            root.transform.localScale = customScale;
            return root;
        }

        private static GameObject CreatePart(GameObject parent, string name, Vector3 localPos, Vector3 size, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = size;

            // Remove primitive collider as parent gets the bounds collider
            Object.DestroyImmediate(part.GetComponent<Collider>());

            Renderer r = part.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = MaterialManager.Instance.CreateFurnitureMaterial(name, color);
            }
            return part;
        }

        private static void BuildSofa(GameObject root, Color color)
        {
            Color seatColor = color != Color.white ? color : new Color(0.25f, 0.45f, 0.75f);
            Color darkAccent = seatColor * 0.8f;
            darkAccent.a = 1f;

            // Base / Seat Cushion
            CreatePart(root, "Base", new Vector3(0f, 0.22f, 0f), new Vector3(1.9f, 0.3f, 0.85f), seatColor);
            // Backrest
            CreatePart(root, "Backrest", new Vector3(0f, 0.55f, 0.35f), new Vector3(1.9f, 0.5f, 0.2f), darkAccent);
            // Left Armrest
            CreatePart(root, "ArmLeft", new Vector3(-0.95f, 0.42f, 0f), new Vector3(0.2f, 0.45f, 0.9f), darkAccent);
            // Right Armrest
            CreatePart(root, "ArmRight", new Vector3(0.95f, 0.42f, 0f), new Vector3(0.2f, 0.45f, 0.9f), darkAccent);
            // Feet
            Color legColor = new Color(0.2f, 0.15f, 0.1f);
            CreatePart(root, "LegFL", new Vector3(-0.9f, 0.04f, -0.35f), new Vector3(0.08f, 0.08f, 0.08f), legColor);
            CreatePart(root, "LegFR", new Vector3(0.9f, 0.04f, -0.35f), new Vector3(0.08f, 0.08f, 0.08f), legColor);
            CreatePart(root, "LegBL", new Vector3(-0.9f, 0.04f, 0.35f), new Vector3(0.08f, 0.08f, 0.08f), legColor);
            CreatePart(root, "LegBR", new Vector3(0.9f, 0.04f, 0.35f), new Vector3(0.08f, 0.08f, 0.08f), legColor);
        }

        private static void BuildArmchair(GameObject root, Color color)
        {
            Color chairColor = color != Color.white ? color : new Color(0.35f, 0.55f, 0.45f);
            Color darkAccent = chairColor * 0.8f;

            CreatePart(root, "Seat", new Vector3(0f, 0.22f, 0f), new Vector3(0.7f, 0.28f, 0.75f), chairColor);
            CreatePart(root, "Back", new Vector3(0f, 0.52f, 0.3f), new Vector3(0.7f, 0.5f, 0.18f), darkAccent);
            CreatePart(root, "ArmL", new Vector3(-0.4f, 0.38f, 0f), new Vector3(0.15f, 0.38f, 0.8f), darkAccent);
            CreatePart(root, "ArmR", new Vector3(0.4f, 0.38f, 0f), new Vector3(0.15f, 0.38f, 0.8f), darkAccent);
        }

        private static void BuildBed(GameObject root, float w, float l, float h, Color color)
        {
            Color sheetColor = color != Color.white ? color : new Color(0.92f, 0.90f, 0.85f);
            Color woodColor = new Color(0.45f, 0.28f, 0.15f);

            // Frame
            CreatePart(root, "Frame", new Vector3(0f, h * 0.3f, 0f), new Vector3(w, h * 0.35f, l), woodColor);
            // Mattress
            CreatePart(root, "Mattress", new Vector3(0f, h * 0.65f, 0f), new Vector3(w * 0.95f, h * 0.35f, l * 0.95f), sheetColor);
            // Headboard
            CreatePart(root, "Headboard", new Vector3(0f, h * 0.95f, l * 0.48f), new Vector3(w, h * 0.9f, 0.1f), woodColor);
            // Pillow
            Color pillowColor = Color.white;
            CreatePart(root, "Pillow", new Vector3(0f, h * 0.85f, l * 0.32f), new Vector3(w * 0.7f, 0.12f, 0.35f), pillowColor);
        }

        private static void BuildTable(GameObject root, float w, float l, float h, Color color)
        {
            Color woodColor = color != Color.white ? color : new Color(0.55f, 0.35f, 0.2f);
            Color legColor = woodColor * 0.85f;

            // Tabletop
            CreatePart(root, "Top", new Vector3(0f, h - 0.025f, 0f), new Vector3(w, 0.05f, l), woodColor);
            // 4 Legs
            float halfW = (w * 0.5f) - 0.08f;
            float halfL = (l * 0.5f) - 0.08f;
            float legH = h - 0.05f;
            float legY = legH * 0.5f;

            CreatePart(root, "Leg1", new Vector3(-halfW, legY, -halfL), new Vector3(0.06f, legH, 0.06f), legColor);
            CreatePart(root, "Leg2", new Vector3(halfW, legY, -halfL), new Vector3(0.06f, legH, 0.06f), legColor);
            CreatePart(root, "Leg3", new Vector3(-halfW, legY, halfL), new Vector3(0.06f, legH, 0.06f), legColor);
            CreatePart(root, "Leg4", new Vector3(halfW, legY, halfL), new Vector3(0.06f, legH, 0.06f), legColor);
        }

        private static void BuildDesk(GameObject root, Color color)
        {
            BuildTable(root, 1.2f, 0.6f, 0.75f, color);
            Color woodColor = color != Color.white ? color : new Color(0.55f, 0.35f, 0.2f);
            // Side drawer cabinet
            CreatePart(root, "DrawerUnit", new Vector3(0.42f, 0.36f, 0f), new Vector3(0.32f, 0.65f, 0.55f), woodColor * 0.9f);
        }

        private static void BuildChair(GameObject root, Color color)
        {
            Color chairColor = color != Color.white ? color : new Color(0.6f, 0.4f, 0.25f);
            // Seat
            CreatePart(root, "Seat", new Vector3(0f, 0.44f, 0f), new Vector3(0.44f, 0.04f, 0.44f), chairColor);
            // Backrest
            CreatePart(root, "Back", new Vector3(0f, 0.72f, 0.2f), new Vector3(0.42f, 0.45f, 0.04f), chairColor);
            // Legs
            CreatePart(root, "LegFL", new Vector3(-0.18f, 0.21f, -0.18f), new Vector3(0.04f, 0.42f, 0.04f), chairColor * 0.85f);
            CreatePart(root, "LegFR", new Vector3(0.18f, 0.21f, -0.18f), new Vector3(0.04f, 0.42f, 0.04f), chairColor * 0.85f);
            CreatePart(root, "LegBL", new Vector3(-0.18f, 0.21f, 0.18f), new Vector3(0.04f, 0.42f, 0.04f), chairColor * 0.85f);
            CreatePart(root, "LegBR", new Vector3(0.18f, 0.21f, 0.18f), new Vector3(0.04f, 0.42f, 0.04f), chairColor * 0.85f);
        }

        private static void BuildWardrobe(GameObject root, Color color)
        {
            Color mainColor = color != Color.white ? color : new Color(0.5f, 0.35f, 0.25f);
            CreatePart(root, "Body", new Vector3(0f, 1.0f, 0f), new Vector3(1.2f, 2.0f, 0.6f), mainColor);
            // Door seam accent
            CreatePart(root, "HandleL", new Vector3(-0.06f, 1.05f, -0.31f), new Vector3(0.03f, 0.16f, 0.02f), Color.gray);
            CreatePart(root, "HandleR", new Vector3(0.06f, 1.05f, -0.31f), new Vector3(0.03f, 0.16f, 0.02f), Color.gray);
        }

        private static void BuildBookshelf(GameObject root, Color color)
        {
            Color woodColor = color != Color.white ? color : new Color(0.48f, 0.32f, 0.2f);
            CreatePart(root, "Back", new Vector3(0f, 0.9f, 0.14f), new Vector3(0.8f, 1.8f, 0.03f), woodColor);
            CreatePart(root, "Left", new Vector3(-0.38f, 0.9f, 0f), new Vector3(0.03f, 1.8f, 0.3f), woodColor);
            CreatePart(root, "Right", new Vector3(0.38f, 0.9f, 0f), new Vector3(0.03f, 1.8f, 0.3f), woodColor);
            // 4 Shelves
            for (int i = 0; i <= 4; i++)
            {
                float y = 0.05f + (i * 0.42f);
                CreatePart(root, $"Shelf_{i}", new Vector3(0f, y, 0f), new Vector3(0.76f, 0.03f, 0.28f), woodColor * 0.9f);
            }
        }

        private static void BuildKitchenCounter(GameObject root, Color color)
        {
            Color cabinetColor = color != Color.white ? color : new Color(0.9f, 0.9f, 0.9f);
            Color topColor = new Color(0.2f, 0.2f, 0.2f); // Black granite top
            // Base
            CreatePart(root, "Base", new Vector3(0f, 0.42f, 0f), new Vector3(2.0f, 0.85f, 0.6f), cabinetColor);
            // Countertop
            CreatePart(root, "Top", new Vector3(0f, 0.88f, 0f), new Vector3(2.04f, 0.05f, 0.64f), topColor);
        }

        private static void BuildRefrigerator(GameObject root, Color color)
        {
            Color steel = color != Color.white ? color : new Color(0.82f, 0.84f, 0.85f);
            CreatePart(root, "Main", new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 1.8f, 0.7f), steel);
            // Door gap
            CreatePart(root, "DoorGap", new Vector3(0f, 1.15f, -0.355f), new Vector3(0.68f, 0.015f, 0.01f), Color.black);
            // Handle
            CreatePart(root, "Handle", new Vector3(0.3f, 1.0f, -0.37f), new Vector3(0.03f, 0.35f, 0.02f), Color.black);
        }

        private static void BuildSink(GameObject root, Color color)
        {
            Color cabinetColor = color != Color.white ? color : new Color(0.9f, 0.9f, 0.9f);
            CreatePart(root, "Cabinet", new Vector3(0f, 0.38f, 0f), new Vector3(0.6f, 0.76f, 0.5f), cabinetColor);
            CreatePart(root, "Basin", new Vector3(0f, 0.8f, 0f), new Vector3(0.64f, 0.1f, 0.54f), Color.white);
            CreatePart(root, "Faucet", new Vector3(0f, 0.95f, 0.18f), new Vector3(0.04f, 0.16f, 0.08f), Color.gray);
        }

        private static void BuildToilet(GameObject root, Color color)
        {
            Color white = Color.white;
            CreatePart(root, "Bowl", new Vector3(0f, 0.2f, -0.1f), new Vector3(0.38f, 0.4f, 0.5f), white);
            CreatePart(root, "Tank", new Vector3(0f, 0.5f, 0.2f), new Vector3(0.38f, 0.42f, 0.2f), white);
        }

        private static void BuildBathtub(GameObject root, Color color)
        {
            Color white = Color.white;
            CreatePart(root, "TubBase", new Vector3(0f, 0.28f, 0f), new Vector3(1.7f, 0.55f, 0.75f), white);
            // Inner hollowed top effect
            CreatePart(root, "Water", new Vector3(0f, 0.45f, 0f), new Vector3(1.5f, 0.1f, 0.58f), new Color(0.4f, 0.7f, 0.85f, 0.8f));
        }

        private static void BuildFloorLamp(GameObject root, Color color)
        {
            Color metal = Color.gray;
            Color shade = color != Color.white ? color : new Color(0.95f, 0.95f, 0.85f);
            CreatePart(root, "Base", new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.04f, 0.3f), metal);
            CreatePart(root, "Pole", new Vector3(0f, 0.75f, 0f), new Vector3(0.04f, 1.45f, 0.04f), metal);
            CreatePart(root, "Shade", new Vector3(0f, 1.42f, 0f), new Vector3(0.32f, 0.25f, 0.32f), shade);
        }

        private static void BuildCeilingLight(GameObject root, Color color)
        {
            Color fixtureColor = color != Color.white ? color : new Color(0.95f, 0.95f, 0.9f);
            CreatePart(root, "Mount", new Vector3(0f, 0.12f, 0f), new Vector3(0.4f, 0.08f, 0.4f), fixtureColor);
        }

        private static void BuildTV(GameObject root, Color color)
        {
            Color black = new Color(0.1f, 0.1f, 0.1f);
            CreatePart(root, "Screen", new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.7f, 0.05f), black);
            CreatePart(root, "Stand", new Vector3(0f, 0.05f, 0f), new Vector3(0.4f, 0.1f, 0.25f), Color.gray);
        }

        private static void BuildNightstand(GameObject root, Color color)
        {
            Color wood = color != Color.white ? color : new Color(0.52f, 0.35f, 0.22f);
            CreatePart(root, "Cabinet", new Vector3(0f, 0.26f, 0f), new Vector3(0.45f, 0.52f, 0.4f), wood);
        }

        private static void BuildRug(GameObject root, Color color)
        {
            Color rugColor = color != Color.white ? color : new Color(0.65f, 0.25f, 0.25f);
            CreatePart(root, "RugMat", new Vector3(0f, 0.01f, 0f), new Vector3(2.0f, 0.02f, 1.5f), rugColor);
        }

        private static void BuildDefaultBox(GameObject root, Vector3 scale, Color color)
        {
            CreatePart(root, "Box", new Vector3(0f, scale.y * 0.5f, 0f), scale, color);
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
    }
}
