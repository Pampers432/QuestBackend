using Application.DTO;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuestsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly QuestRepository _questRepository;
        private readonly QuestService _questService;

        public QuestsController(QuestRepository questRepository, QuestService questService = null)
        {
            _questRepository = questRepository;
            _questService = questService;
        }


        //public List<ZoneDto> zones = new List<ZoneDto>();
        //// GET: api/<ValuesController>
        //[HttpGet]
        //public IActionResult Get(string img)
        //{
        //    try
        //    {
        //        string img1 = img;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(ex.Message);
        //    }
        //    return Ok("Успешно");
        //}

        [HttpPost("PostZones")]
        public IActionResult PostZones([FromBody] List<ZoneDto> zones)
        {
            return Ok(new { message = "Зоны", count = zones.Count, zones });
        }

        //[HttpGet("GetAllTemplates")]
        //public IActionResult GetAllTemplates()
        //{

        //    return Ok();
        //}

        //[HttpPost("SaveImage")]
        //public async Task<IActionResult> SaveImage(IFormFile imageFile)
        //{
        //    if (imageFile == null || imageFile.Length == 0)
        //        return BadRequest("Файл не выбран");

        //    try
        //    {
        //        // 1. Подготовка папки
        //        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //        if (!Directory.Exists(uploadsPath))
        //            Directory.CreateDirectory(uploadsPath);

        //        // 2. Генерация имени и пути
        //        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
        //        var filePath = Path.Combine(uploadsPath, fileName);

        //        // 3. Сохранение физического файла
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await imageFile.CopyToAsync(stream);
        //        }

        //        // 4. Относительный путь для БД
        //        var relativePath = $"/uploads/{fileName}";

        //        // 5. Передача пути в репозиторий
        //        //_questRepository.SaveImage(relativePath);

        //        return Ok(new { url = relativePath, message = "Файл успешно сохранен на сервере" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}

        [HttpPost("PostTemplate")]
        public async Task<IActionResult> PostTemplate([FromForm] RoomTemplateDto dto)
        {
            if (dto == null || dto.PreviewImage == null)
                return BadRequest("Неполные данные или отсутствует изображение");

            try
            {
                // 1. Сохраняем изображение и получаем путь
                var relativePath = await SaveImage(dto.PreviewImage);

                // 2. Создаем сущность для передачи в сервис
                var entity = new RoomTemplate
                {
                    Id = dto.Id ?? Guid.NewGuid(),
                    Name = dto.Name,
                    PreviewImage = relativePath,
                    SceneData = dto.SceneData
                };

                // 3. Сохраняем всё через сервис
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


        //// GET api/<ValuesController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<ValuesController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<ValuesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
