namespace QuestsApi.DTO;

public record TemplateRenameDto(
    Guid Id,
    string SystemKey,
    string DisplayName
);

public record UpsertTemplateRenameRequest(
    string SystemKey,
    string DisplayName
);

public record BulkUpdateTemplateRenamesRequest(
    List<UpsertTemplateRenameRequest> Renames
);
