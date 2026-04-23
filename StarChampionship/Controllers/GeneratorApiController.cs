using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarChampionship.Models;
using StarChampionship.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarChampionship.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeneratorApiController : ControllerBase
    {
        private readonly PlayerService _playerService;
        private readonly GeneratorService _generatorService;

        public GeneratorApiController(PlayerService playerService, GeneratorService generatorService)
        {
            _playerService = playerService;
            _generatorService = generatorService;
        }

        [HttpGet("players")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            var players = await _playerService.FindAllAsync();
            return Ok(players);
        }

        public class GenerateRequest
        {
            public int[] SelectedIds { get; set; } = new int[0];
            public int NumberOfTeams { get; set; }
            public bool HasFixedCaptains { get; set; }
            public Dictionary<string, string>? SelectedCaptains { get; set; }
            public double Margin { get; set; } = 10;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required" });

            var selectedIds = request.SelectedIds?.Distinct().ToHashSet() ?? new HashSet<int>();

            // Validate basic inputs
            if (request.NumberOfTeams < 2 || selectedIds.Count < request.NumberOfTeams)
            {
                return BadRequest(new { error = "Selecione atletas suficientes e número de times válido" });
            }

            var allPlayers = await _playerService.FindAllAsync();
            var selectedPlayers = allPlayers.Where(p => selectedIds.Contains(p.Id)).ToList();

            if (selectedPlayers.Count < request.NumberOfTeams)
            {
                return BadRequest(new { error = "Selecione atletas suficientes para a quantidade de times" });
            }

            var captainsToProcess = new Dictionary<int, int?>();
            var usedCaptains = new HashSet<int>();

            if (request.HasFixedCaptains && request.SelectedCaptains != null)
            {
                foreach (var entry in request.SelectedCaptains)
                {
                    if (int.TryParse(entry.Key, out int teamIndex) && int.TryParse(entry.Value, out int playerId))
                    {
                        if (playerId == 0) continue;
                        if (!selectedIds.Contains(playerId) || !usedCaptains.Add(playerId))
                        {
                            return BadRequest(new { error = "Capitães devem ser atletas selecionados e não podem repetir" });
                        }
                        captainsToProcess[teamIndex] = playerId;
                    }
                }
            }

            var candidateGenerations = new List<(List<Team> Teams, double Score)>();
            var allGenerations = new List<(List<Team> Teams, double Score)>();

            for (int i = 0; i < 200; i++)
            {
                var currentTeams = _generatorService.BuildBalancedTeams(selectedPlayers, request.NumberOfTeams, captainsToProcess);
                if (currentTeams == null || !currentTeams.Any()) continue;

                var totals = currentTeams.Select(t => t.TotalOverall).ToList();
                var avg = totals.Average();
                var variance = totals.Select(x => System.Math.Pow(x - avg, 2)).Average();
                double score = System.Math.Sqrt(variance);
                double diff = totals.Max() - totals.Min();

                allGenerations.Add((currentTeams, score));
                if (diff <= request.Margin)
                {
                    candidateGenerations.Add((currentTeams, score));
                }
            }

            var pool = candidateGenerations.Any() ? candidateGenerations : allGenerations;
            if (!pool.Any()) return StatusCode(500, new { error = "Unable to generate teams" });

            var topCandidates = pool.OrderBy(x => x.Score).Take(System.Math.Min(10, pool.Count)).ToList();
            var selected = topCandidates[System.Random.Shared.Next(topCandidates.Count)];

            var response = new
            {
                Teams = selected.Teams,
                Score = selected.Score
            };

            return Ok(response);
        }
    }
}
