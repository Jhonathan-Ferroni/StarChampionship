using Xunit;
using StarChampionship.Models;

namespace StarChampionship.Tests
{
    public class TeamTests
    {
        [Fact]
        public void Team_DeveCalcularTotalOverallCorretamente()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Id = 1, Name = "Jogador 1", Shoot = 10, Dribble = 10, FirstTouch = 10, BallControl = 10, Defense = 10, Pass = 10, Speed = 10, Strength = 10 },
                new Player { Id = 2, Name = "Jogador 2", Shoot = 10, Dribble = 10, FirstTouch = 10, BallControl = 10, Defense = 10, Pass = 10, Speed = 10, Strength = 10 }
            };

            var team = new Team
            {
                Id = 1,
                Name = "Meu Time",
                Players = players
            };

            // Act
            var totalOverall = team.TotalOverall;

            // Assert
            Assert.Equal(20, totalOverall);
        }

        [Fact]
        public void Team_DeveCalcularMediaOverallCorretamente()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Id = 1, Name = "Jogador 1", Shoot = 10, Dribble = 10, FirstTouch = 10, BallControl = 10, Defense = 10, Pass = 10, Speed = 10, Strength = 10 },
                new Player { Id = 2, Name = "Jogador 2", Shoot = 10, Dribble = 10, FirstTouch = 10, BallControl = 10, Defense = 10, Pass = 10, Speed = 10, Strength = 10 }
            };

            var team = new Team
            {
                Id = 1,
                Name = "Meu Time",
                Players = players
            };

            // Act
            var averageOverall = team.AverageOverall;

            // Assert
            Assert.Equal(10, averageOverall);
        }

        [Fact]
        public void Team_AverageOverallDeveSer0QuandoSemJogadores()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Time Vazio",
                Players = new List<Player>()
            };

            // Act
            var averageOverall = team.AverageOverall;

            // Assert
            Assert.Equal(0, averageOverall);
        }
        [Fact]
        public void Team_AverageOverallDeveRetornarZeroSeTimeEstiverVazio()
        {
            // Arrange
            var team = new Team
            {
                Id = 1,
                Name = "Time Vazio",
                Players = new List<Player>()
            };
            // Act
            var averageOverall = team.AverageOverall;
            // Assert
            Assert.Equal(0, averageOverall);
        }
    }
}