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
            // 1. Validação inicial de segurança
            if (numberOfTeams <= 0 || players == null) return new List<Team>();

            // 2. Inicializa os times
            var teams = new List<Team>();
            for (int i = 1; i <= numberOfTeams; i++)
            {
                teams.Add(new Team { Id = i, Name = $"Time {i}", Players = new List<Player>() });
            }

            // 3. Aloca os capitães pré-definidos (Se houver)
            var remainingPlayers = new List<Player>(players);

            // Verificação de nulidade para o dicionário vindo do Controller
            if (selectedCaptains != null)
            {
                foreach (var selection in selectedCaptains)
                {
                    int teamId = selection.Key;
                    int? playerId = selection.Value;

                    // CORREÇÃO: Só processa se houver um ID válido e maior que zero
                    // Isso ignora a opção "-- Sem Capitão --" vinda do HTML
                    if (playerId.HasValue && playerId.Value > 0)
                    {
                        var captain = remainingPlayers.FirstOrDefault(p => p.Id == playerId.Value);
                        if (captain != null)
                        {
                            var targetTeam = teams.FirstOrDefault(t => t.Id == teamId);
                            if (targetTeam != null)
                            {
                                targetTeam.Players.Add(captain);
                                remainingPlayers.Remove(captain);
                            }
                        }
                    }
                }
            }

            // 4. Ordena aleatoriedade e potes
            var rand = new Random();
            remainingPlayers = remainingPlayers
                .OrderByDescending(p => Math.Floor(p.Overall / 5.0)) // Agrupa em potes (ex: 80-84, 85-89)
                .ThenBy(p => Guid.NewGuid()) // Embaralha jogadores que estão no mesmo pote
                .ToList();

            // 5. Distribuição (Ajuste no critério de desempate)
            foreach (var player in remainingPlayers)
            {
                var weakestTeam = teams
                    .OrderBy(t => t.Players.Sum(p => p.Overall)) // Critério 1: Menor soma total
                    .ThenBy(t => t.Players.Count)                // Critério 2: Menor número de jogadores
                    .ThenBy(t => rand.Next())                    // Critério 3: Sorteio puro se houver empate nos acima
                    .First();

                weakestTeam.Players.Add(player);
            }

            return teams;
        }
    }
}