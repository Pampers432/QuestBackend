using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public class CreateRoomTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SceneData { get; set; } = string.Empty;
        //public IFormFile ImageFile { get; set; }
    }
}
