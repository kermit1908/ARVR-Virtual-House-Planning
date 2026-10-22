using System;
using System.Collections.Generic;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Persistence
{
    /// <summary>
    /// Static utility that generates a fully-populated sample
    /// <see cref="ProjectData"/> for testing and first-launch demos.
    /// The sample represents an L-shaped home with four rooms, walls,
    /// door/window openings, and representative furniture.
    /// </summary>
    public static class SampleProjectGenerator
    {
        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Creates and returns a complete sample project.
        /// </summary>
        /// <returns>A ready-to-save <see cref="ProjectData"/>.</returns>
        public static ProjectData CreateSampleProject()
        {
            ProjectData project = new ProjectData
            {
                projectId = Guid.NewGuid().ToString(),
                projectName = "Sample Home Design",
                schemaVersion = SchemaVersionUpgrader.CurrentVersion,
                createdAtIso = DateTime.UtcNow.ToString("o"),
                modifiedAtIso = DateTime.UtcNow.ToString("o"),
                rooms = new List<RoomData>()
            };

            // -----------------------------------------------------------------
            // Room layout  (L-shaped, origin at bottom-left)
            //
            //   Y
            //   7.5 ┌────────────┐
            //       │  Kitchen   │
            //   4.0 ├────────────┤───────────┐
            //       │            │           │
            //       │ Living Rm  │  Bedroom  │
            //   0   └────────────┴───────────┘
            //       0          5.0         9.0    X
            //
            //   Bathroom is stacked above Kitchen:
            //   9.5 ┌──────┐
            //       │ Bath │
            //   7.5 └──────┘
            //       0     2.5
            // -----------------------------------------------------------------

            RoomData livingRoom = CreateLivingRoom();
            RoomData bedroom = CreateBedroom();
            RoomData kitchen = CreateKitchen();
            RoomData bathroom = CreateBathroom();

            project.rooms.Add(livingRoom);
            project.rooms.Add(bedroom);
            project.rooms.Add(kitchen);
            project.rooms.Add(bathroom);

            return project;
        }

        // -----------------------------------------------------------------------
        // Room builders
        // -----------------------------------------------------------------------

        /// <summary>Living Room – 5 × 4 m at origin (0, 0).</summary>
        private static RoomData CreateLivingRoom()
        {
            RoomData room = new RoomData
            {
                roomId = Guid.NewGuid().ToString(),
                roomName = "Living Room",
                roomType = "Living",
                positionX = 0f,
                positionY = 0f,
                width = 5f,
                height = 4f,
                wallColor = "#F5F0E8",     // warm off-white
                floorMaterial = "Hardwood",
                walls = new List<WallData>(),
                openings = new List<OpeningData>(),
                furniture = new List<FurnitureData>()
            };

            // Walls – defined clockwise from bottom-left.
            room.walls.Add(MakeWall("lr_south", 0f, 0f, 5f, 0f));      // south
            room.walls.Add(MakeWall("lr_east",  5f, 0f, 5f, 4f));      // east  (shared with bedroom)
            room.walls.Add(MakeWall("lr_north", 5f, 4f, 0f, 4f));      // north (shared with kitchen)
            room.walls.Add(MakeWall("lr_west",  0f, 4f, 0f, 0f));      // west

            // Openings.
            room.openings.Add(MakeOpening("lr_door_bedroom",  "Door",   "lr_east",  0.5f, 0.9f));
            room.openings.Add(MakeOpening("lr_door_kitchen",  "Door",   "lr_north", 0.3f, 0.9f));
            room.openings.Add(MakeOpening("lr_window_south",  "Window", "lr_south", 0.4f, 1.2f));
            room.openings.Add(MakeOpening("lr_window_west",   "Window", "lr_west",  0.5f, 1.0f));

            // Furniture.
            room.furniture.Add(MakeFurniture("lr_sofa",  "Sofa",  1.0f, 2.0f, 2.0f, 0.9f, 0f));
            room.furniture.Add(MakeFurniture("lr_table", "Coffee Table", 2.0f, 1.5f, 1.0f, 0.6f, 0f));

            return room;
        }

        /// <summary>Bedroom – 4 × 3.5 m placed to the right of the living room.</summary>
        private static RoomData CreateBedroom()
        {
            RoomData room = new RoomData
            {
                roomId = Guid.NewGuid().ToString(),
                roomName = "Bedroom",
                roomType = "Bedroom",
                positionX = 5f,
                positionY = 0f,
                width = 4f,
                height = 3.5f,
                wallColor = "#D6E5F3",     // soft blue
                floorMaterial = "Carpet",
                walls = new List<WallData>(),
                openings = new List<OpeningData>(),
                furniture = new List<FurnitureData>()
            };

            room.walls.Add(MakeWall("br_south", 5f, 0f,  9f, 0f));
            room.walls.Add(MakeWall("br_east",  9f, 0f,  9f, 3.5f));
            room.walls.Add(MakeWall("br_north", 9f, 3.5f, 5f, 3.5f));
            room.walls.Add(MakeWall("br_west",  5f, 3.5f, 5f, 0f));  // shared with living room

            room.openings.Add(MakeOpening("br_door_living", "Door",   "br_west",  0.5f, 0.9f));
            room.openings.Add(MakeOpening("br_window_east", "Window", "br_east",  0.4f, 1.2f));
            room.openings.Add(MakeOpening("br_window_south","Window", "br_south", 0.5f, 1.0f));

            room.furniture.Add(MakeFurniture("br_bed",      "Bed",      6.0f, 1.0f, 2.0f, 1.8f, 0f));
            room.furniture.Add(MakeFurniture("br_wardrobe", "Wardrobe", 8.0f, 0.3f, 1.0f, 0.6f, 0f));

            return room;
        }

        /// <summary>Kitchen – 3 × 3 m placed above the living room (left column).</summary>
        private static RoomData CreateKitchen()
        {
            RoomData room = new RoomData
            {
                roomId = Guid.NewGuid().ToString(),
                roomName = "Kitchen",
                roomType = "Kitchen",
                positionX = 0f,
                positionY = 4f,
                width = 5f,
                height = 3f,
                wallColor = "#FFF8E1",     // light yellow
                floorMaterial = "Tile",
                walls = new List<WallData>(),
                openings = new List<OpeningData>(),
                furniture = new List<FurnitureData>()
            };

            room.walls.Add(MakeWall("ki_south", 0f, 4f, 5f, 4f));   // shared with living room
            room.walls.Add(MakeWall("ki_east",  5f, 4f, 5f, 7f));
            room.walls.Add(MakeWall("ki_north", 5f, 7f, 0f, 7f));
            room.walls.Add(MakeWall("ki_west",  0f, 7f, 0f, 4f));

            room.openings.Add(MakeOpening("ki_door_living",  "Door",   "ki_south", 0.3f, 0.9f));
            room.openings.Add(MakeOpening("ki_window_north", "Window", "ki_north", 0.5f, 1.0f));
            room.openings.Add(MakeOpening("ki_window_west",  "Window", "ki_west",  0.5f, 0.8f));
            room.openings.Add(MakeOpening("ki_door_bath",    "Door",   "ki_north", 0.1f, 0.8f));

            room.furniture.Add(MakeFurniture("ki_counter", "Kitchen Counter", 0.3f, 4.3f, 4.0f, 0.6f, 0f));
            room.furniture.Add(MakeFurniture("ki_fridge",  "Refrigerator",    4.2f, 4.3f, 0.7f, 0.7f, 0f));

            return room;
        }

        /// <summary>Bathroom – 2.5 × 2 m placed above the kitchen (top-left).</summary>
        private static RoomData CreateBathroom()
        {
            RoomData room = new RoomData
            {
                roomId = Guid.NewGuid().ToString(),
                roomName = "Bathroom",
                roomType = "Bathroom",
                positionX = 0f,
                positionY = 7f,
                width = 2.5f,
                height = 2f,
                wallColor = "#E0F2F1",     // pale teal
                floorMaterial = "Tile",
                walls = new List<WallData>(),
                openings = new List<OpeningData>(),
                furniture = new List<FurnitureData>()
            };

            room.walls.Add(MakeWall("ba_south", 0f,   7f,  2.5f, 7f));  // shared with kitchen
            room.walls.Add(MakeWall("ba_east",  2.5f, 7f,  2.5f, 9f));
            room.walls.Add(MakeWall("ba_north", 2.5f, 9f,  0f,   9f));
            room.walls.Add(MakeWall("ba_west",  0f,   9f,  0f,   7f));

            room.openings.Add(MakeOpening("ba_door_kitchen", "Door",   "ba_south", 0.5f, 0.8f));
            room.openings.Add(MakeOpening("ba_window_north", "Window", "ba_north", 0.4f, 0.6f));

            room.furniture.Add(MakeFurniture("ba_toilet",  "Toilet",  0.3f, 7.3f, 0.5f, 0.4f, 0f));
            room.furniture.Add(MakeFurniture("ba_sink",    "Sink",    1.2f, 7.3f, 0.5f, 0.4f, 0f));
            room.furniture.Add(MakeFurniture("ba_bathtub", "Bathtub", 0.3f, 8.0f, 1.7f, 0.7f, 0f));

            return room;
        }

        // -----------------------------------------------------------------------
        // Factory helpers
        // -----------------------------------------------------------------------

        /// <summary>Creates a <see cref="WallData"/> between two 2-D points.</summary>
        private static WallData MakeWall(string id,
            float startX, float startY, float endX, float endY)
        {
            return new WallData
            {
                wallId = id,
                startX = startX,
                startY = startY,
                endX = endX,
                endY = endY,
                thickness = 0.15f,
                wallHeight = 2.8f
            };
        }

        /// <summary>
        /// Creates an <see cref="OpeningData"/> (door or window) on a wall.
        /// <paramref name="normalizedPosition"/> is 0-1 along the wall length.
        /// </summary>
        private static OpeningData MakeOpening(string id, string type,
            string wallId, float normalizedPosition, float openingWidth)
        {
            bool isDoor = type == "Door";
            return new OpeningData
            {
                openingId = id,
                openingType = type,
                wallId = wallId,
                normalizedPosition = normalizedPosition,
                openingWidth = openingWidth,
                openingHeight = isDoor ? 2.1f : 1.0f,
                sillHeight = isDoor ? 0f : 0.9f
            };
        }

        /// <summary>
        /// Creates a <see cref="FurnitureData"/> item positioned in world space.
        /// </summary>
        private static FurnitureData MakeFurniture(string id, string furnitureName,
            float posX, float posY, float sizeX, float sizeY, float rotation)
        {
            return new FurnitureData
            {
                furnitureId = id,
                furnitureName = furnitureName,
                positionX = posX,
                positionY = posY,
                sizeX = sizeX,
                sizeY = sizeY,
                rotation = rotation
            };
        }
    }
}
