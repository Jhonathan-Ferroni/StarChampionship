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
        public async Task<IActionResult> Generate(int[] selectedIds, double margin)
        {
            if (margin <= 0) margin = 2.0;

            if (selectedIds == null || selectedIds.Length < 2)
            {
                return RedirectToAction(nameof(Index));
            }

            var allPlayers = await _playerService.FindAllAsync();
            var players = allPlayers.Where(p => selectedIds.Contains(p.Id)).ToList();

            int n = players.Count;
            int sizeTeamA = n / 2;

            List<Player> bestTeamA = new List<Player>();
            List<Player> bestTeamB = new List<Player>();
            double minDiff = double.MaxValue;

            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var shuffled = players.OrderBy(x => rand.Next()).ToList();
                var tempA = shuffled.Take(sizeTeamA).ToList();
                var tempB = shuffled.Skip(sizeTeamA).ToList();

                double diff = Math.Abs(tempA.Sum(p => p.Overall) - tempB.Sum(p => p.Overall));

                // Usa a margem que veio do HTML
                if (diff <= margin)
                {
                    bestTeamA = tempA;
                    bestTeamB = tempB;
                    minDiff = diff;
                    break;
                }

                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestTeamA = tempA;
                    bestTeamB = tempB;
                }
            }

            ViewBag.TeamA = bestTeamA;
            ViewBag.TeamB = bestTeamB;
            ViewBag.Difference = minDiff;
            ViewBag.SelectedIds = selectedIds;
            ViewBag.Margin = margin; // Devolvemos para a View usar no botão "Sortear Novamente"

            return View("Result");
        }
    }
}