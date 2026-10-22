using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Persistence
{
    /// <summary>
    /// Static utility that generates a fully-populated sample <see cref="ProjectData"/>
    /// for testing and first-launch demos.
    /// Represents an L-shaped home with four rooms, walls, door/window openings, and furniture.
    /// </summary>
    public static class SampleProjectGenerator
    {
        public static ProjectData CreateSampleProject()
        {
            // First check if Resources/SampleProject exists
            TextAsset sampleAsset = UnityEngine.Resources.Load<TextAsset>("SampleProject");
            if (sampleAsset != null && !string.IsNullOrEmpty(sampleAsset.text))
            {
                try
                {
                    ProjectData fromJson = JsonUtility.FromJson<ProjectData>(sampleAsset.text);
                    if (fromJson != null && fromJson.rooms != null && fromJson.rooms.Count > 0)
                    {
                        return fromJson;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SampleProjectGenerator] Failed to parse SampleProject.json: {ex.Message}");
                }
            }

            // Fallback: Build programmatically
            ProjectData project = new ProjectData
            {
                projectId = Guid.NewGuid().ToString(),
                projectName = "Sample Home Design",
                schemaVersion = SchemaVersionUpgrader.CurrentVersion,
                createdAtIso = DateTime.UtcNow.ToString("o"),
                modifiedAtIso = DateTime.UtcNow.ToString("o"),
                rooms = new List<RoomData>(),
                furnitureItems = new List<FurnitureItemData>()
            };

            // Living Room (5x4m)
            RoomData living = new RoomData
            {
                roomId = "room_living",
                roomName = "Living Room",
                roomType = RoomType.LivingRoom,
                width = 5f,
                length = 4f,
                height = 2.8f,
                position = new Vector2Serializable(0f, 0f),
                wallColor = "#F0EFEA",
                floorMaterial = "wood",
                walls = new List<WallData>()
            };
            living.walls.Add(CreateWall("lr_w1", 0f, 0f, 5f, 0f, OpeningType.Door, 0.5f, 0.9f, 2.1f, 0f));
            living.walls.Add(CreateWall("lr_w2", 5f, 0f, 5f, 4f, OpeningType.Window, 0.5f, 1.4f, 1.2f, 0.9f));
            living.walls.Add(CreateWall("lr_w3", 5f, 4f, 0f, 4f));
            living.walls.Add(CreateWall("lr_w4", 0f, 4f, 0f, 0f));
            project.rooms.Add(living);

            // Kitchen (3x3m)
            RoomData kitchen = new RoomData
            {
                roomId = "room_kitchen",
                roomName = "Kitchen",
                roomType = RoomType.Kitchen,
                width = 3f,
                length = 3f,
                height = 2.8f,
                position = new Vector2Serializable(5f, 0f),
                wallColor = "#E8ECEF",
                floorMaterial = "tile",
                walls = new List<WallData>()
            };
            kitchen.walls.Add(CreateWall("ki_w1", 0f, 0f, 3f, 0f));
            kitchen.walls.Add(CreateWall("ki_w2", 3f, 0f, 3f, 3f));
            kitchen.walls.Add(CreateWall("ki_w3", 3f, 3f, 0f, 3f));
            kitchen.walls.Add(CreateWall("ki_w4", 0f, 3f, 0f, 0f));
            project.rooms.Add(kitchen);

            // Bedroom (4x3.5m)
            RoomData bedroom = new RoomData
            {
                roomId = "room_bedroom",
                roomName = "Bedroom",
                roomType = RoomType.Bedroom,
                width = 4f,
                length = 3.5f,
                height = 2.8f,
                position = new Vector2Serializable(0f, 4f),
                wallColor = "#E2E8F0",
                floorMaterial = "wood",
                walls = new List<WallData>()
            };
            bedroom.walls.Add(CreateWall("br_w1", 0f, 0f, 4f, 0f));
            bedroom.walls.Add(CreateWall("br_w2", 4f, 0f, 4f, 3.5f));
            bedroom.walls.Add(CreateWall("br_w3", 4f, 3.5f, 0f, 3.5f, OpeningType.Window, 0.5f, 1.4f, 1.2f, 0.9f));
            bedroom.walls.Add(CreateWall("br_w4", 0f, 3.5f, 0f, 0f));
            project.rooms.Add(bedroom);

            // Bathroom (2.5x2m)
            RoomData bathroom = new RoomData
            {
                roomId = "room_bathroom",
                roomName = "Bathroom",
                roomType = RoomType.Bathroom,
                width = 2.5f,
                length = 2f,
                height = 2.8f,
                position = new Vector2Serializable(4f, 4f),
                wallColor = "#CBD5E1",
                floorMaterial = "tile",
                walls = new List<WallData>()
            };
            bathroom.walls.Add(CreateWall("ba_w1", 0f, 0f, 2.5f, 0f));
            bathroom.walls.Add(CreateWall("ba_w2", 2.5f, 0f, 2.5f, 2f));
            bathroom.walls.Add(CreateWall("ba_w3", 2.5f, 2f, 0f, 2f));
            bathroom.walls.Add(CreateWall("ba_w4", 0f, 2f, 0f, 0f));
            project.rooms.Add(bathroom);

            // Add furniture items
            project.furnitureItems.Add(CreateFurniture("Sofa", "Living Room Sofa", new Vector3(2.5f, 0f, 1.5f), Vector3.zero, "#2B6CB0"));
            project.furnitureItems.Add(CreateFurniture("CoffeeTable", "Coffee Table", new Vector3(2.5f, 0f, 2.4f), Vector3.zero, "#8B5A2B"));
            project.furnitureItems.Add(CreateFurniture("TV", "Media TV", new Vector3(2.5f, 0f, 3.8f), new Vector3(0f, 180f, 0f), "#1A202C"));
            project.furnitureItems.Add(CreateFurniture("Bed_Double", "Double Bed", new Vector3(2f, 0f, 5.5f), Vector3.zero, "#E2E8F0"));
            project.furnitureItems.Add(CreateFurniture("KitchenCounter", "Kitchen Counter", new Vector3(6.5f, 0f, 0.6f), Vector3.zero, "#F7FAFC"));
            project.furnitureItems.Add(CreateFurniture("Refrigerator", "Fridge", new Vector3(7.5f, 0f, 2.2f), new Vector3(0f, 90f, 0f), "#CBD5E0"));
            project.furnitureItems.Add(CreateFurniture("Toilet", "Toilet", new Vector3(5.8f, 0f, 5.5f), new Vector3(0f, 180f, 0f), "#FFFFFF"));
            project.furnitureItems.Add(CreateFurniture("Bathtub", "Bathtub", new Vector3(4.5f, 0f, 4.5f), new Vector3(0f, 90f, 0f), "#FFFFFF"));

            return project;
        }

        private static WallData CreateWall(string id, float startX, float startY, float endX, float endY,
            OpeningType? opType = null, float opPos = 0.5f, float opW = 0.9f, float opH = 2.1f, float opElev = 0f)
        {
            WallData wall = new WallData
            {
                wallId = id,
                startPoint = new Vector2Serializable(startX, startY),
                endPoint = new Vector2Serializable(endX, endY),
                height = 2.8f,
                thickness = 0.15f,
                materialColor = "#F0EFEA",
                openings = new List<WallOpeningData>()
            };

            if (opType.HasValue)
            {
                wall.openings.Add(new WallOpeningData
                {
                    id = $"{id}_op",
                    openingType = opType.Value,
                    position = opPos,
                    width = opW,
                    height = opH,
                    elevation = opElev,
                    parentWallId = id
                });
            }

            return wall;
        }

        private static FurnitureItemData CreateFurniture(string catId, string name, Vector3 pos, Vector3 rot, string color)
        {
            return new FurnitureItemData
            {
                instanceId = Guid.NewGuid().ToString(),
                catalogueId = catId,
                customName = name,
                position = pos,
                rotation = rot,
                scale = Vector3.one,
                materialColor = color
            };
        }
    }
}
