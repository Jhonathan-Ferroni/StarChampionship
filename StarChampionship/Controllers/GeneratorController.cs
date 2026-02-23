using Microsoft.AspNetCore.Mvc;
using StarChampionship.Services;
using StarChampionship.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarChampionship.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly PlayerService _playerService;
        private readonly GeneratorService _generatorService;

        public GeneratorController(PlayerService playerService, GeneratorService generatorService)
        {
            _playerService = playerService;
            _generatorService = generatorService;
        }

        public async Task<IActionResult> Index()
        {
            var players = await _playerService.FindAllAsync();
            return View(players);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(int[] selectedIds, int numberOfTeams, bool hasFixedCaptains, Dictionary<int, int?> selectedCaptains, double margin = 2.0)
        {
            // 1. Tratamento de Erro: Se não houver jogadores selecionados o suficiente
            if (selectedIds == null || selectedIds.Length < numberOfTeams || numberOfTeams < 2)
            {
                TempData["Error"] = "Selecione atletas suficientes para a quantidade de times.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Correção do BUG: Normaliza o dicionário de capitães
            // Se a função estiver desligada OU o dicionário vier nulo, inicializamos um vazio
            // Isso evita o erro de referência nula no loop e no serviço
            var captainsToProcess = (hasFixedCaptains && selectedCaptains != null)
                                    ? selectedCaptains
                                    : new Dictionary<int, int?>();

            var allPlayers = await _playerService.FindAllAsync();
            var selectedPlayers = allPlayers.Where(p => selectedIds.Contains(p.Id)).ToList();

            List<Team> bestGeneration = null;
            double minVariance = double.MaxValue;
            Random rand = new Random();

            // 3. Lógica de Equilíbrio (1000 tentativas)
            for (int i = 0; i < 1000; i++)
            {
                var shuffledPlayers = selectedPlayers.OrderBy(x => rand.Next()).ToList();

                // Passamos o captainsToProcess (que nunca será nulo agora)
                var currentTeams = _generatorService.BuildBalancedTeams(shuffledPlayers, numberOfTeams, captainsToProcess);

                if (currentTeams == null || !currentTeams.Any()) continue;

                double maxOverall = currentTeams.Max(t => t.TotalOverall);
                double minOverall = currentTeams.Min(t => t.TotalOverall);
                double diff = maxOverall - minOverall;

                if (diff <= margin)
                {
                    bestGeneration = currentTeams;
                    minVariance = diff;
                    break;
                }

                if (diff < minVariance)
                {
                    minVariance = diff;
                    bestGeneration = currentTeams;
                }
            }

            // 4. Preparação dos dados para a View de Resultado
            ViewBag.Teams = bestGeneration;
            ViewBag.Difference = minVariance;
            ViewBag.SelectedIds = selectedIds;
            ViewBag.NumberOfTeams = numberOfTeams;
            ViewBag.Margin = margin;

            return View("Result");
        }
    }
}