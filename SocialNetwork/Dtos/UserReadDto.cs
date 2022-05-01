using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Dtos
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public string AboutText { get; set; } = "";
        public int Trollars { get; set; } = 0;
        public DateTime LastOnlineTime { get; set; }
        public bool IsOnline { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
