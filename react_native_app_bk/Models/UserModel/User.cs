using System.ComponentModel.DataAnnotations;

namespace react_native_app_bk.Models.UserModel
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
    }
}
