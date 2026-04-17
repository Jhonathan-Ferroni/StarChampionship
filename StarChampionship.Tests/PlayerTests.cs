using StarChampionship.Models;
using System.Numerics;
using Xunit;

namespace StarChampionship.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void Player_DeveCalcularOverallCorretamente()
        {
            // Arrange
            var player = new Player
            {
                Id = 1,
                Name = "Jhonathan",
                Shoot = 8,
                Dribble = 7,
                FirstTouch = 9,
                BallControl = 8,
                Defense = 6,
                Pass = 7,
                Speed = 8,
                Strength = 7
            };

            // Act
            var overall = player.Overall;

            // Assert
            Assert.Equal(7.5, overall);
        }

        [Fact]
        public void Player_OverallDeveSer100QuandoTodosAtributosSao100()
        {
            // Arrange
            var player = new Player
            {
                Id = 1,
                Name = "Pelé",
                Shoot = 100,
                Dribble = 100,
                FirstTouch = 100,
                BallControl = 100,
                Defense = 100,
                Pass = 100,
                Speed = 100,
                Strength = 100
            };

            // Act
            var overall = player.Overall;

            // Assert
            Assert.Equal(100, overall);
        }

        [Fact]
        public void Player_OverallDeveSer0QuandoTodosAtributosSao0()
        {
            // Arrange
            var player = new Player
            {
                Id = 1,
                Name = "Player Novo",
                Shoot = 0,
                Dribble = 0,
                FirstTouch = 0,
                BallControl = 0,
                Defense = 0,
                Pass = 0,
                Speed = 0,
                Strength = 0
            };

            // Act
            var overall = player.Overall;

            // Assert
            Assert.Equal(0, overall);
        }
    }
}