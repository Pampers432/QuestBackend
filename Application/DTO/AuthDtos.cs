namespace Application.DTO;

public record RegisterRequest(string Username, string Password, string Role);

public record LoginRequest(string Username, string Password);

public record AuthResponse(string Token, string Username, string Role);
