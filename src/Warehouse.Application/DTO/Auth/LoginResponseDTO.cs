namespace Warehouse.Application.DTO.Auth
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public LoginResponseDTO (string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
    

}