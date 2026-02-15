using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public class CreateRoomTemplateRequest
    {
        public string Name { get; set; }
        public string SceneData { get; set; }
        //public IFormFile ImageFile { get; set; }
    }
}
