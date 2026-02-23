using StarChampionship.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarChampionship.Services
{
    public class GeneratorService
    {
        public List<Team> BuildBalancedTeams(List<Player> players, int numberOfTeams, Dictionary<int, int?> selectedCaptains, double margin)
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

            // 4. Ordena os jogadores restantes pelo Overall (do maior para o menor)
            // Essencial para a lógica de distribuição gulosa (Greedy Algorithm)
            remainingPlayers = remainingPlayers.OrderByDescending(p => p.Overall).ToList();

            // 5. Distribui os jogadores restantes
            // Lógica de equilíbrio: sempre adicionamos o próximo melhor jogador ao time 
            // que possui a menor soma de Overall no momento.
            foreach (var player in remainingPlayers)
            {
                var weakestTeam = teams
                    .OrderBy(t => t.Players.Sum(p => p.Overall))
                    .ThenBy(t => t.Players.Count) // Critério de desempate: menor número de jogadores
                    .First();

                weakestTeam.Players.Add(player);
            }

            return teams;
        }
    }
}