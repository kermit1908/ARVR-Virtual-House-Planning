using System.IO;
using NUnit.Framework;
using UnityEngine;
using HousePlanner.Core;
using HousePlanner.Persistence;

namespace HousePlanner.Tests
{
    [TestFixture]
    public class PersistenceTests
    {
        [Test]
        public void ProjectData_SerializeDeserialize_PreservesAllFields()
        {
            ProjectData original = SampleProjectGenerator.CreateSampleProject();
            string json = JsonUtility.ToJson(original);

            Assert.IsFalse(string.IsNullOrEmpty(json));

            ProjectData deserialized = JsonUtility.FromJson<ProjectData>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.projectId, deserialized.projectId);
            Assert.AreEqual(original.projectName, deserialized.projectName);
            Assert.AreEqual(original.rooms.Count, deserialized.rooms.Count);
            Assert.AreEqual(original.furnitureItems.Count, deserialized.furnitureItems.Count);
        }

        [Test]
        public void SampleProject_ContainsMultipleRoomsAndFurniture()
        {
            ProjectData sample = SampleProjectGenerator.CreateSampleProject();

            Assert.IsNotNull(sample);
            Assert.GreaterOrEqual(sample.rooms.Count, 3);
            Assert.GreaterOrEqual(sample.furnitureItems.Count, 4);
            Assert.IsTrue(sample.Validate());
        }

        [Test]
        public void SchemaVersionUpgrader_ReturnsOriginalForV1()
        {
            string v1Json = "{\"schemaVersion\": 1, \"projectName\": \"Modern House\"}";
            string upgraded = SchemaVersionUpgrader.UpgradeIfNeeded(v1Json);

            Assert.AreEqual(v1Json, upgraded);
        }
    }
}
