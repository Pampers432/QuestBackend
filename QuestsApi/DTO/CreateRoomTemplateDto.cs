using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public record CreateRoomTemplateDto(
        Guid? Id,
        string Name,
        IFormFile? PreviewImage,
        string SceneData
    );
}
