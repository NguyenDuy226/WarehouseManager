using System.ComponentModel.DataAnnotations;

namespace Warehouse.Application.DTO.Auth;

public class LoginDTO
{
    [Required]
    [EmailAddress]
    [MinLength(6)]
    public string Email {get; set;} = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password {get; set;} = string.Empty;

}