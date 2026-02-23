using StarChampionship.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarChampionship.Services
{
    public class GeneratorService
    {
        public List<Team> BuildBalancedTeams(List<Player> players, int numberOfTeams, Dictionary<int, int?> selectedCaptains)
        {
            if (numberOfTeams <= 0) return new List<Team>();

            // 1. Inicializa os times
            var teams = new List<Team>();
            for (int i = 1; i <= numberOfTeams; i++)
            {
                teams.Add(new Team { Id = i, Name = $"Time {i}", Players = new List<Player>() });
            }

            // 2. Aloca os capitães pré-definidos
            var remainingPlayers = new List<Player>(players);

            foreach (var selection in selectedCaptains)
            {
                int teamId = selection.Key;
                int? playerId = selection.Value;

                if (playerId.HasValue)
                {
                    var captain = remainingPlayers.FirstOrDefault(p => p.Id == playerId.Value);
                    if (captain != null)
                    {
                        var targetTeam = teams.FirstOrDefault(t => t.Id == teamId);
                        targetTeam?.Players.Add(captain);
                        remainingPlayers.Remove(captain);
                    }
                }
            }

            // 3. Ordena os jogadores restantes pelo Overall (do maior para o menor)
            // Isso é essencial para o balanceamento heurístico
            remainingPlayers = remainingPlayers.OrderByDescending(p => p.Overall).ToList();

            // 4. Distribui os jogadores restantes
            // Usamos a lógica de "Snake Draft" invertida para equilibrar: 
            // sempre adicionamos o próximo melhor jogador ao time com o menor Overall atual
            foreach (var player in remainingPlayers)
            {
                var weakestTeam = teams
                    .OrderBy(t => t.Players.Sum(p => p.Overall))
                    .ThenBy(t => t.Players.Count) // Desempate pela quantidade de membros
                    .First();

                weakestTeam.Players.Add(player);
            }

            return teams;
        }
    }
}