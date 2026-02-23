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

        // Injetamos o novo GeneratorService no construtor
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
            if (selectedIds == null || selectedIds.Length < numberOfTeams)
            {
                // Adicione uma mensagem de erro aqui se desejar (TempData)
                return RedirectToAction(nameof(Index));
            }

            var allPlayers = await _playerService.FindAllAsync();
            var selectedPlayers = allPlayers.Where(p => selectedIds.Contains(p.Id)).ToList();

            List<Team> bestGeneration = null;
            double minVariance = double.MaxValue;

            Random rand = new Random();

            // Mantemos sua lógica de 1000 tentativas para buscar o melhor equilíbrio
            for (int i = 0; i < 1000; i++)
            {
                // Embaralhamos a lista para que a distribuição gulosa varie a cada iteração
                var shuffledPlayers = selectedPlayers.OrderBy(x => rand.Next()).ToList();

                // Chamamos o serviço que já lida com os capitães fixos
                var currentTeams = _generatorService.BuildBalancedTeams(shuffledPlayers, numberOfTeams, selectedCaptains);

                // Calculamos a diferença entre o time mais forte e o mais fraco (Variância)
                double maxOverall = currentTeams.Max(t => t.TotalOverall);
                double minOverall = currentTeams.Min(t => t.TotalOverall);
                double diff = maxOverall - minOverall;

                // Se atingir a margem desejada, interrompemos e retornamos esse resultado
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

            // Passamos os dados para a View de Resultado
            ViewBag.Teams = bestGeneration;
            ViewBag.Difference = minVariance;
            ViewBag.SelectedIds = selectedIds;
            ViewBag.NumberOfTeams = numberOfTeams;
            ViewBag.Margin = margin;

            return View("Result");
        }
    }
}