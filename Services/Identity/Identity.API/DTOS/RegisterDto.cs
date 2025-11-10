using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOS;

public class RegisterDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    
    [EmailAddress]
    [Required]
    public string Email { get; set; }
    
    [MinLength(6)]
    [Required]
    public string Password { get; set; }
}