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
            if (dto.TimeLimit.HasValue && dto.TimeLimit.Value > 0 && dto.EndsAt.HasValue)
            {
                var maxAllowedEnd = dto.StartsAt.AddMinutes(dto.TimeLimit.Value);
                if (dto.EndsAt.Value < maxAllowedEnd)
                {
                    return BadRequest(new { message = "Время окончания не может быть раньше чем время начала + лимит времени" });
                }
            }

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
                IsActive = dto.EndsAt == null || dto.EndsAt > DateTime.UtcNow
            };

            var newSession = await _sessionService.CreateSessionAsync(session);

            return Ok(newSession);
        }

        [HttpGet("GetByAccessCode/{accessCode}")]
        public async Task<IActionResult> GetByAccessCode([FromRoute] string accessCode)
        {
            var session = await _sessionService.GetByAccessCodeAsync(accessCode);

            if (session == null)
            {
                return NotFound();
            }

            return Ok(session);
        }

        [HttpGet("GetByAccessCode")]
        public async Task<IActionResult> GetByAccessCodeQuery([FromQuery] string accessCode)
        {
            if (string.IsNullOrWhiteSpace(accessCode))
            {
                return BadRequest("accessCode is required");
            }

            var session = await _sessionService.GetByAccessCodeAsync(accessCode);

            if (session == null)
            {
                return NotFound();
            }

            return Ok(session);
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

        [HttpGet("RecentByAuthor")]
        public async Task<IActionResult> GetRecentSessionsByAuthor([FromQuery] int count = 10)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
                return Unauthorized(new { message = "Не удалось определить пользователя." });

            var sessions = await _sessionService.GetRecentSessionsByAuthorAsync(authorId, count);
            return Ok(sessions);
        }

        [HttpGet("Dashboard/{sessionId:guid}")]
        public async Task<IActionResult> GetSessionDashboard(Guid sessionId)
        {
            var dashboard = await _sessionService.GetSessionDashboardAsync(sessionId);
            if (dashboard == null)
                return NotFound();

            return Ok(dashboard);
        }

        [HttpGet("Export/{sessionId:guid}")]
        public async Task<IActionResult> ExportSessionReport(Guid sessionId, [FromQuery] string format = "json")
        {
            var report = await _sessionService.ExportSessionReportAsync(sessionId, format);
            if (report == null)
                return NotFound();

            var contentType = format.ToLower() switch
            {
                "csv" => "text/csv",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/json"
            };
            var ext = format.ToLower() == "xlsx" ? "xlsx" : format;
            var fileName = $"session_{sessionId}_{DateTime.UtcNow:yyyyMMdd}.{ext}";

            return File(report, contentType, fileName);
        }
    }
}
