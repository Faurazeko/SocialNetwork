using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Nickname { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool IsAdmin { get; set; } = false;
        [Required]
        public bool IsBlocked { get; set; } = false;
        [Required]
        public string AboutText { get; set; } = "";
        [Required]
        public int Trollars { get; set; } = 0;
        [DataType("datetime2")]
        public DateTime LastOnlineTime { get; set; }
        public bool IsOnline { get; set; }
        [DataType("datetime2")]
        public DateTime CreatedDateTime { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        [DataType("datetime2")]
        public DateTime ForcedLogOutTime { get; set; } = DateTime.MinValue;
    }
}
