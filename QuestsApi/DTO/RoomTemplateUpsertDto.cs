namespace QuestsApi.DTO
{
    public class RoomTemplateUpsertDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public string? SceneData { get; set; }
    }
}
