using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using QuestsApi.DTO;

namespace QuestsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestSessionsController : ControllerBase
    {
        private readonly QuestSessionService _sessionService;

        public QuestSessionsController(QuestSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("CreateSession")]
        public async Task<IActionResult> CreateSession(CreateSessionDto dto)
        {
            var session = new QuestSession
            {
                Id = Guid.NewGuid(),
                QuestId = dto.QuestId,
                StartedBy = dto.StartedBy,
                TimeLimit = dto.TimeLimit,
                AllowPartialCompletion = dto.AllowPartialCompletion,
                AllowToSkip = dto.AllowToSkip,
                AccessCode = dto.AccessCode,
                StartsAt = dto.StartsAt,
                EndsAt = dto.EndsAt,
                IsActive = true
            };

            var newSession = await _sessionService.CreateSessionAsync(session);

            return Ok(newSession);
        }

        [HttpPost("StartAttempt")]
        public async Task<IActionResult> StartAttempt([FromBody] StartAttemptDto dto)
        {
            var result = await _sessionService.StartAttemptAsync(dto);
            return Ok(result);
        }

        [HttpPost("FinishAttempt/{attemptId}")]
        public async Task<IActionResult> FinishAttempt(Guid attemptId)
        {
            await _sessionService.FinishAttemptAsync(attemptId);
            return Ok(new { message = "Attempt finished" });
        }

        [HttpPost("SaveAnswer")]
        public async Task<IActionResult> SaveAnswer([FromBody] UserAnswerDto dto)
        {
            await _sessionService.SaveAnswerAsync(dto);
            return Ok();
        }
    }
}
