namespace Warehouse.Application.DTO.Auth;

public record RefreshTokenDTO(
    string Token,
    string RefreshToken
);

public record AuthResponseDTO(
    string Token,
    string RefreshToken
    
);