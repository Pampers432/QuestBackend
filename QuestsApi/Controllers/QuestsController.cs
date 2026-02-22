using Application.DTO;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestsApi.DTO;

namespace QuestsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly QuestService _questService;

        public QuestsController(QuestService questService)
        {
            _questService = questService;
        }

        [HttpPost("PostZones")]
        public IActionResult PostZones([FromBody] List<ZoneDto> zones)
        {
            return Ok(new { message = "Зоны", count = zones.Count, zones });
        }

        [HttpGet("GetAllTemplates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _questService.GetAllTemplatesAsync();
            return Ok(templates);
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


        [HttpPost("CreateQuest")]
        public async Task<IActionResult> PostQuest([FromBody] CreateQuestRequest request)
        {
            var quest = new Quest
            {
                //Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Subject = request.Subject,
                Difficulty = request.Difficulty,
                Status = request.Status,
                AuthorId = Guid.Parse("0EF0EA1A-7E15-402B-894F-5D7224607447"),
                QuestRooms = request.Rooms.Select(r => new QuestRoom
                {
                    //Id = Guid.NewGuid(),
                    RoomTemplateId = r.RoomTemplateId,
                    Title = r.Title,
                    OrderIndex = r.OrderIndex,
                    Questions = r.Questions.Select(q => new Question
                    {
                        //Id = Guid.NewGuid(),
                        Text = q.Text,
                        Type = q.Type,
                        Points = q.Points,
                        Hint = q.Hint,
                        OrderIndex = q.OrderIndex,
                        AnswerOptions = q.AnswerOptions.Select(a => new AnswerOption
                        {
                            //Id = Guid.NewGuid(),
                            Text = a.Text,
                            IsCorrect = a.IsCorrect,
                            OrderIndex = a.OrderIndex
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            var res = await _questService.CreateQuestAsync(quest);

            return Ok( new { message = res, quest.Id });
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
