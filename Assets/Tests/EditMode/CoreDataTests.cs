using System;
using NUnit.Framework;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Tests
{
    [TestFixture]
    public class CoreDataTests
    {
        [Test]
        public void ProjectData_CreateDefault_GeneratesValidFields()
        {
            ProjectData project = ProjectData.CreateDefault("Test Dream House");

            Assert.IsNotNull(project);
            Assert.IsFalse(string.IsNullOrEmpty(project.projectId));
            Assert.AreEqual("Test Dream House", project.projectName);
            Assert.AreEqual(ProjectData.CurrentSchemaVersion, project.schemaVersion);
            Assert.IsNotNull(project.rooms);
            Assert.IsNotNull(project.furnitureItems);
            Assert.IsNotNull(project.floorPlanSettings);
            Assert.IsTrue(project.Validate());
        }

        [Test]
        public void RoomData_GetArea_ComputesCorrectly()
        {
            RoomData room = new RoomData
            {
                roomId = "r1",
                roomName = "Living Room",
                width = 5.0f,
                length = 4.0f
            };

            Assert.AreEqual(20.0f, room.GetArea(), 0.001f);
        }

        [Test]
        public void RoomData_GetPerimeter_ComputesCorrectly()
        {
            RoomData room = new RoomData
            {
                roomId = "r1",
                roomName = "Living Room",
                width = 5.0f,
                length = 4.0f
            };

            Assert.AreEqual(18.0f, room.GetPerimeter(), 0.001f);
        }

        [Test]
        public void RoomData_Validate_RejectsZeroDimensions()
        {
            RoomData invalidRoom = new RoomData
            {
                roomId = "r1",
                roomName = "Bad Room",
                width = 0f,
                length = 4.0f
            };

            Assert.IsFalse(invalidRoom.Validate());
        }

        [Test]
        public void WallData_GetLength_ComputesDistance()
        {
            WallData wall = new WallData
            {
                wallId = "w1",
                startPoint = new Vector2(0f, 0f),
                endPoint = new Vector2(3f, 4f)
            };

            Assert.AreEqual(5.0f, wall.GetLength(), 0.001f);
        }

        [Test]
        public void WallData_GetDirection_ReturnsNormalizedVector()
        {
            WallData wall = new WallData
            {
                wallId = "w1",
                startPoint = new Vector2(0f, 0f),
                endPoint = new Vector2(10f, 0f)
            };

            Vector2 dir = wall.GetDirection();
            Assert.AreEqual(1.0f, dir.x, 0.001f);
            Assert.AreEqual(0.0f, dir.y, 0.001f);
        }

        [Test]
        public void SerializableTypes_VectorConversions_MaintainValues()
        {
            Vector3 original = new Vector3(1.23f, 4.56f, 7.89f);
            Vector3Serializable serializable = original;
            Vector3 back = serializable;

            Assert.AreEqual(original.x, back.x, 0.001f);
            Assert.AreEqual(original.y, back.y, 0.001f);
            Assert.AreEqual(original.z, back.z, 0.001f);
        }

        [Test]
        public void FurnitureItemData_Validate_AcceptsValidItem()
        {
            FurnitureItemData item = new FurnitureItemData
            {
                instanceId = Guid.NewGuid().ToString(),
                catalogueId = "Sofa",
                customName = "Blue Sofa",
                position = new Vector3(1f, 0f, 1f),
                rotation = Vector3.zero,
                scale = Vector3.one,
                materialColor = "#FFFFFF"
            };

            Assert.IsTrue(item.Validate());
        }
    }
}
