using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using react_native_app_bk.Models.UserModel;

namespace react_native_app_bk.Models.RefreshTokenModel
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int User_Id { get; set; }
        [ForeignKey("User_Id")]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Refresh_Token { get; set; } = null!;

        [Required]
        public DateTime Refresh_Token_Expiry { get; set; }

        [Required]
        [MaxLength(50)]
        public string Device { get; set; } = null!;
    }
}
