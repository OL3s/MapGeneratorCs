using Xunit;
using MapGeneratorCs;
using System.IO;

namespace MapGeneratorCs.Tests
{
    public class MapConstructorTests
    {
        [Fact]
        public void GenerateMap_Test()
        {
            // Arrange
            var map = new MapConstructor();

            // Act
            map.GenerateMap();
            map.SaveMapAsImage();

            // Assert

            // check nodes
            Assert.True(map.NodeContainer.NodesFloor.Count > 0);
            Assert.True(map.NodeContainer.NodesObjects.Count > 0);
            Assert.True(map.NodeContainer.NodesGenerate.Count > 0);

            // check image file
            Assert.True(File.Exists("export/map_output.png"));

            // check config files
            Assert.True(File.Exists("config/configMapConfig.json"));
            Assert.True(File.Exists("config/configSpawnWeights.json"));
            Assert.True(File.Exists("config/configObjectsWeights.json"));

            // Clean up
            map.ClearAllData();
        }

        [Fact]
        public void SaveAndLoadMapAsJson_Test()
        {
            // Arrange
            var map = new MapConstructor();
            map.GenerateMap();
            map.SaveMapAsJson();

            var newMap = new MapConstructor();
            newMap.LoadMapFromJson();

    
            // check if loaded map has same number of nodes
            Assert.Equal(map.NodeContainer.NodesFloor.Count, newMap.NodeContainer.NodesFloor.Count);
            Assert.Equal(map.NodeContainer.NodesObjects.Count, newMap.NodeContainer.NodesObjects.Count);
            Assert.Equal(map.NodeContainer.NodesGenerate.Count, newMap.NodeContainer.NodesGenerate.Count);

            // Clean up
            map.ClearAllData();
        }
    }
}