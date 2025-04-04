using Xunit;
using GameCore.HexGrid;

namespace GameCore.Tests.HexGrid
{
    public class HexCoordTests
    {
        [Fact]
        public void Distance_SameCoord_ReturnsZero()
        {
            // Arrange
            var coord = new HexCoord(3, 4);
            
            // Act
            var distance = HexCoord.Distance(coord, coord);
            
            // Assert
            Assert.Equal(0, distance);
        }
        
        [Fact]
        public void Distance_NeighborCoords_ReturnsOne()
        {
            // Arrange
            var coord1 = new HexCoord(3, 4);
            var coord2 = new HexCoord(4, 4);
            
            // Act
            var distance = HexCoord.Distance(coord1, coord2);
            
            // Assert
            Assert.Equal(1, distance);
        }
        
        [Fact]
        public void Equals_SameCoords_ReturnsTrue()
        {
            // Arrange
            var coord1 = new HexCoord(3, 4);
            var coord2 = new HexCoord(3, 4);
            
            // Act & Assert
            Assert.Equal(coord1, coord2);
            Assert.True(coord1 == coord2);
        }
    }
}