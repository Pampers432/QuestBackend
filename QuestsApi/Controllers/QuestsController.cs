using Application.DTO;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuestsApi.DTO;
using System.Security.Claims;

namespace QuestsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly QuestService _questService;
        private readonly IQuestNotifier _notifier;
        private readonly QuestGeneratorService _questGenerator;

        public QuestsController(QuestService questService, IQuestNotifier notifier, QuestGeneratorService questGenerator)
        {
            _questService = questService;
            _notifier = notifier;
            _questGenerator = questGenerator;
        }

        [HttpPost("PostZones")]
        public IActionResult PostZones([FromBody] List<ZoneDto> zones)
        {
            return Ok(new { message = "Зоны", count = zones.Count, zones });
        }

        [HttpGet]
        [HttpGet("GetAllTemplates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _questService.GetAllTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            var template = await _questService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            return Ok(template);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] RoomTemplateUpsertDto dto)
        {
            var template = new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                PreviewImage = dto.PreviewImageUrl,
                SceneData = dto.SceneData ?? string.Empty
            };

            var created = await _questService.CreateTemplateAsync(template);
            return Ok(created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromForm] CreateRoomTemplateDto dto)
        {
            if (dto == null)
                return BadRequest("Неполные данные");

            try
            {
                // Получаем существующий шаблон
                var existingTemplate = await _questService.GetTemplateByIdAsync(id);
                if (existingTemplate == null)
                    return NotFound($"Шаблон с ID {id} не найден");

                // Обновляем название
                existingTemplate.Name = dto.Name;
        
                // Обновляем зоны
                existingTemplate.SceneData = dto.SceneData;

                // Если загружено новое изображение
                if (dto.PreviewImage != null)
                {
                    // Сохраняем новое изображение (используем существующий метод SaveImage)
                    var relativePath = await SaveImage(dto.PreviewImage);
                    existingTemplate.PreviewImage = relativePath;
            
                    // Здесь можно добавить удаление старого изображения, если нужно
                    // (но это опционально, т.к. SaveImage уже генерирует новый GUID)
                }

                // Сохраняем изменения в базу данных
                var updated = await _questService.UpdateTemplateAsync(existingTemplate);
                if (updated == null)
                    return NotFound();

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var deleted = await _questService.DeleteTemplateAsync(id);
            if (!deleted)
                return NotFound();

            return Ok();
        }

        [HttpPost("PostTemplate")]
        public async Task<IActionResult> PostTemplate([FromForm] CreateRoomTemplateDto dto)
        {
            if (dto == null || dto.PreviewImage == null)
                return BadRequest("Неполные данные или отсутствует изображение");

            try
            {
                var relativePath = await SaveImage(dto.PreviewImage);

                var entity = new RoomTemplate
                {
                    Id = dto.Id ?? Guid.NewGuid(),
                    Name = dto.Name,
                    PreviewImage = relativePath,
                    SceneData = dto.SceneData
                };

                var res = await _questService.SaveTemplateAsync(entity);

                if (res)
                {
                    return Ok(new { message = "Шаблон успешно сохранен на сервере", path = relativePath });
                }

                return BadRequest("Ошибка при сохранении в базу данных");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("GetAllQuests")]
        public async Task<IActionResult> GetAllQuests()
        {
            return Ok(await _questService.GetAllQuestsAsync());
        }

        [HttpGet("GetLatestQuests")]
        public async Task<IActionResult> GetLatestQuests([FromQuery] int count = 5)
        {
            var quests = await _questService.GetLatestQuestsAsync(count);
            return Ok(quests);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchQuests([FromQuery] string? searchTerm, [FromQuery] Guid? categoryId)
        {
            var quests = await _questService.SearchQuestsAsync(searchTerm, categoryId);
            return Ok(quests);
        }

        [HttpGet("GetById/{id:guid}")]
        public async Task<IActionResult> GetQuestById(Guid id)
        {
            var quest = await _questService.GetQuestByIdAsync(id);
            if (quest == null)
                return NotFound("Квест не найден");
            return Ok(quest);
        }

        [HttpGet("ByStatus")]
        public async Task<IActionResult> GetQuestsByStatus([FromQuery] string status)
        {
            var quests = await _questService.GetQuestsByStatusAsync(status);
            return Ok(quests);
        }

        [HttpGet("ByAuthor")]
        public async Task<IActionResult> GetQuestsByAuthor()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Admin")
            {
                var allQuests = await _questService.GetAllQuestsAsync();
                return Ok(allQuests);
            }

            var quests = await _questService.GetQuestsByAuthorAsync(authorId);
            return Ok(quests);
        }

        [HttpPut("UpdateQuest/{id:guid}")]
        public async Task<IActionResult> UpdateQuest(Guid id, [FromBody] CreateQuestRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var existingQuest = await _questService.GetQuestByIdAsync(id);
            if (existingQuest == null)
            {
                return NotFound("Квест не найден");
            }

            if (role != "Admin" && existingQuest.AuthorId != userId)
            {
                return Forbid("Вы можете редактировать только свои квесты.");
            }

            var quest = new Quest
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                Subject = request.Subject,
                Difficulty = request.Difficulty,
                Status = request.Status,
                CategoryId = request.CategoryId,
                AuthorId = existingQuest.AuthorId,
                Visibility = request.Visibility,
                QuestRooms = request.Rooms.Select(r => new QuestRoom
                {
                    RoomTemplateId = r.RoomTemplateId,
                    Title = r.Title,
                    OrderIndex = r.OrderIndex,
                    Questions = r.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        Type = q.Type,
                        Points = q.Points,
                        Hint = q.Hint,
                        OrderIndex = q.OrderIndex,
                        AnswerOptions = q.AnswerOptions.Select(a => new AnswerOption
                        {
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            OrderIndex = a.OrderIndex
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            var result = await _questService.UpdateQuestAsync(quest);
            if (!result)
            {
                return BadRequest("Ошибка при обновлении квеста");
            }

            var updatedQuest = await _questService.GetQuestByIdAsync(id);
            if (updatedQuest != null)
            {
                _ = _notifier.NotifyQuestUpdatedAsync(updatedQuest);
            }

            return Ok(new { message = "Квест успешно обновлён", questId = id });
        }

        [HttpDelete("DeleteQuest/{id:guid}")]
        public async Task<IActionResult> DeleteQuest(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var existingQuest = await _questService.GetQuestByIdAsync(id);
            if (existingQuest == null)
            {
                return NotFound("Квест не найден");
            }

            if (role != "Admin" && existingQuest.AuthorId != userId)
            {
                return Forbid("Вы можете удалять только свои квесты.");
            }

            var result = await _questService.DeleteQuestAsync(id);
            if (!result)
            {
                return BadRequest("Ошибка при удалении квеста");
            }

            _ = _notifier.NotifyQuestDeletedAsync(id);

            return Ok(new { message = "Квест успешно удалён", questId = id });
        }

        [HttpPost("CreateQuest")]
        public async Task<IActionResult> PostQuest([FromBody] CreateQuestRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя." });
            }

            var quest = new Quest
            {
                Title = request.Title,
                Description = request.Description,
                Subject = request.Subject,
                Difficulty = request.Difficulty,
                Status = request.Status,
                CategoryId = request.CategoryId,
                AuthorId = authorId,
                Visibility = request.Visibility,
                QuestRooms = request.Rooms.Select(r => new QuestRoom
                {
                    RoomTemplateId = r.RoomTemplateId,
                    Title = r.Title,
                    OrderIndex = r.OrderIndex,
                    Questions = r.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        Type = q.Type,
                        Points = q.Points,
                        Hint = q.Hint,
                        OrderIndex = q.OrderIndex,
                        AnswerOptions = q.AnswerOptions.Select(a => new AnswerOption
                        {
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            OrderIndex = a.OrderIndex
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            var res = await _questService.CreateQuestAsync(quest);

            var createdQuest = await _questService.GetQuestByIdAsync(quest.Id);
            if (createdQuest != null)
            {
                _ = _notifier.NotifyQuestCreatedAsync(createdQuest);
            }

            return Ok(new { message = res, quest.Id });
        }

        [HttpPost("GenerateQuest")]
        public async Task<IActionResult> GenerateQuest([FromBody] GenerateQuestRequest request, [FromQuery] bool save = false)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя." });
            }

            if (save)
            {
                try
                {
                    var questId = await _questGenerator.GenerateAndSaveQuestAsync(request, authorId);
                    return Ok(new { message = "Квест сгенерирован и сохранён", questId });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            var result = await _questGenerator.GenerateQuestAsync(request, authorId);
            return Ok(result);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя." });
            }

            var analytics = await _questService.GetAuthorAnalyticsAsync(authorId);
            return Ok(analytics);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }
    }
}
