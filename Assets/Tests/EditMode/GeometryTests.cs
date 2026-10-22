using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using HousePlanner.Core;
using HousePlanner.HouseGeneration;

namespace HousePlanner.Tests
{
    [TestFixture]
    public class GeometryTests
    {
        [Test]
        public void CreateFloorMesh_HasCorrectGeometry()
        {
            Mesh floor = ProceduralMeshBuilder.CreateFloor(5f, 4f);

            Assert.IsNotNull(floor);
            Assert.AreEqual(4, floor.vertexCount);
            Assert.AreEqual(6, floor.triangles.Length); // 2 triangles = 6 indices
            Assert.AreEqual(new Vector3(5f, 0f, 4f), floor.bounds.size);
        }

        [Test]
        public void CreateSolidWallMesh_HasValidDimensions()
        {
            Mesh wall = ProceduralMeshBuilder.CreateWall(6f, 2.8f, 0.15f, null);

            Assert.IsNotNull(wall);
            Assert.Greater(wall.vertexCount, 0);
            Assert.Greater(wall.triangles.Length, 0);
            Assert.AreEqual(6f, wall.bounds.size.x, 0.01f);
            Assert.AreEqual(2.8f, wall.bounds.size.y, 0.01f);
        }

        [Test]
        public void CreateWallWithDoorOpening_GeneratesSubdividedQuads()
        {
            List<WallOpeningData> openings = new List<WallOpeningData>
            {
                new WallOpeningData
                {
                    id = "door_1",
                    openingType = OpeningType.Door,
                    position = 0.5f,
                    width = 0.9f,
                    height = 2.1f,
                    elevation = 0f
                }
            };

            Mesh wallWithDoor = ProceduralMeshBuilder.CreateWall(5f, 2.8f, 0.15f, openings);

            Assert.IsNotNull(wallWithDoor);
            // Wall with opening should have more vertices than a simple 24-vertex box
            Assert.Greater(wallWithDoor.vertexCount, 24);
        }

        [Test]
        public void CreateCeilingMesh_MatchesFloorSize()
        {
            Mesh ceiling = ProceduralMeshBuilder.CreateCeiling(4f, 6f);

            Assert.IsNotNull(ceiling);
            Assert.AreEqual(4, ceiling.vertexCount);
            Assert.AreEqual(new Vector3(4f, 0f, 6f), ceiling.bounds.size);
        }
    }
}
