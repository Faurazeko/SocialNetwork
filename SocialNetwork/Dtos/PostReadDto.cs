using SocialNetwork.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Dtos
{
    public class PostReadDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserReadDto User { get; set; }
        public string[] Files { get; set; }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedTime { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        
    }
}
