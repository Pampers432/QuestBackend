namespace QuestsApi.DTO
{
    public record RoomTemplateDto(
        Guid? Id,
        string Name,
        string PreviewImageUrl,
        string SceneData
    );
}
