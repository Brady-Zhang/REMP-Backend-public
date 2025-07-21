using System.ComponentModel.DataAnnotations;

namespace RealEstate.DTOs.User
{
    public class UpdatePasswordDto
        
    {
        [Required]
        public string currentPassword { get; set; }
        [Required]
        public string newPassword { get; set; }
    }
}
