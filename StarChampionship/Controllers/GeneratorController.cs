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

        public GeneratorController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        // GET: Generator
        public async Task<IActionResult> Index()
        {
            var players = await _playerService.FindAllAsync();
            return View(players);
        }

        // POST: Generator/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(int[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length < 2)
            {
                return RedirectToAction(nameof(Index));
            }

            // Busca apenas os jogadores selecionados no banco
            var allPlayers = await _playerService.FindAllAsync();
            var players = allPlayers.Where(p => selectedIds.Contains(p.Id)).ToList();

            int n = players.Count;
            int sizeTeamA = n / 2; // Divide ao meio (ex: 10 vira 5x5, 11 vira 5x6)

            List<Player> bestTeamA = new List<Player>();
            List<Player> bestTeamB = new List<Player>();
            double minDiff = double.MaxValue;

            Random rand = new Random();

            // Algoritmo de Otimização: Tenta 1000 combinações diferentes
            for (int i = 0; i < 1000; i++)
            {
                // Embaralha a lista de jogadores selecionados
                var shuffled = players.OrderBy(x => rand.Next()).ToList();

                // Divide em dois grupos temporários
                var tempA = shuffled.Take(sizeTeamA).ToList();
                var tempB = shuffled.Skip(sizeTeamA).ToList();

                // Calcula a soma do Overall de cada time
                double sumA = tempA.Sum(p => p.Overall);
                double sumB = tempB.Sum(p => p.Overall);
                double diff = Math.Abs(sumA - sumB);

                // MARGEM DE TOLERÂNCIA: Se a diferença for minúscula (< 2.0),
                // aceitamos essa versão para garantir variedade no sorteio.
                if (diff < 30.0)
                {
                    bestTeamA = tempA;
                    bestTeamB = tempB;
                    minDiff = diff;
                    break;
                }

                // Se não achou na margem, guarda a melhor que encontrar até o fim do loop
                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestTeamA = tempA;
                    bestTeamB = tempB;
                }
            }

            // Passa os dados para a View Result.cshtml
            ViewBag.TeamA = bestTeamA;
            ViewBag.TeamB = bestTeamB;
            ViewBag.Difference = minDiff;

            ViewBag.SelectedIds = selectedIds;

            return View("Result");
        }
    }
}