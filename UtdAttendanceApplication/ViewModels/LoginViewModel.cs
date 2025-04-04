using System.ComponentModel.DataAnnotations;

namespace UtdAttendanceApplication.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UtdId { get; set; } = null!;


        public string? Password { get; set; }
    }
}
