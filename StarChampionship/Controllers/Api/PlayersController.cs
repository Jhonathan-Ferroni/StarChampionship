using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarChampionship.Models;
using StarChampionship.Services;
using StarChampionship.Services.Exceptions;

namespace StarChampionship.Controllers.Api
{
    /// <summary>
    /// Web API Controller para gerenciar Players.
    /// 
    /// Regras de Autorização:
    /// - GET (listar/detalhes): Público (AllowAnonymous)
    /// - POST (criar): Apenas Admin (Authorize com Role "Admin")
    /// - PUT (editar): Apenas Admin (Authorize com Role "Admin")
    /// - DELETE: Apenas Admin (Authorize com Role "Admin")
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(PlayerService playerService, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a lista de todos os players.
        /// Acesso público (sem autenticação necessária).
        /// </summary>
        /// <returns>Lista de players</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Player>>> GetAll()
        {
            try
            {
                var players = await _playerService.FindAllAsync();
                _logger.LogInformation("Retrieved {Count} players", players.Count);
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtém os detalhes de um player específico.
        /// Acesso público (sem autenticação necessária).
        /// </summary>
        /// <param name="id">ID do player</param>
        /// <returns>Dados do player ou NotFound</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Player>> GetById(int id)
        {
            try
            {
                var player = await _playerService.FindByIdAsync(id);
                
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found", id);
                    return NotFound(new { error = "Player not found" });
                }

                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player {PlayerId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Cria um novo player.
        /// ⚠️ REQUER: Autenticação JWT com Role "Admin"
        /// </summary>
        /// <param name="player">Dados do novo player</param>
        /// <returns>Player criado com status 201</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Player>> Create([FromBody] Player player)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for player creation");
                return BadRequest(ModelState);
            }

            try
            {
                await _playerService.InsertAsync(player);
                _logger.LogInformation("Player {PlayerId} created successfully", player.Id);
                return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Atualiza um player existente.
        /// ⚠️ REQUER: Autenticação JWT com Role "Admin"
        /// </summary>
        /// <param name="id">ID do player a atualizar</param>
        /// <param name="player">Novos dados do player</param>
        /// <returns>NoContent se bem-sucedido ou erro apropriado</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Player player)
        {
            if (id != player.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != body ID {BodyId}", id, player.Id);
                return BadRequest(new { error = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for player update");
                return BadRequest(ModelState);
            }

            try
            {
                await _playerService.UpdateAsync(player);
                _logger.LogInformation("Player {PlayerId} updated successfully", id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Player {PlayerId} not found for update", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player {PlayerId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Deleta um player.
        /// ⚠️ REQUER: Autenticação JWT com Role "Admin"
        /// </summary>
        /// <param name="id">ID do player a deletar</param>
        /// <returns>NoContent se bem-sucedido ou erro apropriado</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _playerService.RemoveAsync(id);
                _logger.LogInformation("Player {PlayerId} deleted successfully", id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Player {PlayerId} not found for deletion", id);
                return NotFound(new { error = ex.Message });
            }
            catch (IntegrityException ex)
            {
                _logger.LogWarning(ex, "Integrity error deleting player {PlayerId}", id);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player {PlayerId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
